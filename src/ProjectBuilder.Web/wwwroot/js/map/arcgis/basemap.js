import { PENNDOT_BASEMAP_URL } from "../config.js";

/** Well-known ArcGIS basemap ids plus the custom PennDOT tile service. */
export const BASEMAP_CATALOG = [
    { id: "gray", title: "Light Gray", thumbnail: "/images/map/lightGrayMap.png" },
    { id: "penndot", title: "PennDOT", thumbnail: "/images/map/pendotMap.png", custom: true },
    { id: "dark-gray-vector", title: "Dark Gray" },
    { id: "streets-vector", title: "Streets" },
    { id: "streets-navigation-vector", title: "Navigation" },
    { id: "streets-night-vector", title: "Streets Night" },
    { id: "streets-relief-vector", title: "Streets Relief" },
    { id: "topo-vector", title: "Topographic" },
    { id: "terrain", title: "Terrain" },
    { id: "satellite", title: "Satellite" },
    { id: "hybrid", title: "Hybrid" },
    { id: "oceans", title: "Oceans" },
    { id: "osm", title: "OpenStreetMap" },
];

/**
 * ArcGIS draws basemap.referenceLayers above all map.layers (operational layers).
 * Move them into baseLayers so road/place labels stay visible but under scenario
 * and inventory features.
 */
export async function moveBasemapLabelsBelowFeatures(basemap) {
    if (!basemap) return basemap;

    await basemap.load();
    if (basemap.referenceLayers.length === 0) return basemap;

    const referenceLayers = basemap.referenceLayers.removeAll();
    basemap.baseLayers.addMany(referenceLayers);
    return basemap;
}

function enrichCatalogThumbnails(Basemap) {
    return BASEMAP_CATALOG.map((entry) => {
        if (entry.thumbnail) return { ...entry };

        try {
            const sample = Basemap.fromId(entry.id);
            return {
                ...entry,
                thumbnail: sample?.thumbnailUrl || null,
                title: entry.title || sample?.title || entry.id,
            };
        } catch {
            return { ...entry };
        }
    });
}

export async function setupBasemapControls(view, loadModule) {
    const [
        { default: Basemap },
        { default: TileLayer },
    ] = await Promise.all([
        loadModule("Basemap.js"),
        loadModule("layers/TileLayer.js"),
    ]);

    await moveBasemapLabelsBelowFeatures(view.map.basemap);

    let activeId = "gray";
    const catalog = enrichCatalogThumbnails(Basemap);
    const listeners = new Set();

    function notify() {
        listeners.forEach((fn) => {
            try { fn(activeId); } catch { /* ignore listener errors */ }
        });
    }

    async function applyBasemap(id) {
        const entry = catalog.find((item) => item.id === id) ?? catalog[0];
        if (!entry) return activeId;

        if (entry.custom && entry.id === "penndot") {
            const pennDotLayer = new TileLayer({ url: PENNDOT_BASEMAP_URL });
            view.map.basemap = new Basemap({
                id: "penndot",
                title: "PennDOT",
                baseLayers: [pennDotLayer],
                thumbnailUrl: entry.thumbnail,
            });
        } else {
            const next = Basemap.fromId(entry.id);
            if (!next) throw new Error(`Unknown basemap id: ${entry.id}`);
            view.map.basemap = next;
            await moveBasemapLabelsBelowFeatures(view.map.basemap);
        }

        activeId = entry.id;
        notify();
        return activeId;
    }

    // Backward-compatible toggle used by older callers.
    function togglePenndotBasemap() {
        const nextId = activeId === "penndot" ? "gray" : "penndot";
        void applyBasemap(nextId);
        return nextId === "penndot";
    }

    function isPenndotBasemapActive() {
        return activeId === "penndot";
    }

    return {
        catalog,
        setBasemap: applyBasemap,
        getActiveBasemapId: () => activeId,
        onBasemapChange: (fn) => {
            listeners.add(fn);
            return () => listeners.delete(fn);
        },
        togglePenndotBasemap,
        isPenndotBasemapActive,
    };
}
