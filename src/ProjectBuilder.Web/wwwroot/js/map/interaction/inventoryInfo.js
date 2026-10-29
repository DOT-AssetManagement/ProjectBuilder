import {
    BRIDGES_BASE_LAYER_ID,
    MPMS_LAYER_ID,
    PAVEMENT_BASE_LAYER_ID,
} from "../arcgis/inventoryLayers.js";

const ROAD_INVENTORY_FIELDS = [
    ["DISTRICT_NO", "District"],
    ["CTY_CODE", "County Code"],
    ["ST_RT_NO", "Route"],
    ["IRI_RATING_TEXT", "IRI Rating"],
    ["IRI_YEAR", "IRI Year"],
    ["OVERALL_PVMNT_IDX", "Overall Pave Idx"],
    ["ROUGH_INDX", "Rough Index"],
    ["OPI_RATING_TEXT", "OPI Rating"],
    ["OPI_YEAR", "OPI Year"],
];

const BRIDGE_INVENTORY_FIELDS = [
    ["BRKEY", "BRKEY"],
    ["_BMSID", "BMSID"],
    ["DISTRICT", "District"],
    ["CTY_CODE", "County Code"],
    ["ST_RT_NO", "Route"],
    ["FACILITY", "Facility"],
    ["FEATINT", "Feat Int"],
    ["YEARBUILT", "Year Built"],
    ["BRIDGE_LENGTH", "Bridge Length"],
    ["CONDITION", "Condition"],
    ["DKRATING", "Deck Rating"],
    ["SUPRATING", "Super Rating"],
    ["SUBRATING", "Sub Rating"],
    ["CULVRATING", "Culvert Rating"],
];

const MPMS_INVENTORY_FIELDS = [
    ["PROJECT_ID", "Project Id"],
    ["PLANNING_PARTNER_SHORT_NAME", "Planning Partner Short Name"],
    ["PROJECT_TITLE", "Project Title"],
    ["PROJECT_STATUS", "Project Status"],
    ["STIP_FLAG", "STIP Flag"],
    ["TYP_FLAG", "TYP Flag"],
    ["LET_DATE_DISPLAY", "Let Date Display"],
];

const INVENTORY_FIELD_SETS = {
    road: ROAD_INVENTORY_FIELDS,
    bridge: BRIDGE_INVENTORY_FIELDS,
    mpms: MPMS_INVENTORY_FIELDS,
};

const INVENTORY_TITLES = {
    road: "Pavement (inventory)",
    bridge: "Bridge (inventory)",
    mpms: "MPMS project",
};

const ALWAYS_SHOW_KINDS = new Set(["road", "bridge", "mpms"]);

function formatBridgeCondition(value) {
    if (value === "G") return "Good";
    if (value === "F") return "Fair";
    if (value === "P") return "Poor";
    if (value === "N") return "N";
    if (value == null || value === "") return null;
    return String(value);
}

function getInventoryFieldValue(field, attributes = {}) {
    if (field === "_BMSID") {
        const bmsid = `${attributes.CTY_CODE ?? ""}${attributes.ST_RT_NO ?? ""}${attributes.SEG_NO ?? ""}${attributes.OFFSET ?? ""}`;
        return bmsid || null;
    }
    if (field === "CONDITION") {
        return formatBridgeCondition(attributes.CONDITION);
    }
    return attributes[field];
}

function formatFieldValue(value, emptyLabel = "–") {
    if (value == null || value === "") return emptyLabel;
    if (typeof value === "number" && Number.isFinite(value)) {
        return String(value);
    }
    return String(value).trim() || emptyLabel;
}

export function getInventoryKind(layer) {
    if (!layer?.id) return null;
    if (layer.id === PAVEMENT_BASE_LAYER_ID) return "road";
    if (layer.id === BRIDGES_BASE_LAYER_ID) return "bridge";
    if (layer.id === MPMS_LAYER_ID) return "mpms";
    return null;
}

export function buildInventoryInfo(kind, attributes = {}) {
    const fieldSet = INVENTORY_FIELD_SETS[kind] ?? [];
    const alwaysShow = ALWAYS_SHOW_KINDS.has(kind);
    const emptyLabel = alwaysShow ? "N/A" : "–";

    const rows = fieldSet.map(([field, label]) => ({
        label,
        value: formatFieldValue(getInventoryFieldValue(field, attributes), emptyLabel),
    }));

    return {
        kind,
        title: INVENTORY_TITLES[kind] ?? "Inventory feature",
        rows: alwaysShow ? rows : rows.filter((row) => row.value !== "–"),
    };
}
