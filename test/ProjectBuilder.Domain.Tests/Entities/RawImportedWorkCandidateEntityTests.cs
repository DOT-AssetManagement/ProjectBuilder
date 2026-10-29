using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class RawImportedWorkCandidateEntityTests
{
    [Fact]
    public void Should_Get_ProjectID()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.ProjectID.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ProjectID()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.ProjectID = "PROJ001";
        entity.ProjectID.ShouldBe("PROJ001");
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.District = 5;
        entity.District.ShouldBe((byte?)5);
    }

    [Fact]
    public void Should_Get_County()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.County.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_County()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.County = "Allegheny";
        entity.County.ShouldBe("Allegheny");
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Route.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Route = 30;
        entity.Route.ShouldBe(30);
    }

    [Fact]
    public void Should_Get_FromSection()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.FromSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_FromSection()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.FromSection = 100;
        entity.FromSection.ShouldBe(100);
    }

    [Fact]
    public void Should_Get_ToSection()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.ToSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ToSection()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.ToSection = "200";
        entity.ToSection.ShouldBe("200");
    }

    [Fact]
    public void Should_Get_BRKEY()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.BRKEY.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BRKEY()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.BRKEY = "BR001";
        entity.BRKEY.ShouldBe("BR001");
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Cost = 12.5f;
        entity.Cost.ShouldBe(12.5f);
    }

    [Fact]
    public void Should_Get_Treatment()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Treatment.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Treatment()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Treatment = "Overlay";
        entity.Treatment.ShouldBe("Overlay");
    }

    [Fact]
    public void Should_Get_MinYear()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.MinYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MinYear()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.MinYear = 2020;
        entity.MinYear.ShouldBe(2020);
    }

    [Fact]
    public void Should_Get_MaxYear()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.MaxYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MaxYear()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.MaxYear = 2030;
        entity.MaxYear.ShouldBe(2030);
    }

    [Fact]
    public void Should_Get_Year()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Year.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Year()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Year = 2025;
        entity.Year.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_BRIDGE_ID()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.BRIDGE_ID.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BRIDGE_ID()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.BRIDGE_ID = "BID001";
        entity.BRIDGE_ID.ShouldBe("BID001");
    }

    [Fact]
    public void Should_Get_HSIP_Eligible()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.HSIP_Eligible.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_HSIP_Eligible()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.HSIP_Eligible = 1.0f;
        entity.HSIP_Eligible.ShouldBe(1.0f);
    }

    [Fact]
    public void Should_Get_Excess_Safety_Value()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Excess_Safety_Value.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Excess_Safety_Value()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Excess_Safety_Value = 2.5f;
        entity.Excess_Safety_Value.ShouldBe(2.5f);
    }

    [Fact]
    public void Should_Get_Supported_by_RSA_Safety_Study()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Supported_by_RSA_Safety_Study.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Supported_by_RSA_Safety_Study()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Supported_by_RSA_Safety_Study = 1;
        entity.Supported_by_RSA_Safety_Study.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Degree_of_Safety_Improvement()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Degree_of_Safety_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Degree_of_Safety_Improvement()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Degree_of_Safety_Improvement = 3;
        entity.Degree_of_Safety_Improvement.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_Supports_Hazard_Mitigation()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Supports_Hazard_Mitigation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Supports_Hazard_Mitigation()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Supports_Hazard_Mitigation = 1;
        entity.Supports_Hazard_Mitigation.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Improves_Access_to_Emergency_Services()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Improves_Access_to_Emergency_Services.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Improves_Access_to_Emergency_Services()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Improves_Access_to_Emergency_Services = 1;
        entity.Improves_Access_to_Emergency_Services.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_ADT()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.ADT.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ADT()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.ADT = 5000f;
        entity.ADT.ShouldBe(5000f);
    }

    [Fact]
    public void Should_Get_Percent_Trucks()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Percent_Trucks.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Percent_Trucks()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Percent_Trucks = 12.5f;
        entity.Percent_Trucks.ShouldBe(12.5f);
    }

    [Fact]
    public void Should_Get_LFAR_BOF_Eligible()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.LFAR_BOF_Eligible.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LFAR_BOF_Eligible()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.LFAR_BOF_Eligible = 1;
        entity.LFAR_BOF_Eligible.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_BPN()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.BPN.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BPN()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.BPN = 3;
        entity.BPN.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_Length()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Length.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Length()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Length = 2.5f;
        entity.Length.ShouldBe(2.5f);
    }

    [Fact]
    public void Should_Get_Speed_Limit()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Speed_Limit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Speed_Limit()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Speed_Limit = 55f;
        entity.Speed_Limit.ShouldBe(55f);
    }

    [Fact]
    public void Should_Get_Population_Density()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Population_Density.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Population_Density()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Population_Density = 250f;
        entity.Population_Density.ShouldBe(250f);
    }

    [Fact]
    public void Should_Get_Bike_Commute_Share()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Bike_Commute_Share.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bike_Commute_Share()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Bike_Commute_Share = 0.05f;
        entity.Bike_Commute_Share.ShouldBe(0.05f);
    }

    [Fact]
    public void Should_Get_Functional_Classification()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Functional_Classification.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Functional_Classification()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Functional_Classification = 2;
        entity.Functional_Classification.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_National_Highway_System()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.National_Highway_System.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_National_Highway_System()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.National_Highway_System = 1;
        entity.National_Highway_System.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Pennsylvania_Byway()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Pennsylvania_Byway.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Pennsylvania_Byway()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Pennsylvania_Byway = "Yes";
        entity.Pennsylvania_Byway.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Pavement_Rehabilitated_or_Reconstructed()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Pavement_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Pavement_Rehabilitated_or_Reconstructed()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Pavement_Rehabilitated_or_Reconstructed = 1.5f;
        entity.Pavement_Rehabilitated_or_Reconstructed.ShouldBe(1.5f);
    }

    [Fact]
    public void Should_Get_Bridges_Rehabilitated_or_Replaced()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Bridges_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bridges_Rehabilitated_or_Replaced()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Bridges_Rehabilitated_or_Replaced = 2.0f;
        entity.Bridges_Rehabilitated_or_Replaced.ShouldBe(2.0f);
    }

    [Fact]
    public void Should_Get_Culverts_Rehabilitated_or_Replaced()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Culverts_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Culverts_Rehabilitated_or_Replaced()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Culverts_Rehabilitated_or_Replaced = 1.0f;
        entity.Culverts_Rehabilitated_or_Replaced.ShouldBe(1.0f);
    }

    [Fact]
    public void Should_Get_Guardrail_Rehabilitated_or_Replaced()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Guardrail_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Guardrail_Rehabilitated_or_Replaced()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Guardrail_Rehabilitated_or_Replaced = 0.5f;
        entity.Guardrail_Rehabilitated_or_Replaced.ShouldBe(0.5f);
    }

    [Fact]
    public void Should_Get_Geotechnical_Assets_Rehabilitated_or_Replaced()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Geotechnical_Assets_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Geotechnical_Assets_Rehabilitated_or_Replaced()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Geotechnical_Assets_Rehabilitated_or_Replaced = 0.2f;
        entity.Geotechnical_Assets_Rehabilitated_or_Replaced.ShouldBe(0.2f);
    }

    [Fact]
    public void Should_Get_Facilities_Rehabilitated_or_Reconstructed()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Facilities_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilities_Rehabilitated_or_Reconstructed()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Facilities_Rehabilitated_or_Reconstructed = 0.1f;
        entity.Facilities_Rehabilitated_or_Reconstructed.ShouldBe(0.1f);
    }

    [Fact]
    public void Should_Get_Sidewalks_Rehabilitated_or_Reconstructed()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Sidewalks_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Sidewalks_Rehabilitated_or_Reconstructed()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Sidewalks_Rehabilitated_or_Reconstructed = 0.5f;
        entity.Sidewalks_Rehabilitated_or_Reconstructed.ShouldBe(0.5f);
    }

    [Fact]
    public void Should_Get_Degree_of_Environmental_Improvement()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Degree_of_Environmental_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Degree_of_Environmental_Improvement()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Degree_of_Environmental_Improvement = 3.0f;
        entity.Degree_of_Environmental_Improvement.ShouldBe(3.0f);
    }

    [Fact]
    public void Should_Get_Reduced_Flood_Closure_Risk()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Reduced_Flood_Closure_Risk.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Reduced_Flood_Closure_Risk()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Reduced_Flood_Closure_Risk = 0.8f;
        entity.Reduced_Flood_Closure_Risk.ShouldBe(0.8f);
    }

    [Fact]
    public void Should_Get_Detour_Distance()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Detour_Distance.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Detour_Distance()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Detour_Distance = 5.0f;
        entity.Detour_Distance.ShouldBe(5.0f);
    }

    [Fact]
    public void Should_Get_Wetlands_Improved()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Wetlands_Improved.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Wetlands_Improved()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Wetlands_Improved = 1.0f;
        entity.Wetlands_Improved.ShouldBe(1.0f);
    }

    [Fact]
    public void Should_Get_Wildlife_Crossings()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Wildlife_Crossings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Wildlife_Crossings()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Wildlife_Crossings = 2.0f;
        entity.Wildlife_Crossings.ShouldBe(2.0f);
    }

    [Fact]
    public void Should_Get_Reduced_Fuel_Consumption()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Reduced_Fuel_Consumption.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Reduced_Fuel_Consumption()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Reduced_Fuel_Consumption = 3.0f;
        entity.Reduced_Fuel_Consumption.ShouldBe(3.0f);
    }

    [Fact]
    public void Should_Get_Consistency_with_Land_Use_Plans()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Consistency_with_Land_Use_Plans.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Consistency_with_Land_Use_Plans()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Consistency_with_Land_Use_Plans = "Yes";
        entity.Consistency_with_Land_Use_Plans.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Sidewalks_and_Curb_Ramps_Constructed_or_Improved()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Sidewalks_and_Curb_Ramps_Constructed_or_Improved()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved = 0.3f;
        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved.ShouldBe(0.3f);
    }

    [Fact]
    public void Should_Get_Change_in_Accessible_Destinations()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Change_in_Accessible_Destinations.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Change_in_Accessible_Destinations()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Change_in_Accessible_Destinations = 5;
        entity.Change_in_Accessible_Destinations.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations = 0.15f;
        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations.ShouldBe(0.15f);
    }

    [Fact]
    public void Should_Get_Support_for_Environmental_Justice()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Environmental_Justice.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Environmental_Justice()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Environmental_Justice = "Yes";
        entity.Support_for_Environmental_Justice.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Travel_Time_Savings()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Travel_Time_Savings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Travel_Time_Savings()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Travel_Time_Savings = 4.5f;
        entity.Travel_Time_Savings.ShouldBe(4.5f);
    }

    [Fact]
    public void Should_Get_Operating_Cost_Savings()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Operating_Cost_Savings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Operating_Cost_Savings()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Operating_Cost_Savings = 6.0f;
        entity.Operating_Cost_Savings.ShouldBe(6.0f);
    }

    [Fact]
    public void Should_Get_Degree_of_Mobility_Improvement()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Degree_of_Mobility_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Degree_of_Mobility_Improvement()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Degree_of_Mobility_Improvement = 7.0f;
        entity.Degree_of_Mobility_Improvement.ShouldBe(7.0f);
    }

    [Fact]
    public void Should_Get_Freight_Improvement()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Freight_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Freight_Improvement()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Freight_Improvement = 8.0f;
        entity.Freight_Improvement.ShouldBe(8.0f);
    }

    [Fact]
    public void Should_Get_Facilitates_Transit_Use()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Facilitates_Transit_Use.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilitates_Transit_Use()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Facilitates_Transit_Use = 1;
        entity.Facilitates_Transit_Use.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Facilitates_Active_Transportation()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Facilitates_Active_Transportation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilitates_Active_Transportation()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Facilitates_Active_Transportation = 1;
        entity.Facilitates_Active_Transportation.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Bike_Lanes_Constructed()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Bike_Lanes_Constructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bike_Lanes_Constructed()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Bike_Lanes_Constructed = 2;
        entity.Bike_Lanes_Constructed.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_Support_for_Job_Growth()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Job_Growth.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Job_Growth()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Job_Growth = 1;
        entity.Support_for_Job_Growth.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Support_for_Tourism()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Tourism.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Tourism()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Tourism = 1;
        entity.Support_for_Tourism.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Support_for_Economic_Goals()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Economic_Goals.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Economic_Goals()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Economic_Goals = 1;
        entity.Support_for_Economic_Goals.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Support_for_Recreation()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Recreation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Recreation()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Support_for_Recreation = 1;
        entity.Support_for_Recreation.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Consistency_with_Existing_Plans()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Consistency_with_Existing_Plans.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Consistency_with_Existing_Plans()
    {
        var entity = new RawImportedWorkCandidateEntity();
        entity.Consistency_with_Existing_Plans = 1;
        entity.Consistency_with_Existing_Plans.ShouldBe(1);
    }
}
