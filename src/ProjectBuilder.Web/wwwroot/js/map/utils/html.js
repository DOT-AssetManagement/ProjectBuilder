// Encodes a value for HTML by letting the browser serialize a text node; quotes are encoded too so it is safe inside attributes
export function escapeHtml(value) {
    const holder = document.createElement("p");
    holder.appendChild(document.createTextNode(String(value ?? "")));
    return holder.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
}

// Replaces a <select>/<datalist>'s options with text-only <option> elements built from { value, text, selected } items
export function setOptions(listEl, items) {
    listEl.replaceChildren(...items.map((item) => {
        const selected = Boolean(item.selected);
        return new Option(String(item.text ?? ""), String(item.value ?? ""), selected, selected);
    }));
}
