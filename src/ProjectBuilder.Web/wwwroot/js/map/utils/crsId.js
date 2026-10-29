/** PennDOT CRS: CTY (2) _ ST_RT_NO (4) _ SIDE_IND (2) _ SEG_NO (4) */
export function formatCrsId(cty, route, sideInd, segNo) {
    const ctyStr = String(cty ?? "").trim().padStart(2, "0");
    const routeStr = String(route ?? "").trim().padStart(4, "0");
    const sideStr = String(sideInd ?? "0").trim().padStart(2, "0");
    const segStr = String(segNo ?? "").trim().padStart(4, "0");
    return `${ctyStr}_${routeStr}_${sideStr}_${segStr}`;
}

export function resolveSideIndFromSegments(segments, fromSection) {
    if (!segments?.length) return "00";

    const fromPadded = String(fromSection ?? "").padStart(4, "0");
    const atFrom = segments.find((segment) => {
        const seg = String(segment.segNo ?? "").padStart(4, "0");
        const side = segment.sideInd;
        return seg === fromPadded && side != null && String(side).trim() !== "";
    });
    const pick = atFrom
        ?? segments.find((segment) => segment.sideInd != null && String(segment.sideInd).trim() !== "")
        ?? segments[0];

    return String(pick?.sideInd ?? "0").padStart(2, "0");
}

export function resolveCrsIdFromPavementAttrs(attrs) {
    if (!attrs) return "";
    if (attrs.CRS_ID != null && String(attrs.CRS_ID).trim() !== "") {
        return String(attrs.CRS_ID);
    }
    return formatCrsId(
        attrs.CountyCode ?? attrs.CTY_CODE ?? attrs.Cnty,
        attrs.Route ?? attrs.ST_RT_NO ?? attrs.Rte,
        attrs.SIDE_IND,
        attrs.SectionFrom ?? attrs.FromSection ?? attrs.SEG_NO ?? attrs.Section,
    );
}
