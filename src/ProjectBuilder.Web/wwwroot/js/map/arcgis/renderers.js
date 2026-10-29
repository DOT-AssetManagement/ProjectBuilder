import { conditionColors, iconSymbol, lineSymbol, symbols } from "../symbolConfig.js?v=2";

export function getMPMSLayerRenderer(useRatingSymbology, selectedRatings = []) {
    if (!useRatingSymbology) {
        return {
            type: "simple",
            symbol: lineSymbol(symbols.mpms),
        };
    }

    const ratingColors = symbols.mpms.statuses;

    const buttonToRatings = {
        ACT: ["ACT"],
        PROG: ["PROG"],
    };

    const uniqueValueInfos = Object.entries(ratingColors).map(([rating, color]) => {
        const isSelected = selectedRatings.length === 0
            || selectedRatings.some((btn) => buttonToRatings[btn]?.includes(rating));

        return {
            value: rating,
            symbol: lineSymbol(symbols.mpms, { color: isSelected ? color : symbols.mpms.color }),
            label: rating,
        };
    });

    return {
        type: "unique-value",
        field: "PROJECT_STATUS",
        uniqueValueInfos,
        defaultSymbol: lineSymbol(symbols.mpms),
    };
}

export function getPavementBaseLayerRenderer(useOPIRatingSymbology, selectedRatings = []) {
    if (!useOPIRatingSymbology) {
        return {
            type: "simple",
            symbol: lineSymbol(symbols.inventoryPavement),
        };
    }

    const opiRatingColors = {
        EXCELLENT: conditionColors.good,
        GOOD: conditionColors.good,
        FAIR: conditionColors.fair,
        POOR: conditionColors.poor,
    };

    const buttonToRating = {
        good: ["GOOD", "EXCELLENT"],
        fair: ["FAIR"],
        poor: ["POOR"],
    };

    const uniqueValueInfos = Object.entries(opiRatingColors).map(([rating, color]) => {
        const isSelected = selectedRatings.length === 0
            || selectedRatings.some((btn) => buttonToRating[btn]?.includes(rating));

        return {
            value: rating,
            symbol: lineSymbol(symbols.inventoryPavement, {
                color: isSelected ? color : symbols.inventoryPavement.color,
            }),
            label: rating,
        };
    });

    return {
        type: "unique-value",
        field: "OPI_RATING_TEXT",
        uniqueValueInfos,
        defaultSymbol: lineSymbol(symbols.inventoryPavement),
    };
}

const BRIDGE_ICON = {
    purple: symbols.inventoryBridge.icon,
    G: symbols.inventoryBridge.ratings.good,
    F: symbols.inventoryBridge.ratings.fair,
    P: symbols.inventoryBridge.ratings.poor,
};

function inventoryBridgeIcon(url, size) {
    return iconSymbol(url, size);
}

export function getBridgeBaseLayerRenderer(useConditionSymbology, selectedRatings = [], size = 14) {
    const purple = inventoryBridgeIcon(BRIDGE_ICON.purple, size);
    const buttonToCondition = {
        good: ["G"],
        fair: ["F"],
        poor: ["P"],
    };

    const uniqueValueInfos = ["G", "F", "P", "N"].map((cond) => {
        const isSelected = useConditionSymbology
            && selectedRatings.some((btn) => buttonToCondition[btn]?.includes(cond));
        const url = isSelected ? BRIDGE_ICON[cond] : BRIDGE_ICON.purple;
        return {
            value: cond,
            symbol: inventoryBridgeIcon(url, size),
            label: cond,
        };
    });

    return {
        type: "unique-value",
        field: "CONDITION",
        uniqueValueInfos,
        defaultSymbol: purple,
    };
}
