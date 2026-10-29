import { PA_EXTENT } from "./config.js";
import { fetchScenarioData } from "./api/scenario.js";
import { loadModule } from "./arcgis/loader.js";
import { createScenarioLayers, bridgeIconSizeForZoom } from "./arcgis/layers.js?v=11";
import { createPavementSelectionOverlay } from "./arcgis/pavementSelection.js?v=4";
import { attachInventoryBridgeIcons, createInventoryLayers, scaleInventoryBridgeIcons } from "./arcgis/inventoryLayers.js?v=8";
import { setupBasemapControls } from "./arcgis/basemap.js?v=2";
import { createMapToolbar } from "./ui/mapToolbar.js?v=14";
import { setLoading, createMapLoadProgress } from "./ui/loading.js";
import { loadScenarioTreatments } from "./treatments/loader.js?v=3";
import { setupClickHandling } from "./interaction/click.js?v=3";
import { setupHoverTooltips } from "./interaction/hover.js?v=2";
import { highlightInventoryGraphic, zoomToInventoryGraphic } from "./interaction/inventorySelection.js";
import { getSelectedBridgeFeatureCount, highlightProject, queryProjectGraphics, zoomToProject } from "./interaction/selection.js?v=14";
import { createTreatmentPinManager } from "./arcgis/treatmentPins.js?v=7";
import { clearLayerGraphics } from "./arcgis/graphics.js?v=3";
import { createProjectPanel } from "./ui/projectPanel.js?v=8";
import { createInventoryInfoPanel } from "./ui/inventoryInfoPanel.js";
import { createMapTooltip } from "./ui/mapTooltip.js?v=4";
import { createMapKey } from "./ui/mapKey.js?v=14";
import { getTreatmentsForProject } from "./interaction/tooltipMeta.js";
import { formatProjectSeq, projectIdOf, sequenceNoOf, statusOf, treatmentType } from "./treatments/helpers.js";
import { resolveCrsIdFromPavementAttrs } from "./utils/crsId.js";
import { createProjectSearch } from "./ui/projectSearch.js";
import { createTreatmentEditor } from "./ui/treatmentEditor.js?v=3";
import { createFilterPanel } from "./ui/filterPanel.js?v=1";
import { createAdhocSession } from "./interaction/adhocSession.js?v=2";
import { highlightAdhocFeature, syncAdhocSessionGraphics } from "./interaction/adhocLoader.js?v=6";

function scenarioProjectIdSets(data) {
    const bridge = new Set();
    const pavement = new Set();
    const treatments = data?.treatments ?? data?.Treatments ?? [];
    for (const treatment of treatments) {
        if (statusOf(treatment) === "Unselected") continue;
        const id = projectIdOf(treatment);
        if (id == null || id === "") continue;
        const type = treatmentType(treatment);
        if (type === "B") bridge.add(String(id));
        else if (type === "P") pavement.add(String(id));
    }
    return { bridge, pavement };
}

function projectSelectedText(scenarioData, projectId) {
    const treatments = getTreatmentsForProject(scenarioData, projectId);
    let bridges = 0;
    let pavements = 0;
    for (const treatment of treatments) {
        const type = treatmentType(treatment);
        if (type === "B") bridges += 1;
        else if (type === "P") pavements += 1;
    }

    const projects = scenarioData?.projects ?? scenarioData?.Projects ?? [];
    const project = projects.find((entry) => Number(projectIdOf(entry)) === Number(projectId));
    const label = project
        ? (formatProjectSeq(sequenceNoOf(project), statusOf(project)) ?? projectId)
        : projectId;
    const bridgeLabel = bridges === 1 ? "Bridge" : "Bridges";
    const pavementLabel = pavements === 1 ? "Pavement" : "Pavements";
    return `Project ${label} selected. It has ${bridges} ${bridgeLabel} and ${pavements} ${pavementLabel}.`;
}

function scenarioLoadedText(bridgeCount, pavementCount) {
    const bridges = Number(bridgeCount) || 0;
    const pavements = Number(pavementCount) || 0;
    const bridgeLabel = bridges === 1 ? "bridge" : "bridges";
    const pavementLabel = pavements === 1 ? "pavement" : "pavements";
    return `Scenario loaded that includes ${bridges} ${bridgeLabel} and ${pavements} ${pavementLabel}.`;
}

function updateScenarioStrip(scenarioData, scenarioId) {
    const strip = document.getElementById("scenario-strip");
    const text = document.getElementById("scenario-strip-text");
    if (!strip || !text) return;

    const name = scenarioData.scenario?.name ?? scenarioData.Scenario?.Name ?? `Scenario ${scenarioId}`;
    const hasTreatments = ((scenarioData.treatments ?? scenarioData.Treatments ?? []).length > 0);
    const statusClass = hasTreatments ? "status-ok" : "status-bad";
    const statusText = hasTreatments ? "SUCCESS" : "NO RESULTS";
    text.textContent = "";
    text.appendChild(document.createTextNode("SCENARIO: "));
    const nameEl = document.createElement("b");
    nameEl.textContent = name;
    text.appendChild(nameEl);
    text.appendChild(document.createTextNode(" | STATUS: "));
    const statusSpan = document.createElement("span");
    statusSpan.className = statusClass;
    statusSpan.textContent = statusText;
    text.appendChild(statusSpan);
    strip.hidden = false;
}

async function initMap() {
    const scenarioId = window.__SCENARIO_ID__;
    if (!scenarioId) {
        throw new Error("Scenario id is required.");
    }

    const loadProgress = createMapLoadProgress((visible, message, percent) => {
        setLoading(visible, message, percent);
    });

    // Kick off the scenario data fetch immediately instead of after the ArcGIS view/basemap/
    // toolbar finish initializing (below) — the two are independent, so overlapping them
    // removes dead sequential wait time from every load.
    loadProgress.startFetchingScenario();
    const fetchStart = performance.now();
    const scenarioDataPromise = fetchScenarioData(scenarioId);

    const [
        // Aliased: an unqualified `Map` here would shadow the built-in Map used below.
        { default: EsriMap },
        { default: MapView },
        { default: FeatureLayer },
        { default: Graphic },
        { default: GraphicsLayer },
        { default: Point },
        { default: Home },
        { default: Zoom },
        geometryEngine,
        queryModule,
    ] = await Promise.all([
        loadModule("Map.js"),
        loadModule("views/MapView.js"),
        loadModule("layers/FeatureLayer.js"),
        loadModule("Graphic.js"),
        loadModule("layers/GraphicsLayer.js"),
        loadModule("geometry/Point.js"),
        loadModule("widgets/Home.js"),
        loadModule("widgets/Zoom.js"),
        loadModule("geometry/geometryEngine.js"),
        loadModule("rest/query.js"),
    ]);

    const { pavementLayer, bridgeLayer, unselectedPavementLayer, unselectedBridgeLayer } = createScenarioLayers(FeatureLayer);
    const adhocGraphicsLayer = new GraphicsLayer({ id: "adhoc-session", listMode: "hide" });
    const pavementSelection = createPavementSelectionOverlay(GraphicsLayer, Graphic);

    const map = new EsriMap({
        basemap: "gray",
        // Unselected/not-funded layers are populated the same as the others below, but are
        // created with visible:false (see layers.js) so they stay hidden until the
        // "Unselected" toolbar button flips their visibility — no default map.load() cost since
        // an empty/hidden client-side FeatureLayer renders nothing either way.
        // Border sits under the gray pavement line. The focused segment sits on top of it.
        layers: [
            pavementSelection.borderLayer,
            pavementLayer,
            pavementSelection.focusLayer,
            bridgeLayer,
            unselectedPavementLayer,
            unselectedBridgeLayer,
            adhocGraphicsLayer,
        ],
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
    // MapView adds its own zoom control at the top-left, under the filter panel.
    // The top-right Zoom widget below is the one that stays.
    view.ui.remove("zoom");

    // Declared early so the zoom watcher can refresh the selection overlay.
    let highlightHandle = null;

    let refreshAdhocBridgeIcons = () => {};
    let refreshInventoryBridgeIcons = () => {};
    let lastBridgeIconZoomBucket = Math.floor(view.zoom);
    view.watch("zoom", (zoom) => {
        const bucket = Math.floor(zoom);
        if (bucket === lastBridgeIconZoomBucket) return;
        lastBridgeIconZoomBucket = bucket;
        refreshAdhocBridgeIcons(zoom);
        refreshInventoryBridgeIcons(zoom);
    });

    const basemapControls = await setupBasemapControls(view, loadModule);
    const inventoryLayers = createInventoryLayers(FeatureLayer);
    attachInventoryBridgeIcons(inventoryLayers.bridgesBaseLayer, view);
    refreshInventoryBridgeIcons = (zoom) => scaleInventoryBridgeIcons(inventoryLayers.bridgesBaseLayer, zoom);
    refreshInventoryBridgeIcons(view.zoom);

    let onMapLayerLoaded = () => {};
    let onMapLayerUnloaded = () => {};
    const mapToolbar = createMapToolbar({
        view,
        map,
        inventoryLayers,
        scenarioPavementLayer: pavementLayer,
        scenarioBridgeLayer: bridgeLayer,
        unselectedPavementLayer,
        unselectedBridgeLayer,
        overlayLayer: adhocGraphicsLayer,
        setBasemap: basemapControls.setBasemap,
        getActiveBasemapId: basemapControls.getActiveBasemapId,
        basemapCatalog: basemapControls.catalog,
        onBasemapChange: basemapControls.onBasemapChange,
        togglePenndotBasemap: basemapControls.togglePenndotBasemap,
        isPenndotBasemapActive: basemapControls.isPenndotBasemapActive,
        onLayerLoaded: (key) => onMapLayerLoaded(key),
        onLayerUnloaded: (key) => onMapLayerUnloaded(key),
        onLayersRaised: () => pavementSelection.placeAround(map, pavementLayer),
    });

    view.ui.add(new Home({ view }), "top-right");
    view.ui.add(new Zoom({ view }), "top-right");

    let scenarioData = await scenarioDataPromise;
    console.debug(`[map] generate-map fetch: ${Math.round(performance.now() - fetchStart)}ms`);
    loadProgress.finishFetchingScenario();

    updateScenarioStrip(scenarioData, scenarioId);

    const hasProjects = (scenarioData.projects ?? scenarioData.Projects ?? []).length > 0;
    if (!hasProjects) {
        const mapFilter = document.getElementById("map-filter");
        const toolbarZone = document.getElementById("map-toolbar-zone");
        if (mapFilter) mapFilter.hidden = true;
        if (toolbarZone) toolbarZone.hidden = true;
    }

    async function renderScenarioData({ zoom = true } = {}) {
        await Promise.all([
            clearLayerGraphics(pavementLayer),
            clearLayerGraphics(bridgeLayer),
            clearLayerGraphics(unselectedPavementLayer),
            clearLayerGraphics(unselectedBridgeLayer),
        ]);

        return loadScenarioTreatments(
            view,
            pavementLayer,
            bridgeLayer,
            Graphic,
            geometryEngine,
            queryModule,
            scenarioData,
            (fraction) => loadProgress.reportMapPreparation(fraction),
            { zoom, unselectedPavementLayer, unselectedBridgeLayer },
        );
    }

    let counts = { pavementCount: 0, bridgeCount: 0 };
    let scenarioRendered = false;
    try {
        loadProgress.reportMapPreparation(0);
        const renderStart = performance.now();
        counts = await renderScenarioData();
        scenarioRendered = true;
        console.debug(`[map] treatment layer rendering (ArcGIS batch loading): ${Math.round(performance.now() - renderStart)}ms`);
        loadProgress.complete();
        mapToolbar.bringScenarioLayersToFront();
    } finally {
        loadProgress.stop();
    }

    const panelRoot = document.getElementById("project-panel");
    const editorRoot = document.getElementById("project-treatments-bar");
    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    const [pavementLayerView, bridgeLayerView, unselectedPavementLayerView, unselectedBridgeLayerView] = await Promise.all([
        view.whenLayerView(pavementLayer),
        view.whenLayerView(bridgeLayer),
        view.whenLayerView(unselectedPavementLayer),
        view.whenLayerView(unselectedBridgeLayer),
    ]);

    const scenarioLayers = [pavementLayer, bridgeLayer, unselectedPavementLayer, unselectedBridgeLayer];
    const layerViewsByLayer = new Map([
        [pavementLayer, pavementLayerView],
        [bridgeLayer, bridgeLayerView],
        [unselectedPavementLayer, unselectedPavementLayerView],
        [unselectedBridgeLayer, unselectedBridgeLayerView],
    ]);

    const inventoryInfoPanel = createInventoryInfoPanel(
        document.getElementById("inventory-info-panel"),
        { onClose: () => clearInventoryHighlight() },
    );
    const mapTooltip = createMapTooltip(document.getElementById("map-tooltip-root"));
    const treatmentPins = createTreatmentPinManager(view, Graphic, Point);
    const adhocSession = createAdhocSession();
    let selectedProjectId = null;
    let selectedPavementFeatures = [];
    let focusedPavementTreatmentId = null;
    const mapKey = createMapKey(document.getElementById("map-key"), {
        bandRoot: document.getElementById("map-key-band"),
        getCounts: () => {
            const { bridge: bridgeIds, pavement: pavementIds } = scenarioProjectIdSets(scenarioData);
            const adhocItems = adhocSession.getItems();
            const treatmentCount = selectedProjectId == null
                ? 0
                : getTreatmentsForProject(scenarioData, selectedProjectId).length;
            return {
                proj: new Set([...bridgeIds, ...pavementIds]).size,
                "proj-sel": getSelectedBridgeFeatureCount(),
                pave: pavementIds.size,
                "bridge-proj": bridgeIds.size,
                ptreat: treatmentCount,
                mpms: null,
                bridge: adhocItems.filter((item) => item.type === "bridge").length,
                pinv: adhocItems.filter((item) => item.type === "pavement" || item.type === "road").length,
            };
        },
    });
    mapKey.refreshCounts();
    if (scenarioRendered) {
        mapKey.showFor("proj", scenarioLoadedText(counts.bridgeCount, counts.pavementCount));
    }

    const loadedRowByLayer = {
        road: "road-rating",
        bridge: "bridge",
        mpms: "mpms",
    };
    const UNFUNDED_MESSAGE = "Unfunded projects are on the map.";
    onMapLayerUnloaded = (key) => {
        if (key === "unselected") mapKey.hideMessage(UNFUNDED_MESSAGE);
    };
    onMapLayerLoaded = (key) => {
        if (key === "unselected") {
            mapKey.showMessage({
                color: "#54687a",
                text: UNFUNDED_MESSAGE,
            });
            return;
        }
        const rowId = loadedRowByLayer[key];
        if (!rowId) return;
        const toastColor = key === "bridge" ? "#7C55A0" : key === "road" ? "#BA67EF" : undefined;
        mapKey.showFor(rowId, undefined, toastColor);
    };

    function drawAdhocBridgeIcons(zoom = view.zoom) {
        void syncAdhocSessionGraphics(
            adhocGraphicsLayer,
            inventoryLayers,
            adhocSession.getItems(),
            Graphic,
            adhocSession.currentItem(),
            bridgeIconSizeForZoom(zoom),
        );
    }
    refreshAdhocBridgeIcons = drawAdhocBridgeIcons;
    let lastAdhocCount = adhocSession.getItems().length;
    adhocSession.subscribe(() => {
        mapKey.refreshCounts();
        drawAdhocBridgeIcons();
        const items = adhocSession.getItems();
        if (items.length > lastAdhocCount) {
            const type = String(items[items.length - 1]?.type ?? "").toLowerCase();
            if (type === "bridge") mapKey.showFor("bridge");
            else if (type === "pavement" || type === "road") mapKey.showFor("pinv");
            else if (type === "mpms") mapKey.showFor("mpms");
        }
        lastAdhocCount = items.length;
    });

    let inventoryHighlightHandle = null;
    let adhocHighlightHandle = null;

    async function syncSelectedProjectPresentation() {
        if (selectedProjectId == null) return;

        const activeFilters = scenarioLayers.some((layer) => layer.definitionExpression);

        const visibleTreatmentIds = activeFilters && typeof filterPanel.getVisibleTreatmentIdsForProject === "function"
            ? filterPanel.getVisibleTreatmentIdsForProject(selectedProjectId)
            : null;
        let projectFeatures = await queryProjectGraphics(
            scenarioLayers,
            selectedProjectId,
            visibleTreatmentIds,
        );
        if (visibleTreatmentIds?.size === 0) {
            projectFeatures = [];
        }
        const visiblePolylines = projectFeatures.filter((feature) => {
            const type = feature?.geometry?.type;
            return type === "polyline" || type === "multipolyline";
        });
        selectedPavementFeatures = projectFeatures.filter((feature) => feature.layer === pavementLayer);
        pavementSelection.placeAround(map, pavementLayer);

        highlightHandle = await highlightProject(
            layerViewsByLayer,
            scenarioLayers,
            selectedProjectId,
            highlightHandle,
        );

        if (visiblePolylines.length === 0) {
            treatmentPins.remove();
        } else {
            treatmentPins.replace(projectFeatures);
        }

        if (focusedPavementTreatmentId == null) {
            pavementSelection.clearFocus();
            treatmentPins.clearFocus();
        } else {
            const applied = pavementSelection.focusTreatment(
                selectedPavementFeatures,
                focusedPavementTreatmentId,
            );
            if (applied) {
                treatmentPins.focus(focusedPavementTreatmentId);
            } else {
                focusedPavementTreatmentId = null;
                pavementSelection.clearFocus();
                treatmentPins.clearFocus();
                panel.clearPickedTreatment();
            }
        }
        mapKey.refreshCounts();
    }

    function clearInventoryHighlight() {
        inventoryHighlightHandle?.remove();
        inventoryHighlightHandle = null;
    }

    function clearAdhocHighlight() {
        adhocHighlightHandle?.remove();
        adhocHighlightHandle = null;
    }

    const editor = createTreatmentEditor(editorRoot, {
        scenarioId,
        antiforgeryToken,
        getScenarioData: () => scenarioData,
        // Refresh the underlying scenario data, the bottom editor list, the right-side
        // project panel, and the map's pavement/bridge graphics so everything reflects
        // the add/edit/delete immediately.
        onChanged: async () => {
            try {
                scenarioData = await fetchScenarioData(scenarioId);
            } catch (error) {
                // The save/delete itself already succeeded by this point — a failed
                // refresh (e.g. the scenario now has zero treatments left) shouldn't be
                // reported to the user as if their change failed.
                console.warn("Failed to refresh scenario data after treatment change:", error);
            }
            await editor.refresh();
            panel.refresh();
            filterPanel.render();
            try {
                loadProgress.reportMapPreparation(0);
                // Don't re-zoom the whole scenario after add/edit/delete — keep the
                // user's current project view; just replace graphics + re-highlight.
                counts = await renderScenarioData({ zoom: false });
                mapToolbar.bringScenarioLayersToFront();
                mapKey.showFor("proj", scenarioLoadedText(counts.bridgeCount, counts.pavementCount));
                // renderScenarioData() wipe/rebuilds layer graphics, so any prior
                // highlight handles point at stale OBJECTIDs. Re-sync so a newly
                // added treatment is highlighted without deselecting/re-clicking.
                await syncSelectedProjectPresentation();
            } catch (error) {
                console.warn("Failed to refresh map graphics after treatment change:", error);
            } finally {
                loadProgress.stop();
            }
        },
    });

    const panel = createProjectPanel(panelRoot, {
        scenarioId,
        antiforgeryToken,
        getScenarioData: () => scenarioData,
        onSelectProject: selectProject,
        onShowTreatments: (id) => editor.expand(id),
        onClose: () => clearProjectSelection(),
        adhocSession,
        onAdhocIndexChange: async (item) => {
            adhocHighlightHandle = await highlightAdhocFeature(
                view,
                inventoryLayers,
                item,
                adhocHighlightHandle,
            );
        },
    });

    const projectSearch = createProjectSearch(document.getElementById("map-project-search"), {
        getScenarioData: () => scenarioData,
        onSelectProject: selectProject,
        isUnfundedVisible: () => unselectedPavementLayer.visible || unselectedBridgeLayer.visible,
    });

    const filterPanel = createFilterPanel(
        document.getElementById("gis-filter-sections"),
        document.getElementById("gis-filter-clear"),
        {
            getScenarioData: () => scenarioData,
            pavementLayer,
            bridgeLayer,
            unselectedPavementLayer,
            unselectedBridgeLayer,
            onFiltersChanged: () => syncSelectedProjectPresentation(),
        },
    );
    if (hasProjects) filterPanel.render();

    document.getElementById("gis-filter-collapse")?.addEventListener("click", () => {
        const filter = document.getElementById("map-filter");
        const collapsed = filter?.classList.toggle("collapsed");
        document.querySelector(".map-page")?.classList.toggle("gis-filter-collapsed", !!collapsed);
        const btn = document.getElementById("gis-filter-collapse");
        if (btn) {
            btn.setAttribute("aria-expanded", String(!collapsed));
            btn.setAttribute("aria-label", collapsed ? "Expand filters" : "Collapse filters");
        }
    });

    document.getElementById("gis-mapkey-toggle")?.addEventListener("click", () => {
        mapKey.toggle();
    });

    function clearAdhocSelection() {
        clearAdhocHighlight();
        clearInventoryHighlight();
        adhocSession.clear();
    }

    function showTreatmentSelectedToast(graphic) {
        const attrs = graphic?.attributes ?? {};
        const isBridge = graphic?.layer === bridgeLayer || graphic?.layer === unselectedBridgeLayer;
        const id = isBridge
            ? String(attrs.BRKEY ?? "").trim()
            : resolveCrsIdFromPavementAttrs(attrs).trim();
        const label = isBridge ? "Bridge ID" : "Pavement ID";
        mapKey.showMessage({
            color: "#FBC34D",
            text: id ? `A treatment is selected. ${label}: ${id}` : "A treatment is selected.",
        });
    }

    async function selectProject(id, detail) {
        if (id == null) return;
        const graphic = detail?.graphic;
        const treatmentId = graphic?.attributes?.TreatmentID ?? graphic?.attributes?.TreatmentId;
        const sameProject = selectedProjectId != null && Number(selectedProjectId) === Number(id);
        const isScenarioPavement = graphic?.layer === pavementLayer;
        const isScenarioBridge = graphic?.layer === bridgeLayer || graphic?.layer === unselectedBridgeLayer;
        if (sameProject && isScenarioPavement && treatmentId != null && treatmentId !== "" && selectedPavementFeatures.length) {
            const applied = pavementSelection.focusTreatment(selectedPavementFeatures, treatmentId);
            if (applied) {
                focusedPavementTreatmentId = treatmentId;
                treatmentPins.focus(treatmentId);
                panel.pickTreatment(treatmentId);
                showTreatmentSelectedToast(graphic);
                return;
            }
        }
        if (sameProject && isScenarioBridge && treatmentId != null && treatmentId !== "") {
            focusedPavementTreatmentId = null;
            pavementSelection.clearFocus();
            treatmentPins.clearFocus();
            panel.pickTreatment(treatmentId);
            showTreatmentSelectedToast(graphic);
            return;
        }

        focusedPavementTreatmentId = null;
        pavementSelection.clearFocus();
        treatmentPins.clearFocus();
        clearInventorySelection();
        clearAdhocHighlight();
        adhocSession.beginScenarioProject(id);
        selectedProjectId = id;
        view.padding = { ...view.padding, right: 278 };
        document.querySelector(".map-page")?.classList.add("gis-panel-open");

        // Open the panel/editor immediately — they only need `id` and read straight from
        // scenarioData, so they shouldn't wait on (or be cancelled by a failure in) the
        // map-side highlight/zoom below, which touches ArcGIS layer queries that can fail for
        // reasons unrelated to whether the project's own data exists (e.g. a layer view not
        // ready yet right after the "Unselected" toggle was just flipped on).
        panel.open(id);
        editor.setProject(id);
        if (isScenarioBridge && treatmentId != null && treatmentId !== "") {
            panel.pickTreatment(treatmentId);
        }
        mapKey.showFor("proj-sel", projectSelectedText(scenarioData, id));
        if (isScenarioBridge && treatmentId != null && treatmentId !== "") {
            showTreatmentSelectedToast(graphic);
        }

        try {
            await syncSelectedProjectPresentation();
            mapKey.refreshCounts();
            await zoomToProject(view, scenarioLayers, id, { avoidOverlays: true });
        } catch (error) {
            console.warn("Failed to highlight/zoom to project:", error);
        }
    }

    function clearInventorySelection() {
        clearInventoryHighlight();
        inventoryInfoPanel.close();
    }

    function clearProjectSelection() {
        const handle = highlightHandle;
        highlightHandle = null;
        selectedProjectId = null;
        selectedPavementFeatures = [];
        focusedPavementTreatmentId = null;
        pavementSelection.clear();
        treatmentPins.remove();
        treatmentPins.clearFocus();
        clearAdhocSelection();
        view.padding = { top: 0, left: 0, right: 0, bottom: 0 };
        document.querySelector(".map-page")?.classList.remove("gis-panel-open");
        panel.close();
        editor.close();
        editor.setProject(null);
        mapKey.hideBand();
        const done = typeof handle?.release === "function" ? handle.release() : Promise.resolve();
        Promise.resolve(done).finally(() => mapKey.refreshCounts());
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

    async function appendAdhocInventory({ graphic, layer, item }) {
        adhocSession.append(item);
        inventoryInfoPanel.close();
        if (item?.type !== "bridge") {
            inventoryHighlightHandle = await highlightInventoryGraphic(
                view,
                layer,
                graphic,
                inventoryHighlightHandle,
            );
        } else {
            inventoryHighlightHandle?.remove();
            inventoryHighlightHandle = null;
        }
        adhocHighlightHandle = await highlightAdhocFeature(
            view,
            inventoryLayers,
            item,
            adhocHighlightHandle,
        );
        panel.refresh();
    }

    setupClickHandling({
        view,
        pavementLayer,
        bridgeLayer,
        unselectedPavementLayer,
        unselectedBridgeLayer,
        inventoryLayers: [
            inventoryLayers.pavementBaseLayer,
            inventoryLayers.bridgesBaseLayer,
            inventoryLayers.mpmsLayer,
        ],
        adhocSession,
        onSelectProject: selectProject,
        onClearProjectSelection: clearProjectSelection,
        onSelectInventory: selectInventoryFeature,
        onAppendAdhocInventory: appendAdhocInventory,
        onClearInventorySelection: clearInventorySelection,
        onClearAll: clearSelection,
    });

    setupHoverTooltips({
        view,
        pavementLayer,
        bridgeLayer,
        unselectedPavementLayer,
        unselectedBridgeLayer,
        inventoryBridgeLayer: inventoryLayers.bridgesBaseLayer,
        getScenarioData: () => scenarioData,
        tooltip: mapTooltip,
        onTreatmentHover: (projectId, treatmentId) => {
            if (Number(panel.getProjectId()) !== Number(projectId)) {
                panel.clearTreatmentHighlight();
                return;
            }
            panel.highlightTreatment(treatmentId);
        },
        onTreatmentHoverEnd: () => panel.clearTreatmentHighlight(),
    });
}

initMap().catch((error) => {
    console.error("Failed to initialize map:", error);
    setLoading(true, error?.message || "Unable to load scenario map.", 0);
});
