export const ARCGIS_VERSION = "4.33";
export const ARCGIS_BASE = `https://js.arcgis.com/${ARCGIS_VERSION}/@arcgis/core`;

// PennDOT ArcGIS service limits
export const MAX_RECORDS_PER_QUERY = 2000;
export const PAVEMENT_CLAUSE_BATCH_SIZE = 1000;
export const BRIDGE_KEY_BATCH_SIZE = 1000;
export const PARALLEL_REQUEST_LIMIT = 20;
export const QUERY_TIMEOUT_MS = 180000;
export const MAX_QUERY_RETRIES = 3;
export const RETRY_BASE_DELAY_MS = 1500;

export const PA_EXTENT = {
    xmax: -7981825.454310255,
    xmin: -9230500.748376561,
    ymax: 5382301.238801554,
    ymin: 4609981.505008338,
    spatialReference: { wkid: 102100 },
};

export const PAVEMENT_SERVICE_URL =
    "https://gis.penndot.gov/arcgis/rest/services/opendata/roadwaysegments/MapServer/0";
export const BRIDGE_SERVICE_URL =
    "https://gis.penndot.gov/gis/rest/services/opendata/pennsylvaniabridges/MapServer/0";
export const MPMS_SERVICE_URL =
    "https://gis.penndot.pa.gov/gis/rest/services/opendata/transportationprojectslines/MapServer/0";
export const PENNDOT_BASEMAP_URL =
    "https://gis.penndot.pa.gov/gis/rest/services/basemaps/penndotbasemap/MapServer";

export const LAYERS_ENABLE_ZOOM_LEVEL = 11;

// Layer colors and icons live in symbolConfig.js so the map and the Map Key share them.

export const PAVEMENT_OUT_FIELDS = [
    "ST_RT_NO", "CTY_CODE", "SEG_NO", "SIDE_IND",
    "IRI_RATING_TEXT", "IRI_YEAR", "OVERALL_PVMNT_IDX", "ROUGH_INDX", "OPI_RATING_TEXT", "OPI_YEAR",
];
export const BRIDGE_OUT_FIELDS = ["BRKEY", "CONDITION", "DKRATING", "SUPRATING", "SUBRATING", "CULVRATING"];
