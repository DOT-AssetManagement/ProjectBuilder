import {
    BRIDGE_KEY_BATCH_SIZE,
    BRIDGE_OUT_FIELDS,
    BRIDGE_SERVICE_URL,
    PAVEMENT_CLAUSE_BATCH_SIZE,
    PAVEMENT_OUT_FIELDS,
    PAVEMENT_SERVICE_URL,
    PARALLEL_REQUEST_LIMIT,
} from "../config.js";
import { chunkArray, runWithConcurrency } from "../utils/async.js";
import { applyLayerGraphics, assignObjectIds, buildBridgeGraphics, buildPavementGraphicsFromGrouped, zoomToLayers } from "../arcgis/graphics.js?v=3";
import { matchPavementFeaturesToClauses, queryPaginatedFeatures } from "../arcgis/query.js?v=2";
import {
    buildPavementClause,
    formatBrKeyInList,
    mergePavementRanges,
    projectIdOf,
    treatmentType,
    uniqueBridgeKeys,
} from "./helpers.js?v=2";

// Splits a batch's graphics by Status and adds each subset to its own layer/objectId
// sequence, so one external query batch can still feed both the "selected" (always shown)
// and "unselected" (hidden until toggled on) layers instead of querying twice.
async function applySplitByStatus(graphics, selectedLayer, unselectedLayer, counters) {
    const selected = graphics.filter((g) => g.attributes.Status !== "Unselected");
    const unselected = graphics.filter((g) => g.attributes.Status === "Unselected");

    if (selected.length > 0) {
        counters.selected = assignObjectIds(selected, counters.selected);
        await applyLayerGraphics(selectedLayer, selected);
    }
    if (unselected.length > 0) {
        counters.unselected = assignObjectIds(unselected, counters.unselected);
        await applyLayerGraphics(unselectedLayer, unselected);
    }

    return { selectedCount: selected.length, unselectedCount: unselected.length };
}

// Treatments on the same section range (the same road in different years) share one query and one union.
async function loadPavementTreatments(clauses, rangeWheres, view, queryModule, pavementLayer, unselectedPavementLayer, Graphic, geometryEngine, onBatchComplete) {
    const batches = chunkArray(rangeWheres, PAVEMENT_CLAUSE_BATCH_SIZE);
    const counters = { selected: 1, unselected: 1 };

    const batchFeatureGroups = await runWithConcurrency(
        batches,
        PARALLEL_REQUEST_LIMIT,
        async (batch) => {
            const features = await queryPaginatedFeatures(
                PAVEMENT_SERVICE_URL,
                batch.join(" OR "),
                view,
                queryModule,
                PAVEMENT_OUT_FIELDS,
            );

            onBatchComplete?.();
            return features;
        },
    );

    const grouped = matchPavementFeaturesToClauses(batchFeatureGroups.flat(), clauses);
    const { graphics } = buildPavementGraphicsFromGrouped(grouped, Graphic, geometryEngine, 1, null, new Map());
    if (graphics.length === 0) return { selectedCount: 0, unselectedCount: 0 };

    return applySplitByStatus(graphics, pavementLayer, unselectedPavementLayer, counters);
}

async function loadBridgeTreatments(bridgeKeys, bridgeMeta, view, queryModule, bridgeLayer, unselectedBridgeLayer, Graphic, onBatchComplete) {
    const uniqueKeys = uniqueBridgeKeys(bridgeKeys);
    const batches = chunkArray(uniqueKeys, BRIDGE_KEY_BATCH_SIZE);
    let selectedCount = 0;
    let unselectedCount = 0;
    const counters = { selected: 1, unselected: 1 };

    const batchGraphicsGroups = await runWithConcurrency(
        batches,
        PARALLEL_REQUEST_LIMIT,
        async (batch) => {
            const whereClause = `BRKEY IN (${formatBrKeyInList(batch)})`;

            const features = await queryPaginatedFeatures(
                BRIDGE_SERVICE_URL,
                whereClause,
                view,
                queryModule,
                BRIDGE_OUT_FIELDS,
            );

            const batchKeys = new Set(batch);
            const batchMeta = bridgeMeta.filter((entry) => batchKeys.has(entry.brKey));
            const { graphics } = buildBridgeGraphics(features, batchMeta, Graphic, 1);

            onBatchComplete?.();
            return graphics;
        },
    );

    for (const graphics of batchGraphicsGroups) {
        if (graphics.length === 0) continue;
        const result = await applySplitByStatus(graphics, bridgeLayer, unselectedBridgeLayer, counters);
        selectedCount += result.selectedCount;
        unselectedCount += result.unselectedCount;
    }

    return { selectedCount, unselectedCount };
}

export async function loadScenarioTreatments(
    view,
    pavementLayer,
    bridgeLayer,
    Graphic,
    geometryEngine,
    queryModule,
    scenarioData,
    onProgress,
    options = {},
) {
    const { zoom = true, unselectedPavementLayer = null, unselectedBridgeLayer = null } = options;
    const projects = scenarioData.projects ?? scenarioData.Projects ?? [];
    const treatments = scenarioData.treatments ?? scenarioData.Treatments ?? [];
    const projectById = new Map(projects.map((project) => [projectIdOf(project), project]));

    const pavementClauses = [];
    const bridgeKeys = [];
    const bridgeMeta = [];

    for (const treatment of treatments) {
        const type = treatmentType(treatment);
        const project = projectById.get(projectIdOf(treatment));
        if (!project) continue;

        if (type === "P") {
            const clause = buildPavementClause(treatment);
            if (clause) pavementClauses.push(clause);
        } else if (type === "B") {
            const brKey = treatment.brkey ?? treatment.BRKEY;
            if (!brKey) continue;
            const brKeyText = String(brKey).trim();
            bridgeKeys.push(brKeyText);
            bridgeMeta.push({ type: "B", treatment, project, brKey: brKeyText });
        }
    }

    const pavementRangeWheres = mergePavementRanges(pavementClauses);
    const pavementBatchCount = Math.ceil(pavementRangeWheres.length / PAVEMENT_CLAUSE_BATCH_SIZE);
    const bridgeBatchCount = bridgeKeys.length > 0
        ? Math.ceil(uniqueBridgeKeys(bridgeKeys).length / BRIDGE_KEY_BATCH_SIZE)
        : 0;
    const totalSteps = Math.max(1, pavementBatchCount + bridgeBatchCount + 1);
    let completedSteps = 0;

    const reportStep = () => {
        completedSteps += 1;
        onProgress?.(completedSteps / totalSteps);
    };

    const empty = { selectedCount: 0, unselectedCount: 0 };
    const [pavementResult, bridgeResult] = await Promise.all([
        pavementClauses.length > 0
            ? loadPavementTreatments(
                pavementClauses,
                pavementRangeWheres,
                view,
                queryModule,
                pavementLayer,
                unselectedPavementLayer,
                Graphic,
                geometryEngine,
                reportStep,
            )
            : empty,
        bridgeKeys.length > 0
            ? loadBridgeTreatments(
                bridgeKeys,
                bridgeMeta,
                view,
                queryModule,
                bridgeLayer,
                unselectedBridgeLayer,
                Graphic,
                reportStep,
            )
            : empty,
    ]);
    const pavementCount = pavementResult.selectedCount;
    const unselectedPavementCount = pavementResult.unselectedCount;
    const bridgeCount = bridgeResult.selectedCount;
    const unselectedBridgeCount = bridgeResult.unselectedCount;

    // Zoom to the always-visible "selected" layers only — unselected content stays hidden
    // until toggled on, so it shouldn't drive the initial extent.
    if (zoom) {
        const layersToZoom = [];
        if (pavementCount > 0) layersToZoom.push(pavementLayer);
        if (bridgeCount > 0) layersToZoom.push(bridgeLayer);
        await zoomToLayers(view, layersToZoom);
    }
    reportStep();

    return { pavementCount, bridgeCount, unselectedPavementCount, unselectedBridgeCount };
}
