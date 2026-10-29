import { withTimeout } from "../../map/utils/async.js";
import { zoomToFeatures } from "../../map/interaction/zoom.js";

// DSS project ids are Guids, so the ProjectID column is a string — the where
// clause must quote the value. (map/interaction/selection.js uses an unquoted
// `ProjectID = ${id}`, valid only for the PB map's integer ids.)
function projectWhere(projectId) {
    return `ProjectID = '${String(projectId).replace(/'/g, "''")}'`;
}

async function queryProjectObjectIds(layer, projectId) {
    try {
        return await withTimeout(
            layer.queryObjectIds({ where: projectWhere(projectId) }),
            `Project query on ${layer.id}`,
        );
    } catch (error) {
        console.warn(`Failed to query project on ${layer.id}:`, error);
        return [];
    }
}

export async function highlightProject(pavementLayerView, bridgeLayerView, pavementLayer, bridgeLayer, projectId, previousHandle) {
    previousHandle?.remove();

    const [pavementIds, bridgeIds] = await Promise.all([
        queryProjectObjectIds(pavementLayer, projectId),
        queryProjectObjectIds(bridgeLayer, projectId),
    ]);

    const handles = [];
    if (pavementIds.length > 0 && pavementLayerView) {
        handles.push(pavementLayerView.highlight(pavementIds));
    }
    if (bridgeIds.length > 0 && bridgeLayerView) {
        handles.push(bridgeLayerView.highlight(bridgeIds));
    }

    if (handles.length === 0) return null;

    return {
        remove() {
            handles.forEach((handle) => handle.remove());
        },
    };
}

export async function zoomToProject(view, pavementLayer, bridgeLayer, projectId) {
    const where = projectWhere(projectId);
    const features = [];

    for (const layer of [pavementLayer, bridgeLayer]) {
        try {
            const result = await withTimeout(
                layer.queryFeatures({ where, returnGeometry: true, outFields: ["OBJECTID"] }),
                `Project features on ${layer.id}`,
            );
            features.push(...(result.features ?? []));
        } catch (error) {
            console.warn(`Failed to query features for project on ${layer.id}:`, error);
        }
    }

    await zoomToFeatures(view, features);
}
