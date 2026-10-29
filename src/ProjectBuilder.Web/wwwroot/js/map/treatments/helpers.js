import { formatCrsId } from "../utils/crsId.js";

export function treatmentType(treatment) {
    return (treatment.treatmentType ?? treatment.TreatType ?? "").toString().trim().charAt(0).toUpperCase();
}

export function projectIdOf(item) {
    return item.projectId ?? item.ProjectId ?? item.ProjId ?? item.ProjectID;
}

// Display-only sequential number — projectId stays the real key used for matching/navigation.
export function sequenceNoOf(item) {
    return item.sequenceNo ?? item.SequenceNo;
}

// Sequence number as shown to users, e.g. "PB-123" for a funded project, "UF-123" for an unfunded one.
export function formatProjectSeq(seq, status) {
    if (seq === null || seq === undefined || seq === "") return null;
    return status === "Unselected" ? `UF-${seq}` : `PB-${seq}`;
}

export function treatmentIdOf(treatment) {
    return treatment.treatmentId ?? treatment.TreatmentId ?? treatment.TreatId;
}

export function treatmentLabel(treatment) {
    return treatment.appliedTreatment ?? treatment.AppliedTreatment ?? treatment.treatment ?? "";
}

// ── Display accessors (tolerant of camelCase / PascalCase / odd BRIDGE_ID casing) ──
export function routeOf(t) {
    return t.route ?? t.Route ?? t.Rte ?? "";
}

export function yearOf(t) {
    return t.year ?? t.Year ?? t.selectedYear ?? t.SelectedYear ?? "";
}

export function costOf(t) {
    return t.cost ?? t.Cost ?? null;
}

// Direct cost + indirect costs, matching the bottom "Project Treatments" grid's Total Cost.
export function totalCostOf(t) {
    return t.totalCost ?? t.TotalCost ?? costOf(t);
}

export function benefitOf(t) {
    return t.benefit ?? t.Benefit ?? null;
}

export function bcrOf(t) {
    return t.benefitCostRatio ?? t.BenefitCostRatio ?? null;
}

// Defaults to "Selected" for data predating the Status column (or fetched via a caller that
// doesn't request unselected rows), so existing behavior is unchanged when it's absent.
export function statusOf(t) {
    return t.status ?? t.Status ?? "Selected";
}

// Project-level HasCommitted ("Y"/"N"); empty when the payload doesn't carry it (e.g. DssMap).
export function hasCommittedOf(t) {
    return t.hasCommitted ?? t.HasCommitted ?? "";
}

export function districtOf(t) {
    return t.district ?? t.District ?? "";
}

export function countyNameOf(t) {
    return t.countyName ?? t.CountyName ?? t.county ?? t.County ?? "";
}

export function fromSectionOf(t) {
    return t.fromSection ?? t.FromSection ?? "";
}

export function toSectionOf(t) {
    return t.toSection ?? t.ToSection ?? "";
}

export function sectionLabel(t) {
    const from = fromSectionOf(t);
    const to = toSectionOf(t);
    if (from === "" && to === "") return "";
    return `${from} - ${to}`;
}

export function directionOf(t) {
    return t.direction ?? t.Direction ?? "";
}

export function interstateOf(t) {
    return t.interstate ?? t.Interstate ?? null;
}

export function brkeyOf(t) {
    return t.brkey ?? t.BRKEY ?? t.Brkey ?? "";
}

export function bridgeIdOf(t) {
    return t.bridgE_ID ?? t.BRIDGE_ID ?? t.bridgeId ?? t.BridgeId ?? null;
}

// No CRS column comes back from sp_pb_SelectTreatmentsForGIS, so derive it from Cnty/Route/
// FromSection using the app's PennDOT CRS format, approximating SIDE_IND as FromSection%2
// (same fallback UspGetScenarioResultsForGIS uses for its Direction column).
export function crsIdOf(t) {
    const county = t.county ?? t.County ?? null;
    const route = routeOf(t);
    const fromSection = fromSectionOf(t);
    if (county == null || route === "" || fromSection === "") return "";
    const side = Number.parseInt(String(fromSection), 10) % 2;
    return formatCrsId(county, route, side, fromSection);
}

export function projectYearOf(project) {
    return project?.year ?? project?.Year ?? "";
}

export function userNotesOf(project) {
    return project?.userNotes ?? project?.UserNotes ?? "";
}

export function projectNumBridges(project) {
    return project?.numBridges ?? project?.NumBridges ?? 0;
}

export function projectNumPave(project) {
    return project?.numPaveSections ?? project?.NumPaveSections ?? 0;
}

export function formatCurrency(value) {
    const num = Number(value);
    if (!Number.isFinite(num)) return "–";
    return num.toLocaleString("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 });
}

export function buildPavementClause(treatment) {
    const county = treatment.county ?? treatment.Cnty;
    const route = treatment.route ?? treatment.Rte;
    const fromSection = treatment.fromSection ?? treatment.FromSection;
    const toSection = treatment.toSection ?? treatment.ToSection;

    if (county == null || route == null || fromSection == null || toSection == null) {
        return null;
    }

    const countyStr = String(county).padStart(2, "0");
    const routeStr = String(route).padStart(4, "0");
    const fromNum = Number.parseInt(String(fromSection), 10);
    const toNum = Number.parseInt(String(toSection), 10);
    if (Number.isNaN(fromNum) || Number.isNaN(toNum)) return null;

    const lo = Math.min(fromNum, toNum);
    const hi = Math.max(fromNum, toNum);

    return {
        where: pavementRangeWhere(countyStr, routeStr, lo, hi),
        meta: {
            type: "P",
            treatment,
            county: countyStr,
            route: routeStr,
            fromNum: lo,
            toNum: hi,
        },
    };
}

// Single-line WHERE for one county/route section range; these are OR'd 1000 at a time, so whitespace adds up.
export function pavementRangeWhere(countyStr, routeStr, lo, hi) {
    const fromSecStr = String(lo).padStart(4, "0");
    const toSecStr = String(hi).padStart(4, "0");
    return `(CTY_CODE='${countyStr}' AND ST_RT_NO='${routeStr}' AND SEG_NO>='${fromSecStr}' AND SEG_NO<='${toSecStr}')`;
}

// Collapses treatment clauses into disjoint section ranges per county/route, so each road segment is queried once.
export function mergePavementRanges(clauses) {
    const rangesByRoad = new Map();
    for (const { meta } of clauses) {
        const roadKey = `${meta.county}|${meta.route}`;
        const ranges = rangesByRoad.get(roadKey);
        if (ranges) ranges.push([meta.fromNum, meta.toNum]);
        else rangesByRoad.set(roadKey, [[meta.fromNum, meta.toNum]]);
    }

    const wheres = [];
    for (const [roadKey, ranges] of rangesByRoad) {
        const [county, route] = roadKey.split("|");
        ranges.sort((a, b) => a[0] - b[0]);
        let [lo, hi] = ranges[0];
        for (let i = 1; i < ranges.length; i += 1) {
            const [nextLo, nextHi] = ranges[i];
            if (nextLo <= hi + 1) {
                hi = Math.max(hi, nextHi);
                continue;
            }
            wheres.push(pavementRangeWhere(county, route, lo, hi));
            [lo, hi] = ranges[i];
        }
        wheres.push(pavementRangeWhere(county, route, lo, hi));
    }
    return wheres;
}

export function uniqueBridgeKeys(brKeys) {
    const seen = new Set();
    const unique = [];

    for (const key of brKeys) {
        const normalized = String(key).trim();
        if (!normalized || seen.has(normalized)) continue;
        seen.add(normalized);
        unique.push(normalized);
    }

    return unique;
}

export function formatBrKeyInList(brKeys) {
    return brKeys
        .map((key) => {
            const value = String(key).trim();
            return /^\d+$/.test(value) ? value : `'${value.replace(/'/g, "''")}'`;
        })
        .join(",");
}
