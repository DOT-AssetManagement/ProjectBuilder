using System;
using System.Collections.Generic;

namespace ProjectBuilder.Models.Map.Dtos
{
    public class MapGisDataDto
    {
        public MapGisScenarioDto Scenario { get; set; } = new();
        public List<MapGisProjectDto> Projects { get; set; } = new();
        public List<MapGisTreatmentDto> Treatments { get; set; } = new();
    }

    public class MapGisScenarioDto
    {
        public int ScenId { get; set; }
        public string Name { get; set; }
        public string LibraryName { get; set; }
        public string LastRunBy { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string Notes { get; set; }
    }

    public class MapGisProjectDto
    {
        public int ProjectId { get; set; }
        public int? SequenceNo { get; set; }
        public long SchemaId { get; set; }
        public short ProjectType { get; set; }
        public int? Year { get; set; }
        public int NumBridges { get; set; }
        public int NumPaveSections { get; set; }
        public double TotalCost { get; set; }
        public string UserNotes { get; set; }
        public string Status { get; set; }
    }

    public class MapGisTreatmentDto
    {
        public int ProjectId { get; set; }
        public short ProjectType { get; set; }
        public long TreatmentId { get; set; }
        public string AppliedTreatment { get; set; }
        public string TreatmentType { get; set; }
        public short District { get; set; }
        public short County { get; set; }
        public int Route { get; set; }
        public short Direction { get; set; }
        public int FromSection { get; set; }
        public int ToSection { get; set; }
        public string BRKEY { get; set; }
        public long? BRIDGE_ID { get; set; }
        public string OwnerCode { get; set; }
        public string CountyName { get; set; }
        public string MPO { get; set; }
        public int? Year { get; set; }
        public double? Cost { get; set; }
        public double? TotalCost { get; set; }
        public double? Benefit { get; set; }
        public double? BenefitCostRatio { get; set; }
        public string Status { get; set; }
        // Project-level HasCommitted as "Y"/"N", same values as the Projects grid.
        public string HasCommitted { get; set; }
    }
}
