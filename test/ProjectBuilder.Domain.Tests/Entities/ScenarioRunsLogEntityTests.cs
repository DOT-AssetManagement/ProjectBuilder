using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ScenarioRunsLogEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new ScenarioRunsLogEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_RunNo()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.RunNo.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_RunNo()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.RunNo = 5;
        entity.RunNo.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_ScenId()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.ScenId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ScenId()
    {
        var entity = new ScenarioRunsLogEntity();
        var value = Guid.NewGuid();
        entity.ScenId = value;
        entity.ScenId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ScenNo()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.ScenNo.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ScenNo()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.ScenNo = 10;
        entity.ScenNo.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_StartedAt()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.StartedAt.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Should_Set_StartedAt()
    {
        var entity = new ScenarioRunsLogEntity();
        var value = new DateTime(2024, 1, 15, 8, 0, 0);
        entity.StartedAt = value;
        entity.StartedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_StartedBy()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.StartedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_StartedBy()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.StartedBy = "Admin";
        entity.StartedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_Status()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.Status.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Status()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.Status = "Completed";
        entity.Status.ShouldBe("Completed");
    }

    [Fact]
    public void Should_Get_EndedAt()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.EndedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_EndedAt()
    {
        var entity = new ScenarioRunsLogEntity();
        DateTime? value = new DateTime(2024, 1, 15, 9, 0, 0);
        entity.EndedAt = value;
        entity.EndedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Notes()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.Notes.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Notes()
    {
        var entity = new ScenarioRunsLogEntity();
        entity.Notes = "Run successful";
        entity.Notes.ShouldBe("Run successful");
    }
}
