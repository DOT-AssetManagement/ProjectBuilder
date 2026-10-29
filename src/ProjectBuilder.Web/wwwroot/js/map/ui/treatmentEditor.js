import { formatCurrency, formatProjectSeq, projectIdOf, sequenceNoOf, statusOf } from "../treatments/helpers.js";
import { getJson, postJson } from "../utils/api.js";
import { escapeHtml, setOptions } from "../utils/html.js";

const ASSET_LABEL = { P: "Pavement", B: "Bridge" };
const DIRECTION_LABEL = { 0: "North / East", 1: "South / West" };

function directionLabel(value) {
    if (value === null || value === undefined || value === "") return "-";
    return DIRECTION_LABEL[Number(value)] ?? value;
}

function totalCostOf(t) {
    const parts = [t.directCost, t.indirectCostDesign, t.indirectCostRow, t.indirectCostUtilities, t.indirectCostOther];
    const sum = parts.reduce((acc, v) => acc + (Number(v) || 0), 0);
    return sum > 0 ? formatCurrency(sum) : "-";
}

function dash(value) {
    return value === null || value === undefined || value === "" ? "-" : escapeHtml(value);
}

// Route is displayed zero-padded to 4 digits everywhere else in the app (e.g.
// ProjectTreatments' grid/forms) — display-only, the underlying stored/submitted value stays raw.
function padRoute(value) {
    if (value === null || value === undefined || value === "") return "-";
    return String(value).padStart(4, "0");
}

// Plain-text section label (for option text); sectionRangeOf is the HTML-escaped form for templates
function sectionRangeText(t) {
    if (t.fromSection === null || t.fromSection === undefined || t.toSection === null || t.toSection === undefined) {
        return t.section === null || t.section === undefined || t.section === "" ? "-" : String(t.section);
    }
    return `${t.fromSection} - ${t.toSection}`;
}

function sectionRangeOf(t) {
    return escapeHtml(sectionRangeText(t));
}

// Cost fields are plain-text "$"-prefixed inputs (native number inputs can't show
// comma-grouped values), so amounts are parsed/formatted as comma+2-decimal strings.
function parseMoney(value) {
    if (value === null || value === undefined || value === "") return 0;
    const num = Number(String(value).replace(/,/g, ""));
    return Number.isFinite(num) ? num : 0;
}

function formatMoney(value) {
    if (value === null || value === undefined || value === "") return "";
    return parseMoney(value).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

// The create form's Treatment <select>. A name the user already chose (the seeded
// treatment, or one they just typed via "Create New Treatment") stays selected even
// when it is not in the scenario's existing-name list. "new" is the placeholder
// that opens the name dialog and is never a real treatment.
function treatmentOptionItems(names, selected) {
    const chosen = String(selected ?? "").trim();
    const keep = chosen && chosen !== "new" ? chosen : "";
    const list = [];
    const seen = new Set();
    for (const raw of names ?? []) {
        const name = String(raw ?? "").trim();
        if (!name || name === "new" || seen.has(name)) continue;
        seen.add(name);
        list.push(name);
    }
    if (keep && !seen.has(keep)) list.push(keep);
    return [
        { value: "", text: "Select Treatment", selected: !keep },
        { value: "new", text: "Create New Treatment" },
        ...list.map((name) => ({ value: name, text: name, selected: name === keep })),
    ];
}

function treatmentOptionsHtml(names, selected) {
    return treatmentOptionItems(names, selected).map((o) =>
        `<option value="${escapeHtml(o.value)}" ${o.selected ? "selected" : ""}>${escapeHtml(o.text)}</option>`).join("");
}

function bannerHtml(message, variant) {
    const cls = variant ? `gis-banner gis-banner-${variant}` : "gis-banner";
    return `<div class="${cls}" role="alert">
        <span class="gis-banner-text">${escapeHtml(message)}</span>
        <button type="button" class="gis-banner-close" title="Dismiss" aria-label="Dismiss">&times;</button>
    </div>`;
}

const COLUMNS = [
    "", "Asset Type", "BRKEY", "BMS ID", "District", "County", "Route",
    "Section", "Direction", "Total Cost", "Direct Cost", "Min Year", "Max Year", "Preferred Year",
];

/**
 * Bottom "PROJECT TREATMENTS" bar — legacy-style editable treatment list
 * (radio select + ADD/EDIT/DELETE toolbar) with a 4-card add/edit form.
 */
function buildTreatmentNameModal() {
    const container = document.createElement("div");
    container.innerHTML = `
        <div class="modal fade" id="gis-treatment-name-modal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Create New Treatment</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <label class="form-label" for="gis-treatment-name-input">Treatment Name</label>
                        <input type="text" class="form-control" id="gis-treatment-name-input" />
                        <div class="invalid-feedback" id="gis-treatment-name-feedback">Field is required</div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" id="gis-treatment-name-add" class="btn btn-success">Add</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    const modalEl = container.firstElementChild;
    document.body.appendChild(modalEl);
    return modalEl;
}

export function createTreatmentEditor(root, { scenarioId, antiforgeryToken, getScenarioData, onChanged }) {
    root.hidden = true;
    root.innerHTML = `
        <div class="gis-ptbar-head">
            <span class="gis-ptbar-title">Project Treatments</span>
            <button type="button" class="gis-ptbar-close" id="gis-ptbar-close" aria-label="Close treatments">&times;</button>
        </div>
        <div class="gis-ptbar-body" id="gis-ptbar-body"></div>
    `;

    const body = root.querySelector("#gis-ptbar-body");
    root.querySelector("#gis-ptbar-close").addEventListener("click", () => { root.hidden = true; });

    const treatmentNameModalEl = buildTreatmentNameModal();
    const treatmentNameInput = treatmentNameModalEl.querySelector("#gis-treatment-name-input");
    // Set when Add commits a name, so closing the dialog does not wipe that choice.
    let committedTreatmentName = null;

    function currentTreatmentSelect() {
        return body.querySelector("#gis-treatment");
    }

    function commitTreatmentName() {
        const name = treatmentNameInput.value.trim();
        if (!name) {
            treatmentNameInput.classList.add("is-invalid");
            return;
        }
        const select = currentTreatmentSelect();
        if (select) {
            if (![...select.options].some((o) => o.value === name)) {
                select.append(new Option(name, name));
            }
            select.value = name;
            select.classList.remove("is-invalid");
            select.closest(".gis-field")?.querySelector("#gis-treatment-error")?.setAttribute("hidden", "");
            committedTreatmentName = name;
        }
        window.bootstrap.Modal.getOrCreateInstance(treatmentNameModalEl).hide();
    }

    treatmentNameModalEl.querySelector("#gis-treatment-name-add").addEventListener("click", commitTreatmentName);
    treatmentNameInput.addEventListener("keydown", (event) => {
        if (event.key !== "Enter") return;
        event.preventDefault();
        commitTreatmentName();
    });

    treatmentNameModalEl.addEventListener("show.bs.modal", () => {
        treatmentNameInput.classList.remove("is-invalid");
        treatmentNameInput.value = "";
        committedTreatmentName = null;
    });

    // Cancelling (or dismissing) the sub-modal without adding a name leaves the "new"
    // placeholder selected — reset it back to blank, matching legacy.
    treatmentNameModalEl.addEventListener("hidden.bs.modal", () => {
        const select = currentTreatmentSelect();
        if (!select) return;
        if (committedTreatmentName && select.value === committedTreatmentName) {
            committedTreatmentName = null;
            return;
        }
        if (select.value === "new") select.value = "";
    });

    function isVisible() {
        return !root.hidden;
    }

    let currentProjectId = null;
    let rows = [];
    let selectedIndex = -1;
    let stale = false;
    let successMessage = null;
    let moveState = null;
    const treatmentCategoriesByAssetType = {};
    const treatmentNamesByAssetType = {};

    function scenarioProjects() {
        const data = getScenarioData?.() ?? {};
        return data.projects ?? data.Projects ?? [];
    }

    function currentProject() {
        return scenarioProjects().find((p) => projectIdOf(p) === currentProjectId) ?? null;
    }

    function projectLabel(project) {
        return project == null ? null : formatProjectSeq(sequenceNoOf(project), statusOf(project));
    }

    function isCurrentProjectUnfunded() {
        const project = currentProject();
        return project != null && statusOf(project) === "Unselected";
    }

    // A move always crosses the funding line, in either direction.
    function moveTargetStatus() {
        return isCurrentProjectUnfunded() ? "Selected" : "Unselected";
    }

    // Move targets are the scenario's projects on the other side of the funding line,
    // which the map payload already carries.
    function moveTargets() {
        const wanted = moveTargetStatus();
        return scenarioProjects()
            .filter((p) => statusOf(p) === wanted && projectIdOf(p) !== currentProjectId)
            .map((p) => ({ id: projectIdOf(p), label: projectLabel(p) }))
            .filter((p) => p.id != null && p.label);
    }

    function moveResults() {
        const query = (moveState?.query ?? "").trim().toLowerCase();
        if (!query) return [];
        return moveTargets().filter((p) => p.label.toLowerCase().includes(query)).slice(0, 20);
    }

    function treatmentSummary(t) {
        const asset = ASSET_LABEL[t.assetType] ?? escapeHtml(t.assetType ?? "Treatment");
        const name = t.treatmentType ? ` &ldquo;${escapeHtml(t.treatmentType)}&rdquo;` : "";
        return `${asset}${name} on route ${escapeHtml(padRoute(t.route))}, section ${sectionRangeOf(t)}`;
    }

    async function loadRows() {
        if (currentProjectId == null) { rows = []; return; }
        try {
            rows = await getJson(`/Map?handler=ProjectTreatments&scenarioId=${scenarioId}&projectId=${currentProjectId}`);
        } catch (error) {
            console.error("Failed to load project treatments:", error);
            rows = [];
        }
    }

    // CargoData Category master list for the scenario's library, scoped by asset type —
    // populates the "Treatment Category" pick-list (distinct from the free-text "Treatment" field).
    async function ensureTreatmentCategories(assetType) {
        const key = assetType || "";
        if (treatmentCategoriesByAssetType[key]) return treatmentCategoriesByAssetType[key];
        try {
            treatmentCategoriesByAssetType[key] = await getJson(
                `/Map?handler=TreatmentTypeCategories&scenarioId=${scenarioId}&assetType=${encodeURIComponent(assetType ?? "")}`);
        } catch (error) {
            console.warn("Failed to load treatment categories:", error);
            treatmentCategoriesByAssetType[key] = [];
        }
        return treatmentCategoriesByAssetType[key];
    }

    async function ensureTreatmentNames(assetType) {
        const key = assetType || "";
        if (treatmentNamesByAssetType[key]) return treatmentNamesByAssetType[key];
        try {
            treatmentNamesByAssetType[key] = await getJson(
                `/Map?handler=TreatmentTypes&scenarioId=${scenarioId}&assetType=${encodeURIComponent(assetType ?? "")}`);
        } catch (error) {
            console.warn("Failed to load treatment names:", error);
            treatmentNamesByAssetType[key] = [];
        }
        return treatmentNamesByAssetType[key];
    }

    function renderBody() {
        moveState = null;
        if (currentProjectId == null) {
            body.innerHTML = `<p class="gis-empty-copy">Select a project on the map to view its treatments.</p>`;
            return;
        }

        const rowHtml = rows.map((t, i) => `
            <tr>
                <td><input type="radio" name="gis-treatment-radio" ${i === selectedIndex ? "checked" : ""} data-index="${i}" /></td>
                <td>${ASSET_LABEL[t.assetType] ?? dash(t.assetType)}</td>
                <td>${dash(t.brkey)}</td>
                <td>${dash(t.bridgeId)}</td>
                <td>${dash(t.district)}</td>
                <td>${dash(t.county ?? t.countyId)}</td>
                <td>${escapeHtml(padRoute(t.route))}</td>
                <td>${sectionRangeOf(t)}</td>
                <td>${escapeHtml(directionLabel(t.direction))}</td>
                <td>${totalCostOf(t)}</td>
                <td>${t.directCost ? formatCurrency(t.directCost) : "-"}</td>
                <td>${dash(t.minYear)}</td>
                <td>${dash(t.maxYear)}</td>
                <td>${dash(t.preferredYear)}</td>
            </tr>`).join("");

        body.innerHTML = `
            ${successMessage ? bannerHtml(successMessage, "success") : ""}
            ${stale ? bannerHtml("Scenario marked stale — re-run it to apply these changes.") : ""}
            <div class="gis-ptbar-actions">
                <button type="button" class="gis-btn" id="gis-btn-add">Add Treatment</button>
                <button type="button" class="gis-btn" id="gis-btn-edit" ${selectedIndex < 0 ? "disabled" : ""}>Edit Treatment</button>
                <button type="button" class="gis-btn danger" id="gis-btn-delete" ${selectedIndex < 0 ? "disabled" : ""}>Delete</button>
                ${currentProject() != null
                    ? `<button type="button" class="gis-btn" id="gis-btn-move" ${selectedIndex < 0 ? "disabled" : ""}>Move</button>`
                    : ""}
            </div>
            <div class="gis-edit-scroll">
                <table class="gis-edit-table">
                    <thead><tr>${COLUMNS.map((c) => `<th>${c}</th>`).join("")}</tr></thead>
                    <tbody>${rowHtml || `<tr><td colspan="${COLUMNS.length}">No treatments for this project.</td></tr>`}</tbody>
                </table>
            </div>
            <div id="gis-form-container"></div>
        `;
        successMessage = null;
    }

    async function renderForm(mode, treatment) {
        const container = body.querySelector("#gis-form-container");
        if (!container) return;
        moveState = null;
        const isEdit = mode === "edit";
        const t = treatment ?? {};
        const assetType = t.assetType ?? "P";
        const categories = await ensureTreatmentCategories(assetType);
        const roDisabled = isEdit ? "disabled" : "";
        let cascadeReady = Promise.resolve();

        const categoryOptions = categories.map((c) =>
            `<option value="${escapeHtml(c)}" ${c === t.category ? "selected" : ""}>${escapeHtml(c)}</option>`).join("");
        const v = (key) => escapeHtml(t[key] ?? "");

        const treatmentField = isEdit
            ? `<input type="text" name="treatmentType" value="${v("treatmentType")}" readonly />`
            : `<select id="gis-treatment" name="treatmentType">${treatmentOptionsHtml(await ensureTreatmentNames(assetType), t.treatmentType)}</select>`;

        container.innerHTML = `
            <form class="gis-form" id="gis-tform">
                <h4>${isEdit ? "Edit Treatment" : "Add Treatment"}</h4>

                <div class="gis-form-cards">
                    <div class="gis-form-card gis-form-card--a">
                        <div class="gis-form-card-title">Project Information</div>
                        <div class="gis-form-card-grid">
                            <label class="gis-field">Asset Type
                                <select name="assetType" ${roDisabled}>
                                    <option value="P" ${assetType === "P" ? "selected" : ""}>Pavement</option>
                                    <option value="B" ${assetType === "B" ? "selected" : ""}>Bridge</option>
                                </select>
                            </label>
                            <label class="gis-field">Treatment ${treatmentField}
                                <span class="gis-field-error" id="gis-treatment-error" hidden>Treatment is required</span>
                            </label>
                            <label class="gis-field">Treatment Category
                                <select id="gis-treatment-category" name="category"><option value="">Select</option>${categoryOptions}</select>
                            </label>
                            <label class="gis-field">Priority
                                <select name="priorityOrder">
                                    ${[0, 1, 2, 3, 4].map((p) => `<option value="${p}" ${Number(t.priorityOrder) === p ? "selected" : ""}>${p}</option>`).join("")}
                                </select>
                            </label>
                            <label class="gis-field gis-field-check">
                                <span class="gis-toggle">
                                    <input type="checkbox" name="isCommitted" ${t.isCommitted ? "checked" : ""} />
                                    <span class="gis-toggle-slider"></span>
                                </span>
                                Is Committed
                            </label>
                        </div>
                    </div>

                    <div class="gis-form-card gis-form-card--b">
                        <div class="gis-form-card-title">Location Information</div>
                        <div class="gis-form-card-grid">
                            <label class="gis-field">District
                                <select name="district" data-cascade="district" ${roDisabled}>
                                    <option value="${v("district")}" selected>${dash(t.district)}</option>
                                </select>
                            </label>
                            <label class="gis-field">County
                                <select name="countyId" data-cascade="county" ${roDisabled}>
                                    <option value="${v("countyId")}" selected>${dash(t.county ?? t.countyId)}</option>
                                </select>
                            </label>
                            <label class="gis-field">Route
                                <select name="route" data-cascade="route" ${roDisabled}>
                                    <option value="${v("route")}" selected>${escapeHtml(padRoute(t.route))}</option>
                                </select>
                            </label>
                            <label class="gis-field">Section
                                <select name="section" data-cascade="section" ${roDisabled}>
                                    <option value="${v("section")}" selected>${sectionRangeOf(t)}</option>
                                </select>
                            </label>
                            <label class="gis-field">Interstate
                                <select name="interstate" ${roDisabled}>
                                    <option value="false" ${t.interstate ? "" : "selected"}>No</option>
                                    <option value="true" ${t.interstate ? "selected" : ""}>Yes</option>
                                </select>
                            </label>
                            <label class="gis-field">Direction
                                <select name="direction" ${roDisabled}>
                                    <option value="0" ${Number(t.direction) === 0 ? "selected" : ""}>North / East</option>
                                    <option value="1" ${Number(t.direction) === 1 ? "selected" : ""}>South / West</option>
                                </select>
                            </label>
                            <label class="gis-field" data-asset="B" ${assetType === "B" ? "" : "hidden"}>BRKEY <input type="text" name="brkey" value="${v("brkey")}" ${roDisabled} /></label>
                            <label class="gis-field" data-asset="B" ${assetType === "B" ? "" : "hidden"}>BMS ID
                                <input type="text" inputmode="numeric" name="bridgeId" value="${v("bridgeId")}" list="gis-bridge-candidates" autocomplete="off" ${roDisabled} />
                                <datalist id="gis-bridge-candidates"></datalist>
                            </label>
                        </div>
                    </div>

                    <div class="gis-form-card gis-form-card--a">
                        <div class="gis-form-card-title">Timing</div>
                        <div class="gis-form-card-grid single-col">
                            <label class="gis-field">Preferred Year <input type="number" name="preferredYear" value="${v("preferredYear")}" /></label>
                        </div>
                        <div class="gis-form-card-grid">
                            <label class="gis-field">Minimum Year <input type="number" name="minYear" value="${v("minYear")}" /></label>
                            <label class="gis-field">Maximum Year <input type="number" name="maxYear" value="${v("maxYear")}" /></label>
                        </div>
                    </div>

                    <div class="gis-form-card gis-form-card--b">
                        <div class="gis-form-card-title">Cost</div>
                        <div class="gis-form-card-grid">
                            <label class="gis-field">Construction Cost
                                <span class="gis-money-input"><span class="gis-money-prefix">$</span><input type="text" inputmode="decimal" name="directCost" value="${formatMoney(v("directCost"))}" data-cost /></span>
                                <span class="gis-field-error" data-error-for="directCost" hidden>Construction Cost is required</span>
                            </label>
                            <label class="gis-field">Indirect Cost Row
                                <span class="gis-money-input"><span class="gis-money-prefix">$</span><input type="text" inputmode="decimal" name="indirectCostRow" value="${formatMoney(v("indirectCostRow"))}" data-cost /></span>
                                <span class="gis-field-error" data-error-for="indirectCostRow" hidden>Indirect Cost Row is required</span>
                            </label>
                            <label class="gis-field">Indirect Cost Utilities
                                <span class="gis-money-input"><span class="gis-money-prefix">$</span><input type="text" inputmode="decimal" name="indirectCostUtilities" value="${formatMoney(v("indirectCostUtilities"))}" data-cost /></span>
                                <span class="gis-field-error" data-error-for="indirectCostUtilities" hidden>Indirect Cost Utilities is required</span>
                            </label>
                            <label class="gis-field">Indirect Cost Design
                                <span class="gis-money-input"><span class="gis-money-prefix">$</span><input type="text" inputmode="decimal" name="indirectCostDesign" value="${formatMoney(v("indirectCostDesign"))}" data-cost /></span>
                                <span class="gis-field-error" data-error-for="indirectCostDesign" hidden>Indirect Cost Design is required</span>
                            </label>
                            <label class="gis-field">Indirect Cost Other
                                <span class="gis-money-input"><span class="gis-money-prefix">$</span><input type="text" inputmode="decimal" name="indirectCostOther" value="${formatMoney(v("indirectCostOther"))}" data-cost /></span>
                                <span class="gis-field-error" data-error-for="indirectCostOther" hidden>Indirect Cost Other is required</span>
                            </label>
                            <label class="gis-field">Total Cost
                                <span class="gis-money-input gis-money-input--readonly"><span class="gis-money-prefix">$</span><input type="text" name="totalCostDisplay" readonly /></span>
                            </label>
                        </div>
                    </div>
                </div>

                <input type="hidden" name="benefit" value="${v("benefit")}" />
                <input type="hidden" name="risk" value="${v("risk")}" />
                <p class="gis-form-error" id="gis-form-error" hidden></p>
                <div class="gis-form-buttons">
                    <button type="submit" class="gis-btn primary" id="gis-form-submit">${isEdit ? "Update" : "Save"}</button>
                    <button type="button" class="gis-btn" id="gis-form-cancel">Cancel</button>
                </div>
            </form>
        `;

        const form = container.querySelector("#gis-tform");
        const errorEl = container.querySelector("#gis-form-error");
        form.scrollIntoView({ behavior: "smooth", block: "nearest" });

        // Total cost auto-sum
        const totalDisplay = form.querySelector("[name=totalCostDisplay]");
        const recalcTotal = () => {
            let sum = 0;
            form.querySelectorAll("[data-cost]").forEach((el) => { sum += parseMoney(el.value); });
            totalDisplay.value = formatMoney(sum);
        };
        form.querySelectorAll("[data-cost]").forEach((el) => {
            el.addEventListener("input", recalcTotal);
            el.addEventListener("blur", () => { el.value = formatMoney(el.value); });
        });
        recalcTotal();

        // Cost fields are required (matches legacy) — validated on submit, cleared as the
        // user fills them back in.
        const REQUIRED_COST_FIELDS = [
            "directCost", "indirectCostRow", "indirectCostUtilities", "indirectCostDesign", "indirectCostOther",
        ];
        function setCostFieldValid(name, valid) {
            const input = form.querySelector(`[name=${name}]`);
            input.closest(".gis-money-input").classList.toggle("is-invalid", !valid);
            form.querySelector(`[data-error-for=${name}]`).hidden = valid;
        }
        function validateCosts() {
            let allValid = true;
            REQUIRED_COST_FIELDS.forEach((name) => {
                const valid = form.querySelector(`[name=${name}]`).value.trim() !== "";
                setCostFieldValid(name, valid);
                if (!valid) allValid = false;
            });
            return allValid;
        }
        REQUIRED_COST_FIELDS.forEach((name) => {
            form.querySelector(`[name=${name}]`).addEventListener("input", () => {
                if (form.querySelector(`[name=${name}]`).value.trim() !== "") setCostFieldValid(name, true);
            });
        });

        // Asset type toggles bridge-only fields and reloads the category pick-list
        const assetSelect = form.querySelector("[name=assetType]");
        const categorySelect = form.querySelector("#gis-treatment-category");
        const treatmentSelect = form.querySelector("#gis-treatment");
        // A slower reload must not replace a treatment the user picked while it was in flight.
        let treatmentReload = 0;
        const syncAsset = async () => {
            const token = ++treatmentReload;
            const previousTreatment = treatmentSelect?.value ?? "";
            form.querySelectorAll("[data-asset=B]").forEach((el) => { el.hidden = assetSelect.value !== "B"; });
            const [categoryOptions, treatmentNames] = await Promise.all([
                ensureTreatmentCategories(assetSelect.value),
                treatmentSelect ? ensureTreatmentNames(assetSelect.value) : Promise.resolve([]),
            ]);
            if (token !== treatmentReload) return;
            setOptions(categorySelect, [{ value: "", text: "Select" }, ...categoryOptions.map((c) => ({ value: c, text: c }))]);
            if (treatmentSelect) {
                // Prefer whatever is selected now (the user may have picked one while
                // the name list was loading). "new" is only the dialog placeholder.
                const live = treatmentSelect.value;
                const keep = live && live !== "new" ? live : previousTreatment;
                setOptions(treatmentSelect, treatmentOptionItems(treatmentNames, keep));
            }
        };
        assetSelect.addEventListener("change", syncAsset);

        // "Create New Treatment" escape hatch — same pattern as legacy's #c_treatment.
        treatmentSelect?.addEventListener("change", () => {
            if (treatmentSelect.value && treatmentSelect.value !== "new") {
                treatmentSelect.classList.remove("is-invalid");
                form.querySelector("#gis-treatment-error").hidden = true;
            }
            if (treatmentSelect.value === "new") {
                window.bootstrap.Modal.getOrCreateInstance(treatmentNameModalEl).show();
            }
        });

        // Committed pins years/priority
        const committed = form.querySelector("[name=isCommitted]");
        const preferred = form.querySelector("[name=preferredYear]");
        const minY = form.querySelector("[name=minYear]");
        const maxY = form.querySelector("[name=maxYear]");
        const priority = form.querySelector("[name=priorityOrder]");
        const syncCommitted = () => {
            if (committed.checked) {
                minY.value = preferred.value;
                maxY.value = preferred.value;
                priority.value = "0";
            }
            minY.disabled = committed.checked;
            maxY.disabled = committed.checked;
            priority.disabled = committed.checked;
        };
        committed.addEventListener("change", syncCommitted);
        syncCommitted();

        // Create-mode cascade: district -> county -> route -> section, sourced from master
        // reference data (tbl_pams_MaintainableAssetsSegmentation for Pavement,
        // tbl_pb_BridgeReferenceDataFromPennshare for Bridge) keyed by the form's own Asset
        // Type field — every location PennDOT publishes is offered, not just ones already
        // present in this scenario. Seeded from the project's first existing treatment (see
        // buildAddSeed) only on first render, so each level below is pre-loaded and
        // pre-selected instead of starting blank.
        if (!isEdit) {
            const districtSelect = form.querySelector("[data-cascade=district]");
            const countySelect = form.querySelector("[data-cascade=county]");
            const routeSelect = form.querySelector("[data-cascade=route]");
            const sectionSelect = form.querySelector("[data-cascade=section]");
            const bridgeDatalist = form.querySelector("#gis-bridge-candidates");
            const brkeyInput = form.querySelector("[name=brkey]");
            const bridgeIdInput = form.querySelector("[name=bridgeId]");
            let bridgeCandidates = [];

            // BRKEY/BMS ID are derived from District/County/Route/Section (Bridge only) — cleared
            // whenever an upstream cascade level changes so a stale value from a different bridge
            // can't linger on save. BMS ID stays a normal editable input; the datalist only offers
            // suggestions when Segment resolves to more than one physical bridge.
            function clearBridgeCandidates() {
                bridgeCandidates = [];
                bridgeDatalist.innerHTML = "";
                brkeyInput.value = "";
                bridgeIdInput.value = "";
            }

            function applyBridgeCandidate(candidate) {
                brkeyInput.value = candidate.brKey ?? "";
                bridgeIdInput.value = candidate.bridgeId ?? "";
            }

            // Segment isn't a unique key in the bridge reference table — District/County/Route/
            // Section routinely matches more than one physical bridge (e.g. ramp placeholder
            // segments, parallel spans). Always auto-fill BRKEY/BMS ID from a match (the seeded
            // brkey when it's one of the candidates, otherwise the first one) so the fields are
            // never left blank; multiple matches also populate the BMS ID field's suggestion list
            // so the user can pick a different one if the default guess is wrong.
            async function loadBridgeCandidates(district, county, route, section, preferredBrKey) {
                clearBridgeCandidates();
                if (assetSelect.value !== "B" || !district || !county || !route || !section) return;
                try {
                    bridgeCandidates = await getJson(
                        `/Map?handler=BridgeCandidates&scenarioId=${scenarioId}&district=${district}&county=${county}&route=${route}&section=${encodeURIComponent(section)}`);
                } catch (error) {
                    console.warn("Bridge candidate lookup failed:", error);
                    return;
                }
                if (bridgeCandidates.length === 0) return;
                if (bridgeCandidates.length > 1) {
                    setOptions(bridgeDatalist, bridgeCandidates
                        .map((c) => ({ value: c.bridgeId ?? c.brKey, text: `BRKEY ${c.brKey ?? ""}` })));
                }
                const preferred = preferredBrKey != null
                    && bridgeCandidates.find((c) => String(c.brKey) === String(preferredBrKey));
                applyBridgeCandidate(preferred || bridgeCandidates[0]);
            }

            // Typing or picking a suggested BMS ID resolves BRKEY to match; an unrecognized
            // value (manual override) leaves BRKEY as whatever it last was.
            bridgeIdInput.addEventListener("input", () => {
                const match = bridgeCandidates.find((c) => String(c.bridgeId ?? "") === bridgeIdInput.value.trim());
                if (match) brkeyInput.value = match.brKey ?? "";
            });

            async function loadCounties(district, selectValue) {
                countySelect.innerHTML = `<option value="">Select county</option>`;
                routeSelect.innerHTML = `<option value="">Select route</option>`;
                sectionSelect.innerHTML = `<option value="">Select section</option>`;
                clearBridgeCandidates();
                if (!district) return;
                try {
                    const counties = await getJson(`/Map?handler=Counties&scenarioId=${scenarioId}&district=${district}&assetType=${encodeURIComponent(assetSelect.value)}`);
                    setOptions(countySelect, [{ value: "", text: "Select county" },
                        ...counties.map((c) => ({ value: c.countyId, text: c.countyName }))]);
                    if (selectValue != null && counties.some((c) => String(c.countyId) === String(selectValue))) {
                        countySelect.value = String(selectValue);
                        await loadRoutes(district, selectValue, v("route"));
                    }
                } catch (error) {
                    console.warn("County lookup failed:", error);
                }
            }

            async function loadRoutes(district, county, selectValue) {
                routeSelect.innerHTML = `<option value="">Select route</option>`;
                sectionSelect.innerHTML = `<option value="">Select section</option>`;
                clearBridgeCandidates();
                if (!district || !county) return;
                try {
                    const routes = await getJson(`/Map?handler=Routes&scenarioId=${scenarioId}&district=${district}&county=${county}&assetType=${encodeURIComponent(assetSelect.value)}`);
                    setOptions(routeSelect, [{ value: "", text: "Select route" },
                        ...routes.map((r) => ({ value: r, text: padRoute(r) }))]);
                    if (selectValue != null && routes.some((r) => String(r) === String(selectValue))) {
                        routeSelect.value = String(selectValue);
                        await loadSections(district, county, selectValue, v("section"));
                    }
                } catch (error) {
                    console.warn("Route lookup failed:", error);
                }
            }

            async function loadSections(district, county, route, selectValue) {
                sectionSelect.innerHTML = `<option value="">Select section</option>`;
                clearBridgeCandidates();
                if (!district || !county || !route) return;
                try {
                    // Ranges endpoint (not the plain Sections one) so the option label matches
                    // the treatments table above, which shows "10 - 10" rather than bare "10".
                    const sections = await getJson(`/Map?handler=SectionRanges&scenarioId=${scenarioId}&district=${district}&county=${county}&route=${route}&assetType=${encodeURIComponent(assetSelect.value)}`);
                    setOptions(sectionSelect, [{ value: "", text: "Select section" },
                        ...sections.map((s) => ({ value: s.section, text: sectionRangeText(s) }))]);
                    if (selectValue != null && sections.some((s) => String(s.section) === String(selectValue))) {
                        sectionSelect.value = String(selectValue);
                        await loadBridgeCandidates(district, county, route, selectValue, v("brkey"));
                    }
                } catch (error) {
                    console.warn("Section lookup failed:", error);
                }
            }

            async function loadDistricts(selectValue) {
                districtSelect.innerHTML = `<option value="">Select district</option>`;
                countySelect.innerHTML = `<option value="">Select county</option>`;
                routeSelect.innerHTML = `<option value="">Select route</option>`;
                sectionSelect.innerHTML = `<option value="">Select section</option>`;
                clearBridgeCandidates();
                try {
                    const districts = await getJson(`/Map?handler=Districts&scenarioId=${scenarioId}&assetType=${encodeURIComponent(assetSelect.value)}`);
                    setOptions(districtSelect, [{ value: "", text: "Select district" },
                        ...districts.map((d) => ({ value: d, text: `District ${d}` }))]);
                    if (selectValue != null && selectValue !== "" && districts.some((d) => String(d) === String(selectValue))) {
                        districtSelect.value = String(selectValue);
                        await loadCounties(selectValue, v("countyId"));
                    }
                } catch (error) {
                    console.warn("District lookup failed:", error);
                }
            }

            districtSelect.addEventListener("change", () => loadCounties(districtSelect.value));
            countySelect.addEventListener("change", () => loadRoutes(districtSelect.value, countySelect.value));
            routeSelect.addEventListener("change", () => loadSections(districtSelect.value, countySelect.value, routeSelect.value));
            sectionSelect.addEventListener("change", () => loadBridgeCandidates(
                districtSelect.value, countySelect.value, routeSelect.value, sectionSelect.value));

            // The two master tables have entirely different districts/counties/routes, so
            // switching Asset Type must re-run the whole cascade from scratch rather than just
            // re-filter what's already loaded.
            assetSelect.addEventListener("change", () => loadDistricts());

            cascadeReady = loadDistricts(v("district"));
        }

        form.querySelector("#gis-form-cancel").addEventListener("click", () => renderBody());

        const submitBtn = form.querySelector("#gis-form-submit");
        const submitLabel = submitBtn.textContent;

        form.addEventListener("submit", async (event) => {
            event.preventDefault();
            errorEl.hidden = true;
            if (!validateCosts()) return;
            if (!isEdit) {
                const treatmentSelectEl = form.querySelector("#gis-treatment");
                const treatmentName = String(treatmentSelectEl?.value ?? "").trim();
                const treatmentError = form.querySelector("#gis-treatment-error");
                if (!treatmentName || treatmentName === "new") {
                    treatmentSelectEl?.classList.add("is-invalid");
                    if (treatmentError) treatmentError.hidden = false;
                    errorEl.textContent = "Treatment is required.";
                    errorEl.hidden = false;
                    treatmentSelectEl?.focus();
                    return;
                }
                treatmentSelectEl.classList.remove("is-invalid");
                if (treatmentError) treatmentError.hidden = true;
            }
            const fd = new FormData(form);
            const num = (name) => {
                const val = fd.get(name);
                if (val === null || val === "") return null;
                const parsed = Number(String(val).replace(/,/g, ""));
                return Number.isFinite(parsed) ? parsed : null;
            };

            submitBtn.disabled = true;
            submitBtn.textContent = isEdit ? "Updating..." : "Saving...";

            const committedValues = () => {
                const pref = num("preferredYear") ?? Number(preferred.value);
                return {
                    priorityOrder: 0,
                    minYear: pref,
                    maxYear: pref,
                };
            };
            const timingValues = () => committed.checked
                ? committedValues()
                : {
                    priorityOrder: num("priorityOrder"),
                    minYear: num("minYear"),
                    maxYear: num("maxYear"),
                };

            try {
                if (isEdit) {
                    const timing = timingValues();
                    const payload = {
                        id: t.id,
                        scenarioId,
                        assetType,
                        // Direction/Interstate are read-only in this modal (edit only touches
                        // cost/timing/priority/committed/category) but EditTreatment is a shared
                        // full-replace endpoint (also used by /ProjectTreatments's edit modal,
                        // which DOES let the user change these) — round-trip the existing values
                        // so an edit here doesn't silently null them out.
                        direction: t.direction,
                        interstate: t.interstate,
                        category: fd.get("category") || null,
                        directCost: num("directCost"),
                        benefit: num("benefit"),
                        risk: num("risk"),
                        preferredYear: num("preferredYear"),
                        minYear: timing.minYear,
                        maxYear: timing.maxYear,
                        priorityOrder: timing.priorityOrder,
                        indirectCostDesign: num("indirectCostDesign"),
                        indirectCostRow: num("indirectCostRow"),
                        indirectCostUtilities: num("indirectCostUtilities"),
                        indirectCostOther: num("indirectCostOther"),
                        isCommitted: committed.checked,
                    };
                    const result = await postJson("/Map?handler=EditTreatment", payload, antiforgeryToken);
                    if (!result.success) throw new Error(result.message ?? "Could not save the treatment.");
                } else {
                    const isBridge = fd.get("assetType") === "B";
                    const timing = timingValues();
                    const payload = {
                        scenarioId,
                        projectId: currentProjectId,
                        assetType: fd.get("assetType"),
                        treatmentType: String(form.querySelector("#gis-treatment")?.value ?? "").trim(),
                        category: fd.get("category") || null,
                        district: num("district"),
                        countyId: num("countyId"),
                        route: num("route"),
                        section: fd.get("section"),
                        brkey: isBridge ? fd.get("brkey") : null,
                        bridgeId: isBridge ? num("bridgeId") : null,
                        interstate: fd.get("interstate") === "true",
                        direction: num("direction"),
                        directCost: num("directCost"),
                        benefit: num("benefit"),
                        risk: num("risk"),
                        preferredYear: num("preferredYear"),
                        minYear: timing.minYear,
                        maxYear: timing.maxYear,
                        priorityOrder: timing.priorityOrder,
                        indirectCostDesign: num("indirectCostDesign"),
                        indirectCostRow: num("indirectCostRow"),
                        indirectCostUtilities: num("indirectCostUtilities"),
                        indirectCostOther: num("indirectCostOther"),
                        isCommitted: committed.checked,
                    };
                    const result = await postJson("/Map?handler=CreateTreatment", payload, antiforgeryToken);
                    if (!result.success) throw new Error(result.message ?? "Could not create the treatment.");
                }

                stale = true;
                selectedIndex = -1;
                successMessage = isEdit ? "Treatment updated successfully." : "Treatment added successfully.";
                await onChanged?.(currentProjectId);
            } catch (error) {
                errorEl.textContent = error.message;
                errorEl.hidden = false;
                submitBtn.disabled = false;
                submitBtn.textContent = submitLabel;
            }
        });

        await cascadeReady;
    }

    // The form needs several lookups (categories, treatment names, location cascade) before it is usable.
    async function openForm(mode, treatment) {
        const overlay = document.createElement("div");
        overlay.className = "gis-loading-overlay";
        overlay.innerHTML = `<div class="gis-spinner"></div><span>Loading…</span>`;
        body.appendChild(overlay);
        try {
            await renderForm(mode, treatment);
        } finally {
            overlay.remove();
        }
    }

    // Collapses once a project is picked — the input then shows the choice, and typing reopens it.
    function renderMoveResults() {
        const list = body.querySelector("#gis-move-results");
        if (!list) return;

        if (moveState.targetProjectId != null) {
            list.innerHTML = "";
            list.hidden = true;
            return;
        }

        const matches = moveResults();
        list.innerHTML = matches.map((p) => `
            <button type="button" class="gis-move-result" data-project-id="${escapeHtml(p.id)}">${escapeHtml(p.label)}</button>`).join("");
        list.hidden = matches.length === 0;
    }

    function closeMovePanel() {
        moveState = null;
        const container = body.querySelector("#gis-form-container");
        if (container) container.innerHTML = "";
    }

    function renderMovePanel() {
        const container = body.querySelector("#gis-form-container");
        if (!container || !moveState) return;

        const row = rows[selectedIndex];
        if (!row) {
            closeMovePanel();
            return;
        }

        const sourceLabel = escapeHtml(projectLabel(currentProject()) ?? "this project");
        const target = moveTargets().find((p) => p.id === moveState.targetProjectId);
        const errorHtml = moveState.error ? `<p class="gis-form-error">${escapeHtml(moveState.error)}</p>` : "";

        if (moveState.step === "confirm" && target) {
            container.innerHTML = `
                <div class="gis-form gis-move-panel">
                    <h4>Confirm Move</h4>
                    <p class="gis-move-question">
                        Move ${treatmentSummary(row)} from <strong>${sourceLabel}</strong>
                        to <strong>${escapeHtml(target.label)}</strong>?
                    </p>
                    <p class="gis-move-note">${sourceLabel} stays on the map even if this is its last treatment.</p>
                    ${errorHtml}
                    <div class="gis-form-buttons">
                        <button type="button" class="gis-btn primary" id="gis-move-submit" ${moveState.busy ? "disabled" : ""}>${moveState.busy ? "Moving…" : "Confirm Move"}</button>
                        <button type="button" class="gis-btn" id="gis-move-back" ${moveState.busy ? "disabled" : ""}>Back</button>
                    </div>
                </div>`;
            return;
        }

        const searchHint = moveTargetStatus() === "Selected"
            ? "Search a funded project, e.g. PB-17"
            : "Search an unfunded project, e.g. UF-17";

        container.innerHTML = `
            <div class="gis-form gis-move-panel">
                <h4>Move Treatment</h4>
                <p class="gis-move-question">Moving ${treatmentSummary(row)} out of <strong>${sourceLabel}</strong>.</p>
                <label class="gis-field">Destination Project
                    <input type="search" id="gis-move-search" placeholder="${searchHint}"
                           autocomplete="off" value="${escapeHtml(moveState.query)}" />
                </label>
                <div class="gis-move-results" id="gis-move-results" hidden></div>
                ${errorHtml}
                <div class="gis-form-buttons">
                    <button type="button" class="gis-btn primary" id="gis-move-next" ${target ? "" : "disabled"}>Continue</button>
                    <button type="button" class="gis-btn" id="gis-move-cancel">Cancel</button>
                </div>
            </div>`;

        renderMoveResults();
    }

    function openMovePanel() {
        moveState = { step: "search", query: "", targetProjectId: null, error: "", busy: false };
        renderMovePanel();
        body.querySelector(".gis-move-panel")?.scrollIntoView({ behavior: "smooth", block: "nearest" });
        body.querySelector("#gis-move-search")?.focus();
    }

    async function submitMove() {
        const row = rows[selectedIndex];
        const target = moveTargets().find((p) => p.id === moveState.targetProjectId);
        if (!row || !target) return;

        moveState.busy = true;
        moveState.error = "";
        renderMovePanel();

        // onChanged() re-fetches the scenario and redraws the map, so block the bar for that whole window.
        const overlay = document.createElement("div");
        overlay.className = "gis-loading-overlay";
        overlay.innerHTML = `<div class="gis-spinner"></div><span>Moving…</span>`;
        body.appendChild(overlay);

        try {
            const result = await postJson(
                "/Map?handler=MoveTreatment",
                { scenarioId, treatmentId: row.id, targetProjectId: target.id },
                antiforgeryToken,
            );
            if (!result.success) throw new Error(result.message ?? "Could not move the treatment.");

            stale = true;
            selectedIndex = -1;
            moveState = null;
            successMessage = `Treatment moved to ${target.label}.`;
            await onChanged?.(currentProjectId);
        } catch (error) {
            console.error("Failed to move treatment:", error);
            if (moveState) {
                moveState.busy = false;
                moveState.error = error.message ?? "Could not move the treatment.";
                renderMovePanel();
            }
        } finally {
            overlay.remove();
        }
    }

    // ── Events ──
    body.addEventListener("change", (event) => {
        const radio = event.target.closest("input[name=gis-treatment-radio]");
        if (radio) {
            selectedIndex = Number(radio.dataset.index);
            const editBtn = body.querySelector("#gis-btn-edit");
            const delBtn = body.querySelector("#gis-btn-delete");
            const moveBtn = body.querySelector("#gis-btn-move");
            if (editBtn) editBtn.disabled = false;
            if (delBtn) delBtn.disabled = false;
            if (moveBtn) moveBtn.disabled = false;
            // An open move panel still describes the previously selected row.
            if (moveState) closeMovePanel();
        }
    });

    body.addEventListener("input", (event) => {
        if (!moveState || event.target.id !== "gis-move-search") return;
        moveState.query = event.target.value;
        moveState.targetProjectId = null;
        const nextBtn = body.querySelector("#gis-move-next");
        if (nextBtn) nextBtn.disabled = true;
        renderMoveResults();
    });

    // Every treatment on a project shares the same physical location, so a new treatment
    // added to a project that already has one starts from that first treatment's values
    // (location + timing/priority/committed) instead of blank — matches the legacy React
    // map form's handleAddTreatment. Cost fields are intentionally left blank, as legacy does.
    function buildAddSeed() {
        const source = rows[0];
        if (!source) return null;
        return {
            assetType: source.assetType,
            treatmentType: source.treatmentType,
            priorityOrder: source.priorityOrder,
            district: source.district,
            countyId: source.countyId,
            county: source.county,
            route: source.route,
            section: source.section,
            fromSection: source.fromSection,
            toSection: source.toSection,
            interstate: source.interstate,
            direction: source.direction,
            brkey: source.brkey,
            bridgeId: source.bridgeId,
            preferredYear: source.preferredYear,
            minYear: source.minYear,
            maxYear: source.maxYear,
            isCommitted: source.isCommitted,
        };
    }

    body.addEventListener("click", async (event) => {
        const closeBtn = event.target.closest(".gis-banner-close");
        if (closeBtn) {
            closeBtn.closest(".gis-banner")?.remove();
            return;
        }

        const moveResult = event.target.closest(".gis-move-result");
        if (moveResult && moveState) {
            const picked = moveTargets().find((p) => p.id === Number(moveResult.dataset.projectId));
            if (!picked) return;
            moveState.targetProjectId = picked.id;
            moveState.query = picked.label;
            moveState.error = "";
            renderMovePanel();
            return;
        }

        const target = event.target;
        if (target.id === "gis-btn-move" && selectedIndex >= 0) {
            openMovePanel();
        } else if (target.id === "gis-move-next" && moveState?.targetProjectId != null) {
            moveState.step = "confirm";
            renderMovePanel();
        } else if (target.id === "gis-move-back" && moveState) {
            moveState.step = "search";
            moveState.error = "";
            renderMovePanel();
        } else if (target.id === "gis-move-cancel") {
            closeMovePanel();
        } else if (target.id === "gis-move-submit" && moveState && !moveState.busy) {
            await submitMove();
        } else if (target.id === "gis-btn-add") {
            await openForm("add", buildAddSeed());
        } else if (target.id === "gis-btn-edit" && selectedIndex >= 0) {
            await openForm("edit", rows[selectedIndex]);
        } else if (target.id === "gis-btn-delete" && selectedIndex >= 0) {
            const row = rows[selectedIndex];
            if (!window.confirm("Delete this treatment from the scenario?")) return;
            // onChanged() below re-fetches scenario data and re-renders the map's project
            // panel, which takes a few seconds — show a blocking overlay for that whole
            // window so the delete doesn't look like it silently did nothing. renderBody()
            // (called via onChanged -> editor.refresh()) replaces body.innerHTML wholesale
            // on success, which removes this along with everything else; the finally block
            // covers the error path, where renderBody() isn't reached.
            const overlay = document.createElement("div");
            overlay.className = "gis-loading-overlay";
            overlay.innerHTML = `<div class="gis-spinner"></div><span>Deleting…</span>`;
            body.appendChild(overlay);
            try {
                const result = await postJson(
                    "/Map?handler=DeleteTreatment",
                    { treatmentId: row.id, scenarioId },
                    antiforgeryToken,
                );
                if (!result.success) throw new Error(result.message ?? "Could not delete the treatment.");
                stale = true;
                selectedIndex = -1;
                await onChanged?.(currentProjectId);
            } catch (error) {
                console.error("Failed to delete treatment:", error);
            } finally {
                overlay.remove();
            }
        }
    });

    return {
        // Load a project's treatments into the (hidden) editor without showing it.
        async setProject(projectId) {
            currentProjectId = projectId;
            selectedIndex = -1;
            await loadRows();
            if (isVisible()) renderBody();
        },
        // Reveal the editor for the given project ("Show Treatment(s)").
        async expand(projectId) {
            if (projectId != null && projectId !== currentProjectId) {
                currentProjectId = projectId;
                selectedIndex = -1;
                await loadRows();
            }
            if (currentProjectId == null) return;
            root.hidden = false;
            renderBody();
        },
        close() {
            root.hidden = true;
        },
        async refresh() {
            await loadRows();
            if (isVisible()) renderBody();
        },
    };
}
