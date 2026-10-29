export function createInventoryInfoPanel(root, { onClose } = {}) {
    if (!root) {
        return {
            open() {},
            close() {},
            isOpen: () => false,
        };
    }

    const closeBtn = document.createElement("button");
    closeBtn.type = "button";
    closeBtn.className = "gis-inventory-panel-close";
    closeBtn.setAttribute("aria-label", "Close inventory details");
    closeBtn.textContent = "×";

    const titleEl = document.createElement("h3");
    titleEl.className = "gis-inventory-panel-title";

    const bodyEl = document.createElement("div");
    bodyEl.className = "gis-inventory-panel-body";

    root.appendChild(closeBtn);
    root.appendChild(titleEl);
    root.appendChild(bodyEl);

    closeBtn.addEventListener("click", () => {
        root.hidden = true;
        bodyEl.replaceChildren();
        onClose?.();
    });

    return {
        open(info) {
            if (!info) return;
            titleEl.textContent = info.title ?? "Inventory feature";
            bodyEl.replaceChildren();

            if (!info.rows?.length) {
                const empty = document.createElement("p");
                empty.className = "gis-inventory-panel-empty";
                empty.textContent = "No attributes available for this feature.";
                bodyEl.appendChild(empty);
            } else {
                info.rows.forEach(({ label, value }) => {
                    const row = document.createElement("div");
                    row.className = "gis-inventory-row";

                    const labelEl = document.createElement("span");
                    labelEl.className = "gis-inventory-label";
                    labelEl.textContent = label;

                    const valueEl = document.createElement("span");
                    valueEl.className = "gis-inventory-value";
                    valueEl.textContent = value;

                    row.append(labelEl, valueEl);
                    bodyEl.appendChild(row);
                });
            }

            root.hidden = false;
        },
        close() {
            root.hidden = true;
            bodyEl.replaceChildren();
        },
        isOpen() {
            return !root.hidden;
        },
    };
}
