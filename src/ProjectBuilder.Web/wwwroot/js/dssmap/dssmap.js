import { PA_EXTENT } from "../map/config.js";
import { loadModule } from "../map/arcgis/loader.js";
import { createInventoryLayers } from "../map/arcgis/inventoryLayers.js?v=8";
import { setupBasemapControls } from "../map/arcgis/basemap.js?v=2";
import { setLoading, createMapLoadProgress } from "../map/ui/loading.js";
import { createMapToolbar } from "../map/ui/mapToolbar.js?v=13";
import { createInventoryInfoPanel } from "../map/ui/inventoryInfoPanel.js";
import { setupClickHandling } from "../map/interaction/click.js";
import { highlightInventoryGraphic, zoomToInventoryGraphic } from "../map/interaction/inventorySelection.js";
import { createDssMapLayers } from "./arcgis/layers.js?v=2";
import { fetchDssMapData } from "./api/scenario.js";
import { loadDssMapCandidates } from "./loader.js?v=1";
import { highlightProject, zoomToProject } from "./interaction/selection.js";
import { createDssFilterPanel } from "./ui/filterPanel.js";
import { createDssProjectPanel } from "./ui/projectPanel.js";
import { createDssCandidatesBar } from "./ui/candidatesBar.js";
import { createDssProjectSearch } from "./ui/projectSearch.js";

// DSS map: pool project/work candidates rendered on an ArcGIS map, resolving
// geometry from PennDOT's ArcGIS feature services (no stored geometry in DSS tables),
// same approach as the PB scenario map (see ../map/map.js).
async function initMap() {
    const scenarioId = window.__DSS_SCENARIO_ID__;
    if (!scenarioId) {
        throw new Error("Scenario id is required.");
    }

    const [
        { default: Map },
        { default: MapView },
        { default: FeatureLayer },
        { default: Graphic },
        { default: Home },
        { default: Zoom },
        geometryEngine,
        queryModule,
    ] = await Promise.all([
        loadModule("Map.js"),
        loadModule("views/MapView.js"),
        loadModule("layers/FeatureLayer.js"),
        loadModule("Graphic.js"),
        loadModule("widgets/Home.js"),
        loadModule("widgets/Zoom.js"),
        loadModule("geometry/geometryEngine.js"),
        loadModule("rest/query.js"),
    ]);

    const { pavementLayer, bridgeLayer } = createDssMapLayers(FeatureLayer);

    const map = new Map({
        basemap: "gray",
        layers: [pavementLayer, bridgeLayer],
    });

    const view = new MapView({
        container: "map-view",
        map,
        extent: PA_EXTENT,
        constraints: { snapToZoom: false },
        highlightOptions: {
            color: [0, 255, 255],
            haloColor: [0, 255, 255],
            haloOpacity: 0.9,
            fillOpacity: 0.3,
        },
    });

    await view.when();
    view.ui.remove("attribution");

    const basemapControls = await setupBasemapControls(view, loadModule);
    const inventoryLayers = createInventoryLayers(FeatureLayer);

    const mapToolbar = createMapToolbar({
        view,
        map,
        inventoryLayers,
        scenarioPavementLayer: pavementLayer,
        scenarioBridgeLayer: bridgeLayer,
        setBasemap: basemapControls.setBasemap,
        getActiveBasemapId: basemapControls.getActiveBasemapId,
        basemapCatalog: basemapControls.catalog,
        onBasemapChange: basemapControls.onBasemapChange,
        togglePenndotBasemap: basemapControls.togglePenndotBasemap,
        isPenndotBasemapActive: basemapControls.isPenndotBasemapActive,
    });

    // Zoom/Home go top-right so they don't sit under the top-left filter panel.
    view.ui.remove("zoom");
    view.ui.add(new Zoom({ view }), "top-right");
    view.ui.add(new Home({ view }), "top-right");

    const loadProgress = createMapLoadProgress((visible, message, percent) => setLoading(visible, message, percent));

    loadProgress.startFetchingScenario();
    const dssMapData = await fetchDssMapData(scenarioId);
    loadProgress.finishFetchingScenario();

    let counts = { pavementCount: 0, bridgeCount: 0 };
    try {
        loadProgress.reportMapPreparation(0);
        counts = await loadDssMapCandidates(
            view,
            pavementLayer,
            bridgeLayer,
            Graphic,
            geometryEngine,
            queryModule,
            dssMapData,
            (fraction) => loadProgress.reportMapPreparation(fraction),
        );
        loadProgress.complete();
        mapToolbar.bringScenarioLayersToFront();
    } finally {
        loadProgress.stop();
    }

    const workCandidates = dssMapData.workCandidates ?? dssMapData.WorkCandidates ?? [];
    const mappedCount = counts.pavementCount + counts.bridgeCount;
    if (workCandidates.length > 0 && mappedCount === 0) {
        setLoading(
            true,
            `${workCandidates.length} work candidate(s) loaded from the scenario, but none could be matched to map geometry.`,
            100,
        );
    }

    // ── Panels & interaction ──────────────────────────────────────────────
    const getMapData = () => dssMapData;

    const [pavementLayerView, bridgeLayerView] = await Promise.all([
        view.whenLayerView(pavementLayer),
        view.whenLayerView(bridgeLayer),
    ]);

    const pavementToggle = document.getElementById("toggle-pavement");
    const bridgeToggle = document.getElementById("toggle-bridges");

    pavementToggle?.addEventListener("change", (event) => {
        pavementLayer.visible = event.target.checked;
    });
    bridgeToggle?.addEventListener("change", (event) => {
        bridgeLayer.visible = event.target.checked;
    });

    // These two toggles live in the same Filters panel but are wired independently of
    // filterPanel.js's own clear handler (which only resets its dynamic Year/AssetType/etc.
    // sections), so "Clear All Filters" left them untouched — reset them here too.
    document.getElementById("gis-filter-clear")?.addEventListener("click", () => {
        if (pavementToggle) pavementToggle.checked = true;
        if (bridgeToggle) bridgeToggle.checked = true;
        pavementLayer.visible = true;
        bridgeLayer.visible = true;
    });

    let highlightHandle = null;
    let inventoryHighlightHandle = null;

    const inventoryInfoPanel = createInventoryInfoPanel(
        document.getElementById("inventory-info-panel"),
        { onClose: () => clearInventoryHighlight() },
    );

    const candidatesBar = createDssCandidatesBar(
        document.getElementById("dss-candidates-bar"),
        { getMapData },
    );

    const projectPanel = createDssProjectPanel(
        document.getElementById("dss-project-panel"),
        {
            getMapData,
            onSelectProject: selectProject,
            onShowCandidates: (id) => candidatesBar.expand(id),
            onClose: () => clearProjectSelection(),
        },
    );

    const projectSearch = createDssProjectSearch(document.getElementById("map-project-search"), {
        getMapData,
        onSelectProject: selectProject,
    });

    const filterPanel = createDssFilterPanel(
        document.getElementById("gis-filter-sections"),
        document.getElementById("gis-filter-clear"),
        { getMapData, pavementLayer, bridgeLayer },
    );
    filterPanel.render();

    document.getElementById("gis-filter-collapse")?.addEventListener("click", () => {
        const filter = document.getElementById("map-filter");
        const collapsed = filter?.classList.toggle("collapsed");
        document.querySelector(".dssmap-page")?.classList.toggle("gis-filter-collapsed", !!collapsed);
        const btn = document.getElementById("gis-filter-collapse");
        if (btn) {
            btn.setAttribute("aria-expanded", String(!collapsed));
            btn.setAttribute("aria-label", collapsed ? "Expand filters" : "Collapse filters");
        }
    });

    function clearInventoryHighlight() {
        inventoryHighlightHandle?.remove();
        inventoryHighlightHandle = null;
    }

    function clearInventorySelection() {
        clearInventoryHighlight();
        inventoryInfoPanel.close();
    }

    async function selectProject(id, clickInfo) {
        if (id == null) return;
        clearInventorySelection();
        view.padding = { ...view.padding, right: 278 };
        document.querySelector(".dssmap-page")?.classList.add("gis-panel-open");
        highlightHandle = await highlightProject(
            pavementLayerView,
            bridgeLayerView,
            pavementLayer,
            bridgeLayer,
            id,
            highlightHandle,
        );
        await zoomToProject(view, pavementLayer, bridgeLayer, id);
        projectPanel.open(id, clickInfo);
        candidatesBar.setProject(id);
    }

    function clearProjectSelection() {
        highlightHandle?.remove();
        highlightHandle = null;
        view.padding = { ...view.padding, right: 0 };
        document.querySelector(".dssmap-page")?.classList.remove("gis-panel-open");
        projectPanel.close();
        candidatesBar.close();
        candidatesBar.setProject(null);
    }

    function clearSelection() {
        clearProjectSelection();
        clearInventorySelection();
    }

    async function selectInventoryFeature({ graphic, layer, info }) {
        inventoryHighlightHandle = await highlightInventoryGraphic(
            view,
            layer,
            graphic,
            inventoryHighlightHandle,
        );
        inventoryInfoPanel.open(info);
        await zoomToInventoryGraphic(view, graphic);
    }

    setupClickHandling({
        view,
        pavementLayer,
        bridgeLayer,
        inventoryLayers: [
            inventoryLayers.pavementBaseLayer,
            inventoryLayers.bridgesBaseLayer,
            inventoryLayers.mpmsLayer,
        ],
        onSelectProject: selectProject,
        onClearProjectSelection: clearProjectSelection,
        onSelectInventory: selectInventoryFeature,
        onClearInventorySelection: clearInventorySelection,
        onClearAll: clearSelection,
    });
}

initMap().catch((error) => {
    console.error("Failed to initialize DSS map:", error);
    setLoading(true, error?.message || "Unable to load DSS map.", 0);
});
