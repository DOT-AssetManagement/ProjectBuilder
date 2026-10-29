import {
    projectIdOf,
    formatCurrency,
    routeOf,
    fromSectionOf,
    toSectionOf,
    brkeyOf,
} from "../../map/treatments/helpers.js";
import { escapeHtml } from "../../map/utils/html.js";

function dash(value) {
    return value === null || value === undefined || value === "" ? "–" : escapeHtml(value);
}

// DSS project fields are camelCased by ABP; keep PascalCase fallbacks so the panel
// also works against a raw (unserialized) DTO shape.
function field(project, camel, pascal) {
    return project?.[camel] ?? project?.[pascal];
}

function projectNoOf(project) {
    return field(project, "projectNo", "ProjectNo");
}

function yearRange(project) {
    const min = field(project, "minYear", "MinYear");
    const max = field(project, "maxYear", "MaxYear");
    if (min == null && max == null) return "–";
    if (min == null) return String(max);
    if (max == null) return String(min);
    return min === max ? String(min) : `${min} – ${max}`;
}

function formatEfficiency(value) {
    const num = Number(value);
    return Number.isFinite(num) ? num.toFixed(2) : "–";
}

function normPart(value) {
    if (value === null || value === undefined || value === "") return "";
    const num = Number(value);
    return Number.isFinite(num) ? String(num) : String(value).trim();
}

function sameId(a, b) {
    return String(a) === String(b);
}

/**
 * Right-side project panel for DssMap: a PROJECT INFORMATION card and a
 * SCORING & ECONOMICS card (Cost / Benefit / Relative Efficiency). Read-only —
 * DSS scenario results are computed outputs with no edit/notes backend.
 *
 * PROJECT NO dropdown and header arrows match /Map: they only appear when the
 * selected pavement/bridge has another project on the same location (segment
 * or BRKEY), not for every project in the scenario.
 */
export function createDssProjectPanel(root, { getMapData, onSelectProject, onShowCandidates, onClose = null }) {
    let projectId = null;
    // Frozen at map-click so dropdown/arrow navigation stays on the overlapping
    // set for that feature instead of expanding to the whole scenario.
    let relatedIds = null;

    function projects() {
        const data = getMapData();
        return data.projects ?? data.Projects ?? [];
    }

    function workCandidates() {
        const data = getMapData();
        return data.workCandidates ?? data.WorkCandidates ?? [];
    }

    // ProjectNo is the pool's own ProjectName (e.g. "4", "108", "Test") -- not guaranteed
    // numeric, so sort naturally rather than assuming Number() parses cleanly.
    function sortedProjects() {
        return projects().slice().sort((a, b) =>
            String(projectNoOf(a) ?? "").localeCompare(String(projectNoOf(b) ?? ""), undefined, { numeric: true, sensitivity: "base" }));
    }

    function currentProject() {
        return sortedProjects().find((p) => sameId(projectIdOf(p), projectId)) ?? { projectId };
    }

    // Same Cnty/Rte/From/To tuple as Map's getProjectsByMatchingTreatmentsFromDB,
    // plus BRKEY so two projects on the same bridge still match (DSS draws one
    // graphic per BRKEY, so hitTest alone cannot see the overlap).
    function locationKeysOf(workCandidate) {
        const keys = [];
        const county = workCandidate.county ?? workCandidate.County ?? workCandidate.cnty ?? workCandidate.Cnty;
        const segmentKey = [
            normPart(county),
            normPart(routeOf(workCandidate)),
            normPart(fromSectionOf(workCandidate)),
            normPart(toSectionOf(workCandidate)),
        ].join("|");
        if (segmentKey !== "|||") keys.push(`SEG|${segmentKey}`);

        const brkey = String(brkeyOf(workCandidate) ?? "").trim();
        if (brkey) keys.push(`BRKEY|${brkey}`);
        return keys;
    }

    function locationKeysFromGraphic(attrs) {
        if (!attrs) return [];
        const keys = [];
        const brkey = String(attrs.BRKEY ?? "").trim();
        if (brkey) keys.push(`BRKEY|${brkey}`);

        const county = attrs.CountyCode ?? attrs.countyCode;
        const route = attrs.Route ?? attrs.route;
        const from = attrs.SectionFrom ?? attrs.sectionFrom;
        const to = attrs.Section ?? attrs.section;
        const segmentKey = [normPart(county), normPart(route), normPart(from), normPart(to)].join("|");
        if (segmentKey !== "|||") keys.push(`SEG|${segmentKey}`);
        return keys;
    }

    function projectsForIds(ids) {
        const idSet = new Set((ids ?? []).map((id) => String(id)));
        return sortedProjects().filter((p) => idSet.has(String(projectIdOf(p))));
    }

    // Overlaps for the clicked pavement/bridge only — not every location on the project.
    function computeRelatedIds(clickInfo) {
        const ids = new Set([String(projectId)]);
        const candidates = workCandidates();

        for (const hit of clickInfo?.hits ?? []) {
            const pid = hit.graphic?.attributes?.ProjectID;
            if (pid != null && pid !== "") ids.add(String(pid));
        }

        const keys = new Set(locationKeysFromGraphic(clickInfo?.graphic?.attributes));
        if (keys.size) {
            for (const wc of candidates) {
                if (locationKeysOf(wc).some((key) => keys.has(key))) {
                    ids.add(String(projectIdOf(wc)));
                }
            }
        }

        return [...ids];
    }

    function relatedProjects() {
        if (relatedIds?.length) return projectsForIds(relatedIds);
        return projectsForIds([projectId]);
    }

    function infoRows(project, related) {
        const district = field(project, "district", "District");
        const county = field(project, "county", "County");
        const location = (district == null && (county == null || county === ""))
            ? "–"
            : `${district == null || district === "" ? "–" : escapeHtml(district)} / ${county == null || county === "" ? "–" : escapeHtml(county)}`;

        const projectNoCell = related.length > 1
            ? `<select class="gis-info-select" id="dss-project-select">
                    ${related.map((p) => {
                        const id = projectIdOf(p);
                        const no = projectNoOf(p);
                        const selected = sameId(id, projectId) ? "selected" : "";
                        return `<option value="${escapeHtml(id)}" ${selected}>${escapeHtml(no)}</option>`;
                    }).join("")}
               </select>`
            : dash(projectNoOf(project));

        const rows = [
            ["PROJECT NO", projectNoCell],
            ["DISTRICT / COUNTY", location],
            ["DESCRIPTION", dash(field(project, "description", "Description"))],
            ["TREATMENT TYPE", dash(field(project, "treatment", "Treatment"))],
            ["YEARS", escapeHtml(yearRange(project))],
            ["PAVEMENT / BRIDGE", escapeHtml(`${field(project, "numPave", "NumPave") ?? 0} / ${field(project, "numBridge", "NumBridge") ?? 0}`)],
        ];

        return rows.map(([label, value]) => `
            <div class="gis-info-row">
                <div class="gis-info-label">${label}</div>
                <div class="gis-info-value">${value}</div>
            </div>`).join("");
    }

    function economicsRows(project) {
        const rows = [
            ["TOTAL COST", escapeHtml(formatCurrency(field(project, "cost", "Cost")))],
            ["TOTAL BENEFIT", escapeHtml(formatCurrency(field(project, "benefit", "Benefit")))],
            ["REL. EFFICIENCY", escapeHtml(formatEfficiency(field(project, "efficiency", "Efficiency")))],
        ];
        return rows.map(([label, value]) => `
            <div class="gis-info-row">
                <div class="gis-info-label">${label}</div>
                <div class="gis-info-value">${value}</div>
            </div>`).join("");
    }

    function render() {
        const project = currentProject();
        const related = relatedProjects();
        const multiProject = related.length > 1;

        root.innerHTML = `
            <button type="button" class="gis-panel-close" id="dss-panel-close" aria-label="Close panel">&times;</button>

            <div class="gis-card">
                <div class="gis-card-header">
                    <span>Project Information</span>
                    ${multiProject ? `
                        <div class="gis-header-actions">
                            <button type="button" class="gis-nav-btn" id="dss-project-prev" aria-label="Previous project">&#8249;</button>
                            <button type="button" class="gis-nav-btn" id="dss-project-next" aria-label="Next project">&#8250;</button>
                        </div>` : ""}
                </div>
                <div class="gis-info-body">${infoRows(project, related)}</div>
            </div>

            <div class="gis-card">
                <div class="gis-card-header">
                    <span>Scoring &amp; Economics</span>
                </div>
                <div class="gis-info-body">${economicsRows(project)}</div>
            </div>

            <div class="gis-action-row">
                <button type="button" class="gis-action-btn" id="dss-show-candidates">Show work candidates</button>
            </div>
        `;

        wireEvents(related);
    }

    function wireEvents(list) {
        root.querySelector("#dss-panel-close")?.addEventListener("click", () => {
            close();
            onClose?.();
        });

        root.querySelector("#dss-show-candidates")?.addEventListener("click", () => {
            onShowCandidates?.(projectId);
        });

        root.querySelector("#dss-project-select")?.addEventListener("change", (event) => {
            onSelectProject?.(event.target.value);
        });

        const step = (delta) => {
            const ids = list.map((p) => projectIdOf(p));
            const idx = ids.findIndex((id) => sameId(id, projectId));
            if (idx === -1) return;
            const nextIdx = (idx + delta + ids.length) % ids.length;
            onSelectProject?.(ids[nextIdx]);
        };
        root.querySelector("#dss-project-prev")?.addEventListener("click", () => step(-1));
        root.querySelector("#dss-project-next")?.addEventListener("click", () => step(1));
    }

    function open(id, clickInfo) {
        projectId = id;
        if (clickInfo) relatedIds = computeRelatedIds(clickInfo);
        render();
        root.hidden = false;
    }

    function refresh() {
        if (!root.hidden && projectId != null) render();
    }

    function close() {
        root.hidden = true;
        projectId = null;
        relatedIds = null;
    }

    close();

    return {
        open,
        refresh,
        close,
        isOpen: () => !root.hidden,
        getProjectId: () => projectId,
    };
}
