export function adhocInventoryItemKey(item) {
    if (!item) return "";
    const type = String(item.type ?? "").toLowerCase();
    if (type === "bridge") return `bridge:${item.BRKEY ?? item.BRIDGE_ID ?? ""}`;
    if (type === "pavement" || type === "road") return `pavement:${item.OBJECTID ?? item.GPID ?? ""}`;
    if (type === "mpms") return `mpms:${item.OBJECTID ?? item.PROJECT_ID ?? ""}`;
    return `${type}:${item.OBJECTID ?? item.BRIDGE_ID ?? item.GPID ?? ""}`;
}

export function createAdhocSession() {
    let items = [];
    let carouselIndex = 0;
    let mode = "none"; // none | scenario-project
    let selectedProjectId = null;
    let listeners = [];

    function notify() {
        listeners.forEach((listener) => listener({
            items: items.slice(),
            carouselIndex,
            mode,
            selectedProjectId,
        }));
    }

    function beginScenarioProject(projectId) {
        const id = projectId == null ? null : Number(projectId);
        // Always reset adhoc list when starting/switching a project session.
        items = [];
        carouselIndex = 0;
        selectedProjectId = id;
        mode = id == null ? "none" : "scenario-project";
        notify();
    }

    function clear() {
        items = [];
        carouselIndex = 0;
        mode = "none";
        selectedProjectId = null;
        notify();
    }

    function shouldAppend() {
        return mode === "scenario-project" && selectedProjectId != null;
    }

    function append(item) {
        if (!item) return false;
        const key = adhocInventoryItemKey(item);
        const existing = items.findIndex((entry) => adhocInventoryItemKey(entry) === key);
        if (existing >= 0) {
            carouselIndex = existing;
            notify();
            return false;
        }
        items = [...items, item];
        carouselIndex = items.length - 1;
        notify();
        return true;
    }

    function setCarouselIndex(index) {
        if (!items.length) {
            carouselIndex = 0;
            notify();
            return;
        }
        carouselIndex = Math.max(0, Math.min(items.length - 1, index));
        notify();
    }

    function currentItem() {
        return items[carouselIndex] ?? null;
    }

    return {
        beginScenarioProject,
        clear,
        shouldAppend,
        append,
        setCarouselIndex,
        currentItem,
        getItems: () => items.slice(),
        getCarouselIndex: () => carouselIndex,
        getMode: () => mode,
        getSelectedProjectId: () => selectedProjectId,
        subscribe(listener) {
            listeners.push(listener);
            return () => {
                listeners = listeners.filter((entry) => entry !== listener);
            };
        },
    };
}
