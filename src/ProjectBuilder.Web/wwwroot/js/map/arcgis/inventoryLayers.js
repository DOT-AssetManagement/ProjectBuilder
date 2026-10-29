import {
    BRIDGE_INVENTORY_OUT_FIELDS,
    MPMS_INVENTORY_OUT_FIELDS,
    ROAD_INVENTORY_OUT_FIELDS,
} from "./inventoryFields.js";
import {
    BRIDGE_SERVICE_URL,
    MPMS_SERVICE_URL,
    PAVEMENT_SERVICE_URL,
} from "../config.js";
import { bridgeIconSizeForZoom } from "./layers.js?v=10";
import {
    getBridgeBaseLayerRenderer,
    getMPMSLayerRenderer,
    getPavementBaseLayerRenderer,
} from "./renderers.js?v=5";

export const PAVEMENT_BASE_LAYER_ID = "PavementBaseLayer";
export const BRIDGES_BASE_LAYER_ID = "BridgesBaseLayer";
export const MPMS_LAYER_ID = "MPMSLayer";

export function createInventoryLayers(FeatureLayer) {
    const pavementBaseLayer = new FeatureLayer({
        url: PAVEMENT_SERVICE_URL,
        title: "Pavement Base Data",
        id: PAVEMENT_BASE_LAYER_ID,
        definitionExpression: "1=1",
        outFields: ROAD_INVENTORY_OUT_FIELDS,
        popupEnabled: false,
        visible: false,
        renderer: getPavementBaseLayerRenderer(false, []),
    });

    const bridgesBaseLayer = new FeatureLayer({
        url: BRIDGE_SERVICE_URL,
        title: "Bridge Base Data",
        id: BRIDGES_BASE_LAYER_ID,
        definitionExpression: "BRIDGE_CATEGORY IN ('S', 'O', 'A')",
        outFields: BRIDGE_INVENTORY_OUT_FIELDS,
        popupEnabled: false,
        visible: false,
        renderer: getBridgeBaseLayerRenderer(false, []),
    });

    const mpmsLayer = new FeatureLayer({
        url: MPMS_SERVICE_URL,
        title: "MPMS Layer Data",
        id: MPMS_LAYER_ID,
        definitionExpression: "PROJECT_STATUS IN ('ACT', 'PROG')",
        outFields: MPMS_INVENTORY_OUT_FIELDS,
        popupEnabled: false,
        visible: false,
        renderer: getMPMSLayerRenderer(false, []),
    });

    return { pavementBaseLayer, bridgesBaseLayer, mpmsLayer };
}

export function bringScenarioLayersToFront(map, ...scenarioLayers) {
    if (!map) return;

    scenarioLayers.forEach((layer) => {
        if (layer && map.layers.includes(layer)) {
            map.reorder(layer, map.layers.length - 1);
        }
    });
}

export function applyRoadRatings(pavementBaseLayer, selectedRatings, scenarioPavementLayer, map, scenarioLayers) {
    if (pavementBaseLayer) {
        pavementBaseLayer.renderer = selectedRatings.length > 0
            ? getPavementBaseLayerRenderer(true, selectedRatings)
            : getPavementBaseLayerRenderer(false, []);
        pavementBaseLayer.definitionExpression = null;
    }
    if (scenarioPavementLayer) {
        scenarioPavementLayer.visible = true;
    }
    bringScenarioLayersToFront(map, ...scenarioLayers);
}

function rendererIsBridgeIcons(renderer) {
    const symbol = renderer?.defaultSymbol ?? renderer?.symbol;
    return symbol?.type === "picture-marker" && String(symbol.url ?? "").includes("/icons/bridge");
}

function paintInventoryBridges(bridgesBaseLayer) {
    if (!bridgesBaseLayer || bridgesBaseLayer.__pbApplyingRenderer) return;
    const ratings = bridgesBaseLayer.__pbBridgeRatings ?? [];
    const size = bridgesBaseLayer.__pbBridgeIconSize ?? bridgeIconSizeForZoom(10);
    bridgesBaseLayer.__pbApplyingRenderer = true;
    bridgesBaseLayer.renderer = getBridgeBaseLayerRenderer(ratings.length > 0, ratings, size);
    bridgesBaseLayer.__pbApplyingRenderer = false;
}

function paintInventoryBridgesAfterLoad(bridgesBaseLayer) {
    paintInventoryBridges(bridgesBaseLayer);
    if (!bridgesBaseLayer?.__pbBridgeIconHooked && typeof bridgesBaseLayer?.when === "function") {
        bridgesBaseLayer.__pbBridgeIconHooked = true;
        bridgesBaseLayer.when(() => paintInventoryBridges(bridgesBaseLayer));
    }
}

/** Service drawing info replaces a client renderer. Put the icons back whenever that happens. */
export function attachInventoryBridgeIcons(bridgesBaseLayer, view) {
    if (!bridgesBaseLayer || bridgesBaseLayer.__pbBridgeIconWatch) return;
    bridgesBaseLayer.__pbBridgeIconWatch = true;
    bridgesBaseLayer.watch("renderer", (renderer) => {
        if (bridgesBaseLayer.__pbApplyingRenderer || rendererIsBridgeIcons(renderer)) return;
        paintInventoryBridges(bridgesBaseLayer);
    });
    view?.whenLayerView?.(bridgesBaseLayer).then(() => paintInventoryBridges(bridgesBaseLayer)).catch(() => {});
    paintInventoryBridgesAfterLoad(bridgesBaseLayer);
}

export function applyBridgeRatings(bridgesBaseLayer, selectedRatings, scenarioBridgeLayer, map, scenarioLayers) {
    if (bridgesBaseLayer) {
        bridgesBaseLayer.__pbBridgeRatings = selectedRatings ?? [];
        bridgesBaseLayer.definitionExpression = null;
        paintInventoryBridgesAfterLoad(bridgesBaseLayer);
    }
    if (scenarioBridgeLayer) {
        scenarioBridgeLayer.visible = true;
    }
    bringScenarioLayersToFront(map, ...scenarioLayers);
}

export function scaleInventoryBridgeIcons(bridgesBaseLayer, zoom) {
    if (!bridgesBaseLayer) return;
    bridgesBaseLayer.__pbBridgeIconSize = bridgeIconSizeForZoom(zoom);
    paintInventoryBridgesAfterLoad(bridgesBaseLayer);
}

export function applyMPMSRatings(mpmsLayer, selectedRatings, scenarioPavementLayer, map, scenarioLayers) {
    if (mpmsLayer) {
        mpmsLayer.renderer = selectedRatings.length > 0
            ? getMPMSLayerRenderer(true, selectedRatings)
            : getMPMSLayerRenderer(false, []);
        mpmsLayer.definitionExpression = null;
    }
    if (scenarioPavementLayer) {
        scenarioPavementLayer.visible = true;
    }
    bringScenarioLayersToFront(map, ...scenarioLayers);
}

