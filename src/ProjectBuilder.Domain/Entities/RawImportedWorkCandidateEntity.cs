using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Entities
{
    public class RawImportedWorkCandidateEntity
    {
        public string? ProjectID { get; set; }
        public byte? District { get; set; }
        public string? County { get; set; }
        public int? Route { get; set; }
        public int? FromSection { get; set; }
        public string? ToSection { get; set; }
        public string? BRKEY { get; set; }
        public string? BRIDGE_ID { get; set; }
        public string? AssetType { get; set; }
        public string? Description { get; set; }
        public float? Cost { get; set; }
        public float? HSIP_Eligible { get; set; }
        public float? Excess_Safety_Value { get; set; }
        public int? Supported_by_RSA_Safety_Study { get; set; }
        public int? Degree_of_Safety_Improvement { get; set; }
        public int? Supports_Hazard_Mitigation { get; set; }
        public int? Improves_Access_to_Emergency_Services { get; set; }
        public float? ADT { get; set; }
        public float? Percent_Trucks { get; set; }
        public int? LFAR_BOF_Eligible { get; set; }
        public int? BPN { get; set; }
        public float? Length { get; set; }
        public float? Speed_Limit { get; set; }
        public float? Population_Density { get; set; }
        public float? Bike_Commute_Share { get; set; }
        public int? Functional_Classification { get; set; }
        public int? National_Highway_System { get; set; }
        public string? Pennsylvania_Byway { get; set; }
        public float? Pavement_Rehabilitated_or_Reconstructed { get; set; }
        public float? Bridges_Rehabilitated_or_Replaced { get; set; }
        public float? Culverts_Rehabilitated_or_Replaced { get; set; }
        public float? Guardrail_Rehabilitated_or_Replaced { get; set; }
        public float? Geotechnical_Assets_Rehabilitated_or_Replaced { get; set; }
        public float? Facilities_Rehabilitated_or_Reconstructed { get; set; }
        public float? Sidewalks_Rehabilitated_or_Reconstructed { get; set; }
        public float? Degree_of_Environmental_Improvement { get; set; }
        public float? Reduced_Flood_Closure_Risk { get; set; }
        public float? Detour_Distance { get; set; }
        public float? Wetlands_Improved { get; set; }
        public float? Wildlife_Crossings { get; set; }
        public float? Reduced_Fuel_Consumption { get; set; }
        public string? Consistency_with_Land_Use_Plans { get; set; }
        public float? Sidewalks_and_Curb_Ramps_Constructed_or_Improved { get; set; }
        public int? Change_in_Accessible_Destinations { get; set; }
        public float? Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations { get; set; }
        public string? Support_for_Environmental_Justice { get; set; }
        public float? Travel_Time_Savings { get; set; }
        public float? Operating_Cost_Savings { get; set; }
        public float? Degree_of_Mobility_Improvement { get; set; }
        public float? Freight_Improvement { get; set; }
        public int? Facilitates_Transit_Use { get; set; }
        public int? Facilitates_Active_Transportation { get; set; }
        public int? Bike_Lanes_Constructed { get; set; }
        public int? Support_for_Job_Growth { get; set; }
        public int? Support_for_Tourism { get; set; }
        public int? Support_for_Economic_Goals { get; set; }
        public int? Support_for_Recreation { get; set; }
        public int? Consistency_with_Existing_Plans { get; set; }
        public string? Treatment { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public int? Year { get; set; }
    }
}
