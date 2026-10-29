import {
    MAX_RECORDS_PER_QUERY,
    MAX_QUERY_RETRIES,
    PARALLEL_REQUEST_LIMIT,
    RETRY_BASE_DELAY_MS,
} from "../config.js";
import { runWithConcurrency, sleep, withTimeout } from "../utils/async.js";
import { projectIdOf, treatmentIdOf } from "../treatments/helpers.js";

function isRetryableQueryError(error) {
    const message = `${error?.name ?? ""} ${error?.message ?? ""}`.toLowerCase();
    return (
        message.includes("request:server")
        || message.includes("timeout")
        || message.includes("unable to complete")
        || message.includes("rate")
        || message.includes("429")
        || message.includes("503")
    );
}

async function retryQuery(operation, label) {
    let lastError;

    for (let attempt = 0; attempt < MAX_QUERY_RETRIES; attempt += 1) {
        try {
            return await operation();
        } catch (error) {
            lastError = error;
            const canRetry = attempt < MAX_QUERY_RETRIES - 1 && isRetryableQueryError(error);
            if (!canRetry) break;
            const delay = RETRY_BASE_DELAY_MS * (2 ** attempt);
            console.warn(`${label} failed (attempt ${attempt + 1}/${MAX_QUERY_RETRIES}), retrying in ${delay}ms`, error);
            await sleep(delay);
        }
    }

    throw lastError;
}

export async function queryPaginatedFeatures(serviceUrl, whereClause, view, queryModule, outFields, onPageProgress) {
    let totalCount = 0;

    try {
        totalCount = await retryQuery(
            () => withTimeout(
                queryModule.executeForCount(serviceUrl, { where: whereClause }),
                "Feature count",
            ),
            "Feature count",
        );
    } catch (error) {
        console.warn("PennDOT count failed:", whereClause, error);
        return [];
    }

    const totalPages = Math.max(1, Math.ceil(totalCount / MAX_RECORDS_PER_QUERY));
    const pageIndexes = Array.from({ length: totalPages }, (_, index) => index);
    // Web Mercator is in meters, so 1 decimal (10cm) is visually lossless and roughly halves the coordinate payload.
    const precision = view.spatialReference.isWebMercator ? { geometryPrecision: 1 } : null;

    const pageFeatureGroups = await runWithConcurrency(
        pageIndexes,
        PARALLEL_REQUEST_LIMIT,
        async (page) => {
            try {
                const result = await retryQuery(
                    () => withTimeout(
                        queryModule.executeQueryJSON(serviceUrl, {
                            where: whereClause,
                            returnGeometry: true,
                            outFields,
                            outSR: view.spatialReference.wkid,
                            ...precision,
                            resultOffset: page * MAX_RECORDS_PER_QUERY,
                            resultRecordCount: MAX_RECORDS_PER_QUERY,
                        }),
                        `Feature page ${page + 1}/${totalPages}`,
                    ),
                    `Feature page ${page + 1}/${totalPages}`,
                );

                const features = result.features ?? [];
                onPageProgress?.(page + 1, totalPages, features.length);
                return features;
            } catch (error) {
                console.warn(`PennDOT page ${page + 1}/${totalPages} failed:`, whereClause, error);
                return [];
            }
        },
    );

    return pageFeatureGroups.flat();
}

export function matchPavementFeaturesToClauses(features, clauses) {
    const grouped = new Map();

    // Indexed by county/route so each feature is only tested against clauses on its own road.
    const clausesByRoad = new Map();
    for (const clause of clauses) {
        const roadKey = `${clause.meta.county}|${clause.meta.route}`;
        const roadClauses = clausesByRoad.get(roadKey);
        if (roadClauses) roadClauses.push(clause);
        else clausesByRoad.set(roadKey, [clause]);
    }

    for (const feature of features) {
        const attrs = feature.attributes ?? {};
        const featureCounty = String(attrs.CTY_CODE ?? "").padStart(2, "0");
        const featureRoute = String(attrs.ST_RT_NO ?? "").padStart(4, "0");
        const featureSegNum = Number.parseInt(String(attrs.SEG_NO ?? ""), 10);
        if (Number.isNaN(featureSegNum) || !feature.geometry) continue;

        const roadClauses = clausesByRoad.get(`${featureCounty}|${featureRoute}`);
        if (!roadClauses) continue;

        for (const clause of roadClauses) {
            const { meta } = clause;
            if (featureSegNum < meta.fromNum || featureSegNum > meta.toNum) continue;

            const key = `${projectIdOf(meta.treatment)}_${treatmentIdOf(meta.treatment)}`;
            const bucket = grouped.get(key) ?? { clause, geometries: [], segments: [] };
            bucket.geometries.push(feature.geometry);
            bucket.segments.push({
                sideInd: attrs.SIDE_IND,
                segNo: attrs.SEG_NO,
                condition: {
                    iriRating: attrs.IRI_RATING_TEXT,
                    iriYear: attrs.IRI_YEAR,
                    overallPavementIndex: attrs.OVERALL_PVMNT_IDX,
                    roughIndex: attrs.ROUGH_INDX,
                    opiRating: attrs.OPI_RATING_TEXT,
                    opiYear: attrs.OPI_YEAR,
                },
            });
            grouped.set(key, bucket);
        }
    }

    return grouped;
}
