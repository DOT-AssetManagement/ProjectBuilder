// PennDOT inventory layer outFields (base-layer information that show up in the popups)

export const ROAD_INVENTORY_OUT_FIELDS = [
    "OBJECTID",
    "DISTRICT_NO",
    "CTY_CODE",
    "ST_RT_NO",
    "IRI_RATING_TEXT",
    "IRI_YEAR",
    "OVERALL_PVMNT_IDX",
    "ROUGH_INDX",
    "OPI_RATING_TEXT",
    "OPI_YEAR",
];

export const BRIDGE_INVENTORY_OUT_FIELDS = [
    "OBJECTID",
    "BRKEY",
    "BRIDGE_CATEGORY",
    "BRIDGE_ID",
    "CTY_CODE",
    "ST_RT_NO",
    "SEG_NO",
    "OFFSET",
    "DISTRICT",
    "FEATINT",
    "FACILITY",
    "YEARBUILT",
    "BRIDGE_LENGTH",
    "DKRATING",
    "SUPRATING",
    "SUBRATING",
    "CULVRATING",
    "CONDITION",
];

export const MPMS_INVENTORY_OUT_FIELDS = [
    "OBJECTID",
    "PROJECT_ID",
    "PROJECT_TITLE",
    "PROJECT_STATUS",
    "STIP_FLAG",
    "TYP_FLAG",
    "LET_DATE_DISPLAY",
    "PLANNING_PARTNER_SHORT_NAME",
];
