import { withTimeout } from "../utils/async.js";

// Bridge scenario graphics are points; a zero-width extent does not change map scale.
const POINT_ZOOM_SCALE = 24000;
const OVERLAY_MARGIN = 24;

/** Map area covered by the filter rail, project panel and notice band, plus a breathing margin. */
function overlayPadding(view) {
    const mapRect = view.container?.getBoundingClientRect();
    const padding = { top: OVERLAY_MARGIN, left: OVERLAY_MARGIN, right: OVERLAY_MARGIN, bottom: OVERLAY_MARGIN };
    if (!mapRect) return padding;

    const filter = document.getElementById("map-filter");
    if (filter && !filter.hidden) {
        padding.left = Math.max(padding.left, filter.getBoundingClientRect().right - mapRect.left + OVERLAY_MARGIN);
    }

    const panel = document.getElementById("project-panel");
    if (panel && !panel.hidden) {
        padding.right = Math.max(padding.right, mapRect.right - panel.getBoundingClientRect().left + OVERLAY_MARGIN);
    }

    const band = document.getElementById("map-key-band");
    if (band && !band.hidden) {
        const bandBottom = parseFloat(getComputedStyle(band).bottom) || 0;
        padding.bottom = Math.max(padding.bottom, bandBottom + band.offsetHeight + OVERLAY_MARGIN);
    }

    return padding;
}

export async function zoomToFeatures(view, features, { avoidOverlays = false } = {}) {
    if (!view || !features?.length) return;

    const withGeometry = features.filter((feature) => feature?.geometry);
    if (withGeometry.length === 0) return;

    try {
        if (avoidOverlays) view.padding = overlayPadding(view);

        if (withGeometry.length === 1 && withGeometry[0].geometry.type === "point") {
            await withTimeout(
                view.goTo({ target: withGeometry[0].geometry, scale: POINT_ZOOM_SCALE }),
                "Feature zoom",
            );
            return;
        }

        await withTimeout(view.goTo(withGeometry), "Feature zoom");
    } catch (error) {
        console.warn("Feature zoom failed:", error);
    }
}
