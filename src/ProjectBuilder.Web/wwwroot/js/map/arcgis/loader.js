import { ARCGIS_BASE } from "../config.js";

export async function loadModule(path) {
    return import(`${ARCGIS_BASE}/${path}`);
}
