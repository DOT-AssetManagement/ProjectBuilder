using System;
using System.Collections.Generic;

namespace ProjectBuilder.Models.DssMap.Dtos
{
    public class DssMapGisDataDto
    {
        public DssMapGisScenarioDto Scenario { get; set; } = new();
        public List<DssMapGisProjectDto> Projects { get; set; } = new();
        public List<DssMapGisWorkCandidateDto> WorkCandidates { get; set; } = new();
    }

    public class DssMapGisScenarioDto
    {
        public Guid ScenId { get; set; }
        public string Name { get; set; }
        public string PoolName { get; set; }
        public string User { get; set; }
        public DateTime? LastRun { get; set; }
        public double EfficiencyThreshold { get; set; }
    }

    public class DssMapGisProjectDto
    {
        public Guid ProjectId { get; set; }
        // The pool's own ProjectName (e.g. "4", "108", "Test") -- not a recomputed ordinal, so it
        // matches what Outputs/Reports/Projects shows for the same project. Can be non-numeric.
        public string ProjectNo { get; set; }
        public string PlanningPartner { get; set; }
        public byte? District { get; set; }
        public byte? Cnty { get; set; }
        public string County { get; set; }
        public string Description { get; set; }
        public string Treatment { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public int NumPave { get; set; }
        public int NumBridge { get; set; }
        public double? Cost { get; set; }
        public double? Benefit { get; set; }
        public double? Efficiency { get; set; }
    }

    public class DssMapGisWorkCandidateDto
    {
        public Guid ProjectId { get; set; }
        public Guid TreatmentId { get; set; }
        public int TreatNo { get; set; }
        public string Partner { get; set; }
        public string AssetType { get; set; }
        public string TreatmentType { get; set; }
        public string TreatmentName { get; set; }
        public byte? District { get; set; }
        // Numeric county code -- named "County" (not "Cnty") so it camelCases to "county",
        // matching map/treatments/helpers.js's buildPavementClause()/countyNameOf() accessors,
        // which this DTO's JSON is fed into unmodified on the client.
        public byte? County { get; set; }
        public string CountyName { get; set; }
        public int? Route { get; set; }
        public int? FromSection { get; set; }
        public int? ToSection { get; set; }
        public string BRKEY { get; set; }
        public long? BRIDGE_ID { get; set; }
        public string CRS { get; set; }
        public int? Direction { get; set; }
        public int? Year { get; set; }
        public double? Cost { get; set; }
        public double? Benefit { get; set; }
        public double? BCRatio { get; set; }
    }
}
