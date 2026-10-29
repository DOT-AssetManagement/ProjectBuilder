import {
    BRIDGE_KEY_BATCH_SIZE,
    BRIDGE_OUT_FIELDS,
    BRIDGE_SERVICE_URL,
    PAVEMENT_CLAUSE_BATCH_SIZE,
    PAVEMENT_OUT_FIELDS,
    PAVEMENT_SERVICE_URL,
    PARALLEL_REQUEST_LIMIT,
} from "../map/config.js";
import { chunkArray, runWithConcurrency } from "../map/utils/async.js";
import {
    applyLayerGraphics,
    assignObjectIds,
    buildBridgeGraphics,
    buildPavementGraphicsFromGrouped,
    zoomToLayers,
} from "../map/arcgis/graphics.js?v=3";
import { matchPavementFeaturesToClauses, queryPaginatedFeatures } from "../map/arcgis/query.js";
import {
    buildPavementClause,
    formatBrKeyInList,
    projectIdOf,
    uniqueBridgeKeys,
} from "../map/treatments/helpers.js";

// DSS work candidates carry AssetType directly ("P"/"B") and use `treatmentName`/
// `treatmentType`, which the shared graphic builder's treatment-based accessors
// (treatmentType()/treatmentLabel()) get wrong. Override those two attributes so the
// left filter panel's Asset Type / Treatment facets resolve correctly.
function dssGraphicAttributes(workCandidate) {
    return {
        AssetType: (workCandidate.assetType ?? workCandidate.AssetType ?? "").toString().trim().toUpperCase() || null,
        Treatment: workCandidate.treatmentName ?? workCandidate.TreatmentName
            ?? workCandidate.treatmentType ?? workCandidate.TreatmentType ?? null,
    };
}

async function loadPavementCandidates(clauses, view, queryModule, pavementLayer, Graphic, geometryEngine, onBatchComplete) {
    const batches = chunkArray(clauses, PAVEMENT_CLAUSE_BATCH_SIZE);
    let renderedCount = 0;
    let nextObjectId = 1;

    const batchGraphicsGroups = await runWithConcurrency(
        batches,
        PARALLEL_REQUEST_LIMIT,
        async (batch) => {
            const whereClause = batch.map((clause) => clause.where).join(" OR ");

            const features = await queryPaginatedFeatures(
                PAVEMENT_SERVICE_URL,
                whereClause,
                view,
                queryModule,
                PAVEMENT_OUT_FIELDS,
            );

            const grouped = matchPavementFeaturesToClauses(features, batch);
            const { graphics } = buildPavementGraphicsFromGrouped(
                grouped,
                Graphic,
                geometryEngine,
                1,
                dssGraphicAttributes,
            );

            onBatchComplete?.();
            return graphics;
        },
    );

    for (const graphics of batchGraphicsGroups) {
        if (graphics.length === 0) continue;
        nextObjectId = assignObjectIds(graphics, nextObjectId);
        await applyLayerGraphics(pavementLayer, graphics);
        renderedCount += graphics.length;
    }

    return renderedCount;
}

async function loadBridgeCandidates(bridgeKeys, bridgeMeta, view, queryModule, bridgeLayer, Graphic, onBatchComplete) {
    const uniqueKeys = uniqueBridgeKeys(bridgeKeys);
    const batches = chunkArray(uniqueKeys, BRIDGE_KEY_BATCH_SIZE);
    let renderedCount = 0;
    let nextObjectId = 1;

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

            const batchMeta = bridgeMeta.filter((entry) => batch.includes(entry.brKey));
            const { graphics } = buildBridgeGraphics(features, batchMeta, Graphic, 1, dssGraphicAttributes);

            onBatchComplete?.();
            return graphics;
        },
    );

    for (const graphics of batchGraphicsGroups) {
        if (graphics.length === 0) continue;
        nextObjectId = assignObjectIds(graphics, nextObjectId);
        await applyLayerGraphics(bridgeLayer, graphics);
        renderedCount += graphics.length;
    }

    return renderedCount;
}

// DSS work candidates are keyed by AssetType ("P"/"B") directly -- unlike PB treatments,
// which derive it from a TreatType/treatmentType string -- and reference Guid project/
// treatment ids rather than ints, so project matching below compares as strings.
export async function loadDssMapCandidates(view, pavementLayer, bridgeLayer, Graphic, geometryEngine, queryModule, dssMapData, onProgress) {
    const projects = dssMapData.projects ?? dssMapData.Projects ?? [];
    const workCandidates = dssMapData.workCandidates ?? dssMapData.WorkCandidates ?? [];
    const projectById = new Map(projects.map((project) => [String(projectIdOf(project)), project]));

    const pavementClauses = [];
    const bridgeKeys = [];
    const bridgeMeta = [];

    for (const workCandidate of workCandidates) {
        const assetType = (workCandidate.assetType ?? workCandidate.AssetType ?? "").toString().trim().toUpperCase();
        const project = projectById.get(String(projectIdOf(workCandidate)));
        if (!project) continue;

        if (assetType === "P") {
            const clause = buildPavementClause(workCandidate);
            if (clause) pavementClauses.push(clause);
        } else if (assetType === "B") {
            const brKey = workCandidate.brkey ?? workCandidate.BRKEY;
            if (!brKey) continue;
            const brKeyText = String(brKey).trim();
            bridgeKeys.push(brKeyText);
            bridgeMeta.push({ type: "B", treatment: workCandidate, project, brKey: brKeyText });
        }
    }

    const pavementBatchCount = pavementClauses.length > 0
        ? Math.ceil(pavementClauses.length / PAVEMENT_CLAUSE_BATCH_SIZE)
        : 0;
    const bridgeBatchCount = bridgeKeys.length > 0
        ? Math.ceil(uniqueBridgeKeys(bridgeKeys).length / BRIDGE_KEY_BATCH_SIZE)
        : 0;
    const totalSteps = Math.max(1, pavementBatchCount + bridgeBatchCount + 1);
    let completedSteps = 0;

    const reportStep = () => {
        completedSteps += 1;
        onProgress?.(completedSteps / totalSteps);
    };

    let pavementCount = 0;
    let bridgeCount = 0;

    if (pavementClauses.length > 0) {
        pavementCount = await loadPavementCandidates(
            pavementClauses,
            view,
            queryModule,
            pavementLayer,
            Graphic,
            geometryEngine,
            reportStep,
        );
    }

    if (bridgeKeys.length > 0) {
        bridgeCount = await loadBridgeCandidates(
            bridgeKeys,
            bridgeMeta,
            view,
            queryModule,
            bridgeLayer,
            Graphic,
            reportStep,
        );
    }

    const layersToZoom = [];
    if (pavementCount > 0) layersToZoom.push(pavementLayer);
    if (bridgeCount > 0) layersToZoom.push(bridgeLayer);
    await zoomToLayers(view, layersToZoom);
    reportStep();

    return { pavementCount, bridgeCount };
}
