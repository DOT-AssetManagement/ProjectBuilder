import {
    districtOf,
    countyNameOf,
    routeOf,
    toSectionOf,
    yearOf,
} from "../../map/treatments/helpers.js";

// DSS work candidates key AssetType directly ("P"/"B") and carry treatmentName —
// unlike PB treatments, whose helpers infer these — so these two facets read the
// DSS fields directly instead of using treatmentType()/treatmentLabel().
function assetTypeOf(wc) {
    return (wc.assetType ?? wc.AssetType ?? "").toString().trim().toUpperCase();
}

function treatmentNameOf(wc) {
    return wc.treatmentName ?? wc.TreatmentName ?? wc.treatmentType ?? wc.TreatmentType ?? "";
}

// Each facet maps to a graphic attribute baked onto the DSS layers (see
// dssmap/loader.js + map/arcgis/graphics.js). `quote` marks string fields for the
// definitionExpression IN(...) clause; the `get` accessor derives distinct values
// from the in-memory workCandidates array.
const FILTERS = [
    { key: "Year", label: "Year", field: "Year", quote: false, get: yearOf },
    { key: "AssetType", label: "Asset Type", field: "AssetType", quote: true, get: assetTypeOf, display: (v) => ({ P: "Pavement", B: "Bridge" }[v] ?? v) },
    { key: "District", label: "District", field: "District", quote: false, get: districtOf },
    { key: "County", label: "County", field: "County", quote: true, get: countyNameOf },
    { key: "Route", label: "Route", field: "Route", quote: false, get: routeOf },
    { key: "Section", label: "Section", field: "Section", quote: false, get: toSectionOf },
    { key: "Treatment", label: "Treatment(s)", field: "Treatment", quote: true, get: treatmentNameOf },
];

function distinctValues(items, getter) {
    const seen = new Map();
    for (const item of items) {
        const raw = getter(item);
        if (raw === null || raw === undefined || raw === "") continue;
        const value = String(raw);
        if (!seen.has(value)) seen.set(value, raw);
    }
    return [...seen.values()].sort((a, b) => {
        const na = Number(a);
        const nb = Number(b);
        if (!Number.isNaN(na) && !Number.isNaN(nb)) return na - nb;
        return String(a).localeCompare(String(b));
    });
}

export function createDssFilterPanel(sectionsRoot, clearButton, { getMapData, pavementLayer, bridgeLayer }) {
    const selected = {};
    FILTERS.forEach((f) => { selected[f.key] = new Set(); });

    function workCandidates() {
        const data = getMapData();
        return data.workCandidates ?? data.WorkCandidates ?? [];
    }

    function buildClause(filter) {
        const values = [...selected[filter.key]];
        if (values.length === 0) return null;
        const list = filter.quote
            ? values.map((v) => `'${String(v).replace(/'/g, "''")}'`).join(",")
            : values.join(",");
        return `${filter.field} IN (${list})`;
    }

    function applyFilters() {
        const clauses = FILTERS.map(buildClause).filter(Boolean);
        const expression = clauses.length ? clauses.join(" AND ") : null;
        pavementLayer.definitionExpression = expression;
        bridgeLayer.definitionExpression = expression;
    }

    function render() {
        const items = workCandidates();

        sectionsRoot.innerHTML = FILTERS.map((filter) => {
            const values = distinctValues(items, filter.get);
            const options = values.map((v) => {
                const label = filter.display ? filter.display(v) : v;
                const checked = selected[filter.key].has(String(v)) ? "checked" : "";
                return `<label class="gis-filter-option">
                    <input type="checkbox" data-filter="${filter.key}" value="${v}" ${checked} />
                    <span>${label}</span>
                </label>`;
            }).join("");

            return `
                <div class="gis-filter-section">
                    <button type="button" class="gis-filter-section-head" data-section="${filter.key}" aria-expanded="false">
                        <span>${filter.label}</span>
                        <span class="gis-filter-caret" aria-hidden="true">+</span>
                    </button>
                    <div class="gis-filter-options" hidden>${options || `<p class="gis-filter-empty">No values</p>`}</div>
                </div>`;
        }).join("");
    }

    sectionsRoot.addEventListener("click", (event) => {
        const head = event.target.closest(".gis-filter-section-head");
        if (!head) return;
        const section = head.closest(".gis-filter-section");
        const options = head.nextElementSibling;
        const nowOpen = options.hidden;
        options.hidden = !nowOpen;
        head.setAttribute("aria-expanded", String(nowOpen));
        section?.classList.toggle("is-open", nowOpen);
    });

    sectionsRoot.addEventListener("change", (event) => {
        const checkbox = event.target.closest("input[data-filter]");
        if (!checkbox) return;
        const set = selected[checkbox.dataset.filter];
        if (checkbox.checked) set.add(checkbox.value);
        else set.delete(checkbox.value);
        applyFilters();
    });

    clearButton?.addEventListener("click", () => {
        FILTERS.forEach((f) => selected[f.key].clear());
        pavementLayer.definitionExpression = null;
        bridgeLayer.definitionExpression = null;
        render();
    });

    return { render };
}
