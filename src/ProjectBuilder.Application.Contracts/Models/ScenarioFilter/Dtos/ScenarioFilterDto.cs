namespace ProjectBuilder.Models.ScenarioFilter.Dtos
{
    // Posted from the Analysis Method criteria builder (one row per condition).
    public class ScenarioFilterDto
    {
        public int EntityId { get; set; }   // ScenId
        public int FilterNo { get; set; }
        public string AssetType { get; set; }
        public string AttributeName { get; set; }
        public string Operation { get; set; }
        public string TextValue { get; set; }
    }
}
