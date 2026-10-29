using ProjectBuilder.DataAccess;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ProjectSummaryEntityTests
{
    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new ProjectSummaryEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new ProjectSummaryEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new ProjectSummaryEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new ProjectSummaryEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_SelectedYear()
    {
        var entity = new ProjectSummaryEntity();
        entity.SelectedYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_SelectedYear()
    {
        var entity = new ProjectSummaryEntity();
        entity.SelectedYear = 2025;
        entity.SelectedYear.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new ProjectSummaryEntity();
        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new ProjectSummaryEntity();
        entity.Cost = 1000000.0;
        entity.Cost.ShouldBe(1000000.0);
    }

    [Fact]
    public void Should_Get_Benefit()
    {
        var entity = new ProjectSummaryEntity();
        entity.Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Benefit()
    {
        var entity = new ProjectSummaryEntity();
        entity.Benefit = 1500000.0;
        entity.Benefit.ShouldBe(1500000.0);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new ProjectSummaryEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new ProjectSummaryEntity();
        entity.Id = 42;
        entity.Id.ShouldBe(42);
    }

    [Fact]
    public void Should_Get_TreatmentId()
    {
        var entity = new ProjectSummaryEntity();
        entity.TreatmentId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_TreatmentId()
    {
        var entity = new ProjectSummaryEntity();
        entity.TreatmentId = 99;
        entity.TreatmentId.ShouldBe(99);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new ProjectSummaryEntity();
        entity.Id = 7;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(7);
    }
}
