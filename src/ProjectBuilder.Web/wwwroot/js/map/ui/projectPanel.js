import {
    projectIdOf,
    sequenceNoOf,
    formatProjectSeq,
    treatmentType,
    treatmentLabel,
    treatmentIdOf,
    sectionLabel,
    bridgeIdOf,
    brkeyOf,
    crsIdOf,
    routeOf,
    fromSectionOf,
    toSectionOf,
    directionOf,
    districtOf,
    countyNameOf,
    interstateOf,
    totalCostOf,
    projectYearOf,
    userNotesOf,
    formatCurrency,
    statusOf,
} from "../treatments/helpers.js";
import { postJson } from "../utils/api.js";
import { escapeHtml } from "../utils/html.js";

// Display-only label for the Project Information panel's STATUS row — statusOf() itself stays
// "Selected"/"Unselected" everywhere else, since that's the value used for funding-line logic
// (moveTargetStatus, formatProjectSeq's PB-/UF- prefix, filters, etc.).
function statusLabel(status) {
    return status === "Unselected" ? "Unfunded" : "Funded";
}

const ASSET_LABEL = { P: "Pavement", B: "Bridge" };

// Route is displayed zero-padded to 4 digits everywhere else in the app.
function padRoute(value) {
    return value === null || value === undefined || value === "" ? "–" : String(value).padStart(4, "0");
}
const TREATMENT_PANELS = ["TREATMENTS", "CONDITION", "LOCATION INFORMATION"];
const TREATMENT_ICON = "/images/map/icons/key-ptreat.svg";
const TREATMENT_ICON_CYAN = "/images/map/icons/key-ptreat-selected.svg";

function firstOf(list) {
    return list && list.length ? list[0] : null;
}

// Sum of the project's treatment Total Costs (direct + indirect), so PROJECT COST always
// agrees with the Total Cost column shown in the TREATMENTS table right below it.
function projectTotalCost(treatments) {
    if (!treatments.length) return null;
    return treatments.reduce((sum, t) => sum + (Number(totalCostOf(t)) || 0), 0);
}

/**
 * Right-side project panel: PROJECT INFORMATION card + a 3-panel
 * TREATMENTS / CONDITION / LOCATION carousel, matching the legacy GisMap UI.
 */
export function createProjectPanel(root, {
    scenarioId,
    antiforgeryToken,
    getScenarioData,
    onSelectProject,
    onShowTreatments,
    onClose = null,
    adhocSession = null,
    onAdhocIndexChange = null,
}) {
    let projectId = null;
    let treatmentPanelIndex = 0;
    let unsubscribeAdhoc = null;
    let notesValue = "";
    let notesError = "";
    let notesSuccess = null;
    let isSavingNotes = false;

    function sortedProjects() {
        const data = getScenarioData();
        const projects = (data.projects ?? data.Projects ?? []).slice();
        projects.sort((a, b) => (projectIdOf(a) ?? 0) - (projectIdOf(b) ?? 0));
        return projects;
    }

    function projectTreatments() {
        const data = getScenarioData();
        const treatments = data.treatments ?? data.Treatments ?? [];
        return treatments.filter((t) => projectIdOf(t) === projectId);
    }

    function currentProject() {
        return sortedProjects().find((p) => projectIdOf(p) === projectId) ?? { projectId };
    }

    // A treatment's road-segment identity — same shape as legacy's
    // getProjectsByMatchingTreatmentsFromDB (Cnty, Rte, FromSection, ToSection tuple).
    function locationKeyOf(t) {
        const county = t.county ?? t.County ?? "";
        return `${county}|${routeOf(t)}|${fromSectionOf(t)}|${toSectionOf(t)}`;
    }

    // Legacy's PROJECT ID dropdown only offers projects that share a road segment with the
    // currently open one (multiple scenario projects can land on the same segment, e.g. a
    // pavement + a bridge project) — NOT every project in the scenario. Mirrors
    // getProjectsByMatchingTreatmentsFromDB's default (no matchAssetType) branch.
    function relatedProjects() {
        const data = getScenarioData();
        const allTreatments = data.treatments ?? data.Treatments ?? [];
        const currentTreatments = allTreatments.filter((t) => projectIdOf(t) === projectId);
        const projects = sortedProjects();

        if (!currentTreatments.length) {
            return projects.filter((p) => projectIdOf(p) === projectId);
        }

        const keys = new Set(currentTreatments.map(locationKeyOf));
        const relatedIds = new Set(
            allTreatments.filter((t) => keys.has(locationKeyOf(t))).map((t) => projectIdOf(t)),
        );
        relatedIds.add(projectId);

        return projects.filter((p) => relatedIds.has(projectIdOf(p)));
    }

    function bridgeTreatment(treatments) {
        return treatments.find((t) => treatmentType(t) === "B") ?? null;
    }

    function infoRows(project, treatments, projects) {
        const first = firstOf(treatments);
        const bridge = bridgeTreatment(treatments);

        const projectIdCell = projects.length > 1
            ? `<select class="gis-info-select" id="gis-project-select">
                    ${projects.map((p) => {
                        const id = projectIdOf(p);
                        const seq = formatProjectSeq(sequenceNoOf(p), statusOf(p)) ?? id;
                        return `<option value="${escapeHtml(id)}" ${id === projectId ? "selected" : ""}>${escapeHtml(seq)}</option>`;
                    }).join("")}
               </select>`
            : escapeHtml(formatProjectSeq(sequenceNoOf(project), statusOf(project)) ?? "–");

        const rows = [
            ["PROJECT ID", projectIdCell],
            ["ROUTE", escapeHtml(first ? padRoute(routeOf(first)) : "–")],
        ];
        if (bridge) rows.push(["BRIDGE ID", escapeHtml(bridgeIdOf(bridge) ?? "–")]);
        rows.push(["YEAR", escapeHtml(projectYearOf(project) || (first ? (first.year ?? first.Year ?? "–") : "–"))]);
        rows.push(["PROJECT COST", escapeHtml(formatCurrency(projectTotalCost(treatments)))]);
        rows.push(["STATUS", escapeHtml(statusLabel(statusOf(project)))]);

        return rows.map(([label, value]) => `
            <div class="gis-info-row">
                <div class="gis-info-label">${label}</div>
                <div class="gis-info-value">${value}</div>
            </div>`).join("");
    }

    function treatmentsPanelBody(treatments) {
        const rows = treatments.map((t) => {
            const type = treatmentType(t);
            const id = type === "B" ? brkeyOf(t) : crsIdOf(t);
            const treatmentId = treatmentIdOf(t);
            return `
                <tr data-treatment-id="${escapeHtml(treatmentId)}">
                    <td>${escapeHtml(ASSET_LABEL[type] ?? type)}</td>
                    <td>${escapeHtml(id || "–")}</td>
                    <td>${escapeHtml(sectionLabel(t) || "–")}</td>
                    <td>${escapeHtml(treatmentLabel(t))}</td>
                    <td>${escapeHtml(formatCurrency(totalCostOf(t)))}</td>
                </tr>`;
        }).join("");

        return `
            <div class="gis-table-frame">
                <div class="gis-table-scroll">
                    <table class="gis-table">
                        <thead><tr><th>Type</th><th>ID</th><th>Section</th><th>Treatment</th><th>Total Cost</th></tr></thead>
                        <tbody>${rows || `<tr><td colspan="5">No treatments.</td></tr>`}</tbody>
                    </table>
                </div>
            </div>`;
    }

    // A project can carry several treatments (e.g. multiple bridge components or pavement
    // segments on the same section) — matches legacy, which tables one row per treatment
    // here rather than collapsing to a single representative row.
    function locationPanelBody(treatments) {
        if (!treatments.length) return `<p class="gis-empty-copy">Location Data Not Available</p>`;

        const rows = treatments.map((t) => {
            const interstate = interstateOf(t);
            const cells = [
                sectionLabel(t) || "–",
                districtOf(t) || "–",
                countyNameOf(t) || "–",
                padRoute(routeOf(t)),
                interstate == null ? "–" : (interstate ? "Yes" : "No"),
                directionOf(t) === "" ? "–" : directionOf(t),
            ];
            return `<tr>${cells.map((v) => `<td>${escapeHtml(v)}</td>`).join("")}</tr>`;
        }).join("");

        return `
            <div class="gis-table-frame">
                <div class="gis-table-scroll">
                    <table class="gis-table gis-table--wide">
                        <thead><tr>
                            <th>Section</th><th>District</th><th>County</th><th>Route</th><th>Interstate</th><th>Direction</th>
                        </tr></thead>
                        <tbody>${rows}</tbody>
                    </table>
                </div>
            </div>`;
    }

    const BRIDGE_CONDITION_LABELS = { G: "GOOD", F: "FAIR", P: "POOR", N: "N/A" };

    function bridgeConditionLabel(code) {
        if (code == null || code === "") return null;
        return BRIDGE_CONDITION_LABELS[String(code).trim().toUpperCase()] ?? code;
    }

    // Condition-rating fields attached onto each treatment by arcgis/graphics.js as a side
    // effect of the scenario's geometry load (see PAVEMENT_OUT_FIELDS/BRIDGE_OUT_FIELDS) —
    // one table row per treatment, same "many treatments per project" shape as legacy.
    function conditionRowOf(t) {
        const condition = t?.condition;
        const isBridge = treatmentType(t) === "B";
        const rating = isBridge
            ? bridgeConditionLabel(condition?.condition)
            : (condition?.opiRating ?? condition?.iriRating ?? null);

        return [
            sectionLabel(t) || "–",
            rating ?? "–",
            isBridge ? (condition?.deckRating ?? "–") : "–",
            isBridge ? (condition?.superstructureRating ?? "–") : "–",
            isBridge ? (condition?.substructureRating ?? "–") : "–",
            isBridge ? (condition?.culvertRating ?? "–") : "–",
            !isBridge ? (condition?.iriRating ?? "–") : "–",
            !isBridge ? (condition?.opiRating ?? "–") : "–",
        ];
    }

    function conditionPanelBody(treatments) {
        if (!treatments.length || !treatments.some((t) => t.condition)) {
            return `<p class="gis-empty-copy">Condition Data Not Available</p>`;
        }

        const rows = treatments.map((t) => `<tr>${conditionRowOf(t).map((v) => `<td>${escapeHtml(v)}</td>`).join("")}</tr>`).join("");

        return `
            <div class="gis-table-frame">
                <div class="gis-table-scroll">
                    <table class="gis-table gis-table--wide">
                        <thead><tr>
                            <th>Section</th><th>Condition</th><th>Deck Rating</th><th>Superstructure Rating</th>
                            <th>Substructure Rating</th><th>Culvert Rating</th><th>IRI Rating</th><th>OPI Rating</th>
                        </tr></thead>
                        <tbody>${rows}</tbody>
                    </table>
                </div>
            </div>`;
    }

    function adhocRows(item) {
        if (!item) return "";
        const type = String(item.type ?? "").toLowerCase();
        const pairs = type === "bridge"
            ? [
                ["CONDITION", item.CONDITION],
                ["BRKEY", item.BRKEY],
                ["FEAT INT", item.FEATINT],
                ["FACILITY", item.FACILITY],
                ["YEAR BUILT", item.YEARBUILT],
                ["BRIDGE LENGTH", item.BRIDGE_LENGTH],
            ]
            : type === "mpms"
                ? [
                    ["PROJECT ID", item.PROJECT_ID],
                    ["PLANNING PARTNER", item.PLANNING_PARTNER_SHORT_NAME],
                    ["PROJECT TITLE", item.PROJECT_TITLE],
                    ["PROJECT STATUS", item.PROJECT_STATUS],
                    ["STIP FLAG", item.STIP_FLAG],
                    ["TYP FLAG", item.TYP_FLAG],
                    ["LET DATE", item.LET_DATE_DISPLAY],
                ]
                : [
                    ["DISTRICT", item.DISTRICT_NO],
                    ["COUNTY CODE", item.CTY_CODE],
                    ["ROUTE", item.ST_RT_NO],
                    ["IRI RATING", item.IRI_RATING_TEXT],
                    ["IRI YEAR", item.IRI_YEAR],
                    ["OVERALL PAVE IDX", item.OVERALL_PVMNT_IDX],
                    ["ROUGH INDEX", item.ROUGH_INDX],
                    ["OPI RATING", item.OPI_RATING_TEXT],
                    ["OPI YEAR", item.OPI_YEAR],
                ];

        return pairs.map(([label, value]) => `
            <tr>
                <th>${label}</th>
                <td>${value == null || value === "" ? "N/A" : escapeHtml(value)}</td>
            </tr>`).join("");
    }

    function adhocCardHtml() {
        if (!adhocSession) return "";
        const items = adhocSession.getItems();
        if (!items.length) return "";
        const index = adhocSession.getCarouselIndex();
        const item = items[index];
        const typeLabel = String(item?.type ?? "item").toUpperCase();
        const count = items.length > 1 ? ` (${index + 1}/${items.length})` : "";

        return `
            <div class="gis-card">
                <div class="gis-card-header">
                    <h3>ADHOC/INVENTORY: ${escapeHtml(typeLabel)}${count}</h3>
                    <div class="gis-header-actions">
                        <button type="button" class="gis-nav-btn" id="gis-adhoc-prev" aria-label="Previous adhoc item" ${index === 0 ? "disabled" : ""}>&#8249;</button>
                        <button type="button" class="gis-nav-btn" id="gis-adhoc-next" aria-label="Next adhoc item" ${index >= items.length - 1 ? "disabled" : ""}>&#8250;</button>
                    </div>
                </div>
                <div class="gis-table-frame">
                    <table class="gis-table">
                        <tbody>${adhocRows(item)}</tbody>
                    </table>
                </div>
            </div>`;
    }

    function treatmentCardBody(treatments) {
        if (treatmentPanelIndex === 0) return treatmentsPanelBody(treatments);
        if (treatmentPanelIndex === 1) return conditionPanelBody(treatments);
        return locationPanelBody(treatments);
    }

    function notesCardHtml() {
        return `
            ${notesSuccess ? `<p class="gis-banner gis-banner-success">${escapeHtml(notesSuccess)}</p>` : ""}
            <div class="gis-notes-card">
                <p class="gis-notes-label">Notes</p>
                <textarea
                    class="gis-notes-textarea${notesError ? " is-invalid" : ""}"
                    id="gis-notes-textarea"
                    placeholder="Enter your notes here...">${escapeHtml(notesValue)}</textarea>
                ${notesError ? `<div class="gis-notes-error">${escapeHtml(notesError)}</div>` : ""}
            </div>
            <div class="gis-action-row">
                <button type="button" class="gis-action-btn" id="gis-notes-save" ${isSavingNotes ? "disabled" : ""}>${isSavingNotes ? "Saving..." : "Save"}</button>
            </div>`;
    }

    function render() {
        const project = currentProject();
        const treatments = projectTreatments();
        const projects = relatedProjects();
        const multiProject = projects.length > 1;

        root.innerHTML = `
            <button type="button" class="gis-panel-close" id="gis-panel-close" aria-label="Close panel">&times;</button>

            <div class="gis-card">
                <div class="gis-card-header">
                    <span class="gis-card-title"><img class="gis-card-title-icon" src="/images/map/icons/bridge-selected.svg" alt="" />Project Information</span>
                    ${multiProject ? `
                        <div class="gis-header-actions">
                            <button type="button" class="gis-nav-btn" id="gis-project-prev" aria-label="Previous project">&#8249;</button>
                            <button type="button" class="gis-nav-btn" id="gis-project-next" aria-label="Next project">&#8250;</button>
                        </div>` : ""}
                </div>
                <div class="gis-info-body">${infoRows(project, treatments, projects)}</div>
            </div>

            <div class="gis-card">
                <div class="gis-card-header">
                    <h3 class="gis-card-title">${treatmentTitleIconHtml()}${TREATMENT_PANELS[treatmentPanelIndex]}</h3>
                    <div class="gis-header-actions">
                        <button type="button" class="gis-nav-btn" id="gis-panel-prev" aria-label="Previous panel" ${treatmentPanelIndex === 0 ? "disabled" : ""}>&#8249;</button>
                        <button type="button" class="gis-nav-btn" id="gis-panel-next" aria-label="Next panel" ${treatmentPanelIndex === TREATMENT_PANELS.length - 1 ? "disabled" : ""}>&#8250;</button>
                    </div>
                </div>
                ${treatmentCardBody(treatments)}
                <div class="gis-action-row">
                    <button type="button" class="gis-action-btn" id="gis-show-treatments">Show Treatment(s)</button>
                </div>
            </div>

            ${notesCardHtml()}

            ${adhocCardHtml()}
        `;
        notesSuccess = null;

        wireEvents(projects);
        applyTreatmentHighlight();
        applyPickedTreatment();
    }

    function wireEvents(projects) {
        root.querySelector("#gis-panel-close")?.addEventListener("click", () => {
            close();
            onClose?.();
        });

        root.querySelector("#gis-show-treatments")?.addEventListener("click", () => {
            onShowTreatments?.(projectId);
        });

        const notesTextarea = root.querySelector("#gis-notes-textarea");
        notesTextarea?.addEventListener("input", (event) => {
            notesValue = event.target.value;
            if (notesError && notesValue.trim() !== "") {
                notesError = "";
                notesTextarea.classList.remove("is-invalid");
                root.querySelector(".gis-notes-error")?.remove();
            }
        });

        root.querySelector("#gis-notes-save")?.addEventListener("click", async () => {
            const trimmed = (notesTextarea?.value ?? "").trim();
            if (trimmed === "") {
                notesError = "Notes cannot be empty.";
                render();
                return;
            }

            isSavingNotes = true;
            render();
            try {
                const result = await postJson(
                    "/Map?handler=UpdateProjectNotes",
                    { scenarioId, projectId, userNotes: trimmed },
                    antiforgeryToken,
                );
                if (!result.success) throw new Error(result.message ?? "Could not save notes.");
                notesValue = trimmed;
                notesError = "";
                notesSuccess = "Notes saved successfully.";
            } catch (error) {
                notesError = error.message ?? "Could not save notes.";
            } finally {
                isSavingNotes = false;
                render();
            }
        });

        root.querySelector("#gis-panel-prev")?.addEventListener("click", () => {
            if (treatmentPanelIndex > 0) { treatmentPanelIndex -= 1; render(); }
        });
        root.querySelector("#gis-panel-next")?.addEventListener("click", () => {
            if (treatmentPanelIndex < TREATMENT_PANELS.length - 1) { treatmentPanelIndex += 1; render(); }
        });

        root.querySelector("#gis-adhoc-prev")?.addEventListener("click", () => {
            adhocSession?.setCarouselIndex(adhocSession.getCarouselIndex() - 1);
            onAdhocIndexChange?.(adhocSession?.currentItem());
        });
        root.querySelector("#gis-adhoc-next")?.addEventListener("click", () => {
            adhocSession?.setCarouselIndex(adhocSession.getCarouselIndex() + 1);
            onAdhocIndexChange?.(adhocSession?.currentItem());
        });

        const select = root.querySelector("#gis-project-select");
        select?.addEventListener("change", (event) => {
            const id = Number(event.target.value);
            onSelectProject?.(id);
        });

        const step = (delta) => {
            const ids = projects.map((p) => projectIdOf(p));
            const idx = ids.indexOf(projectId);
            if (idx === -1) return;
            const nextIdx = (idx + delta + ids.length) % ids.length;
            onSelectProject?.(ids[nextIdx]);
        };
        root.querySelector("#gis-project-prev")?.addEventListener("click", () => step(-1));
        root.querySelector("#gis-project-next")?.addEventListener("click", () => step(1));
    }

    let updatedTimer = 0;
    let hotTreatmentId = null;
    let pickedTreatmentId = null;

    function scrollTreatmentRow(row) {
        const scroller = row.closest(".gis-table-scroll");
        if (!scroller) return;
        const headerHeight = scroller.querySelector("thead")?.getBoundingClientRect().height ?? 0;
        const scrollerRect = scroller.getBoundingClientRect();
        const rowRect = row.getBoundingClientRect();
        const topLimit = scrollerRect.top + headerHeight;
        if (rowRect.top < topLimit) {
            scroller.scrollTop -= topLimit - rowRect.top;
        } else if (rowRect.bottom > scrollerRect.bottom) {
            scroller.scrollTop += rowRect.bottom - scrollerRect.bottom;
        }
    }

    function applyTreatmentHighlight() {
        root.querySelectorAll("tr.is-treatment-hot").forEach((row) => row.classList.remove("is-treatment-hot"));
        if (hotTreatmentId == null || treatmentPanelIndex !== 0) return;
        const row = [...root.querySelectorAll("tr[data-treatment-id]")]
            .find((entry) => entry.dataset.treatmentId === hotTreatmentId);
        if (!row) return;
        row.classList.add("is-treatment-hot");
        scrollTreatmentRow(row);
    }

    function highlightTreatment(treatmentId) {
        if (root.hidden || projectId == null || treatmentId == null || treatmentId === "") return;
        const nextId = String(treatmentId);
        const sameRow = hotTreatmentId === nextId
            && treatmentPanelIndex === 0
            && root.querySelector("tr.is-treatment-hot")?.dataset.treatmentId === nextId;
        hotTreatmentId = nextId;
        if (treatmentPanelIndex !== 0) {
            treatmentPanelIndex = 0;
            render();
            return;
        }
        if (sameRow) {
            scrollTreatmentRow(root.querySelector("tr.is-treatment-hot"));
            return;
        }
        applyTreatmentHighlight();
    }

    function clearTreatmentHighlight() {
        if (hotTreatmentId == null && !root.querySelector("tr.is-treatment-hot")) return;
        hotTreatmentId = null;
        root.querySelectorAll("tr.is-treatment-hot").forEach((row) => row.classList.remove("is-treatment-hot"));
    }

    function treatmentTitleIconHtml() {
        if (treatmentPanelIndex !== 0) return "";
        const src = pickedTreatmentId ? TREATMENT_ICON_CYAN : TREATMENT_ICON;
        return `<img class="gis-card-title-icon" id="gis-treatments-title-icon" src="${src}" alt="" />`;
    }

    function syncTreatmentTitleIcon() {
        const icon = root.querySelector("#gis-treatments-title-icon");
        if (!icon) return;
        icon.src = pickedTreatmentId ? TREATMENT_ICON_CYAN : TREATMENT_ICON;
    }

    function applyPickedTreatment() {
        root.querySelectorAll("tr.is-treatment-picked").forEach((row) => row.classList.remove("is-treatment-picked"));
        syncTreatmentTitleIcon();
        if (pickedTreatmentId == null || treatmentPanelIndex !== 0) return;
        const row = [...root.querySelectorAll("tr[data-treatment-id]")]
            .find((entry) => entry.dataset.treatmentId === pickedTreatmentId);
        if (!row) return;
        row.classList.add("is-treatment-picked");
        scrollTreatmentRow(row);
    }

    function pickTreatment(treatmentId) {
        if (root.hidden || projectId == null || treatmentId == null || treatmentId === "") return;
        const nextId = String(treatmentId);
        pickedTreatmentId = nextId;
        if (treatmentPanelIndex !== 0) {
            treatmentPanelIndex = 0;
            render();
            return;
        }
        applyPickedTreatment();
    }

    function clearPickedTreatment() {
        if (pickedTreatmentId == null && !root.querySelector("tr.is-treatment-picked")) return;
        pickedTreatmentId = null;
        root.querySelectorAll("tr.is-treatment-picked").forEach((row) => row.classList.remove("is-treatment-picked"));
        syncTreatmentTitleIcon();
    }

    function markUpdated() {
        root.classList.remove("is-updated");
        void root.offsetWidth;
        root.classList.add("is-updated");
        clearTimeout(updatedTimer);
        updatedTimer = window.setTimeout(() => root.classList.remove("is-updated"), 3700);
    }

    function open(id) {
        hotTreatmentId = null;
        pickedTreatmentId = null;
        projectId = id;
        treatmentPanelIndex = 0;
        notesValue = userNotesOf(currentProject());
        notesError = "";
        isSavingNotes = false;
        render();
        root.hidden = false;
        markUpdated();
    }

    function refresh() {
        if (!root.hidden && projectId != null) render();
    }

    function close() {
        root.hidden = true;
        projectId = null;
        hotTreatmentId = null;
        pickedTreatmentId = null;
    }

    if (adhocSession) {
        unsubscribeAdhoc = adhocSession.subscribe(() => {
            if (!root.hidden && projectId != null) render();
        });
    }

    close();

    return {
        open,
        refresh,
        close,
        isOpen: () => !root.hidden,
        getProjectId: () => projectId,
        highlightTreatment,
        clearTreatmentHighlight,
        pickTreatment,
        clearPickedTreatment,
        destroy() {
            unsubscribeAdhoc?.();
        },
    };
}
