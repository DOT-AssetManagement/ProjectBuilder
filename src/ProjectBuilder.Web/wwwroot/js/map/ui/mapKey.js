import {
    roadT,
    bridgeT,
} from "./mapKeyIcons.js";
import { conditionColors, symbols } from "../symbolConfig.js?v=8";

const RATE_DOTS = [conditionColors.good, conditionColors.fair, conditionColors.poor];
const MPMS_DOTS = [symbols.mpms.statuses.ACT, symbols.mpms.statuses.PROG];
const MPMS_STATUS_ICON = "/images/map/icons/MPMS%205.svg";

function mpmsStatusGraphic() {
    return `<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 24 24" width="29" height="29" aria-hidden="true"><rect x="2" y="4.6" width="20" height="18.8" rx="4.4" fill="#06A35C"/><image href="${MPMS_STATUS_ICON}" xlink:href="${MPMS_STATUS_ICON}" x="4.5" y="7.1" width="15" height="14" preserveAspectRatio="xMidYMid meet"/></svg>`;
}

function markerHtml(url) {
    return `<img src="${url}" alt="" />`;
}

function symbolHtml(entry, selected = false) {
    if (entry.kind === "icon") return markerHtml(selected ? entry.selectedIcon : entry.icon);
    return lineIcon(entry.color, selected ? entry.halo : null);
}

function tileIcon(gfx, dots) {
    return `<span class="gis-mapkey-icon gis-mapkey-icon--tile"><span class="gis-mapkey-dots">${
        dots.map((c) => `<i style="background:${c}"></i>`).join("")
    }</span>${gfx}</span>`;
}

function lineIcon(stroke, halo = null) {
    const marks = halo
        ? `<line x1="1" y1="16" x2="24" y2="16" stroke="${halo}" stroke-width="11" transform="rotate(150 16 16)" stroke-linecap="square"/>
           <line x1="-1" y1="16" x2="26" y2="16" stroke="${stroke}" stroke-width="7" transform="rotate(150 16 16)" stroke-linecap="square"/>`
        : `<line x1="0" y1="16" x2="26" y2="16" stroke="${stroke}" stroke-width="7" transform="rotate(150 16 16)" stroke-linecap="square"/>`;
    return `<span class="gis-mapkey-icon gis-mapkey-icon--line"><svg viewBox="0 0 35 28" aria-hidden="true">${marks}</svg></span>`;
}

function keyRow(id, entry, extra = {}) {
    const selected = Boolean(extra.selected);
    const html = symbolHtml(entry, selected);
    return {
        id,
        cat: extra.cat,
        of: extra.of,
        countKey: extra.countKey,
        layerKeys: extra.layerKeys,
        label: selected ? entry.selectedLabel : entry.label,
        desc: selected ? entry.selectedDesc : entry.desc,
        color: entry.swatch || entry.color,
        iconHtml: html,
    };
}

const KEY_ROWS = [
    keyRow("proj", symbols.scenarioBridge, {
        cat: "proj",
        countKey: "bridge-proj",
        layerKeys: ["scenarioBridge", "scenarioPavement"],
    }),
    keyRow("proj-sel", symbols.scenarioBridge, { of: "proj", countKey: "proj-sel", selected: true }),
    keyRow("pave", symbols.scenarioPavement, { cat: "pave", layerKeys: ["scenarioPavement"] }),
    keyRow("pave-sel", symbols.scenarioPavement, { of: "pave", selected: true }),
    keyRow("ptreat", symbols.treatment, { cat: "ptreat", layerKeys: ["treatmentPins"] }),
    keyRow("mpms", symbols.mpms, { cat: "mpms", layerKeys: ["mpms"] }),
    keyRow("mpms-sel", symbols.mpms, { of: "mpms", selected: true }),
    {
        id: "mpms-status",
        label: "MPMS Statuses",
        sub: "MPMS",
        desc: "MPMS project statuses by colour.",
        color: "#06A35C",
        iconHtml: tileIcon(mpmsStatusGraphic(), MPMS_DOTS),
    },
    keyRow("bridge", symbols.inventoryBridge, { cat: "bridge", layerKeys: ["bridgesBase"] }),
    keyRow("bridge-sel", symbols.inventoryBridge, { of: "bridge", selected: true }),
    keyRow("pinv", symbols.inventoryPavement, { cat: "pinv", layerKeys: ["pavementBase"] }),
    keyRow("pinv-sel", symbols.inventoryPavement, { of: "pinv", selected: true }),
    keyRow("unfunded", symbols.unfunded, { cat: "unfunded" }),
    keyRow("unfunded-sel", symbols.unfunded, { of: "unfunded", selected: true }),
    keyRow("unfunded-pave", symbols.unselectedPavement, { cat: "unfunded-pave" }),
    keyRow("unfunded-pave-sel", symbols.unselectedPavement, { of: "unfunded-pave", selected: true }),
    {
        id: "road-rating",
        label: "Highlighted Condition Ratings",
        sub: "Roads",
        desc: "Road condition ratings: good, fair and poor.",
        color: "#4f7fab",
        iconHtml: tileIcon(roadT, RATE_DOTS),
    },
    {
        id: "bridge-rating",
        label: "Highlighted Condition Ratings",
        sub: "Bridge",
        desc: "Bridge condition ratings: good, fair and poor.",
        color: "#4f7fab",
        iconHtml: tileIcon(bridgeT, RATE_DOTS),
    },
];

function luminance(hex) {
    const n = parseInt(String(hex).replace("#", ""), 16);
    if (!Number.isFinite(n)) return 0;
    const r = ((n >> 16) & 255) / 255;
    const g = ((n >> 8) & 255) / 255;
    const b = (n & 255) / 255;
    const f = (v) => (v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4);
    return 0.2126 * f(r) + 0.7152 * f(g) + 0.0722 * f(b);
}

function iconCell(row) {
    const html = row.iconHtml;
    if (html.includes("gis-mapkey-icon")) return html;
    return `<span class="gis-mapkey-icon">${html}</span>`;
}

function labelHtml(row) {
    if (row.sub) {
        return `<span class="gis-mapkey-label gis-mapkey-rating-inline"><strong>${row.label}</strong><span>${row.sub}</span></span>`;
    }
    return `<span class="gis-mapkey-label">${row.label}</span>`;
}

/**
 * Inline Map Key legend (inside left filter panel) + bottom description band.
 * Icons and line colors come from symbolConfig.js.
 */
export function createMapKey(root, {
    bandRoot = null,
    getCounts = null,
    onClose = null,
} = {}) {
    if (!root) {
        return {
            open() {},
            close() {},
            toggle() {},
            isOpen: () => false,
            refreshCounts() {},
            showFor() {},
            showMessage() {},
            hideMessage() {},
            hideBand() {},
        };
    }

    const page = document.querySelector(".map-page");
    const band = bandRoot ?? document.getElementById("map-key-band");
    const bandText = band?.querySelector("#map-key-band-text") ?? band?.querySelector(".gis-key-band-text");
    const bandClose = band?.querySelector("#map-key-band-close") ?? band?.querySelector(".gis-key-band-close");

    let pickedId = null;
    let noticeId = null;
    let bandToken = 0;

    function countFor(row) {
        const key = row.countKey || row.cat;
        if (!key || typeof getCounts !== "function") return null;
        try {
            const counts = getCounts() ?? {};
            const value = counts[key];
            return Number.isFinite(value) ? value : null;
        } catch {
            return null;
        }
    }

    function render() {
        root.innerHTML = `<div class="gis-mapkey-body">${KEY_ROWS.map((row) => {
            const count = countFor(row);
            const cnt = (row.cat || row.countKey)
                ? `<span class="gis-mapkey-cnt">${count == null ? "–" : count}</span>`
                : "";
            const picked = pickedId === row.id ? " is-picked" : "";
            return `<div class="gis-mapkey-row${picked}" role="button" tabindex="0"
                data-id="${row.id}" data-cat="${row.cat ?? ""}" style="--c:${row.color}" title="${row.desc}">
                ${iconCell(row)}
                ${labelHtml(row)}
                ${cnt}
            </div>`;
        }).join("")}</div>`;
    }

    function writeNotice(row) {
        if (!bandText || !row) return;
        const n = countFor(row);
        bandText.innerHTML = row.desc
            + ((row.cat || row.countKey) && n != null ? `<small>${n} on map</small>` : "");
    }

    function openBand(color) {
        band.hidden = false;
        band.style.setProperty("--bc", color);
        band.classList.toggle("is-ink", luminance(color) > 0.45);
        band.classList.add("is-show");
        page?.classList.add("gis-key-banded");
    }

    function showBand(row, text, color) {
        if (!band || !row) return;
        bandToken += 1;
        noticeId = text ? null : row.id;
        openBand(color || row.color);
        if (text) {
            if (bandText) bandText.textContent = text;
        } else {
            writeNotice(row);
        }
    }

    function showMessage({ color = "#54687a", text = "" } = {}) {
        if (!band) return;
        bandToken += 1;
        noticeId = null;
        openBand(color);
        if (bandText) bandText.textContent = text;
    }

    function hideMessage(text) {
        if (!band || band.hidden || bandText?.textContent !== text) return;
        hideBand();
    }

    function hideBand() {
        if (!band) return;
        const token = ++bandToken;
        noticeId = null;
        band.classList.remove("is-show");
        page?.classList.remove("gis-key-banded");
        setTimeout(() => {
            if (token !== bandToken) return;
            if (!band.classList.contains("is-show")) band.hidden = true;
        }, 360);
    }

    function showFor(rowId, text, color) {
        const row = KEY_ROWS.find((entry) => entry.id === rowId);
        if (row) showBand(row, text, color);
    }

    function pickRow(id) {
        pickedId = pickedId === id ? null : id;
        render();
    }

    function open() {
        root.hidden = false;
    }

    function close() {
        root.hidden = true;
        pickedId = null;
        render();
        onClose?.();
    }

    function toggle() {
        if (root.hidden) open();
        else close();
    }

    root.addEventListener("click", (event) => {
        const rowEl = event.target.closest(".gis-mapkey-row[data-id]");
        if (!rowEl) return;
        pickRow(rowEl.dataset.id);
    });

    root.addEventListener("keydown", (event) => {
        const rowEl = event.target.closest(".gis-mapkey-row[data-id]");
        if (!rowEl) return;
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            pickRow(rowEl.dataset.id);
        }
    });

    bandClose?.addEventListener("click", () => hideBand());

    root.hidden = true;
    render();

    return {
        open,
        close,
        toggle,
        isOpen: () => !root.hidden,
        refreshCounts() {
            render();
            if (!noticeId || !band || band.hidden) return;
            const row = KEY_ROWS.find((entry) => entry.id === noticeId);
            if (row) writeNotice(row);
        },
        showFor,
        showMessage,
        hideMessage,
        hideBand,
    };
}
