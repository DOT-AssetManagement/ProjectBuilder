namespace ProjectBuilder.Models.ProjectTreatment.Dtos
{
    // Reassigns one treatment from its current project to another project in the same scenario.
    public class MoveProjectTreatmentDto
    {
        public int ScenarioId { get; set; }
        public long TreatmentId { get; set; }
        public int TargetProjectId { get; set; }
    }
}
