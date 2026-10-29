import { iconSymbol, lineSymbol, symbols } from "../symbolConfig.js?v=2";

/** Adhoc inventory highlight helpers (inventory layers only). */

export async function highlightAdhocFeature(view, inventoryLayers, item, previousHandle) {
    previousHandle?.remove();
    if (!view || !item) return null;

    const { pavementBaseLayer, bridgesBaseLayer, mpmsLayer } = inventoryLayers;
    let layer = null;
    let where = null;

    if (item.type === "bridge") {
        layer = bridgesBaseLayer;
        const brkey = String(item.BRKEY ?? "").trim();
        where = /^\d+$/.test(brkey) ? `BRKEY = ${brkey}` : `BRKEY = '${brkey.replace(/'/g, "''")}'`;
    } else if (item.type === "pavement" || item.type === "road") {
        layer = pavementBaseLayer;
        where = item.GPID != null
            ? `OBJECTID = ${item.OBJECTID} AND GPID = ${item.GPID}`
            : `OBJECTID = ${item.OBJECTID}`;
    } else if (item.type === "mpms") {
        layer = mpmsLayer;
        where = `PROJECT_ID = ${item.PROJECT_ID}`;
    }

    if (!layer || !where) return null;

    try {
        if (!layer.visible) {
            layer.visible = true;
            if (!view.map.layers.includes(layer)) {
                view.map.add(layer);
            }
        }

        const layerView = await view.whenLayerView(layer);
        const result = await layer.queryFeatures({
            where,
            outFields: ["*"],
            returnGeometry: true,
        });
        const graphic = result.features?.[0];
        if (!graphic) return null;

        const handle = item.type === "bridge" ? null : layerView.highlight(graphic);
        if (graphic.geometry?.type === "point") {
            await view.goTo({ target: graphic.geometry, scale: 24000 });
        } else if (graphic.geometry?.extent) {
            await view.goTo(graphic.geometry.extent.expand(2));
        }
        return handle;
    } catch (error) {
        console.warn("Failed to highlight adhoc inventory feature:", error);
        return null;
    }
}

function whereForAdhocItem(item) {
    if (item.type === "bridge") {
        const brkey = String(item.BRKEY ?? "").trim();
        return /^\d+$/.test(brkey) ? `BRKEY = ${brkey}` : `BRKEY = '${brkey.replace(/'/g, "''")}'`;
    }
    if (item.type === "pavement" || item.type === "road") {
        return item.GPID != null
            ? `OBJECTID = ${item.OBJECTID} AND GPID = ${item.GPID}`
            : `OBJECTID = ${item.OBJECTID}`;
    }
    return null;
}

function sameAdhocBridge(item, selectedItem) {
    if (!selectedItem || item?.type !== "bridge" || selectedItem.type !== "bridge") return false;
    return String(item.BRKEY ?? "").trim() === String(selectedItem.BRKEY ?? "").trim();
}

export async function syncAdhocSessionGraphics(graphicsLayer, inventoryLayers, items, Graphic, selectedItem = null, size = 26) {
    if (!graphicsLayer) return;
    graphicsLayer.removeAll();
    const list = items ?? [];
    if (!list.length) return;

    const graphics = [];
    for (const item of list) {
        const where = whereForAdhocItem(item);
        const layer = item.type === "bridge"
            ? inventoryLayers.bridgesBaseLayer
            : (item.type === "pavement" || item.type === "road")
                ? inventoryLayers.pavementBaseLayer
                : null;
        if (!layer || !where) continue;
        try {
            const result = await layer.queryFeatures({ where, returnGeometry: true, outFields: ["*"] });
            const feature = result.features?.[0];
            if (!feature?.geometry) continue;
            const isBridge = item.type === "bridge";
            const bridgeIcon = sameAdhocBridge(item, selectedItem)
                ? symbols.inventoryBridge.selectedIcon
                : symbols.inventoryBridge.icon;
            graphics.push(new Graphic({
                geometry: feature.geometry,
                symbol: isBridge
                    ? iconSymbol(bridgeIcon, size)
                    : lineSymbol(symbols.adhocPavement),
            }));
        } catch (error) {
            console.warn("Failed to draw adhoc inventory item:", error);
        }
    }
    if (graphics.length) graphicsLayer.addMany(graphics);
    const map = graphicsLayer.map;
    if (map?.layers?.includes(graphicsLayer)) {
        map.reorder(graphicsLayer, map.layers.length - 1);
    }
}

export function toAdhocItem(kind, attributes) {
    const type = kind === "road" ? "pavement" : kind;
    return { ...attributes, type };
}
