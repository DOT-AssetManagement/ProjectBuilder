/** Match a scenario graphic to its 1-based position among all project treatments. */
export function resolveProjectTreatmentPosition(treatments, attrs) {
    if (!treatments?.length || !attrs) return null;

    const total = treatments.length;
    const treatId = attrs.TreatId ?? attrs.TreatmentID ?? attrs.TreatmentId;
    const brkey = attrs.BRKEY;
    const treatmentId = attrs.TreatmentID ?? attrs.TreatmentId;

    const idx = treatments.findIndex((treatment) => {
        const rowTreatId = treatment.treatId ?? treatment.TreatId ?? treatment.TreatmentId;
        if (treatId != null && rowTreatId != null && Number(rowTreatId) === Number(treatId)) {
            return true;
        }
        const rowBrkey = treatment.brkey ?? treatment.BRKEY;
        if (brkey != null && rowBrkey != null && String(rowBrkey).trim() === String(brkey).trim()) {
            return true;
        }
        const rowTreatmentId = treatment.treatmentId ?? treatment.TreatmentId ?? treatment.TreatmentID;
        if (treatmentId != null && rowTreatmentId != null && String(rowTreatmentId) === String(treatmentId)) {
            return true;
        }
        return false;
    });

    if (idx < 0) return null;
    return { index: idx + 1, total };
}

export function getTreatmentsForProject(scenarioData, projectId) {
    const treatments = scenarioData?.treatments ?? scenarioData?.Treatments ?? [];
    const id = Number(projectId);
    return treatments.filter((treatment) => {
        const pid = treatment.projectId ?? treatment.ProjectId ?? treatment.ProjId ?? treatment.ProjectID;
        return Number(pid) === id;
    });
}

// Display-only sequence number for a real projectId, so hover tooltips agree with the project panel/search.
export function getSequenceNoForProject(scenarioData, projectId) {
    const projects = scenarioData?.projects ?? scenarioData?.Projects ?? [];
    const id = Number(projectId);
    const project = projects.find((p) => {
        const pid = p.projectId ?? p.ProjectId ?? p.ProjId ?? p.ProjectID;
        return Number(pid) === id;
    });
    return project ? (project.sequenceNo ?? project.SequenceNo) : null;
}
