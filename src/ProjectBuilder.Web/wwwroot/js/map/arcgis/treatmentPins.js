import { polylinePathsFromGeometry } from "../utils/polyline.js";
import { iconSymbol, symbols } from "../symbolConfig.js?v=5";

const helmetPinSymbol = {
    ...iconSymbol(symbols.treatment.icon, symbols.treatment.size),
    yoffset: symbols.treatment.yoffset,
};

const focusedPinSymbol = {
    ...iconSymbol(symbols.treatment.selectedIcon, symbols.treatment.size),
    yoffset: symbols.treatment.yoffset,
};

export function createTreatmentPinGraphics(features, Graphic, Point) {
    const markers = [];

    for (const feature of features) {
        const pathsData = polylinePathsFromGeometry(feature?.geometry);
        if (!pathsData) continue;

        const { paths, spatialReference } = pathsData;
        const startPath = paths[0];
        const endPath = paths[paths.length - 1];
        if (!startPath?.length || !endPath?.length) continue;

        const startVertex = startPath[0];
        const endVertex = endPath[endPath.length - 1];

        const treatmentId = feature.attributes?.TreatmentID ?? feature.attributes?.TreatmentId ?? null;
        markers.push(new Graphic({
            geometry: new Point({
                x: startVertex[0],
                y: startVertex[1],
                spatialReference,
            }),
            attributes: { TreatmentID: treatmentId },
            symbol: helmetPinSymbol,
        }));

        markers.push(new Graphic({
            geometry: new Point({
                x: endVertex[0],
                y: endVertex[1],
                spatialReference,
            }),
            attributes: { TreatmentID: treatmentId },
            symbol: helmetPinSymbol,
        }));
    }

    return markers;
}

export function createTreatmentPinManager(view, Graphic, Point) {
    let markerGraphics = [];
    let lastFeatures = null;
    let visible = true;
    let focusedTreatmentId = null;

    function applyFocus() {
        const id = focusedTreatmentId == null ? null : String(focusedTreatmentId);
        const rest = [];
        const focused = [];
        markerGraphics.forEach((marker) => {
            const match = id != null && String(marker.attributes?.TreatmentID ?? "") === id;
            marker.symbol = match ? focusedPinSymbol : helmetPinSymbol;
            (match ? focused : rest).push(marker);
        });
        markerGraphics = [...rest, ...focused];
        if (!focused.length || !view?.graphics?.includes(focused[0])) return;
        markerGraphics.forEach((marker) => view.graphics.remove(marker));
        markerGraphics.forEach((marker) => view.graphics.add(marker));
    }

    function remove() {
        if (!view?.graphics || markerGraphics.length === 0) return;
        markerGraphics.forEach((marker) => view.graphics.remove(marker));
        markerGraphics = [];
    }

    function replace(features) {
        lastFeatures = features ?? null;
        remove();
        if (!visible || !view?.graphics || !features?.length) return;
        const polylines = features.filter((feature) => {
            const type = feature?.geometry?.type;
            return type === "polyline" || type === "multipolyline";
        });
        markerGraphics = createTreatmentPinGraphics(polylines, Graphic, Point);
        applyFocus();
        markerGraphics.forEach((marker) => view.graphics.add(marker));
    }

    return {
        remove,
        replace,
        focus(treatmentId) {
            focusedTreatmentId = treatmentId == null || treatmentId === "" ? null : treatmentId;
            applyFocus();
        },
        clearFocus() {
            focusedTreatmentId = null;
            applyFocus();
        },
        get visible() {
            return visible;
        },
        set visible(value) {
            const next = !!value;
            if (next === visible) return;
            visible = next;
            if (!visible) remove();
            else if (lastFeatures) replace(lastFeatures);
        },
        count() {
            return markerGraphics.length;
        },
    };
}
