using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class DssProjectEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new DssProjectEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new DssProjectEntity();
        entity.Id = 1;
        entity.Id.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_PlanningPartner()
    {
        var entity = new DssProjectEntity();
        entity.PlanningPartner.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PlanningPartner()
    {
        var entity = new DssProjectEntity();
        entity.PlanningPartner = 5;
        entity.PlanningPartner.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new DssProjectEntity();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new DssProjectEntity();
        entity.District = 3;
        entity.District.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_County()
    {
        var entity = new DssProjectEntity();
        entity.County.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_County()
    {
        var entity = new DssProjectEntity();
        entity.County = 10;
        entity.County.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new DssProjectEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new DssProjectEntity();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_EarliestYear()
    {
        var entity = new DssProjectEntity();
        entity.EarliestYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_EarliestYear()
    {
        var entity = new DssProjectEntity();
        entity.EarliestYear = 2020;
        entity.EarliestYear.ShouldBe(2020);
    }

    [Fact]
    public void Should_Get_LatestYear()
    {
        var entity = new DssProjectEntity();
        entity.LatestYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LatestYear()
    {
        var entity = new DssProjectEntity();
        entity.LatestYear = 2030;
        entity.LatestYear.ShouldBe(2030);
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new DssProjectEntity();
        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new DssProjectEntity();
        entity.Cost = 500000f;
        entity.Cost.ShouldBe(500000f);
    }

    [Fact]
    public void Should_Get_SafetyScore()
    {
        var entity = new DssProjectEntity();
        entity.SafetyScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_SafetyScore()
    {
        var entity = new DssProjectEntity();
        entity.SafetyScore = 75.0f;
        entity.SafetyScore.ShouldBe(75.0f);
    }

    [Fact]
    public void Should_Get_MobilityAndEconomyScore()
    {
        var entity = new DssProjectEntity();
        entity.MobilityAndEconomyScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MobilityAndEconomyScore()
    {
        var entity = new DssProjectEntity();
        entity.MobilityAndEconomyScore = 60.0f;
        entity.MobilityAndEconomyScore.ShouldBe(60.0f);
    }

    [Fact]
    public void Should_Get_EquityAndAccessScore()
    {
        var entity = new DssProjectEntity();
        entity.EquityAndAccessScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_EquityAndAccessScore()
    {
        var entity = new DssProjectEntity();
        entity.EquityAndAccessScore = 55.0f;
        entity.EquityAndAccessScore.ShouldBe(55.0f);
    }

    [Fact]
    public void Should_Get_ResilienceAndEnvironmentScore()
    {
        var entity = new DssProjectEntity();
        entity.ResilienceAndEnvironmentScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ResilienceAndEnvironmentScore()
    {
        var entity = new DssProjectEntity();
        entity.ResilienceAndEnvironmentScore = 70.0f;
        entity.ResilienceAndEnvironmentScore.ShouldBe(70.0f);
    }

    [Fact]
    public void Should_Get_ConditionAndPerformanceScore()
    {
        var entity = new DssProjectEntity();
        entity.ConditionAndPerformanceScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ConditionAndPerformanceScore()
    {
        var entity = new DssProjectEntity();
        entity.ConditionAndPerformanceScore = 80.0f;
        entity.ConditionAndPerformanceScore.ShouldBe(80.0f);
    }

    [Fact]
    public void Should_Get_TotalScore()
    {
        var entity = new DssProjectEntity();
        entity.TotalScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TotalScore()
    {
        var entity = new DssProjectEntity();
        entity.TotalScore = 87.5f;
        entity.TotalScore.ShouldBe(87.5f);
    }

    [Fact]
    public void Should_Get_RelativeEfficiency()
    {
        var entity = new DssProjectEntity();
        entity.RelativeEfficiency.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RelativeEfficiency()
    {
        var entity = new DssProjectEntity();
        entity.RelativeEfficiency = 0.95f;
        entity.RelativeEfficiency.ShouldBe(0.95f);
    }
}
