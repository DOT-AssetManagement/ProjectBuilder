import { projectIdOf, sequenceNoOf, formatProjectSeq, statusOf } from "../treatments/helpers.js";
import { escapeHtml } from "../utils/html.js";

export function createProjectSearch(root, { getScenarioData, onSelectProject, isUnfundedVisible }) {
    if (!root) {
        return { refresh() {} };
    }

    let expanded = false;
    let query = "";

    function projects() {
        const data = getScenarioData?.() ?? {};
        const all = (data.projects ?? data.Projects ?? []).slice();
        // UF-* projects are only findable while their layer is toggled on — picking one otherwise
        // would select something the user can't see on the map.
        if (isUnfundedVisible?.()) return all;
        return all.filter((project) => statusOf(project) !== "Unselected");
    }

    function results() {
        const q = query.trim().toLowerCase();
        if (!q) return [];
        return projects()
            .filter((project) => String(formatProjectSeq(sequenceNoOf(project), statusOf(project)) ?? "").toLowerCase().includes(q))
            .slice(0, 20);
    }

    function renderResults() {
        const list = root.querySelector(".gis-search-results");
        if (!list) return;
        const matches = results();
        list.innerHTML = matches.map((project) => {
            const id = projectIdOf(project);
            const seq = formatProjectSeq(sequenceNoOf(project), statusOf(project));
            return `<button type="button" class="gis-search-result" data-project-id="${escapeHtml(id)}">${escapeHtml(seq)}</button>`;
        }).join("");
        list.hidden = matches.length === 0;
        list.querySelectorAll(".gis-search-result").forEach((button) => {
            button.addEventListener("click", () => {
                onSelectProject?.(Number(button.dataset.projectId));
                expanded = false;
                query = "";
                render();
            });
        });
    }

    function render() {
        if (!expanded) {
            root.innerHTML = `
                <button type="button" class="gis-search-toggle" title="Search project" aria-label="Search project">
                    <span class="gis-search-icon" aria-hidden="true">⌕</span>
                </button>`;
            root.querySelector(".gis-search-toggle")?.addEventListener("click", () => {
                expanded = true;
                render();
                root.querySelector(".gis-search-input")?.focus();
            });
            return;
        }

        root.innerHTML = `
            <div class="gis-search-expanded">
                <input type="search" class="gis-search-input" placeholder="Search Project ID" value="${escapeHtml(query)}" autocomplete="off" />
                <div class="gis-search-results" hidden></div>
            </div>`;

        const input = root.querySelector(".gis-search-input");
        input?.addEventListener("input", (event) => {
            query = event.target.value;
            renderResults();
        });
        renderResults();
        input?.focus();
    }

    document.addEventListener("mousedown", (event) => {
        if (!expanded || root.contains(event.target)) return;
        expanded = false;
        query = "";
        render();
    });

    render();

    return {
        refresh() {
            if (expanded) renderResults();
        },
    };
}
