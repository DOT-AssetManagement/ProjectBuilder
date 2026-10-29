import { withTimeout } from "../utils/async.js";
import {
    fromSectionOf,
    projectIdOf,
    routeOf,
    treatmentIdOf,
    treatmentLabel,
    treatmentType,
    districtOf,
    countyNameOf,
    toSectionOf,
    yearOf,
    statusOf,
    hasCommittedOf,
} from "../treatments/helpers.js?v=3";
import { formatCrsId, resolveSideIndFromSegments } from "../utils/crsId.js";

function toInt(value) {
    const n = Number.parseInt(String(value), 10);
    return Number.isNaN(n) ? null : n;
}

function hasAnyValue(values) {
    return Object.values(values).some((v) => v != null && v !== "");
}

// Filterable attributes attached to every scenario graphic so the left filter
// panel can apply layer definitionExpressions.
function filterAttributes(treatment) {
    return {
        AssetType: treatmentType(treatment) || null,
        District: toInt(districtOf(treatment)),
        County: (countyNameOf(treatment) || "").toString() || null,
        Route: toInt(routeOf(treatment)),
        Section: toInt(toSectionOf(treatment)),
        Year: toInt(yearOf(treatment)),
        Status: statusOf(treatment),
        HasCommitted: hasCommittedOf(treatment) || null,
    };
}

// `extraAttributes(treatment)` is an optional per-graphic attribute merge applied
// LAST, so callers can override the default baked attributes (e.g. DssMap supplies
// its own AssetType/Treatment, which the shared treatment-based accessors get wrong).
// `unionCache` (optional Map) reuses one unioned geometry per section range, since the same range recurs across years.
export function buildPavementGraphicsFromGrouped(grouped, Graphic, geometryEngine, startObjectId, extraAttributes = null, unionCache = null) {
    const graphics = [];
    let objectId = startObjectId;

    for (const { clause, geometries, segments = [] } of grouped.values()) {
        if (geometries.length === 0) continue;

        const rangeKey = `${clause.meta.county}|${clause.meta.route}|${clause.meta.fromNum}|${clause.meta.toNum}`;
        let unioned = geometries[0];
        if (unionCache?.has(rangeKey)) {
            unioned = unionCache.get(rangeKey);
        } else if (geometries.length > 1) {
            try {
                unioned = geometryEngine.union(geometries);
            } catch (error) {
                console.warn("Failed to union pavement geometry:", clause.where, error);
                continue;
            }
            unionCache?.set(rangeKey, unioned);
        }

        if (!unioned) continue;

        const treatment = clause.meta.treatment;
        const countyCode = clause.meta.county;
        const route = clause.meta.route;
        const sectionFrom = fromSectionOf(treatment) || String(clause.meta.fromNum).padStart(4, "0");
        const sideInd = resolveSideIndFromSegments(segments, sectionFrom);
        const crsId = formatCrsId(countyCode, route, sideInd, sectionFrom);

        // Surface the condition-rating fields queried alongside the geometry (see
        // PAVEMENT_OUT_FIELDS) onto the treatment itself — the right-side panel's
        // CONDITION card reads this straight off the same treatment object.
        const conditionSegment = segments.find((s) => s.condition && hasAnyValue(s.condition));
        if (conditionSegment) treatment.condition = conditionSegment.condition;

        graphics.push(
            new Graphic({
                geometry: unioned,
                attributes: {
                    OBJECTID: objectId,
                    ProjectID: projectIdOf(treatment),
                    TreatmentID: treatmentIdOf(treatment),
                    Treatment: treatmentLabel(treatment),
                    CRS_ID: crsId,
                    SIDE_IND: sideInd,
                    CountyCode: countyCode,
                    Route: Number.parseInt(route, 10) || route,
                    SectionFrom: sectionFrom,
                    ...filterAttributes(treatment),
                    ...(extraAttributes ? extraAttributes(treatment) : null),
                },
            }),
        );
        objectId += 1;
    }

    return { graphics, nextObjectId: objectId };
}

export function buildBridgeGraphics(bridgeFeatures, bridgeMeta, Graphic, startObjectId, extraAttributes = null) {
    const graphics = [];
    const metaByKey = new Map(bridgeMeta.map((entry) => [entry.brKey, entry]));
    let objectId = startObjectId;

    for (const feature of bridgeFeatures) {
        const attrs = feature.attributes ?? {};
        const brKey = String(attrs.BRKEY ?? "").trim();
        const meta = metaByKey.get(brKey);
        if (!meta || !feature.geometry) continue;

        // Surface the condition-rating fields queried alongside the geometry (see
        // BRIDGE_OUT_FIELDS) onto the treatment itself — the right-side panel's
        // CONDITION card reads this straight off the same treatment object.
        const condition = {
            condition: attrs.CONDITION,
            deckRating: attrs.DKRATING,
            superstructureRating: attrs.SUPRATING,
            substructureRating: attrs.SUBRATING,
            culvertRating: attrs.CULVRATING,
        };
        if (hasAnyValue(condition)) meta.treatment.condition = condition;

        graphics.push(
            new Graphic({
                geometry: feature.geometry,
                attributes: {
                    OBJECTID: objectId,
                    ProjectID: projectIdOf(meta.treatment),
                    TreatmentID: treatmentIdOf(meta.treatment),
                    Treatment: treatmentLabel(meta.treatment),
                    BRKEY: brKey,
                    Selected: 0,
                    ...filterAttributes(meta.treatment),
                    ...(extraAttributes ? extraAttributes(meta.treatment) : null),
                },
            }),
        );
        objectId += 1;
    }

    return { graphics, nextObjectId: objectId };
}

export async function clearLayerGraphics(layer) {
    // Features are added via applyEdits (see applyLayerGraphics). Clearing only
    // layer.source can leave those edits in the layer feature store, so deleted
    // treatments stay visible until a full page reload. Delete through applyEdits
    // first, then reset the source collection as a belt-and-suspenders cleanup.
    try {
        const result = await layer.queryFeatures({
            where: "1=1",
            returnGeometry: false,
            outFields: [layer.objectIdField || "OBJECTID"],
        });
        const existing = result?.features ?? [];
        if (existing.length > 0) {
            await layer.applyEdits({ deleteFeatures: existing });
        }
    } catch (error) {
        console.warn(`Failed to clear graphics on ${layer.id}:`, error);
    }

    try {
        layer.source?.removeAll?.();
    } catch (error) {
        console.warn(`Failed to reset source on ${layer.id}:`, error);
    }
}

export async function applyLayerGraphics(layer, graphics) {
    if (graphics.length === 0) return;

    try {
        await layer.applyEdits({ addFeatures: graphics });
    } catch (error) {
        console.warn(`Failed to add graphics to ${layer.id}:`, error);
    }
}

export async function zoomToLayers(view, layers) {
    const extents = [];

    for (const layer of layers) {
        try {
            const result = await withTimeout(layer.queryExtent(), "Map extent query");
            if (result?.extent) extents.push(result.extent);
        } catch (error) {
            console.warn(`Failed to query extent for ${layer.id}:`, error);
        }
    }

    if (extents.length === 0) return;

    let target = extents[0];
    for (let i = 1; i < extents.length; i += 1) {
        target = target.union(extents[i]);
    }

    try {
        await withTimeout(view.goTo(target.expand(1.2)), "Map zoom");
    } catch (error) {
        console.warn("Map zoom failed:", error);
    }
}

export function assignObjectIds(graphics, startObjectId = 1) {
    let objectId = startObjectId;
    for (const graphic of graphics) {
        graphic.attributes.OBJECTID = objectId;
        objectId += 1;
    }
    return objectId;
}
