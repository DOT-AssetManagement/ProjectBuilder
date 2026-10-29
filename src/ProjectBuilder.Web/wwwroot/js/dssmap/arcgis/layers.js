import { iconSymbol, lineSymbol, symbols } from "../../map/symbolConfig.js?v=2";

export const DSS_PAVEMENT_LAYER_ID = "dss-pavement-layer";
export const DSS_BRIDGE_LAYER_ID = "dss-bridge-layer";

export function createDssMapLayers(FeatureLayer) {
    const pavementLayer = new FeatureLayer({
        title: "Pool Pavement Candidates",
        id: DSS_PAVEMENT_LAYER_ID,
        objectIdField: "OBJECTID",
        geometryType: "polyline",
        spatialReference: { wkid: 102100 },
        source: [],
        fields: [
            { name: "OBJECTID", type: "oid" },
            { name: "ProjectID", type: "string" },
            { name: "TreatmentID", type: "string" },
            { name: "Treatment", type: "string" },
            { name: "CRS_ID", type: "string" },
            { name: "SIDE_IND", type: "string" },
            { name: "CountyCode", type: "string" },
            { name: "SectionFrom", type: "string" },
            { name: "AssetType", type: "string" },
            { name: "District", type: "integer" },
            { name: "County", type: "string" },
            { name: "Route", type: "integer" },
            { name: "Section", type: "integer" },
            { name: "Year", type: "integer" },
        ],
        outFields: ["*"],
        popupEnabled: false,
        renderer: {
            type: "simple",
            symbol: lineSymbol(symbols.dssPavement),
        },
    });

    const bridgeLayer = new FeatureLayer({
        title: "Pool Bridge Candidates",
        id: DSS_BRIDGE_LAYER_ID,
        objectIdField: "OBJECTID",
        geometryType: "point",
        spatialReference: { wkid: 102100 },
        source: [],
        fields: [
            { name: "OBJECTID", type: "oid" },
            { name: "ProjectID", type: "string" },
            { name: "TreatmentID", type: "string" },
            { name: "Treatment", type: "string" },
            { name: "BRKEY", type: "string" },
            { name: "AssetType", type: "string" },
            { name: "District", type: "integer" },
            { name: "County", type: "string" },
            { name: "Route", type: "integer" },
            { name: "Section", type: "integer" },
            { name: "Year", type: "integer" },
        ],
        outFields: ["*"],
        popupEnabled: false,
        renderer: {
            type: "simple",
            symbol: iconSymbol(symbols.scenarioBridge.icon, symbols.scenarioBridge.size),
        },
    });

    return { pavementLayer, bridgeLayer };
}
