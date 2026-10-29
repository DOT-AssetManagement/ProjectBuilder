namespace ProjectBuilder.Models.Scenario.Dtos
{
    /// <summary>
    /// Data for the EditScenario → Scenario Settings tab. FirstYear/LastYear are
    /// stored as scenario parameters (YFST / YLST), not columns on the scenario.
    /// </summary>
    public class ScenarioSettingsDto
    {
        public int ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public string ScenarioNotes { get; set; }
        public bool Locked { get; set; }
        public bool Stale { get; set; }
        public bool IsShared { get; set; }
        public int? FirstYear { get; set; }
        public int? LastYear { get; set; }
    }
}
