import { buildInventoryInfo, getInventoryKind } from "./inventoryInfo.js";
import { resolveGraphicAttributes } from "./inventorySelection.js";
import { toAdhocItem } from "./adhocLoader.js?v=2";

function isScenarioLayer(graphic, scenarioLayers) {
    const layer = graphic?.layer;
    return scenarioLayers.includes(layer);
}

function isInventoryLayer(graphic, inventoryLayers) {
    return getInventoryKind(graphic?.layer) != null
        && inventoryLayers.some((layer) => layer === graphic.layer && layer.visible);
}

async function resolveProjectId(hitGraphic) {
    const attrs = hitGraphic.attributes ?? {};
    if (attrs.ProjectID != null) return attrs.ProjectID;

    const layer = hitGraphic.layer;
    const oid = attrs[layer.objectIdField] ?? attrs.OBJECTID;
    if (oid == null) return null;

    try {
        const { features } = await layer.queryFeatures({
            objectIds: [oid],
            outFields: ["*"],
            returnGeometry: false,
        });
        return features?.[0]?.attributes?.ProjectID ?? null;
    } catch (error) {
        console.warn("Failed to query graphic attributes:", error);
        return null;
    }
}

function getVisibleInventoryLayers(inventoryLayers) {
    return inventoryLayers.filter((layer) => layer?.visible);
}

export function setupClickHandling({
    view,
    pavementLayer,
    bridgeLayer,
    unselectedPavementLayer = null,
    unselectedBridgeLayer = null,
    inventoryLayers = [],
    adhocSession = null,
    onSelectProject,
    onClearProjectSelection,
    onSelectInventory,
    onAppendAdhocInventory,
    onClearInventorySelection,
    onClearAll,
}) {
    const scenarioLayers = [pavementLayer, bridgeLayer, unselectedPavementLayer, unselectedBridgeLayer]
        .filter(Boolean);

    view.on("click", async (event) => {
        try {
            const visibleScenarioLayers = scenarioLayers.filter((layer) => layer.visible);
            const visibleInventoryLayers = getVisibleInventoryLayers(inventoryLayers);
            const includeLayers = [...visibleScenarioLayers, ...visibleInventoryLayers];
            const response = await view.hitTest(event, { include: includeLayers });

            const hit = response.results.find((result) => result.graphic
                && includeLayers.includes(result.graphic.layer));

            if (!hit?.graphic) {
                onClearAll?.();
                return;
            }

            if (isScenarioLayer(hit.graphic, scenarioLayers)) {
                onClearInventorySelection?.();
                const projectId = await resolveProjectId(hit.graphic);
                if (projectId == null) return;
                const overlaps = response.results.filter((result) =>
                    result.graphic && isScenarioLayer(result.graphic, scenarioLayers));
                await onSelectProject?.(projectId, { graphic: hit.graphic, hits: overlaps });
                return;
            }

            if (isInventoryLayer(hit.graphic, inventoryLayers)) {
                const graphic = hit.graphic;
                const kind = getInventoryKind(graphic.layer);
                const attributes = await resolveGraphicAttributes(graphic);
                const item = toAdhocItem(kind, attributes);
                const info = buildInventoryInfo(kind, attributes);

                if (adhocSession?.shouldAppend()) {
                    onAppendAdhocInventory?.({
                        graphic,
                        layer: graphic.layer,
                        kind,
                        item,
                        info,
                    });
                    return;
                }

                onClearProjectSelection?.();
                onSelectInventory?.({
                    graphic,
                    layer: graphic.layer,
                    kind,
                    item,
                    info,
                });
            }
        } catch (error) {
            // A failure anywhere above (hitTest, attribute resolution, selection callbacks)
            // would otherwise be an unhandled rejection that silently aborts the click —
            // e.g. the project panel never opening with no visible error at all. Surface it.
            console.error("Map click handling failed:", error);
        }
    });
}
