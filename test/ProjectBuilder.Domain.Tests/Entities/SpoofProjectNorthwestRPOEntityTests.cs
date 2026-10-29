using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class SpoofProjectNorthwestRPOEntityTests
{
    [Fact]
    public void Should_Get_ProjectID()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.ProjectID.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ProjectID()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.ProjectID = "PROJ001";
        entity.ProjectID.ShouldBe("PROJ001");
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.District = 5;
        entity.District.ShouldBe((byte?)5);
    }

    [Fact]
    public void Should_Get_County()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.County.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_County()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.County = "Allegheny";
        entity.County.ShouldBe("Allegheny");
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Cost = 500000f;
        entity.Cost.ShouldBe(500000f);
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Route.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Route = 30;
        entity.Route.ShouldBe(30);
    }

    [Fact]
    public void Should_Get_BRKEY()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.BRKEY.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BRKEY()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.BRKEY = "BR001";
        entity.BRKEY.ShouldBe("BR001");
    }

    [Fact]
    public void Should_Get_Functional_Classification()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Functional_Classification.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Functional_Classification()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Functional_Classification = 2;
        entity.Functional_Classification.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_FromSection()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.FromSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_FromSection()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.FromSection = 100;
        entity.FromSection.ShouldBe(100);
    }

    [Fact]
    public void Should_Get_ToSection()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.ToSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ToSection()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.ToSection = "200";
        entity.ToSection.ShouldBe("200");
    }

    [Fact]
    public void Should_Get_BRIDGE_ID()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.BRIDGE_ID.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BRIDGE_ID()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.BRIDGE_ID = "BID001";
        entity.BRIDGE_ID.ShouldBe("BID001");
    }

    [Fact]
    public void Should_Get_HSIP_Eligible()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.HSIP_Eligible.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_HSIP_Eligible()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.HSIP_Eligible = 1.0f;
        entity.HSIP_Eligible.ShouldBe(1.0f);
    }

    [Fact]
    public void Should_Get_Excess_Safety_Value()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Excess_Safety_Value.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Excess_Safety_Value()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Excess_Safety_Value = 2.5f;
        entity.Excess_Safety_Value.ShouldBe(2.5f);
    }

    [Fact]
    public void Should_Get_Supported_by_RSA_Safety_Study()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Supported_by_RSA_Safety_Study.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Supported_by_RSA_Safety_Study()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Supported_by_RSA_Safety_Study = "Yes";
        entity.Supported_by_RSA_Safety_Study.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Degree_of_Safety_Improvement()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Degree_of_Safety_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Degree_of_Safety_Improvement()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Degree_of_Safety_Improvement = 3.0f;
        entity.Degree_of_Safety_Improvement.ShouldBe(3.0f);
    }

    [Fact]
    public void Should_Get_Supports_Hazard_Mitigation()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Supports_Hazard_Mitigation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Supports_Hazard_Mitigation()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Supports_Hazard_Mitigation = "Yes";
        entity.Supports_Hazard_Mitigation.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Improves_Access_to_Emergency_Sevices()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Improves_Access_to_Emergency_Sevices.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Improves_Access_to_Emergency_Sevices()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Improves_Access_to_Emergency_Sevices = "Yes";
        entity.Improves_Access_to_Emergency_Sevices.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_AADT()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.AADT.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AADT()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.AADT = 5000f;
        entity.AADT.ShouldBe(5000f);
    }

    [Fact]
    public void Should_Get_Percent_Trucks()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Percent_Trucks.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Percent_Trucks()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Percent_Trucks = 12.5f;
        entity.Percent_Trucks.ShouldBe(12.5f);
    }

    [Fact]
    public void Should_Get_LFAR_BOF_Eligible()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.LFAR_BOF_Eligible.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LFAR_BOF_Eligible()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.LFAR_BOF_Eligible = "Yes";
        entity.LFAR_BOF_Eligible.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_BPN()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.BPN.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BPN()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.BPN = "BPN001";
        entity.BPN.ShouldBe("BPN001");
    }

    [Fact]
    public void Should_Get_Population_Density()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Population_Density.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Population_Density()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Population_Density = 250f;
        entity.Population_Density.ShouldBe(250f);
    }

    [Fact]
    public void Should_Get_Bike_Commute_Share()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Bike_Commute_Share.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bike_Commute_Share()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Bike_Commute_Share = "5%";
        entity.Bike_Commute_Share.ShouldBe("5%");
    }

    [Fact]
    public void Should_Get_National_Highway_System()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.National_Highway_System.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_National_Highway_System()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.National_Highway_System = "Yes";
        entity.National_Highway_System.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Pennsylvania_Byway()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Pennsylvania_Byway.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Pennsylvania_Byway()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Pennsylvania_Byway = "Yes";
        entity.Pennsylvania_Byway.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Pavement_Rehabilitated_or_Reconstructed()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Pavement_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Pavement_Rehabilitated_or_Reconstructed()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Pavement_Rehabilitated_or_Reconstructed = 1.5f;
        entity.Pavement_Rehabilitated_or_Reconstructed.ShouldBe(1.5f);
    }

    [Fact]
    public void Should_Get_Bridges_Rehabilitated_or_Replaced()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Bridges_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bridges_Rehabilitated_or_Replaced()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Bridges_Rehabilitated_or_Replaced = 2.0f;
        entity.Bridges_Rehabilitated_or_Replaced.ShouldBe(2.0f);
    }

    [Fact]
    public void Should_Get_Reduced_Flood_Closure_Risk()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Reduced_Flood_Closure_Risk.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Reduced_Flood_Closure_Risk()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Reduced_Flood_Closure_Risk = 0.8f;
        entity.Reduced_Flood_Closure_Risk.ShouldBe(0.8f);
    }

    [Fact]
    public void Should_Get_Environmental_Benefit()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Environmental_Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Environmental_Benefit()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Environmental_Benefit = 5.0f;
        entity.Environmental_Benefit.ShouldBe(5.0f);
    }

    [Fact]
    public void Should_Get_Wetlands_Improved()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Wetlands_Improved.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Wetlands_Improved()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Wetlands_Improved = 1.0f;
        entity.Wetlands_Improved.ShouldBe(1.0f);
    }

    [Fact]
    public void Should_Get_Wildlife_Crossings()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Wildlife_Crossings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Wildlife_Crossings()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Wildlife_Crossings = 2.0f;
        entity.Wildlife_Crossings.ShouldBe(2.0f);
    }

    [Fact]
    public void Should_Get_Reduced_Fuel_Consumption()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Reduced_Fuel_Consumption.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Reduced_Fuel_Consumption()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Reduced_Fuel_Consumption = 3.0f;
        entity.Reduced_Fuel_Consumption.ShouldBe(3.0f);
    }

    [Fact]
    public void Should_Get_Consistency_with_Land_Use_Plans()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Consistency_with_Land_Use_Plans.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Consistency_with_Land_Use_Plans()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Consistency_with_Land_Use_Plans = "Yes";
        entity.Consistency_with_Land_Use_Plans.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Travel_Time_Savings()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Travel_Time_Savings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Travel_Time_Savings()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Travel_Time_Savings = 4.5f;
        entity.Travel_Time_Savings.ShouldBe(4.5f);
    }

    [Fact]
    public void Should_Get_Operating_Cost_Savings()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Operating_Cost_Savings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Operating_Cost_Savings()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Operating_Cost_Savings = 6.0f;
        entity.Operating_Cost_Savings.ShouldBe(6.0f);
    }

    [Fact]
    public void Should_Get_Degree_of_Mobility_Improvement()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Degree_of_Mobility_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Degree_of_Mobility_Improvement()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Degree_of_Mobility_Improvement = 7.0f;
        entity.Degree_of_Mobility_Improvement.ShouldBe(7.0f);
    }

    [Fact]
    public void Should_Get_Freight_Improvement()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Freight_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Freight_Improvement()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Freight_Improvement = 8.0f;
        entity.Freight_Improvement.ShouldBe(8.0f);
    }

    [Fact]
    public void Should_Get_Facilitiates_Transit_Use()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Facilitiates_Transit_Use.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilitiates_Transit_Use()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Facilitiates_Transit_Use = "Yes";
        entity.Facilitiates_Transit_Use.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Facilitiates_Active_Transportation()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Facilitiates_Active_Transportation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilitiates_Active_Transportation()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Facilitiates_Active_Transportation = "Yes";
        entity.Facilitiates_Active_Transportation.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Bike_Lanes_Constructed()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Bike_Lanes_Constructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bike_Lanes_Constructed()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Bike_Lanes_Constructed = 2;
        entity.Bike_Lanes_Constructed.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_Support_for_Job_Growth()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Job_Growth.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Job_Growth()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Job_Growth = "Yes";
        entity.Support_for_Job_Growth.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Support_for_Tourism()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Tourism.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Tourism()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Tourism = "Yes";
        entity.Support_for_Tourism.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Support_for_Economic_Goals()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Economic_Goals.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Economic_Goals()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Economic_Goals = "Yes";
        entity.Support_for_Economic_Goals.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Support_for_Recreation()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Recreation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Recreation()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Recreation = "Yes";
        entity.Support_for_Recreation.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Consistency_with_Existing_Plans()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Consistency_with_Existing_Plans.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Consistency_with_Existing_Plans()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Consistency_with_Existing_Plans = "Yes";
        entity.Consistency_with_Existing_Plans.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Sidewalks_Rehabilitated_or_Reconstructed()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Sidewalks_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Sidewalks_Rehabilitated_or_Reconstructed()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Sidewalks_Rehabilitated_or_Reconstructed = 0.5f;
        entity.Sidewalks_Rehabilitated_or_Reconstructed.ShouldBe(0.5f);
    }

    [Fact]
    public void Should_Get_Sidewalks_and_Curb_Ramps_Constructed_or_Improved()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Sidewalks_and_Curb_Ramps_Constructed_or_Improved()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved = 0.3f;
        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved.ShouldBe(0.3f);
    }

    [Fact]
    public void Should_Get_Change_in_Accessible_Destinations()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Change_in_Accessible_Destinations.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Change_in_Accessible_Destinations()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Change_in_Accessible_Destinations = 5;
        entity.Change_in_Accessible_Destinations.ShouldBe((short?)5);
    }

    [Fact]
    public void Should_Get_Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations = 0.15f;
        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations.ShouldBe(0.15f);
    }

    [Fact]
    public void Should_Get_Support_for_Environmental_Justice()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Environmental_Justice.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Environmental_Justice()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Support_for_Environmental_Justice = "Yes";
        entity.Support_for_Environmental_Justice.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_Culverts_Rehabilitated_or_Replaced()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Culverts_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Culverts_Rehabilitated_or_Replaced()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Culverts_Rehabilitated_or_Replaced = 1.0f;
        entity.Culverts_Rehabilitated_or_Replaced.ShouldBe(1.0f);
    }

    [Fact]
    public void Should_Get_Guardrail_Rehabilitated_or_Replaced()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Guardrail_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Guardrail_Rehabilitated_or_Replaced()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Guardrail_Rehabilitated_or_Replaced = 0.5f;
        entity.Guardrail_Rehabilitated_or_Replaced.ShouldBe(0.5f);
    }

    [Fact]
    public void Should_Get_Geotechnical_Assets_Rehabilitated_or_Replaced()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Geotechnical_Assets_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Geotechnical_Assets_Rehabilitated_or_Replaced()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Geotechnical_Assets_Rehabilitated_or_Replaced = 0.2f;
        entity.Geotechnical_Assets_Rehabilitated_or_Replaced.ShouldBe(0.2f);
    }

    [Fact]
    public void Should_Get_Facilities_Rehabilitated_or_Reconstructed()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Facilities_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilities_Rehabilitated_or_Reconstructed()
    {
        var entity = new SpoofProjectNorthwestRPOEntity();
        entity.Facilities_Rehabilitated_or_Reconstructed = 0.1f;
        entity.Facilities_Rehabilitated_or_Reconstructed.ShouldBe(0.1f);
    }
}
