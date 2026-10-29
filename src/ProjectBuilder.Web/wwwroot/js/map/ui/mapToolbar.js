import { LAYERS_ENABLE_ZOOM_LEVEL } from "../config.js";
import {
    applyBridgeRatings,
    applyMPMSRatings,
    applyRoadRatings,
    bringScenarioLayersToFront,
} from "../arcgis/inventoryLayers.js?v=8";
import { createBasemapPicker } from "./basemapPicker.js?v=2";

const ASSET_BASE = "/images/map";

function createRatingsPopup(root, options) {
    if (!root) {
        return {
            root: null,
            showAt() {},
            hide() {},
            setEnabled() {},
            getSelected: () => [],
            setSelected() {},
            onToggle() {},
        };
    }

    const buttons = options.keys.map((key) => {
        const btn = document.createElement("button");
        btn.type = "button";
        btn.className = `gis-rating-btn gis-rating-btn--${key}`;
        btn.textContent = options.labels[key];
        btn.title = options.titles[key];
        btn.dataset.rating = key;
        return btn;
    });

    buttons.forEach((btn) => root.appendChild(btn));

    return {
        root,
        showAt(x, y) {
            root.hidden = false;
            root.style.left = `${x}px`;
            root.style.top = `${y}px`;
        },
        hide() {
            root.hidden = true;
        },
        setEnabled(enabled) {
            root.classList.toggle("is-disabled", !enabled);
            buttons.forEach((btn) => { btn.disabled = !enabled; });
        },
        getSelected() {
            return buttons.filter((btn) => btn.classList.contains("is-selected")).map((btn) => btn.dataset.rating);
        },
        setSelected(keys) {
            const selected = new Set(keys);
            buttons.forEach((btn) => {
                btn.classList.toggle("is-selected", selected.has(btn.dataset.rating));
            });
        },
        onToggle(handler) {
            buttons.forEach((btn) => {
                btn.addEventListener("click", (event) => {
                    event.stopPropagation();
                    if (btn.disabled) return;
                    handler(btn.dataset.rating);
                });
            });
        },
    };
}

export function createMapToolbar({
    view,
    map,
    inventoryLayers,
    scenarioPavementLayer,
    scenarioBridgeLayer,
    unselectedPavementLayer = null,
    unselectedBridgeLayer = null,
    overlayLayer = null,
    setBasemap,
    getActiveBasemapId,
    basemapCatalog = [],
    onBasemapChange,
    togglePenndotBasemap,
    isPenndotBasemapActive,
    onLayerLoaded = null,
    onLayerUnloaded = null,
    onLayersRaised = null,
}) {
    const {
        pavementBaseLayer,
        bridgesBaseLayer,
        mpmsLayer,
    } = inventoryLayers;

    const scenarioLayers = [
        scenarioPavementLayer,
        scenarioBridgeLayer,
        unselectedPavementLayer,
        unselectedBridgeLayer,
    ].filter(Boolean);

    let raisingLayers = false;
    function raiseLayers() {
        if (raisingLayers) return;
        raisingLayers = true;
        try {
            bringScenarioLayersToFront(map, ...scenarioLayers, overlayLayer);
            onLayersRaised?.();
        } finally {
            raisingLayers = false;
        }
    }

    const toolbarZone = document.getElementById("map-toolbar-zone");
    const toolbar = document.getElementById("map-toolbar");
    const panel = document.getElementById("map-toolbar-panel");
    const layersBtn = document.querySelector(".gis-toolbar-layers");
    const basemapBtn = document.getElementById("map-toolbar-basemap")
        || document.getElementById("map-toolbar-penndot");
    const roadBtn = document.getElementById("map-toolbar-road");
    const bridgeBtn = document.getElementById("map-toolbar-bridge");
    const mpmsBtn = document.getElementById("map-toolbar-mpms");
    const unselectedBtn = document.getElementById("map-toolbar-unselected");

    let pickerRoot = document.getElementById("map-basemap-picker");
    if (!pickerRoot) {
        pickerRoot = document.createElement("div");
        pickerRoot.id = "map-basemap-picker";
        pickerRoot.className = "gis-basemap-picker";
        pickerRoot.hidden = true;
        pickerRoot.setAttribute("aria-hidden", "true");
        document.body.appendChild(pickerRoot);
    }

    const resolveActiveId = () => getActiveBasemapId?.()
        ?? (isPenndotBasemapActive?.() ? "penndot" : "gray");

    const basemapPicker = createBasemapPicker(pickerRoot, {
        catalog: basemapCatalog,
        getActiveId: resolveActiveId,
        trigger: basemapBtn,
        setBasemap: async (id) => {
            if (setBasemap) return setBasemap(id);
            if (!togglePenndotBasemap) return id;
            const wantPenndot = id === "penndot";
            if (wantPenndot !== isPenndotBasemapActive?.()) togglePenndotBasemap();
            return resolveActiveId();
        },
        onOpenChange: (open) => {
            basemapBtn?.classList.toggle("is-open", open);
            basemapBtn?.setAttribute("aria-expanded", open ? "true" : "false");
            if (open) keepToolbarOpen();
        },
    });

    const roadPopup = createRatingsPopup(document.getElementById("map-ratings-road"), {
        keys: ["good", "fair", "poor"],
        labels: { good: "G", fair: "F", poor: "P" },
        titles: { good: "Good", fair: "Fair", poor: "Poor" },
    });
    const bridgePopup = createRatingsPopup(document.getElementById("map-ratings-bridge"), {
        keys: ["good", "fair", "poor"],
        labels: { good: "G", fair: "F", poor: "P" },
        titles: { good: "Good", fair: "Fair", poor: "Poor" },
    });
    const mpmsPopup = createRatingsPopup(document.getElementById("map-ratings-mpms"), {
        keys: ["ACT", "PROG"],
        labels: { ACT: "A", PROG: "P" },
        titles: { ACT: "Active", PROG: "Programmed" },
    });

    let panelCloseTimer = null;
    let ratingsCloseTimer = null;
    let panelPinned = false;
    let zoomLevel = view.zoom ?? 0;

    let roadsActive = false;
    let bridgesActive = false;
    let mpmsActive = false;

    let selectedRoadRatings = [];
    let selectedBridgeRatings = [];
    let selectedMpmsRatings = [];

    const loadingWatches = {
        road: null,
        bridge: null,
        mpms: null,
        // Holds an array of watch handles (one per layer) instead of a single handle.
        unselected: null,
    };

    function setButtonLoading(button, loading) {
        button?.classList.toggle("is-loading", loading);
    }

    function setButtonActive(button, active) {
        button?.classList.toggle("is-active", active);
    }

    function layersEnabled() {
        return zoomLevel >= LAYERS_ENABLE_ZOOM_LEVEL;
    }

    function updateZoomGate() {
        const enabled = layersEnabled();
        [roadBtn, bridgeBtn, mpmsBtn].forEach((btn) => {
            btn?.classList.toggle("is-zoom-gated", !enabled);
            btn?.setAttribute("aria-disabled", enabled ? "false" : "true");
        });
    }

    function updateBasemapButton() {
        if (!basemapBtn) return;
        const activeId = resolveActiveId();
        const entry = basemapCatalog.find((item) => item.id === activeId);
        const thumb = entry?.thumbnail
            || (activeId === "penndot" ? "/images/map/pendotMap.png" : "/images/map/lightGrayMap.png");
        basemapBtn.style.backgroundImage = `url("${thumb}")`;
        const label = basemapBtn.querySelector(".gis-toolbar-basemap-label, .gis-toolbar-penndot-label");
        if (label) label.textContent = entry?.title || "Map type";
        basemapPicker.refresh();
    }

    function clearLoadingWatch(key) {
        const watch = loadingWatches[key];
        if (!watch) return;
        (Array.isArray(watch) ? watch : [watch]).forEach((handle) => handle?.remove());
        loadingWatches[key] = null;
    }

    function anyRatingsPopupVisible() {
        return [roadPopup, bridgePopup, mpmsPopup].some(
            (popup) => popup.root && !popup.root.hidden,
        );
    }

    async function watchLayerLoading(layer, key, button) {
        clearLoadingWatch(key);
        if (!layer || !button) return;

        setButtonLoading(button, true);
        let settled = false;
        const loadingTimeout = window.setTimeout(() => {
            settled = true;
            setButtonLoading(button, false);
            clearLoadingWatch(key);
        }, 20000);

        try {
            await layer.load();
            const layerView = await view.whenLayerView(layer);

            const finishLoading = () => {
                if (settled) return;
                settled = true;
                window.clearTimeout(loadingTimeout);
                setButtonLoading(button, false);
                clearLoadingWatch(key);
                if (button.classList.contains("is-active")) onLayerLoaded?.(key);
            };

            if (!layerView.updating) {
                finishLoading();
                return;
            }

            loadingWatches[key] = layerView.watch("updating", (updating) => {
                if (!updating) {
                    finishLoading();
                }
            });
        } catch (error) {
            window.clearTimeout(loadingTimeout);
            console.warn(`Layer loading watch failed for ${key}:`, error);
            setButtonLoading(button, false);
            clearLoadingWatch(key);
        }
    }

    // Same idea as watchLayerLoading, but for a button that controls several layers at once
    // (the "Unselected" toggle drives both the pavement and bridge unselected layers) — the
    // spinner stays on until every one of them finishes rendering its (already-loaded) graphics.
    // Since that data is already in memory (no fetch), `updating` can flip back to false almost
    // synchronously, which made the spinner a one-frame flash — MIN_VISIBLE_MS guarantees it
    // stays on screen long enough to actually read as a loading indicator.
    async function watchLayersLoading(layers, key, button) {
        const MIN_VISIBLE_MS = 400;
        clearLoadingWatch(key);
        const activeLayers = layers.filter(Boolean);
        if (!activeLayers.length || !button) return;

        setButtonLoading(button, true);
        const startedAt = performance.now();
        let settled = false;
        const loadingTimeout = window.setTimeout(() => {
            settled = true;
            setButtonLoading(button, false);
            clearLoadingWatch(key);
        }, 20000);

        const finish = () => {
            if (settled) return;
            settled = true;
            window.clearTimeout(loadingTimeout);
            const elapsed = performance.now() - startedAt;
            const remaining = Math.max(0, MIN_VISIBLE_MS - elapsed);
            window.setTimeout(() => {
                setButtonLoading(button, false);
                clearLoadingWatch(key);
                if (button.classList.contains("is-active")) onLayerLoaded?.(key);
            }, remaining);
        };

        try {
            const layerViews = await Promise.all(activeLayers.map(async (layer) => {
                await layer.load();
                return view.whenLayerView(layer);
            }));

            const finishIfDone = () => {
                if (layerViews.every((layerView) => !layerView.updating)) {
                    finish();
                }
            };

            if (layerViews.every((layerView) => !layerView.updating)) {
                finish();
                return;
            }

            loadingWatches[key] = layerViews.map((layerView) => layerView.watch("updating", finishIfDone));
        } catch (error) {
            window.clearTimeout(loadingTimeout);
            console.warn(`Layer loading watch failed for ${key}:`, error);
            setButtonLoading(button, false);
            clearLoadingWatch(key);
        }
    }

    function showLayer(layer) {
        if (!layer || !map) return;
        if (!map.layers.includes(layer)) {
            map.add(layer);
        }
        layer.visible = true;
        raiseLayers();
    }

    function hideLayer(layer, key, button) {
        clearLoadingWatch(key);
        setButtonLoading(button, false);
        if (!layer || !map) return;
        layer.visible = false;
        if (map.layers.includes(layer)) {
            map.remove(layer);
        }
    }

    function loadRoads() {
        showLayer(pavementBaseLayer);
        void watchLayerLoading(pavementBaseLayer, "road", roadBtn);
        applyRoadRatings(
            pavementBaseLayer,
            selectedRoadRatings,
            scenarioPavementLayer,
            map,
            scenarioLayers,
        );
    }

    function loadBridges() {
        showLayer(bridgesBaseLayer);
        void watchLayerLoading(bridgesBaseLayer, "bridge", bridgeBtn);
        applyBridgeRatings(
            bridgesBaseLayer,
            selectedBridgeRatings,
            scenarioBridgeLayer,
            map,
            scenarioLayers,
        );
    }

    function loadMpms() {
        showLayer(mpmsLayer);
        void watchLayerLoading(mpmsLayer, "mpms", mpmsBtn);
        applyMPMSRatings(
            mpmsLayer,
            selectedMpmsRatings,
            scenarioPavementLayer,
            map,
            scenarioLayers,
        );
    }

    function deactivateRoads() {
        hideLayer(pavementBaseLayer, "road", roadBtn);
        setButtonActive(roadBtn, false);
        roadsActive = false;
        selectedRoadRatings = [];
        roadPopup.setSelected([]);
        roadPopup.hide();
        applyRoadRatings(pavementBaseLayer, [], scenarioPavementLayer, map, scenarioLayers);
    }

    function deactivateBridges() {
        hideLayer(bridgesBaseLayer, "bridge", bridgeBtn);
        setButtonActive(bridgeBtn, false);
        bridgesActive = false;
        selectedBridgeRatings = [];
        bridgePopup.setSelected([]);
        bridgePopup.hide();
        applyBridgeRatings(bridgesBaseLayer, [], scenarioBridgeLayer, map, scenarioLayers);
    }

    function deactivateMpms() {
        hideLayer(mpmsLayer, "mpms", mpmsBtn);
        setButtonActive(mpmsBtn, false);
        mpmsActive = false;
        selectedMpmsRatings = [];
        mpmsPopup.setSelected([]);
        mpmsPopup.hide();
        applyMPMSRatings(mpmsLayer, [], scenarioPavementLayer, map, scenarioLayers);
    }

    function deactivateAllInventoryLayers() {
        if (roadsActive) deactivateRoads();
        if (bridgesActive) deactivateBridges();
        if (mpmsActive) deactivateMpms();
    }

    function isHoverZoneElement(element) {
        if (!element) return false;
        if (toolbarZone?.contains(element)) return true;
        return [roadPopup, bridgePopup, mpmsPopup].some(
            (popup) => popup.root && (popup.root === element || popup.root.contains(element)),
        );
    }

    function isToolbarInteractionHovered() {
        if (toolbarZone?.matches(":hover")) return true;
        return [roadPopup, bridgePopup, mpmsPopup].some(
            (popup) => popup.root && !popup.root.hidden && popup.root.matches(":hover"),
        );
    }

    function keepToolbarOpen() {
        if (panelCloseTimer) {
            clearTimeout(panelCloseTimer);
            panelCloseTimer = null;
        }
        openPanel();
    }

    function openPanel() {
        if (panelCloseTimer) {
            clearTimeout(panelCloseTimer);
            panelCloseTimer = null;
        }
        panel?.removeAttribute("hidden");
    }

    function closePanel() {
        if (panelPinned || anyRatingsPopupVisible() || basemapPicker.isOpen()) return;
        panel?.setAttribute("hidden", "");
    }

    function scheduleClosePanel() {
        if (panelCloseTimer) clearTimeout(panelCloseTimer);
        panelCloseTimer = window.setTimeout(() => {
            if (panelPinned || isToolbarInteractionHovered() || anyRatingsPopupVisible() || basemapPicker.isOpen()) {
                panelCloseTimer = null;
                return;
            }
            closePanel();
            panelCloseTimer = null;
        }, 400);
    }

    function clearRatingsCloseTimer() {
        if (ratingsCloseTimer) {
            clearTimeout(ratingsCloseTimer);
            ratingsCloseTimer = null;
        }
    }

    function showRatingsPopup(button, popup, closeOthers = []) {
        if (!button) return;
        closeOthers.forEach((p) => p.hide());
        keepToolbarOpen();
        const rect = button.getBoundingClientRect();
        popup.showAt(rect.left + rect.width / 2, rect.top);
    }

    function scheduleHideRatings(popup) {
        if (ratingsCloseTimer) clearTimeout(ratingsCloseTimer);
        ratingsCloseTimer = window.setTimeout(() => {
            const hoveredLayerBtn = [roadBtn, bridgeBtn, mpmsBtn].some((btn) => btn?.matches(":hover"));
            const hoveredPopup = [roadPopup, bridgePopup, mpmsPopup].some(
                (item) => item.root && !item.root.hidden && item.root.matches(":hover"),
            );
            if (hoveredLayerBtn || hoveredPopup) {
                ratingsCloseTimer = null;
                return;
            }
            popup.hide();
            ratingsCloseTimer = null;
        }, 250);
    }

    function toggleRatingSelection(current, rating) {
        return current.includes(rating)
            ? current.filter((item) => item !== rating)
            : [...current, rating];
    }

    basemapBtn?.addEventListener("click", (event) => {
        event.preventDefault();
        event.stopPropagation();
        keepToolbarOpen();
        basemapPicker.toggle();
    });

    onBasemapChange?.(updateBasemapButton);

    roadBtn?.addEventListener("click", () => {
        if (!layersEnabled()) return;
        if (roadsActive) {
            deactivateRoads();
            return;
        }
        roadsActive = true;
        setButtonActive(roadBtn, true);
        loadRoads();
    });

    bridgeBtn?.addEventListener("click", () => {
        if (!layersEnabled()) return;
        if (bridgesActive) {
            deactivateBridges();
            return;
        }
        bridgesActive = true;
        setButtonActive(bridgeBtn, true);
        loadBridges();
    });

    mpmsBtn?.addEventListener("click", () => {
        if (!layersEnabled()) return;
        if (mpmsActive) {
            deactivateMpms();
            return;
        }
        mpmsActive = true;
        setButtonActive(mpmsBtn, true);
        loadMpms();
    });

    // Unlike Road/Bridge/MPMS, the unselected layers are already loaded (populated by the same
    // scenario data fetch as everything else) — toggling is just a visibility flip, no fetch,
    // no zoom gate, no ratings popup. The spinner still shows briefly while the view draws the
    // (already in-memory) graphics for the first time, same visual language as the other buttons.
    let unselectedActive = false;
    const unselectedTargetLayers = [unselectedPavementLayer, unselectedBridgeLayer].filter(Boolean);
    unselectedBtn?.addEventListener("click", () => {
        unselectedActive = !unselectedActive;
        setButtonActive(unselectedBtn, unselectedActive);
        unselectedTargetLayers.forEach((layer) => { layer.visible = unselectedActive; });
        raiseLayers();

        if (unselectedActive) {
            void watchLayersLoading(unselectedTargetLayers, "unselected", unselectedBtn);
        } else {
            clearLoadingWatch("unselected");
            setButtonLoading(unselectedBtn, false);
            onLayerUnloaded?.("unselected");
        }
    });

    roadBtn?.addEventListener("mouseenter", () => {
        if (!roadsActive) return;
        clearRatingsCloseTimer();
        keepToolbarOpen();
        roadPopup.setEnabled(layersEnabled());
        showRatingsPopup(roadBtn, roadPopup, [bridgePopup, mpmsPopup]);
    });
    roadBtn?.addEventListener("mouseleave", (event) => {
        if (isHoverZoneElement(event.relatedTarget)) return;
        scheduleHideRatings(roadPopup);
    });

    bridgeBtn?.addEventListener("mouseenter", () => {
        if (!bridgesActive) return;
        clearRatingsCloseTimer();
        keepToolbarOpen();
        bridgePopup.setEnabled(layersEnabled());
        showRatingsPopup(bridgeBtn, bridgePopup, [roadPopup, mpmsPopup]);
    });
    bridgeBtn?.addEventListener("mouseleave", (event) => {
        if (isHoverZoneElement(event.relatedTarget)) return;
        scheduleHideRatings(bridgePopup);
    });

    mpmsBtn?.addEventListener("mouseenter", () => {
        if (!mpmsActive) return;
        clearRatingsCloseTimer();
        keepToolbarOpen();
        mpmsPopup.setEnabled(layersEnabled());
        showRatingsPopup(mpmsBtn, mpmsPopup, [roadPopup, bridgePopup]);
    });
    mpmsBtn?.addEventListener("mouseleave", (event) => {
        if (isHoverZoneElement(event.relatedTarget)) return;
        scheduleHideRatings(mpmsPopup);
    });

    [roadPopup, bridgePopup, mpmsPopup].forEach((popup) => {
        popup.root?.addEventListener("mouseenter", () => {
            clearRatingsCloseTimer();
            keepToolbarOpen();
        });
        popup.root?.addEventListener("mouseleave", (event) => {
            if (isHoverZoneElement(event.relatedTarget)) return;
            scheduleHideRatings(popup);
        });
    });

    roadPopup.onToggle((rating) => {
        if (!roadsActive) {
            roadsActive = true;
            setButtonActive(roadBtn, true);
            loadRoads();
        }
        selectedRoadRatings = toggleRatingSelection(selectedRoadRatings, rating);
        roadPopup.setSelected(selectedRoadRatings);
        applyRoadRatings(
            pavementBaseLayer,
            selectedRoadRatings,
            scenarioPavementLayer,
            map,
            scenarioLayers,
        );
    });

    bridgePopup.onToggle((rating) => {
        if (!bridgesActive) {
            bridgesActive = true;
            setButtonActive(bridgeBtn, true);
            loadBridges();
        }
        selectedBridgeRatings = toggleRatingSelection(selectedBridgeRatings, rating);
        bridgePopup.setSelected(selectedBridgeRatings);
        applyBridgeRatings(
            bridgesBaseLayer,
            selectedBridgeRatings,
            scenarioBridgeLayer,
            map,
            scenarioLayers,
        );
    });

    mpmsPopup.onToggle((rating) => {
        if (!mpmsActive) {
            mpmsActive = true;
            setButtonActive(mpmsBtn, true);
            loadMpms();
        }
        selectedMpmsRatings = toggleRatingSelection(selectedMpmsRatings, rating);
        mpmsPopup.setSelected(selectedMpmsRatings);
        applyMPMSRatings(
            mpmsLayer,
            selectedMpmsRatings,
            scenarioPavementLayer,
            map,
            scenarioLayers,
        );
    });

    layersBtn?.addEventListener("click", () => {
        panelPinned = !panelPinned;
        if (panelPinned) {
            openPanel();
            return;
        }
        if (!isToolbarInteractionHovered()) {
            scheduleClosePanel();
        }
    });

    toolbarZone?.addEventListener("mouseenter", openPanel);
    toolbar?.addEventListener("mouseenter", openPanel);
    panel?.addEventListener("mouseenter", openPanel);
    toolbarZone?.addEventListener("mouseleave", (event) => {
        if (panelPinned) return;
        if (isHoverZoneElement(event.relatedTarget)) return;
        scheduleClosePanel();
    });

    view.watch("zoom", (newZoom) => {
        zoomLevel = newZoom;
        updateZoomGate();
        if (!layersEnabled()) {
            deactivateAllInventoryLayers();
        }
    });

    map.layers.on("change", () => raiseLayers());

    updateZoomGate();
    updateBasemapButton();

    return {
        bringScenarioLayersToFront: () => raiseLayers(),
    };
}

export { ASSET_BASE };
