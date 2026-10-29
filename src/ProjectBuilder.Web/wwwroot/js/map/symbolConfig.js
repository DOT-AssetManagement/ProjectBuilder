/**
 * Symbols for every map layer and the Map Key.
 * Change a color, line width, or icon here. The map and the Map Key both read this file.
 *
 * Bridges are icons (kind: "icon"). Lines stay colors (kind: "line").
 * The swatch on an icon entry is only the Map Key row and toast accent.
 * Unfunded scenario bridges use the unfunded icon until one is selected.
 */

const ICONS = "/images/map/icons";

/** Road and bridge condition colors. Bridge pictures below use these same fills. */
export const conditionColors = {
    good: "#91CC49",
    fair: "#FDD84E",
    poor: "#AB102E",
};

export const symbols = {
    scenarioBridge: {
        kind: "icon",
        label: "Bridge Projects",
        selectedLabel: "Selected Bridge Project",
        desc: "Scenario projects that include bridges.",
        selectedDesc: "The bridge project currently selected on the map.",
        swatch: "#2278c4",
        icon: `${ICONS}/bridge.svg`,
        selectedIcon: `${ICONS}/bridge-selected.svg`,
        size: 18,
        selectedSize: 21,
    },
    scenarioPavement: {
        kind: "line",
        label: "Pavements Project",
        selectedLabel: "Select Pavement Project",
        desc: "Scenario projects that include pavement.",
        selectedDesc: "Selected pavement on the map.",
        color: "#898F94",
        width: 3,
        halo: "#00FFFF",
        gentleBorder: "#898F94",
        gentleBorderWidth: 5,
        focus: "#4BE4E4",
        focusWidth: 5,
    },
    treatment: {
        kind: "icon",
        label: "Pavement Treatment Marker",
        desc: "Locations with a programmed pavement treatment.",
        swatch: "#ffc34d",
        icon: `${ICONS}/key-ptreat.svg`,
        selectedIcon: `${ICONS}/key-ptreat-selected.svg`,
        size: 22,
        yoffset: 11,
    },
    mpms: {
        kind: "line",
        label: "MPMS Projects Color",
        selectedLabel: "MPMS Projects Color When Highlighted",
        desc: "Projects programmed in MPMS.",
        selectedDesc: "MPMS projects highlighted on the map.",
        color: "#06A35C",
        width: 2,
        halo: "#00FFFF",
        statuses: {
            ACT: "#DF377F",
            PROG: "#002574",
        },
    },
    inventoryBridge: {
        kind: "icon",
        label: "Bridges: Adhoc/Inventory",
        selectedLabel: "Selected Bridges: Adhoc/Inventory",
        desc: "Bridges added ad hoc or from inventory.",
        selectedDesc: "Selected ad hoc or inventory bridges.",
        swatch: "#b76bf5",
        icon: `${ICONS}/bridge-adhoc.svg`,
        selectedIcon: `${ICONS}/bridge-adhoc-selected.svg`,
        size: 14,
        ratings: {
            good: `${ICONS}/bridge-green.svg`,
            fair: `${ICONS}/bridge-yellow.svg`,
            poor: `${ICONS}/bridge-red.svg`,
        },
    },
    unfunded: {
        kind: "icon",
        label: "Unfunded Bridge Projects",
        selectedLabel: "Unfunded Selected Bridge Project",
        desc: "Scenario bridge projects that are not funded.",
        selectedDesc: "The unfunded bridge project currently selected on the map.",
        swatch: "#9FC0F8",
        icon: `${ICONS}/unfunded-bridge.svg`,
        selectedIcon: `${ICONS}/unfunded-bridge-selected.svg`,
        size: 18,
        selectedSize: 21,
    },
    inventoryPavement: {
        kind: "line",
        label: "Pavement: Adhoc/Inventory",
        selectedLabel: "Selected Pavement: Adhoc/Inventory",
        desc: "Pavement segments added ad hoc or from inventory.",
        selectedDesc: "Selected ad hoc or inventory pavement segments.",
        color: "#BA67EF",
        width: 2,
        halo: "#00FFFF",
    },
    /** Session graphic drawn for an ad hoc pavement the user just added. */
    adhocPavement: {
        kind: "line",
        color: "#545454",
        width: 4,
    },
    unselectedPavement: {
        kind: "line",
        label: "Unfunded Pavements Project",
        selectedLabel: "Unfunded Selected Pavement Project",
        desc: "Scenario pavement projects that are not funded.",
        selectedDesc: "The unfunded pavement project currently selected on the map.",
        color: "#9FC0F8",
        width: 3,
        style: "dash",
        halo: "#00FFFF",
    },
    /** DSS pool pavement candidates. DSS bridges use scenarioBridge. */
    dssPavement: {
        kind: "line",
        color: "#545454",
        width: 3,
    },
};

export function hexToRgb(hex) {
    const n = parseInt(String(hex).replace("#", ""), 16);
    return [(n >> 16) & 255, (n >> 8) & 255, n & 255];
}

export function lineSymbol(entry, overrides = {}) {
    const style = overrides.style ?? entry.style;
    return {
        type: "simple-line",
        color: hexToRgb(overrides.color ?? entry.color),
        width: overrides.width ?? entry.width,
        ...(style ? { style } : {}),
    };
}

export function iconSymbol(url, size) {
    return {
        type: "picture-marker",
        url,
        width: size,
        height: size,
    };
}
