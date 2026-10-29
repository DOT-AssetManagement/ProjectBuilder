import { projectIdOf, formatCurrency } from "../../map/treatments/helpers.js";
import { escapeHtml } from "../../map/utils/html.js";

const ASSET_LABEL = { P: "Pavement", B: "Bridge" };

const COLUMNS = [
    "Asset Type", "Treatment", "District", "County", "Route",
    "Section", "BRKEY", "Year", "Cost", "Benefit", "B/C Ratio",
];

function dash(value) {
    return value === null || value === undefined || value === "" ? "–" : escapeHtml(value);
}

function get(wc, camel, pascal) {
    return wc?.[camel] ?? wc?.[pascal];
}

function assetLabel(wc) {
    const type = (get(wc, "assetType", "AssetType") ?? "").toString().trim().toUpperCase();
    return ASSET_LABEL[type] ?? dash(type);
}

function sectionRange(wc) {
    const from = get(wc, "fromSection", "FromSection");
    const to = get(wc, "toSection", "ToSection");
    if (from == null && to == null) return "–";
    if (from == null) return escapeHtml(to);
    if (to == null) return escapeHtml(from);
    return `${escapeHtml(from)} – ${escapeHtml(to)}`;
}

function ratio(value) {
    const num = Number(value);
    return Number.isFinite(num) ? num.toFixed(2) : "–";
}

/**
 * Bottom work-candidates bar for DssMap — a read-only grid of the selected
 * project's work candidates, drawn from the in-memory scenario data (no fetch,
 * no editing).
 */
export function createDssCandidatesBar(root, { getMapData }) {
    root.hidden = true;
    root.innerHTML = `
        <div class="gis-ptbar-head">
            <span class="gis-ptbar-title">Work Candidates</span>
            <button type="button" class="gis-ptbar-close" id="dss-ptbar-close" aria-label="Close work candidates">&times;</button>
        </div>
        <div class="gis-ptbar-body" id="dss-ptbar-body"></div>
    `;

    const body = root.querySelector("#dss-ptbar-body");
    root.querySelector("#dss-ptbar-close").addEventListener("click", () => { root.hidden = true; });

    let currentProjectId = null;

    function isVisible() {
        return !root.hidden;
    }

    function rowsForProject() {
        if (currentProjectId == null) return [];
        const data = getMapData();
        const candidates = data.workCandidates ?? data.WorkCandidates ?? [];
        return candidates.filter((wc) => String(projectIdOf(wc)) === String(currentProjectId));
    }

    function renderBody() {
        if (currentProjectId == null) {
            body.innerHTML = `<p class="gis-empty-copy">Select a project on the map to view its work candidates.</p>`;
            return;
        }

        const rows = rowsForProject();
        const rowHtml = rows.map((wc) => `
            <tr>
                <td>${assetLabel(wc)}</td>
                <td>${dash(get(wc, "treatmentName", "TreatmentName") ?? get(wc, "treatmentType", "TreatmentType"))}</td>
                <td>${dash(get(wc, "district", "District"))}</td>
                <td>${dash(get(wc, "countyName", "CountyName"))}</td>
                <td>${dash(get(wc, "route", "Route"))}</td>
                <td>${sectionRange(wc)}</td>
                <td>${dash(get(wc, "brkey", "BRKEY"))}</td>
                <td>${dash(get(wc, "year", "Year"))}</td>
                <td>${escapeHtml(formatCurrency(get(wc, "cost", "Cost")))}</td>
                <td>${escapeHtml(formatCurrency(get(wc, "benefit", "Benefit")))}</td>
                <td>${escapeHtml(ratio(get(wc, "bcRatio", "BCRatio")))}</td>
            </tr>`).join("");

        body.innerHTML = `
            <div class="gis-edit-scroll">
                <table class="gis-edit-table gis-candidates-table">
                    <thead><tr>${COLUMNS.map((c) => `<th>${c}</th>`).join("")}</tr></thead>
                    <tbody>${rowHtml || `<tr><td colspan="${COLUMNS.length}">No work candidates for this project.</td></tr>`}</tbody>
                </table>
            </div>
        `;
    }

    return {
        // Preload a project's candidates without revealing the bar.
        setProject(projectId) {
            currentProjectId = projectId;
            if (isVisible()) renderBody();
        },
        // Reveal the bar for the given project ("Show work candidates").
        expand(projectId) {
            if (projectId != null) currentProjectId = projectId;
            if (currentProjectId == null) return;
            root.hidden = false;
            renderBody();
        },
        close() {
            root.hidden = true;
        },
        refresh() {
            if (isVisible()) renderBody();
        },
    };
}
