using ProjectBuilder.DataAccess;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class CombinedProjectSummaryEntityTests
{
    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_ProjectType()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.ProjectType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ProjectType()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.ProjectType = "Bridge";
        entity.ProjectType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_Projects()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Projects.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Projects()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Projects = 10;
        entity.Projects.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_Treatments()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Treatments.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Treatments()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Treatments = 25;
        entity.Treatments.ShouldBe(25);
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Cost = 500000.0;
        entity.Cost.ShouldBe(500000.0);
    }

    [Fact]
    public void Should_Get_Benefit()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Benefit()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Benefit = 750000.0;
        entity.Benefit.ShouldBe(750000.0);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Id = 2024;
        entity.Id.ShouldBe(2024);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new CombinedProjectSummaryEntity();
        entity.Id = 2025;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2025);
    }
}
