import { projectIdOf } from "../../map/treatments/helpers.js";
import { escapeHtml } from "../../map/utils/html.js";

// DSS project fields are camelCased by ABP; keep PascalCase fallbacks (see projectPanel.js).
function projectNoOf(project) {
    return project?.projectNo ?? project?.ProjectNo;
}

export function createDssProjectSearch(root, { getMapData, onSelectProject }) {
    if (!root) {
        return { refresh() {} };
    }

    let expanded = false;
    let query = "";

    function projects() {
        const data = getMapData?.() ?? {};
        return (data.projects ?? data.Projects ?? []).slice();
    }

    function results() {
        const q = query.trim().toLowerCase();
        if (!q) return [];
        return projects()
            .filter((project) => String(projectNoOf(project) ?? "").toLowerCase().includes(q))
            .slice(0, 20);
    }

    function renderResults() {
        const list = root.querySelector(".gis-search-results");
        if (!list) return;
        const matches = results();
        list.innerHTML = matches.map((project) => {
            const id = projectIdOf(project);
            const no = projectNoOf(project);
            return `<button type="button" class="gis-search-result" data-project-id="${escapeHtml(id)}">${escapeHtml(no)}</button>`;
        }).join("");
        list.hidden = matches.length === 0;
        list.querySelectorAll(".gis-search-result").forEach((button) => {
            button.addEventListener("click", () => {
                onSelectProject?.(button.dataset.projectId);
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
                <input type="search" class="gis-search-input" placeholder="Search Project No" value="${escapeHtml(query)}" autocomplete="off" />
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
