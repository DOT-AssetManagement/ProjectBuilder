/**
 * Exact Map Key glyph builders from projectbuilder-map-revised3.html mockup.
 * Keep these in sync with docs/mockups/projectbuilder-map-revised3.html.
 */

export const badge = (fill, inner, ring) =>
    `<svg viewBox="0 0 24 24" width="26" height="26" aria-hidden="true">
  <circle cx="12" cy="12" r="10" fill="${fill}" stroke="${ring || "#fff"}" stroke-width="1.6"/>${inner || ""}</svg>`;

export const ring = (fill, inner) =>
    `<svg viewBox="0 0 24 24" width="26" height="26" aria-hidden="true">
  <circle cx="12" cy="12" r="9.5" fill="${fill}" stroke="#4dd9dd" stroke-width="2.7"/>${inner}</svg>`;

export const DOT = `<circle cx="12" cy="12" r="4.6" fill="#fff"/>`;

export const pave = (c) =>
    `<rect x="5.8" y="5.6" width="3.7" height="12.8" fill="${c}"/><rect x="14.5" y="5.6" width="3.7" height="12.8" fill="${c}"/><rect x="11.2" y="5.8" width="1.7" height="3" rx=".5" fill="${c}"/><rect x="11.2" y="10.5" width="1.7" height="3" rx=".5" fill="${c}"/><rect x="11.2" y="15.2" width="1.7" height="3" rx=".5" fill="${c}"/>`;

export const pavInv = (c, bg) =>
    `<g fill="${c}"><rect x="9.6" y="4.4" width="4.8" height="15.2"/><rect x="6.9" y="4.4" width="1.5" height="15.2"/><rect x="15.6" y="4.4" width="1.5" height="15.2"/><rect x="6.3" y="4.4" width="2.7" height="1.05"/><rect x="6.3" y="18.55" width="2.7" height="1.05"/><rect x="15.0" y="4.4" width="2.7" height="1.05"/><rect x="15.0" y="18.55" width="2.7" height="1.05"/><rect x="4.5" y="13.4" width="0.95" height="2.3" rx=".35"/><circle cx="4.98" cy="16.9" r=".55"/><rect x="18.55" y="7.6" width="0.95" height="2.3" rx=".35"/><circle cx="19.03" cy="6.1" r=".55"/></g><g fill="${bg}"><rect x="11.55" y="6.2" width="0.9" height="2.2"/><rect x="11.55" y="10.9" width="0.9" height="2.2"/><rect x="11.55" y="15.6" width="0.9" height="2.2"/></g>`;

export const susp = (c) =>
    `<g fill="${c}"><path d="M3.3 10.3C4.7 10 6.4 8.8 7.4 6.1 9.3 10.3 14.7 10.3 16.6 6.1c1 2.7 2.7 3.9 4.1 4.2" fill="none" stroke="${c}" stroke-width="1.15" stroke-linecap="round" stroke-linejoin="round"/><path d="M6.4 12.1 7.4 6.1l1 6Z"/><path d="M15.6 12.1l1-6 1 6Z"/><rect x="3.4" y="12.1" width="17.2" height="2.5" rx=".8"/><rect x="6.8" y="14.8" width="1.7" height="3.2"/><rect x="15.5" y="14.8" width="1.7" height="3.2"/><rect x="5.9" y="17.8" width="3.5" height="1.2" rx=".4"/><rect x="14.6" y="17.8" width="3.5" height="1.2" rx=".4"/></g>`;

export const boxes = (c) =>
    `<g fill="none" stroke="${c}" stroke-width="1.35" stroke-linejoin="round"><rect x="8.4" y="3.6" width="7.2" height="7.2" rx="1.7"/><path d="M10.65 3.6v1.9a.75.75 0 0 0 .75.75h1.2a.75.75 0 0 0 .75-.75V3.6"/><rect x="4.4" y="11.6" width="7.2" height="7.2" rx="1.7"/><path d="M6.65 11.6v1.9a.75.75 0 0 0 .75.75h1.2a.75.75 0 0 0 .75-.75v-1.9"/><rect x="12.4" y="11.6" width="7.2" height="7.2" rx="1.7"/><path d="M14.65 11.6v1.9a.75.75 0 0 0 .75.75h1.2a.75.75 0 0 0 .75-.75v-1.9"/></g>`;

export const stripes = (c) =>
    `<g stroke="${c}" stroke-width="3.1" stroke-linecap="butt"><path d="M4.6 15.3 11.4 6.2"/><path d="M8.6 17.4 15.4 8.3"/><path d="M12.6 18.8 19.4 9.7"/></g>`;

export const roadT =
    `<svg viewBox="0 0 24 24" width="29" height="29" aria-hidden="true"><rect x="2" y="4.6" width="20" height="18.8" rx="4.4" fill="#4f7fab"/><g fill="#fff"><path d="M5.9 21.4 7.9 7.4h1.05L7.1 21.4Z"/><path d="M18.1 21.4 16.1 7.4h-1.05l1.85 14Z"/><rect x="11.2" y="18.3" width="1.6" height="2.8"/><rect x="11.3" y="14.3" width="1.4" height="2.5"/><rect x="11.37" y="10.7" width="1.26" height="2.2"/><rect x="11.44" y="7.5" width="1.12" height="1.9"/></g></svg>`;

export const bridgeT =
    `<svg viewBox="0 0 24 24" width="29" height="29" aria-hidden="true"><rect x="2" y="4.6" width="20" height="18.8" rx="4.4" fill="#4f7fab"/><g transform="translate(12 14.2) scale(.82) translate(-12 -11.7)">${susp("#fff")}</g></svg>`;

export const mpmsT =
    `<svg viewBox="0 0 24 24" width="29" height="29" aria-hidden="true"><rect x="2" y="4.6" width="20" height="18.8" rx="4.4" fill="#4f7fab"/><g fill="#fff"><rect x="14.55" y="8.4" width="1.15" height="12.6" rx=".2"/><rect x="14.4" y="7.5" width="1.45" height="1" rx=".35"/><path d="M8.5 11.05 10.35 9.7H17.8v2.7H10.35Z"/><path d="M12.9 13.9h4.9l1.75 1.3-1.75 1.35H12.9Z"/><path d="M8 10.4c.5 1 .9 1.5 1.3 2l2.9 7.2H3.9l2.85-7.2c.4-.5.75-1 1.25-2Z"/><rect x="3.1" y="19.5" width="9.7" height="1.6" rx=".65"/></g><g fill="#4f7fab"><rect x="11.4" y="10.6" width="3.7" height=".85" rx=".4"/><rect x="14.1" y="14.8" width="2.9" height=".8" rx=".38"/><rect x="6.95" y="13.15" width="2.1" height=".85"/><rect x="6.1" y="15.55" width="3.8" height=".9"/><rect x="5.2" y="17.95" width="5.6" height=".9"/></g></svg>`;

export const CATS = {
    proj: {
        label: "Projects",
        icon: badge("#2278c4", DOT, "#2278c4"),
        sel: ring("#2278c4", DOT),
    },
    pave: {
        label: "Pavements",
        icon: badge("#545454", pave("#fff"), "#545454"),
        sel: ring("#545454", pave("#fff")),
    },
    ptreat: {
        label: "Pavement Treatment Marker",
        icon: badge("#ffc34d", stripes("#fff"), "#ffc34d"),
        sel: ring("#ffc34d", stripes("#fff")),
    },
    mpms: {
        label: "MPMS Projects Color",
        icon: badge("#f4610d", boxes("#fff"), "#f4610d"),
        sel: ring("#f4610d", boxes("#fff")),
    },
    bridge: {
        label: "Bridges: Adhoc/Inventory",
        icon: badge("#b76bf5", susp("#fff"), "#b76bf5"),
        sel: ring("#b76bf5", susp("#fff")),
    },
    pinv: {
        label: "Pavement: Adhoc/Inventory",
        icon: badge("#545454", pavInv("#fff", "#545454"), "#545454"),
        sel: ring("#545454", pavInv("#fff", "#545454")),
    },
};

/** Standalone SVG documents for ArcGIS picture-marker URLs. */
export function svgDocument(innerSvgMarkup) {
    // innerSvgMarkup is a full <svg>...</svg>; ensure xmlns for ArcGIS fetch.
    if (innerSvgMarkup.includes("xmlns=")) return innerSvgMarkup;
    return innerSvgMarkup.replace("<svg ", '<svg xmlns="http://www.w3.org/2000/svg" ');
}
