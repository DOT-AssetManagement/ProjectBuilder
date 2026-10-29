import { BRIDGES_BASE_LAYER_ID } from "../arcgis/inventoryLayers.js";
import { resolveCrsIdFromPavementAttrs } from "../utils/crsId.js";
import {
    getTreatmentsForProject,
    getSequenceNoForProject,
    resolveProjectTreatmentPosition,
} from "./tooltipMeta.js";
import { formatProjectSeq } from "../treatments/helpers.js";

function buildTooltipItem(event, text, treatmentIndex, treatmentTotal, offsetX = 0) {
    return {
        x: event.x + offsetX,
        y: event.y + 1,
        text,
        treatmentIndex,
        treatmentTotal,
    };
}

export function setupHoverTooltips({
    view,
    pavementLayer,
    bridgeLayer,
    unselectedPavementLayer = null,
    unselectedBridgeLayer = null,
    inventoryBridgeLayer,
    getScenarioData,
    tooltip,
    onTreatmentHover = null,
    onTreatmentHoverEnd = null,
}) {
    let hoverHandle = null;
    let hoverToken = 0;
    const pavementLayers = [pavementLayer, unselectedPavementLayer].filter(Boolean);
    const bridgeLayers = [bridgeLayer, unselectedBridgeLayer].filter(Boolean);

    function treatmentIdOfHit(attrs) {
        return attrs.TreatmentID ?? attrs.TreatmentId ?? attrs.TreatId ?? null;
    }

    function showScenarioHover(event, projectId, attrs, text, offsetX) {
        const treatments = getTreatmentsForProject(getScenarioData(), projectId);
        const position = resolveProjectTreatmentPosition(treatments, attrs);
        tooltip.show(buildTooltipItem(event, text, position?.index, position?.total, offsetX));
        const treatmentId = treatmentIdOfHit(attrs);
        if (treatmentId == null || treatmentId === "") onTreatmentHoverEnd?.();
        else onTreatmentHover?.(projectId, treatmentId);
    }

    view.on("pointer-leave", () => {
        hoverToken += 1;
        tooltip.hide();
        onTreatmentHoverEnd?.();
    });

    hoverHandle = view.on("pointer-move", async (event) => {
        const token = ++hoverToken;
        const includeLayers = [
            ...pavementLayers.filter((layer) => layer.visible),
            ...bridgeLayers.filter((layer) => layer.visible),
        ];
        if (inventoryBridgeLayer?.visible) {
            includeLayers.push(inventoryBridgeLayer);
        }

        const response = await view.hitTest(event, { include: includeLayers });
        if (token !== hoverToken) return;
        const results = response.results ?? [];

        const pavementHit = results.find(
            (result) => pavementLayers.includes(result.graphic?.layer),
        )?.graphic;
        const bridgeHit = results.find(
            (result) => bridgeLayers.includes(result.graphic?.layer),
        )?.graphic;
        const inventoryBridgeHit = results.find(
            (result) => result.graphic?.layer?.id === BRIDGES_BASE_LAYER_ID,
        )?.graphic;

        if (pavementHit) {
            const attrs = pavementHit.attributes ?? {};
            const projectId = attrs.ProjectID ?? "";
            const displayId = formatProjectSeq(getSequenceNoForProject(getScenarioData(), projectId), attrs.Status) ?? projectId;
            const crsId = resolveCrsIdFromPavementAttrs(attrs);
            showScenarioHover(
                event,
                projectId,
                attrs,
                `Project ID: ${displayId}, CRS ID: ${crsId}, Status: ${attrs.Status ?? "Selected"}`,
                -19,
            );
            return;
        }

        if (bridgeHit) {
            const attrs = bridgeHit.attributes ?? {};
            const projectId = attrs.ProjectID ?? "";
            const displayId = formatProjectSeq(getSequenceNoForProject(getScenarioData(), projectId), attrs.Status) ?? projectId;
            const brkey = attrs.BRKEY ?? "";
            showScenarioHover(
                event,
                projectId,
                attrs,
                `Project ID: ${displayId}, BRKey: ${brkey}, Status: ${attrs.Status ?? "Selected"}`,
                19,
            );
            return;
        }

        if (inventoryBridgeHit) {
            const attrs = inventoryBridgeHit.attributes ?? {};
            tooltip.show(buildTooltipItem(
                event,
                attrs.FEATINT || "Unnamed Bridge",
                null,
                null,
                -19,
            ));
            onTreatmentHoverEnd?.();
            return;
        }

        tooltip.hide();
        onTreatmentHoverEnd?.();
    });

    return () => {
        hoverHandle?.remove();
        hoverHandle = null;
    };
}
