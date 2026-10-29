import {
    BRIDGE_INVENTORY_OUT_FIELDS,
    MPMS_INVENTORY_OUT_FIELDS,
    ROAD_INVENTORY_OUT_FIELDS,
} from "../arcgis/inventoryFields.js";
import { withTimeout } from "../utils/async.js";
import { getInventoryKind } from "./inventoryInfo.js";

const INVENTORY_OUT_FIELDS = {
    road: ROAD_INVENTORY_OUT_FIELDS,
    bridge: BRIDGE_INVENTORY_OUT_FIELDS,
    mpms: MPMS_INVENTORY_OUT_FIELDS,
};

const INVENTORY_REQUIRED_FIELDS = {
    road: ["IRI_RATING_TEXT", "IRI_YEAR", "OVERALL_PVMNT_IDX", "ROUGH_INDX", "OPI_RATING_TEXT", "OPI_YEAR"],
    bridge: ["FACILITY", "FEATINT", "YEARBUILT", "BRIDGE_LENGTH", "DKRATING", "SUPRATING", "SUBRATING", "CULVRATING"],
    mpms: ["PLANNING_PARTNER_SHORT_NAME", "PROJECT_TITLE", "STIP_FLAG", "TYP_FLAG", "LET_DATE_DISPLAY"],
};

function getOutFieldsForLayer(layer) {
    const kind = getInventoryKind(layer);
    return kind ? INVENTORY_OUT_FIELDS[kind] : ["*"];
}

function hasInventoryAttributes(kind, attributes = {}) {
    if (!kind) return true;
    const required = INVENTORY_REQUIRED_FIELDS[kind] ?? [];
    return required.every((field) => attributes[field] != null && attributes[field] !== "");
}

export async function resolveGraphicAttributes(graphic) {
    const attrs = graphic?.attributes ?? {};
    const layer = graphic?.layer;
    if (!layer) return attrs;

    const oidField = layer.objectIdField ?? "OBJECTID";
    const oid = attrs[oidField] ?? attrs.OBJECTID;
    if (oid == null) return attrs;

    const kind = getInventoryKind(layer);
    const needsQuery = kind
        ? !hasInventoryAttributes(kind, attrs)
        : Object.keys(attrs).filter((key) => attrs[key] != null && attrs[key] !== "").length <= 3;

    if (!needsQuery) return attrs;

    try {
        const { features } = await layer.queryFeatures({
            objectIds: [oid],
            outFields: getOutFieldsForLayer(layer),
            returnGeometry: true,
        });
        const feature = features?.[0];
        if (!feature) return attrs;
        if (feature.geometry && !graphic.geometry) {
            graphic.geometry = feature.geometry;
        }
        return feature.attributes ?? attrs;
    } catch (error) {
        console.warn("Failed to query inventory feature attributes:", error);
        return attrs;
    }
}

export async function highlightInventoryGraphic(view, layer, graphic, previousHandle) {
    previousHandle?.remove();
    if (!view || !layer?.visible || !graphic) return null;

    try {
        const layerView = await view.whenLayerView(layer);
        const oidField = layer.objectIdField ?? "OBJECTID";
        const oid = graphic.attributes?.[oidField] ?? graphic.attributes?.OBJECTID;
        if (oid != null) {
            return layerView.highlight(oid);
        }
        return layerView.highlight(graphic);
    } catch (error) {
        console.warn("Failed to highlight inventory feature:", error);
        return null;
    }
}

export async function zoomToInventoryGraphic(view, graphic) {
    const geometry = graphic?.geometry;
    if (!view || !geometry) return;

    try {
        if (geometry.type === "point") {
            await withTimeout(view.goTo({ target: geometry, scale: 24000 }), "Inventory zoom");
            return;
        }

        if (geometry.extent) {
            await withTimeout(view.goTo(geometry.extent.expand(2)), "Inventory zoom");
            return;
        }

        await withTimeout(view.goTo(geometry), "Inventory zoom");
    } catch (error) {
        console.warn("Inventory zoom failed:", error);
    }
}
