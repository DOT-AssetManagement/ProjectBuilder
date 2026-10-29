import { withTimeout } from "../utils/async.js";
import { zoomToFeatures } from "./zoom.js?v=2";
import {
    SCENARIO_BRIDGE_LAYER_ID,
    SCENARIO_BRIDGE_UNSELECTED_LAYER_ID,
} from "../arcgis/layers.js";

function parseInValue(token) {
    const trimmed = token.trim();
    if (trimmed.startsWith("'") && trimmed.endsWith("'")) {
        return trimmed.slice(1, -1).replace(/''/g, "'");
    }
    const num = Number(trimmed);
    return Number.isFinite(num) ? num : trimmed;
}

function splitInList(listStr) {
    const values = [];
    let current = "";
    let inQuote = false;

    for (let i = 0; i < listStr.length; i += 1) {
        const ch = listStr[i];
        if (ch === "'") {
            if (inQuote && listStr[i + 1] === "'") {
                current += "'";
                i += 1;
            } else {
                inQuote = !inQuote;
            }
            continue;
        }
        if (ch === "," && !inQuote) {
            if (current.trim()) values.push(parseInValue(current));
            current = "";
            continue;
        }
        current += ch;
    }
    if (current.trim()) values.push(parseInValue(current));
    return values;
}

function parseInClause(clause) {
    const match = clause.trim().match(/^(\w+)\s+IN\s+\((.+)\)$/i);
    if (!match) return null;
    return { field: match[1], values: splitInList(match[2]) };
}

export function featureMatchesDefinitionExpression(attributes, expression) {
    if (!expression) return true;

    return expression.split(/\s+AND\s+/i).every((clause) => {
        const parsed = parseInClause(clause);
        if (!parsed) return false;
        const raw = attributes?.[parsed.field];
        if (raw === null || raw === undefined || raw === "") return false;
        return parsed.values.some((value) => String(raw) === String(value));
    });
}

async function queryVisibleProjectFeatures(layer, projectId) {
    const where = `ProjectID = ${Number(projectId)}`;

    try {
        const result = await withTimeout(
            layer.queryFeatures({
                where,
                outFields: ["*"],
                returnGeometry: true,
            }),
            `Project graphics on ${layer.id}`,
        );
        const allFeatures = result.features ?? [];
        return allFeatures.filter((feature) =>
            featureMatchesDefinitionExpression(feature.attributes, layer.definitionExpression),
        );
    } catch (error) {
        console.warn(`Failed to query graphics for project on ${layer.id}:`, error);
        return [];
    }
}

export async function queryProjectObjectIds(layer, projectId) {
    const features = await queryVisibleProjectFeatures(layer, projectId);
    return features
        .map((feature) => feature.attributes?.OBJECTID)
        .filter((id) => id != null);
}

export async function queryProjectGraphics(layers, projectId, visibleTreatmentIds = null) {
    const featureLists = await Promise.all(
        layers.filter(Boolean).map((layer) => queryVisibleProjectFeatures(layer, projectId)),
    );
    const all = featureLists.flat();
    if (visibleTreatmentIds == null) {
        return all;
    }
    if (visibleTreatmentIds.size === 0) {
        return [];
    }
    return all.filter((feature) => {
        const treatmentId = Number(feature.attributes?.TreatmentID);
        return Number.isFinite(treatmentId) && visibleTreatmentIds.has(treatmentId);
    });
}

let selectedProjectsVisible = true;
let selectedBridgeFeatureCount = 0;

export function setSelectedProjectsVisible(visible) {
    selectedProjectsVisible = visible !== false;
}

export function getSelectedBridgeFeatureCount() {
    return selectedBridgeFeatureCount;
}

function isScenarioBridgeLayer(layer) {
    const id = layer?.id;
    return id === SCENARIO_BRIDGE_LAYER_ID || id === SCENARIO_BRIDGE_UNSELECTED_LAYER_ID;
}

function skipsArcGisHighlight(layer) {
    // Bridges swap to a selected icon; pavements get the cyan highlight shown in the Map Key.
    return isScenarioBridgeLayer(layer);
}

async function queryLayerFeatures(layer, where) {
    try {
        const result = await layer.queryFeatures({
            where,
            outFields: ["*"],
            returnGeometry: true,
        });
        return result?.features ?? [];
    } catch (error) {
        console.warn(`Failed to query ${layer?.id}:`, error);
        return [];
    }
}

function reclassifyBridgeRenderer(layer) {
    // A new renderer instance forces the unique-value symbols to be read again.
    // applyEdits alone leaves the previous picture marker on screen.
    const renderer = layer?.renderer;
    if (renderer?.clone) layer.renderer = renderer.clone();
}

async function writeBridgeSelected(layer, features, selected) {
    if (!layer || !features?.length) return false;
    const value = selected ? 1 : 0;
    // Mutate the features queryFeatures returned. Those are the features the
    // layer is drawing. Cloning them (or replacing .attributes) makes applyEdits
    // drop the change, so the default bridge.svg stays on the map.
    features.forEach((feature) => {
        if (feature?.attributes) feature.attributes.Selected = value;
    });

    try {
        const outcome = await layer.applyEdits({ updateFeatures: features });
        const errors = (outcome?.updateFeatureResults ?? []).filter((item) => item?.error);
        if (errors.length) {
            console.warn(`Bridge selection update failed on ${layer.id}:`, errors);
            return false;
        }
    } catch (error) {
        console.warn(`Failed to update bridge selection on ${layer.id}:`, error);
        return false;
    }

    reclassifyBridgeRenderer(layer);
    return true;
}

async function clearBridgeSelection(layers) {
    await Promise.all(layers.filter(isScenarioBridgeLayer).map(async (layer) => {
        const flagged = await queryLayerFeatures(layer, "Selected = 1");
        await writeBridgeSelected(layer, flagged, false);
    }));
    selectedBridgeFeatureCount = 0;
}

async function markBridgeProjectSelected(layers, projectId) {
    const want = Number(projectId);
    if (!Number.isFinite(want)) return false;
    const lists = await Promise.all(layers.filter(isScenarioBridgeLayer).map(async (layer) => {
        const features = await queryLayerFeatures(layer, `ProjectID = ${want}`);
        const wrote = await writeBridgeSelected(layer, features, true);
        return wrote ? features.length : 0;
    }));
    selectedBridgeFeatureCount = lists.reduce((sum, count) => sum + count, 0);
    return selectedBridgeFeatureCount > 0;
}

/**
 * Scenario pavements are outlined by a separate border layer. Scenario bridges swap
 * to the Map Key Selected Projects icon. The Selected attribute stays in sync with
 * that icon and the Map Key count.
 */
export async function highlightProject(layerViewsByLayer, layers, projectId, previousHandle) {
    if (typeof previousHandle?.release === "function") {
        await previousHandle.release();
    } else {
        previousHandle?.remove?.();
    }

    const activeLayers = layers.filter(Boolean);
    const bridgeLayers = activeLayers.filter(isScenarioBridgeLayer);
    const idsByLayer = await Promise.all(
        activeLayers.map((layer) => queryProjectObjectIds(layer, projectId)),
    );

    const handles = [];
    activeLayers.forEach((layer, index) => {
        const ids = idsByLayer[index];
        const layerView = layerViewsByLayer.get(layer);
        if (ids.length === 0 || !layerView || !selectedProjectsVisible || skipsArcGisHighlight(layer)) return;
        handles.push(layerView.highlight(ids));
    });

    await clearBridgeSelection(bridgeLayers);
    const bridgesMarked = await markBridgeProjectSelected(bridgeLayers, projectId);

    if (handles.length === 0 && !bridgesMarked) return null;

    return {
        async release() {
            handles.forEach((handle) => handle.remove());
            await clearBridgeSelection(bridgeLayers);
        },
        remove() {
            handles.forEach((handle) => handle.remove());
            clearBridgeSelection(bridgeLayers);
        },
        projectId,
    };
}

export async function zoomToProject(view, layers, projectId, options) {
    const features = await queryProjectGraphics(layers, projectId);
    await zoomToFeatures(view, features, options);
}
