import { iconSymbol, lineSymbol, symbols } from "../symbolConfig.js?v=9";

export const SCENARIO_PAVEMENT_LAYER_ID = "scenario-pavement-layer";
export const SCENARIO_BRIDGE_LAYER_ID = "scenario-bridge-layer";
export const SCENARIO_PAVEMENT_UNSELECTED_LAYER_ID = "scenario-pavement-unselected-layer";
export const SCENARIO_BRIDGE_UNSELECTED_LAYER_ID = "scenario-bridge-unselected-layer";

const TREATMENT_FIELDS_BASE = [
    { name: "OBJECTID", type: "oid" },
    { name: "ProjectID", type: "integer" },
    { name: "TreatmentID", type: "integer" },
    { name: "Treatment", type: "string" },
    { name: "AssetType", type: "string" },
    { name: "District", type: "integer" },
    { name: "County", type: "string" },
    { name: "Route", type: "integer" },
    { name: "Section", type: "integer" },
    { name: "Year", type: "integer" },
    { name: "Status", type: "string" },
    { name: "HasCommitted", type: "string" },
];

function pavementFields() {
    return [
        ...TREATMENT_FIELDS_BASE.slice(0, 4),
        { name: "CRS_ID", type: "string" },
        { name: "SIDE_IND", type: "string" },
        { name: "CountyCode", type: "string" },
        { name: "SectionFrom", type: "string" },
        ...TREATMENT_FIELDS_BASE.slice(4),
    ];
}

function bridgeFields() {
    return [
        ...TREATMENT_FIELDS_BASE.slice(0, 4),
        { name: "BRKEY", type: "string" },
        { name: "Selected", type: "integer" },
        ...TREATMENT_FIELDS_BASE.slice(4),
    ];
}

function pavementLineRenderer(entry) {
    return { type: "simple", symbol: lineSymbol(entry) };
}

/** Marker size in screen px for inventory and ad hoc bridge icons. */
export function bridgeIconSizeForZoom(zoom) {
    const z = Number(zoom);
    if (!Number.isFinite(z)) return 18;
    if (z >= 16) return 26;
    if (z >= 14) return 21;
    if (z >= 12) return 17;
    if (z >= 10) return 14;
    if (z >= 8) return 12;
    return 9;
}

function projectBridgeSymbol(url, size = symbols.scenarioBridge.size) {
    return iconSymbol(url, size);
}

/** Funded scenario bridges use the Map Key Projects icon, and Selected Projects when chosen. */
function scenarioBridgeRenderer() {
    return {
        type: "unique-value",
        field: "Selected",
        defaultSymbol: projectBridgeSymbol(symbols.scenarioBridge.icon),
        uniqueValueInfos: [
            {
                value: 1,
                symbol: projectBridgeSymbol(symbols.scenarioBridge.selectedIcon, symbols.scenarioBridge.selectedSize),
            },
        ],
    };
}

export function createScenarioLayers(FeatureLayer) {
    const pavementLayer = new FeatureLayer({
        title: "Scenario Pavement Treatments",
        id: SCENARIO_PAVEMENT_LAYER_ID,
        objectIdField: "OBJECTID",
        geometryType: "polyline",
        spatialReference: { wkid: 102100 },
        source: [],
        fields: pavementFields(),
        outFields: ["*"],
        popupEnabled: false,
        renderer: pavementLineRenderer(symbols.scenarioPavement),
    });

    const bridgeLayer = new FeatureLayer({
        title: "Scenario Bridge Treatments",
        id: SCENARIO_BRIDGE_LAYER_ID,
        objectIdField: "OBJECTID",
        geometryType: "point",
        spatialReference: { wkid: 102100 },
        source: [],
        fields: bridgeFields(),
        outFields: ["*"],
        popupEnabled: false,
        renderer: scenarioBridgeRenderer(),
    });

    const unselectedPavementLayer = new FeatureLayer({
        title: "Unselected Pavement Treatments",
        id: SCENARIO_PAVEMENT_UNSELECTED_LAYER_ID,
        objectIdField: "OBJECTID",
        geometryType: "polyline",
        spatialReference: { wkid: 102100 },
        source: [],
        fields: pavementFields(),
        outFields: ["*"],
        popupEnabled: false,
        visible: false,
        renderer: pavementLineRenderer(symbols.unselectedPavement),
    });

    const unselectedBridgeLayer = new FeatureLayer({
        title: "Unselected Bridge Treatments",
        id: SCENARIO_BRIDGE_UNSELECTED_LAYER_ID,
        objectIdField: "OBJECTID",
        geometryType: "point",
        spatialReference: { wkid: 102100 },
        source: [],
        fields: bridgeFields(),
        outFields: ["*"],
        popupEnabled: false,
        visible: false,
        renderer: {
            type: "unique-value",
            field: "Selected",
            defaultSymbol: iconSymbol(symbols.unfunded.icon, symbols.unfunded.size),
            uniqueValueInfos: [
                {
                    value: 1,
                    symbol: iconSymbol(symbols.unfunded.selectedIcon, symbols.unfunded.selectedSize),
                },
            ],
        },
    });

    return { pavementLayer, bridgeLayer, unselectedPavementLayer, unselectedBridgeLayer };
}
