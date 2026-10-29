import {
    treatmentType,
    treatmentLabel,
    districtOf,
    countyNameOf,
    routeOf,
    toSectionOf,
    yearOf,
    projectIdOf,
    treatmentIdOf,
    hasCommittedOf,
} from "../treatments/helpers.js?v=3";

// Each filter maps to a graphic attribute. `quote` marks string fields for the
// definitionExpression IN(...) clause. Filters are ordered top-to-bottom as a cascade:
// each filter's option list is narrowed to treatments matching every filter above it.
// `gated` filters additionally stay empty (prompting the user) until the filter directly
// above them has a selection, since e.g. Route numbers repeat across counties and an
// unfiltered list would be meaningless.
const FILTERS = [
    { key: "Year", label: "Year", field: "Year", quote: false, get: yearOf },
    { key: "AssetType", label: "Project Type(s)", field: "AssetType", quote: true, get: treatmentType, display: (v) => ({ P: "Pavement", B: "Bridge" }[v] ?? v) },
    { key: "District", label: "District", field: "District", quote: false, get: districtOf },
    { key: "County", label: "County", field: "County", quote: true, get: countyNameOf, gated: true },
    { key: "Route", label: "Route", field: "Route", quote: false, get: routeOf, gated: true },
    { key: "Section", label: "Section", field: "Section", quote: false, get: toSectionOf, gated: true },
    { key: "Treatment", label: "Treatment(s)", field: "Treatment", quote: true, get: treatmentLabel },
    { key: "HasCommitted", label: "Has Committed", field: "HasCommitted", quote: true, get: hasCommittedOf, display: (v) => ({ Y: "Committed (Y)", N: "Not Committed (N)" }[v] ?? v) },
];

function distinctValues(treatments, getter) {
    const seen = new Map();
    for (const t of treatments) {
        const raw = getter(t);
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

export function createFilterPanel(sectionsRoot, clearButton, {
    getScenarioData,
    pavementLayer,
    bridgeLayer,
    unselectedPavementLayer = null,
    unselectedBridgeLayer = null,
    onFiltersChanged,
}) {
    const selected = {};
    FILTERS.forEach((f) => { selected[f.key] = new Set(); });
    const openSections = new Set();

    // Treatments matching every filter above `index` in the cascade — used both to
    // compute that filter's own option list and to decide whether a gated filter unlocks.
    function baseTreatmentsFor(index, treatments) {
        const priorFilters = FILTERS.slice(0, index);
        return treatments.filter((t) => priorFilters.every((f) => {
            const set = selected[f.key];
            if (set.size === 0) return true;
            const raw = f.get(t);
            if (raw === null || raw === undefined || raw === "") return false;
            return set.has(String(raw));
        }));
    }

    function matchesTreatment(treatment) {
        return FILTERS.every((f) => {
            const set = selected[f.key];
            if (set.size === 0) return true;
            const raw = f.get(treatment);
            if (raw === null || raw === undefined || raw === "") return false;
            return set.has(String(raw));
        });
    }

    function getVisibleTreatmentIdsForProject(projectId) {
        const data = getScenarioData();
        const treatments = data.treatments ?? data.Treatments ?? [];
        const ids = treatments
            .filter((t) => String(projectIdOf(t)) === String(projectId) && matchesTreatment(t))
            .map((t) => Number(treatmentIdOf(t)))
            .filter((id) => Number.isFinite(id));
        return new Set(ids);
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
        if (unselectedPavementLayer) unselectedPavementLayer.definitionExpression = expression;
        if (unselectedBridgeLayer) unselectedBridgeLayer.definitionExpression = expression;
    }

    function render() {
        const data = getScenarioData();
        const treatments = data.treatments ?? data.Treatments ?? [];

        // Rebuilding innerHTML below wipes scrollTop on any open options list —
        // capture it first and restore it after, so toggling a checkbox doesn't
        // snap a long list (e.g. Route) back to the top.
        const scrollPositions = {};
        sectionsRoot.querySelectorAll(".gis-filter-options[data-options]").forEach((el) => {
            scrollPositions[el.dataset.options] = el.scrollTop;
        });

        sectionsRoot.innerHTML = FILTERS.map((filter, index) => {
            const parent = FILTERS[index - 1];
            const isGated = filter.gated && (!parent || selected[parent.key].size === 0);

            if (isGated) {
                return `
                    <div class="gis-filter-section gis-filter-section--gated">
                        <div class="gis-filter-section-head-static">
                            <span>${filter.label}</span>
                        </div>
                        <p class="gis-filter-empty">Select a ${parent.label} above first</p>
                    </div>`;
            }

            const base = baseTreatmentsFor(index, treatments);
            const values = distinctValues(base, filter.get);
            const isOpen = openSections.has(filter.key);
            const options = values.map((v) => {
                const label = filter.display ? filter.display(v) : v;
                const checked = selected[filter.key].has(String(v)) ? "checked" : "";
                return `<label class="gis-filter-option">
                    <input type="checkbox" data-filter="${filter.key}" value="${v}" ${checked} />
                    <span>${label}</span>
                </label>`;
            }).join("");

            return `
                <div class="gis-filter-section${isOpen ? " is-open" : ""}">
                    <button type="button" class="gis-filter-section-head" data-section="${filter.key}" aria-expanded="${isOpen}">
                        <span>${filter.label}</span>
                        <span class="gis-filter-caret" aria-hidden="true">+</span>
                    </button>
                    <div class="gis-filter-options" data-options="${filter.key}" ${isOpen ? "" : "hidden"}>${options || `<p class="gis-filter-empty">No values</p>`}</div>
                </div>`;
        }).join("");

        sectionsRoot.querySelectorAll(".gis-filter-options[data-options]").forEach((el) => {
            const pos = scrollPositions[el.dataset.options];
            if (pos) el.scrollTop = pos;
        });
    }

    sectionsRoot.addEventListener("click", (event) => {
        const head = event.target.closest(".gis-filter-section-head");
        if (!head) return;
        const key = head.dataset.section;
        if (openSections.has(key)) openSections.delete(key);
        else openSections.add(key);
        render();
    });

    sectionsRoot.addEventListener("change", async (event) => {
        const checkbox = event.target.closest("input[data-filter]");
        if (!checkbox) return;
        const filterKey = checkbox.dataset.filter;
        const set = selected[filterKey];
        if (checkbox.checked) set.add(checkbox.value);
        else set.delete(checkbox.value);

        // Downstream filters cascade off this one — their previous selections may no
        // longer be valid, so clear them and let the user re-pick top-to-bottom.
        const index = FILTERS.findIndex((f) => f.key === filterKey);
        FILTERS.slice(index + 1).forEach((f) => selected[f.key].clear());

        // Auto-expand the next section once it unlocks, so the user sees its narrowed
        // options immediately instead of having to click it open themselves.
        const next = FILTERS[index + 1];
        if (checkbox.checked && next) openSections.add(next.key);

        applyFilters();
        await onFiltersChanged?.();
        render();
    });

    clearButton?.addEventListener("click", async () => {
        FILTERS.forEach((f) => selected[f.key].clear());
        openSections.clear();
        pavementLayer.definitionExpression = null;
        bridgeLayer.definitionExpression = null;
        if (unselectedPavementLayer) unselectedPavementLayer.definitionExpression = null;
        if (unselectedBridgeLayer) unselectedBridgeLayer.definitionExpression = null;
        await onFiltersChanged?.();
        render();
    });

    return { render, getVisibleTreatmentIdsForProject };
}
