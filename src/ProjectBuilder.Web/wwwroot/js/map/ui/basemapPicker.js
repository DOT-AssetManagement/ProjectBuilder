/**
 * Google Maps–style basemap picker that expands upward from a trigger button.
 * Uses fixed positioning so parent overflow cannot clip the panel.
 */
export function createBasemapPicker(root, {
    catalog = [],
    getActiveId,
    setBasemap,
    onOpenChange,
    trigger = null,
} = {}) {
    if (!root) {
        console.warn("[basemapPicker] root element missing");
        return {
            open() {},
            close() {},
            toggle() {},
            isOpen: () => false,
            refresh() {},
            destroy() {},
        };
    }

    let open = false;
    let ignoreOutsideUntil = 0;

    function render() {
        const activeId = getActiveId?.() ?? catalog[0]?.id;
        const cards = (catalog.length ? catalog : [{ id: "gray", title: "Light Gray" }]).map((entry) => {
            const selected = entry.id === activeId;
            const thumb = entry.thumbnail
                ? `<img src="${entry.thumbnail}" alt="" loading="lazy" />`
                : `<span class="gis-basemap-card-fallback" aria-hidden="true"></span>`;
            return `
                <button type="button"
                    class="gis-basemap-card${selected ? " is-selected" : ""}"
                    data-basemap-id="${entry.id}"
                    aria-pressed="${selected ? "true" : "false"}"
                    title="${entry.title}">
                    <span class="gis-basemap-card-thumb">${thumb}</span>
                    <span class="gis-basemap-card-label">${entry.title}</span>
                </button>`;
        }).join("");

        root.innerHTML = `
            <div class="gis-basemap-picker-panel" role="dialog" aria-label="Map type">
                <div class="gis-basemap-picker-header">
                    <h5>Map type</h5>
                    <button type="button" class="gis-basemap-picker-close" aria-label="Close map type picker">&times;</button>
                </div>
                <div class="gis-basemap-picker-grid">
                    ${cards}
                </div>
            </div>`;
    }

    function positionNearTrigger() {
        const anchor = trigger;
        if (!anchor) return;

        const rect = anchor.getBoundingClientRect();
        const width = Math.min(360, window.innerWidth - 24);
        let left = rect.left;
        if (left + width > window.innerWidth - 12) {
            left = Math.max(12, window.innerWidth - width - 12);
        }

        root.style.position = "fixed";
        root.style.left = `${left}px`;
        root.style.width = `${width}px`;
        root.style.bottom = `${Math.max(12, window.innerHeight - rect.top + 10)}px`;
        root.style.top = "auto";
        root.style.right = "auto";
        root.style.zIndex = "10050";
    }

    function setOpen(next) {
        open = next;
        if (open) {
            ignoreOutsideUntil = Date.now() + 350;
            render();
            positionNearTrigger();
            root.hidden = false;
            root.setAttribute("aria-hidden", "false");
            // Force reflow, then animate open.
            void root.offsetWidth;
            requestAnimationFrame(() => {
                root.classList.add("is-open");
            });
        } else {
            root.classList.remove("is-open");
            root.setAttribute("aria-hidden", "true");
            window.setTimeout(() => {
                if (!open) root.hidden = true;
            }, 220);
        }
        onOpenChange?.(open);
    }

    function openPicker() {
        setOpen(true);
    }

    function closePicker() {
        setOpen(false);
    }

    function toggle() {
        setOpen(!open);
    }

    root.addEventListener("click", async (event) => {
        event.stopPropagation();
        if (event.target.closest(".gis-basemap-picker-close")) {
            closePicker();
            return;
        }

        const card = event.target.closest("[data-basemap-id]");
        if (!card) return;

        const id = card.dataset.basemapId;
        if (!id) return;

        try {
            await setBasemap?.(id);
            render();
            window.setTimeout(() => closePicker(), 180);
        } catch (error) {
            console.warn("Failed to switch basemap:", error);
        }
    });

    function isTriggerEvent(event) {
        const t = event.target;
        if (!t || typeof t.closest !== "function") return false;
        if (trigger && (trigger === t || trigger.contains(t))) return true;
        return Boolean(t.closest("#map-toolbar-basemap, #map-toolbar-penndot"));
    }

    function onDocumentPointerDown(event) {
        if (!open) return;
        if (Date.now() < ignoreOutsideUntil) return;
        if (root.contains(event.target)) return;
        if (isTriggerEvent(event)) return;
        closePicker();
    }

    function onKeyDown(event) {
        if (!open) return;
        if (event.key === "Escape") closePicker();
    }

    function onViewportChange() {
        if (open) positionNearTrigger();
    }

    document.addEventListener("pointerdown", onDocumentPointerDown, true);
    document.addEventListener("keydown", onKeyDown);
    window.addEventListener("resize", onViewportChange);
    window.addEventListener("scroll", onViewportChange, true);

    root.hidden = true;
    root.classList.add("gis-basemap-picker");
    root.setAttribute("aria-hidden", "true");
    // Prefer body so map/toolbar overflow never clips the panel.
    if (root.parentElement !== document.body) {
        document.body.appendChild(root);
    }
    render();

    return {
        open: openPicker,
        close: closePicker,
        toggle,
        isOpen: () => open,
        refresh: () => {
            render();
            if (open) positionNearTrigger();
        },
        destroy() {
            document.removeEventListener("pointerdown", onDocumentPointerDown, true);
            document.removeEventListener("keydown", onKeyDown);
            window.removeEventListener("resize", onViewportChange);
            window.removeEventListener("scroll", onViewportChange, true);
            root.innerHTML = "";
            root.remove();
        },
    };
}
