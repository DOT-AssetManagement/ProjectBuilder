import { lineSymbol, symbols } from "../symbolConfig.js?v=6";

const borderSymbol = lineSymbol(symbols.scenarioPavement, {
    color: symbols.scenarioPavement.gentleBorder,
    width: symbols.scenarioPavement.gentleBorderWidth,
});

const focusSymbol = lineSymbol(symbols.scenarioPavement, {
    color: symbols.scenarioPavement.focus,
    width: symbols.scenarioPavement.focusWidth,
});

function isPolyline(feature) {
    const type = feature?.geometry?.type;
    return type === "polyline" || type === "multipolyline";
}

/**
 * Selected scenario pavements are drawn a little thicker, in the same gray.
 * Clicking one segment paints only that segment cyan.
 */
export function createPavementSelectionOverlay(GraphicsLayer, Graphic) {
    const borderLayer = new GraphicsLayer({
        id: "scenario-pavement-border",
        listMode: "hide",
    });
    const focusLayer = new GraphicsLayer({
        id: "scenario-pavement-focus",
        listMode: "hide",
    });

    function showBorder(features) {
        borderLayer.removeAll();
        const graphics = (features ?? []).filter(isPolyline).map((feature) => new Graphic({
            geometry: feature.geometry,
            symbol: borderSymbol,
        }));
        if (graphics.length) borderLayer.addMany(graphics);
    }

    function focusTreatment(features, treatmentId) {
        focusLayer.removeAll();
        const id = String(treatmentId);
        const graphics = (features ?? []).filter((feature) =>
            isPolyline(feature) && String(feature.attributes?.TreatmentID ?? "") === id,
        ).map((feature) => new Graphic({
            geometry: feature.geometry,
            symbol: focusSymbol,
        }));
        if (!graphics.length) return false;
        focusLayer.addMany(graphics);
        return true;
    }

    function clearFocus() {
        focusLayer.removeAll();
    }

    function clear() {
        borderLayer.removeAll();
        focusLayer.removeAll();
    }

    /** Keep the thicker line under the pavement and the clicked segment just above it. */
    function placeAround(map, pavementLayer) {
        if (!map?.layers?.includes(pavementLayer)) return;
        if (!map.layers.includes(borderLayer) || !map.layers.includes(focusLayer)) return;
        map.reorder(borderLayer, map.layers.indexOf(pavementLayer));
        map.reorder(focusLayer, map.layers.indexOf(pavementLayer) + 1);
    }

    return { borderLayer, focusLayer, showBorder, focusTreatment, clearFocus, clear, placeAround };
}
