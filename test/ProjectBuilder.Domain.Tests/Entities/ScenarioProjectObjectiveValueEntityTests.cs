using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ScenarioProjectObjectiveValueEntityTests
{
    [Fact]
    public void Should_Get_ScenId()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        entity.ScenId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_ScenId()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        var value = Guid.NewGuid();
        entity.ScenId = value;
        entity.ScenId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ProjectId()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        entity.ProjectId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_ProjectId()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        var value = Guid.NewGuid();
        entity.ProjectId = value;
        entity.ProjectId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_RelativeEfficiency()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        entity.RelativeEfficiency.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RelativeEfficiency()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        entity.RelativeEfficiency = 0.85f;
        entity.RelativeEfficiency.ShouldBe(0.85f);
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new ScenarioProjectObjectiveValueEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }
}
