import { symbols } from "../symbolConfig.js?v=2";

const HELMET_ICON_URL = symbols.treatment.icon;

function createCountBadge(index, total) {
    if (index == null || total == null || total <= 0) return null;

    const wrap = document.createElement("span");
    wrap.className = "gis-map-tooltip-count";

    const icon = document.createElement("img");
    icon.src = HELMET_ICON_URL;
    icon.alt = "";
    icon.setAttribute("aria-hidden", "true");
    icon.width = 16;
    icon.height = 16;

    const text = document.createElement("span");
    text.textContent = `${index} of ${total}`;

    wrap.append(icon, text);
    return wrap;
}

export function createMapTooltip(root) {
    if (!root) {
        return { show() {}, hide() {} };
    }

    const card = document.createElement("div");
    card.className = "gis-map-tooltip-card";
    card.hidden = true;
    root.appendChild(card);

    return {
        show(item) {
            if (!item) {
                card.hidden = true;
                return;
            }

            card.replaceChildren();
            card.style.left = `${item.x}px`;
            card.style.top = `${item.y}px`;

            const row = document.createElement("div");
            row.className = "gis-map-tooltip-row";

            const label = document.createElement("span");
            label.className = "gis-map-tooltip-label";
            label.textContent = item.text ?? "";
            row.appendChild(label);

            const badge = createCountBadge(item.treatmentIndex, item.treatmentTotal);
            if (badge) row.appendChild(badge);

            card.appendChild(row);
            card.hidden = false;
        },
        hide() {
            card.hidden = true;
            card.replaceChildren();
        },
    };
}
