using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class WorkCandidateEntity_Tests
{
    [Fact]
    public void Should_Get_ImportSessionId()
    {
        var entity = new WorkCandidateEntity();

        entity.ImportSessionId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_ImportSessionId()
    {
        var entity = new WorkCandidateEntity();
        var value = Guid.NewGuid();

        entity.ImportSessionId = value;

        entity.ImportSessionId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ImportTimeGeneratedId()
    {
        var entity = new WorkCandidateEntity();

        entity.ImportTimeGeneratedId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_ImportTimeGeneratedId()
    {
        var entity = new WorkCandidateEntity();
        var value = Guid.NewGuid();

        entity.ImportTimeGeneratedId = value;

        entity.ImportTimeGeneratedId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_UserLibraryId()
    {
        var entity = new WorkCandidateEntity();

        entity.UserLibraryId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UserLibraryId()
    {
        var entity = new WorkCandidateEntity();
        Guid? value = Guid.NewGuid();

        entity.UserLibraryId = value;

        entity.UserLibraryId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ProjectCandidateId()
    {
        var entity = new WorkCandidateEntity();

        entity.ProjectCandidateId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ProjectCandidateId()
    {
        var entity = new WorkCandidateEntity();
        Guid? value = Guid.NewGuid();

        entity.ProjectCandidateId = value;

        entity.ProjectCandidateId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new WorkCandidateEntity();

        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_AssetType";

        entity.AssetType = value;

        entity.AssetType.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new WorkCandidateEntity();

        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new WorkCandidateEntity();
        byte? value = 5;

        entity.District = value;

        entity.District.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Cnty()
    {
        var entity = new WorkCandidateEntity();

        entity.Cnty.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cnty()
    {
        var entity = new WorkCandidateEntity();
        byte? value = 5;

        entity.Cnty = value;

        entity.Cnty.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new WorkCandidateEntity();

        entity.Route.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Route = value;

        entity.Route.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Direction()
    {
        var entity = new WorkCandidateEntity();

        entity.Direction.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Direction()
    {
        var entity = new WorkCandidateEntity();
        byte? value = 5;

        entity.Direction = value;

        entity.Direction.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_FromSection()
    {
        var entity = new WorkCandidateEntity();

        entity.FromSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_FromSection()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.FromSection = value;

        entity.FromSection.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ToSection()
    {
        var entity = new WorkCandidateEntity();

        entity.ToSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ToSection()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_ToSection";

        entity.ToSection = value;

        entity.ToSection.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Offset()
    {
        var entity = new WorkCandidateEntity();

        entity.Offset.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Offset()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Offset = value;

        entity.Offset.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_BRKEY()
    {
        var entity = new WorkCandidateEntity();

        entity.BRKEY.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BRKEY()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_BRKEY";

        entity.BRKEY = value;

        entity.BRKEY.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_BRIDGE_ID()
    {
        var entity = new WorkCandidateEntity();

        entity.BRIDGE_ID.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BRIDGE_ID()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_BRIDGE_ID";

        entity.BRIDGE_ID = value;

        entity.BRIDGE_ID.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new WorkCandidateEntity();

        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_Description";

        entity.Description = value;

        entity.Description.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new WorkCandidateEntity();

        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.Cost = value;

        entity.Cost.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Benefit()
    {
        var entity = new WorkCandidateEntity();

        entity.Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Benefit()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.Benefit = value;

        entity.Benefit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IsIsolatedBridge()
    {
        var entity = new WorkCandidateEntity();

        entity.IsIsolatedBridge.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_IsIsolatedBridge()
    {
        var entity = new WorkCandidateEntity();
        var value = true;

        entity.IsIsolatedBridge = value;

        entity.IsIsolatedBridge.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_SafetyScore()
    {
        var entity = new WorkCandidateEntity();

        entity.SafetyScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_SafetyScore()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.SafetyScore = value;

        entity.SafetyScore.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_MobilityAndEconomyScore()
    {
        var entity = new WorkCandidateEntity();

        entity.MobilityAndEconomyScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MobilityAndEconomyScore()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.MobilityAndEconomyScore = value;

        entity.MobilityAndEconomyScore.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_EquityAndAccessScore()
    {
        var entity = new WorkCandidateEntity();

        entity.EquityAndAccessScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_EquityAndAccessScore()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.EquityAndAccessScore = value;

        entity.EquityAndAccessScore.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ResilienceAndEnvironmentScore()
    {
        var entity = new WorkCandidateEntity();

        entity.ResilienceAndEnvironmentScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ResilienceAndEnvironmentScore()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.ResilienceAndEnvironmentScore = value;

        entity.ResilienceAndEnvironmentScore.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ConditionAndPerformanceScore()
    {
        var entity = new WorkCandidateEntity();

        entity.ConditionAndPerformanceScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ConditionAndPerformanceScore()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.ConditionAndPerformanceScore = value;

        entity.ConditionAndPerformanceScore.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_TotalScore()
    {
        var entity = new WorkCandidateEntity();

        entity.TotalScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TotalScore()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.TotalScore = value;

        entity.TotalScore.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_RelativeEfficiency()
    {
        var entity = new WorkCandidateEntity();

        entity.RelativeEfficiency.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RelativeEfficiency()
    {
        var entity = new WorkCandidateEntity();
        float? value = 12.5f;

        entity.RelativeEfficiency = value;

        entity.RelativeEfficiency.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new WorkCandidateEntity();

        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_CreatedBy";

        entity.CreatedBy = value;

        entity.CreatedBy.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new WorkCandidateEntity();

        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new WorkCandidateEntity();
        DateTime? value = new DateTime(2024, 1, 15, 10, 30, 0);

        entity.CreatedAt = value;

        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ProjectName()
    {
        var entity = new WorkCandidateEntity();

        entity.ProjectName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ProjectName()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_ProjectName";

        entity.ProjectName = value;

        entity.ProjectName.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_HSIP_Eligible()
    {
        var entity = new WorkCandidateEntity();

        entity.HSIP_Eligible.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_HSIP_Eligible()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.HSIP_Eligible = value;

        entity.HSIP_Eligible.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Excess_Safety_Value()
    {
        var entity = new WorkCandidateEntity();

        entity.Excess_Safety_Value.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Excess_Safety_Value()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Excess_Safety_Value = value;

        entity.Excess_Safety_Value.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Supported_by_RSA_Safety_Study()
    {
        var entity = new WorkCandidateEntity();

        entity.Supported_by_RSA_Safety_Study.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Supported_by_RSA_Safety_Study()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Supported_by_RSA_Safety_Study = value;

        entity.Supported_by_RSA_Safety_Study.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Degree_of_Safety_Improvement()
    {
        var entity = new WorkCandidateEntity();

        entity.Degree_of_Safety_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Degree_of_Safety_Improvement()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Degree_of_Safety_Improvement = value;

        entity.Degree_of_Safety_Improvement.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Supports_Hazard_Mitigation()
    {
        var entity = new WorkCandidateEntity();

        entity.Supports_Hazard_Mitigation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Supports_Hazard_Mitigation()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Supports_Hazard_Mitigation = value;

        entity.Supports_Hazard_Mitigation.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Improves_Access_to_Emergency_Services()
    {
        var entity = new WorkCandidateEntity();

        entity.Improves_Access_to_Emergency_Services.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Improves_Access_to_Emergency_Services()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Improves_Access_to_Emergency_Services = value;

        entity.Improves_Access_to_Emergency_Services.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Predicted_Safety_Benefit()
    {
        var entity = new WorkCandidateEntity();

        entity.Predicted_Safety_Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Predicted_Safety_Benefit()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Predicted_Safety_Benefit = value;

        entity.Predicted_Safety_Benefit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Safety_Score()
    {
        var entity = new WorkCandidateEntity();

        entity.Safety_Score.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Safety_Score()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Safety_Score = value;

        entity.Safety_Score.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Travel_Time_Savings()
    {
        var entity = new WorkCandidateEntity();

        entity.Travel_Time_Savings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Travel_Time_Savings()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Travel_Time_Savings = value;

        entity.Travel_Time_Savings.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Operating_Cost_Savings()
    {
        var entity = new WorkCandidateEntity();

        entity.Operating_Cost_Savings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Operating_Cost_Savings()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Operating_Cost_Savings = value;

        entity.Operating_Cost_Savings.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Degree_of_Mobility_Improvement()
    {
        var entity = new WorkCandidateEntity();

        entity.Degree_of_Mobility_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Degree_of_Mobility_Improvement()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Degree_of_Mobility_Improvement = value;

        entity.Degree_of_Mobility_Improvement.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Freight_Improvement()
    {
        var entity = new WorkCandidateEntity();

        entity.Freight_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Freight_Improvement()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Freight_Improvement = value;

        entity.Freight_Improvement.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Facilitates_Transit_Use()
    {
        var entity = new WorkCandidateEntity();

        entity.Facilitates_Transit_Use.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilitates_Transit_Use()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Facilitates_Transit_Use = value;

        entity.Facilitates_Transit_Use.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Facilitates_Active_Transportation()
    {
        var entity = new WorkCandidateEntity();

        entity.Facilitates_Active_Transportation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilitates_Active_Transportation()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Facilitates_Active_Transportation = value;

        entity.Facilitates_Active_Transportation.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Bike_Lanes_Constructed()
    {
        var entity = new WorkCandidateEntity();

        entity.Bike_Lanes_Constructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bike_Lanes_Constructed()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Bike_Lanes_Constructed = value;

        entity.Bike_Lanes_Constructed.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_New_Cyclists()
    {
        var entity = new WorkCandidateEntity();

        entity.New_Cyclists.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_New_Cyclists()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.New_Cyclists = value;

        entity.New_Cyclists.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Support_for_Job_Growth()
    {
        var entity = new WorkCandidateEntity();

        entity.Support_for_Job_Growth.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Job_Growth()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Support_for_Job_Growth = value;

        entity.Support_for_Job_Growth.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Support_for_Tourism()
    {
        var entity = new WorkCandidateEntity();

        entity.Support_for_Tourism.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Tourism()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Support_for_Tourism = value;

        entity.Support_for_Tourism.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Support_for_Economic_Goals()
    {
        var entity = new WorkCandidateEntity();

        entity.Support_for_Economic_Goals.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Economic_Goals()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Support_for_Economic_Goals = value;

        entity.Support_for_Economic_Goals.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Support_for_Recreation()
    {
        var entity = new WorkCandidateEntity();

        entity.Support_for_Recreation.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Recreation()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Support_for_Recreation = value;

        entity.Support_for_Recreation.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Consistency_with_Existing_Plans()
    {
        var entity = new WorkCandidateEntity();

        entity.Consistency_with_Existing_Plans.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Consistency_with_Existing_Plans()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Consistency_with_Existing_Plans = value;

        entity.Consistency_with_Existing_Plans.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Predicted_Mobility_Benefit()
    {
        var entity = new WorkCandidateEntity();

        entity.Predicted_Mobility_Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Predicted_Mobility_Benefit()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Predicted_Mobility_Benefit = value;

        entity.Predicted_Mobility_Benefit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Mobility_and_Access_Score()
    {
        var entity = new WorkCandidateEntity();

        entity.Mobility_and_Access_Score.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Mobility_and_Access_Score()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Mobility_and_Access_Score = value;

        entity.Mobility_and_Access_Score.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Sidewalks_and_Curb_Ramps_Constructed_or_Improved()
    {
        var entity = new WorkCandidateEntity();

        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Sidewalks_and_Curb_Ramps_Constructed_or_Improved()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved = value;

        entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Change_in_Accessible_Destinations()
    {
        var entity = new WorkCandidateEntity();

        entity.Change_in_Accessible_Destinations.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Change_in_Accessible_Destinations()
    {
        var entity = new WorkCandidateEntity();
        Int16? value = 5;

        entity.Change_in_Accessible_Destinations = value;

        entity.Change_in_Accessible_Destinations.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations()
    {
        var entity = new WorkCandidateEntity();

        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations = value;

        entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Support_for_Environmental_Justice()
    {
        var entity = new WorkCandidateEntity();

        entity.Support_for_Environmental_Justice.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Support_for_Environmental_Justice()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Support_for_Environmental_Justice = value;

        entity.Support_for_Environmental_Justice.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Predicted_Equity_and_Access_Benefit()
    {
        var entity = new WorkCandidateEntity();

        entity.Predicted_Equity_and_Access_Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Predicted_Equity_and_Access_Benefit()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Predicted_Equity_and_Access_Benefit = value;

        entity.Predicted_Equity_and_Access_Benefit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Equity_and_Access_Score()
    {
        var entity = new WorkCandidateEntity();

        entity.Equity_and_Access_Score.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Equity_and_Access_Score()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Equity_and_Access_Score = value;

        entity.Equity_and_Access_Score.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Reduced_Flood_Closure_Risk()
    {
        var entity = new WorkCandidateEntity();

        entity.Reduced_Flood_Closure_Risk.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Reduced_Flood_Closure_Risk()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Reduced_Flood_Closure_Risk = value;

        entity.Reduced_Flood_Closure_Risk.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Wetlands_Improved()
    {
        var entity = new WorkCandidateEntity();

        entity.Wetlands_Improved.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Wetlands_Improved()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Wetlands_Improved = value;

        entity.Wetlands_Improved.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Wildlife_Crossings()
    {
        var entity = new WorkCandidateEntity();

        entity.Wildlife_Crossings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Wildlife_Crossings()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Wildlife_Crossings = value;

        entity.Wildlife_Crossings.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Reduced_Fuel_Consumption()
    {
        var entity = new WorkCandidateEntity();

        entity.Reduced_Fuel_Consumption.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Reduced_Fuel_Consumption()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Reduced_Fuel_Consumption = value;

        entity.Reduced_Fuel_Consumption.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Degree_of_Environmental_Improvement()
    {
        var entity = new WorkCandidateEntity();

        entity.Degree_of_Environmental_Improvement.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Degree_of_Environmental_Improvement()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Degree_of_Environmental_Improvement = value;

        entity.Degree_of_Environmental_Improvement.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Predicted_Environmental_Benefit()
    {
        var entity = new WorkCandidateEntity();

        entity.Predicted_Environmental_Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Predicted_Environmental_Benefit()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Predicted_Environmental_Benefit = value;

        entity.Predicted_Environmental_Benefit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Resilience_and_Environment_Score()
    {
        var entity = new WorkCandidateEntity();

        entity.Resilience_and_Environment_Score.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Resilience_and_Environment_Score()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Resilience_and_Environment_Score = value;

        entity.Resilience_and_Environment_Score.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Consistency_with_Land_Use_Plans()
    {
        var entity = new WorkCandidateEntity();

        entity.Consistency_with_Land_Use_Plans.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Consistency_with_Land_Use_Plans()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Consistency_with_Land_Use_Plans = value;

        entity.Consistency_with_Land_Use_Plans.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Pavement_Rehabilitated_or_Reconstructed()
    {
        var entity = new WorkCandidateEntity();

        entity.Pavement_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Pavement_Rehabilitated_or_Reconstructed()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Pavement_Rehabilitated_or_Reconstructed = value;

        entity.Pavement_Rehabilitated_or_Reconstructed.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Bridges_Rehabilitated_or_Replaced()
    {
        var entity = new WorkCandidateEntity();

        entity.Bridges_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bridges_Rehabilitated_or_Replaced()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Bridges_Rehabilitated_or_Replaced = value;

        entity.Bridges_Rehabilitated_or_Replaced.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Culverts_Rehabilitated_or_Replaced()
    {
        var entity = new WorkCandidateEntity();

        entity.Culverts_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Culverts_Rehabilitated_or_Replaced()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Culverts_Rehabilitated_or_Replaced = value;

        entity.Culverts_Rehabilitated_or_Replaced.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Guardrail_Rehabilitated_or_Replaced()
    {
        var entity = new WorkCandidateEntity();

        entity.Guardrail_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Guardrail_Rehabilitated_or_Replaced()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Guardrail_Rehabilitated_or_Replaced = value;

        entity.Guardrail_Rehabilitated_or_Replaced.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Geotechnical_Assets_Rehabilitated_or_Replaced()
    {
        var entity = new WorkCandidateEntity();

        entity.Geotechnical_Assets_Rehabilitated_or_Replaced.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Geotechnical_Assets_Rehabilitated_or_Replaced()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Geotechnical_Assets_Rehabilitated_or_Replaced = value;

        entity.Geotechnical_Assets_Rehabilitated_or_Replaced.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Facilities_Rehabilitated_or_Reconstructed()
    {
        var entity = new WorkCandidateEntity();

        entity.Facilities_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Facilities_Rehabilitated_or_Reconstructed()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Facilities_Rehabilitated_or_Reconstructed = value;

        entity.Facilities_Rehabilitated_or_Reconstructed.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Sidewalks_Rehabilitated_or_Reconstructed()
    {
        var entity = new WorkCandidateEntity();

        entity.Sidewalks_Rehabilitated_or_Reconstructed.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Sidewalks_Rehabilitated_or_Reconstructed()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Sidewalks_Rehabilitated_or_Reconstructed = value;

        entity.Sidewalks_Rehabilitated_or_Reconstructed.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Predicted_Condition_and_Performance_Benefit()
    {
        var entity = new WorkCandidateEntity();

        entity.Predicted_Condition_and_Performance_Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Predicted_Condition_and_Performance_Benefit()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Predicted_Condition_and_Performance_Benefit = value;

        entity.Predicted_Condition_and_Performance_Benefit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Condition_and_Performance_Score()
    {
        var entity = new WorkCandidateEntity();

        entity.Condition_and_Performance_Score.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Condition_and_Performance_Score()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Condition_and_Performance_Score = value;

        entity.Condition_and_Performance_Score.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ADT()
    {
        var entity = new WorkCandidateEntity();

        entity.ADT.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ADT()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.ADT = value;

        entity.ADT.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Percent_Trucks()
    {
        var entity = new WorkCandidateEntity();

        entity.Percent_Trucks.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Percent_Trucks()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Percent_Trucks = value;

        entity.Percent_Trucks.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Population_Density()
    {
        var entity = new WorkCandidateEntity();

        entity.Population_Density.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Population_Density()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Population_Density = value;

        entity.Population_Density.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Bike_Commute_Share()
    {
        var entity = new WorkCandidateEntity();

        entity.Bike_Commute_Share.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bike_Commute_Share()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Bike_Commute_Share = value;

        entity.Bike_Commute_Share.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_BPN()
    {
        var entity = new WorkCandidateEntity();

        entity.BPN.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BPN()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_BPN";

        entity.BPN = value;

        entity.BPN.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Length()
    {
        var entity = new WorkCandidateEntity();

        entity.Length.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Length()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Length = value;

        entity.Length.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Speed_Limit()
    {
        var entity = new WorkCandidateEntity();

        entity.Speed_Limit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Speed_Limit()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Speed_Limit = value;

        entity.Speed_Limit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Detour_Distance()
    {
        var entity = new WorkCandidateEntity();

        entity.Detour_Distance.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Detour_Distance()
    {
        var entity = new WorkCandidateEntity();
        double? value = 12.5;

        entity.Detour_Distance = value;

        entity.Detour_Distance.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Functional_Classification()
    {
        var entity = new WorkCandidateEntity();

        entity.Functional_Classification.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Functional_Classification()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Functional_Classification = value;

        entity.Functional_Classification.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_National_Highway_System()
    {
        var entity = new WorkCandidateEntity();

        entity.National_Highway_System.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_National_Highway_System()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.National_Highway_System = value;

        entity.National_Highway_System.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Pennsylvania_Byway()
    {
        var entity = new WorkCandidateEntity();

        entity.Pennsylvania_Byway.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Pennsylvania_Byway()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Pennsylvania_Byway = value;

        entity.Pennsylvania_Byway.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Bicycle_PA_Route()
    {
        var entity = new WorkCandidateEntity();

        entity.Bicycle_PA_Route.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Bicycle_PA_Route()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Bicycle_PA_Route = value;

        entity.Bicycle_PA_Route.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Interstate_Emergency_Detour()
    {
        var entity = new WorkCandidateEntity();

        entity.Interstate_Emergency_Detour.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Interstate_Emergency_Detour()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Interstate_Emergency_Detour = value;

        entity.Interstate_Emergency_Detour.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LFAR_BOF_Eligible()
    {
        var entity = new WorkCandidateEntity();

        entity.LFAR_BOF_Eligible.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LFAR_BOF_Eligible()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.LFAR_BOF_Eligible = value;

        entity.LFAR_BOF_Eligible.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Treatment()
    {
        var entity = new WorkCandidateEntity();

        entity.Treatment.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Treatment()
    {
        var entity = new WorkCandidateEntity();
        var value = "Test_Treatment";

        entity.Treatment = value;

        entity.Treatment.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_BenefitsComputedAt()
    {
        var entity = new WorkCandidateEntity();

        entity.BenefitsComputedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BenefitsComputedAt()
    {
        var entity = new WorkCandidateEntity();
        DateTime? value = new DateTime(2024, 1, 15, 10, 30, 0);

        entity.BenefitsComputedAt = value;

        entity.BenefitsComputedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ScoresComputedAt()
    {
        var entity = new WorkCandidateEntity();

        entity.ScoresComputedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ScoresComputedAt()
    {
        var entity = new WorkCandidateEntity();
        DateTime? value = new DateTime(2024, 1, 15, 10, 30, 0);

        entity.ScoresComputedAt = value;

        entity.ScoresComputedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Year()
    {
        var entity = new WorkCandidateEntity();

        entity.Year.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Year()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.Year = value;

        entity.Year.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_MinYear()
    {
        var entity = new WorkCandidateEntity();

        entity.MinYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MinYear()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.MinYear = value;

        entity.MinYear.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_MaxYear()
    {
        var entity = new WorkCandidateEntity();

        entity.MaxYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MaxYear()
    {
        var entity = new WorkCandidateEntity();
        int? value = 42;

        entity.MaxYear = value;

        entity.MaxYear.ShouldBe(value);
    }

}

