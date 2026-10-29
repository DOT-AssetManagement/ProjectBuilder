/** Vertex paths + SR for pavement polylines. */
export function polylinePathsFromGeometry(geo) {
    if (!geo) return null;

    const sr = geo.spatialReference
        ?? (typeof geo.toJSON === "function" ? geo.toJSON()?.spatialReference : undefined);

    let paths = geo.paths
        ?? (typeof geo.toJSON === "function" ? geo.toJSON()?.paths : undefined);

    if (!Array.isArray(paths) || !paths.length) return null;
    const firstPath = paths[0];
    if (!Array.isArray(firstPath) || !firstPath.length) return null;

    return { paths, spatialReference: sr };
}
