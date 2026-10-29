using System;

namespace ProjectBuilder.Models
{
    public class TreatmentSearchModel
    {
        public string AssetType { get; set; }
        public byte? Cnty { get; set; }
        public byte? District { get; set; }
        public Guid? LibraryId { get; set; }
        public int? Route { get; set; }
        public int? ProjectId { get; set; }
        public int? BridgeToSection { get; set; }
        public string Category { get; set; }
        public int? Year { get; set; }
        public int? ScenarioId { get; set; }
        public int? FromSection { get; set; }
        public int? ToSection { get; set; }
        public bool? Direction { get; set; }
        // The bottom "Project Treatments" editor bar on the Map page sets this true so an
        // unselected/not-funded project still shows its treatments; the scenario-wide
        // "final plan" report (LoadScenarioTreatments) leaves it false.
        public bool IncludeUnselected { get; set; }

    }
}
