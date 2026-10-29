using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ProjectCandidateEntityTests
{
    [Fact]
    public void Should_Get_LibraryId()
    {
        var entity = new ProjectCandidateEntity();
        entity.LibraryId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_LibraryId()
    {
        var entity = new ProjectCandidateEntity();
        var value = Guid.NewGuid();
        entity.LibraryId = value;
        entity.LibraryId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new ProjectCandidateEntity();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new ProjectCandidateEntity();
        entity.District = 5;
        entity.District.ShouldBe((byte?)5);
    }

    [Fact]
    public void Should_Get_Cnty()
    {
        var entity = new ProjectCandidateEntity();
        entity.Cnty.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cnty()
    {
        var entity = new ProjectCandidateEntity();
        entity.Cnty = 10;
        entity.Cnty.ShouldBe((byte?)10);
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new ProjectCandidateEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new ProjectCandidateEntity();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_CountyName()
    {
        var entity = new ProjectCandidateEntity();
        entity.CountyName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CountyName()
    {
        var entity = new ProjectCandidateEntity();
        entity.CountyName = "Allegheny";
        entity.CountyName.ShouldBe("Allegheny");
    }

    [Fact]
    public void Should_Get_NumberOfWorkCandidates()
    {
        var entity = new ProjectCandidateEntity();
        entity.NumberOfWorkCandidates.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_NumberOfWorkCandidates()
    {
        var entity = new ProjectCandidateEntity();
        entity.NumberOfWorkCandidates = 5;
        entity.NumberOfWorkCandidates.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_Treatment()
    {
        var entity = new ProjectCandidateEntity();
        entity.Treatment.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Treatment()
    {
        var entity = new ProjectCandidateEntity();
        entity.Treatment = "Overlay";
        entity.Treatment.ShouldBe("Overlay");
    }

    [Fact]
    public void Should_Get_YearEarliest()
    {
        var entity = new ProjectCandidateEntity();
        entity.YearEarliest.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_YearEarliest()
    {
        var entity = new ProjectCandidateEntity();
        entity.YearEarliest = 2020;
        entity.YearEarliest.ShouldBe(2020);
    }

    [Fact]
    public void Should_Get_YearLatest()
    {
        var entity = new ProjectCandidateEntity();
        entity.YearLatest.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_YearLatest()
    {
        var entity = new ProjectCandidateEntity();
        entity.YearLatest = 2030;
        entity.YearLatest.ShouldBe(2030);
    }

    [Fact]
    public void Should_Get_TotalCost()
    {
        var entity = new ProjectCandidateEntity();
        entity.TotalCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TotalCost()
    {
        var entity = new ProjectCandidateEntity();
        entity.TotalCost = 500000f;
        entity.TotalCost.ShouldBe(500000f);
    }

    [Fact]
    public void Should_Get_TotalScaledBenefit()
    {
        var entity = new ProjectCandidateEntity();
        entity.TotalScaledBenefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TotalScaledBenefit()
    {
        var entity = new ProjectCandidateEntity();
        entity.TotalScaledBenefit = 1200.5f;
        entity.TotalScaledBenefit.ShouldBe(1200.5f);
    }

    [Fact]
    public void Should_Get_SafetyScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.SafetyScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_SafetyScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.SafetyScore = 75.0f;
        entity.SafetyScore.ShouldBe(75.0f);
    }

    [Fact]
    public void Should_Get_MobilityAndEconomyScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.MobilityAndEconomyScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MobilityAndEconomyScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.MobilityAndEconomyScore = 60.0f;
        entity.MobilityAndEconomyScore.ShouldBe(60.0f);
    }

    [Fact]
    public void Should_Get_EquityAndAccessScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.EquityAndAccessScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_EquityAndAccessScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.EquityAndAccessScore = 55.0f;
        entity.EquityAndAccessScore.ShouldBe(55.0f);
    }

    [Fact]
    public void Should_Get_ResilienceAndEnvironmentScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.ResilienceAndEnvironmentScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ResilienceAndEnvironmentScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.ResilienceAndEnvironmentScore = 70.0f;
        entity.ResilienceAndEnvironmentScore.ShouldBe(70.0f);
    }

    [Fact]
    public void Should_Get_ConditionAndPerformanceScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.ConditionAndPerformanceScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ConditionAndPerformanceScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.ConditionAndPerformanceScore = 80.0f;
        entity.ConditionAndPerformanceScore.ShouldBe(80.0f);
    }

    [Fact]
    public void Should_Get_TotalScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.TotalScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TotalScore()
    {
        var entity = new ProjectCandidateEntity();
        entity.TotalScore = 87.5f;
        entity.TotalScore.ShouldBe(87.5f);
    }

    [Fact]
    public void Should_Get_RelativeEfficiency()
    {
        var entity = new ProjectCandidateEntity();
        entity.RelativeEfficiency.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RelativeEfficiency()
    {
        var entity = new ProjectCandidateEntity();
        entity.RelativeEfficiency = 0.95f;
        entity.RelativeEfficiency.ShouldBe(0.95f);
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new ProjectCandidateEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new ProjectCandidateEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new ProjectCandidateEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new ProjectCandidateEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_RelativeEfficiencySetAt()
    {
        var entity = new ProjectCandidateEntity();
        entity.RelativeEfficiencySetAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RelativeEfficiencySetAt()
    {
        var entity = new ProjectCandidateEntity();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.RelativeEfficiencySetAt = value;
        entity.RelativeEfficiencySetAt.ShouldBe(value);
    }
}
