using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ScenarioTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new Scenario();
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new Scenario();
        entity.Id = 10;
        entity.Id.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_LibraryId()
    {
        var entity = new Scenario();
        entity.LibraryId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LibraryId()
    {
        var entity = new Scenario();
        var value = Guid.NewGuid();
        entity.LibraryId = value;
        entity.LibraryId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ScenarioName_Default()
    {
        var entity = new Scenario();
        entity.ScenarioName.ShouldBe("Unknown");
    }

    [Fact]
    public void Should_Set_ScenarioName()
    {
        var entity = new Scenario();
        entity.ScenarioName = "Test Scenario";
        entity.ScenarioName.ShouldBe("Test Scenario");
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new Scenario();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new Scenario();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_DeaInputSproc()
    {
        var entity = new Scenario();
        entity.DeaInputSproc.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeaInputSproc()
    {
        var entity = new Scenario();
        entity.DeaInputSproc = "sp_Input";
        entity.DeaInputSproc.ShouldBe("sp_Input");
    }

    [Fact]
    public void Should_Get_DeaOutputSproc()
    {
        var entity = new Scenario();
        entity.DeaOutputSproc.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeaOutputSproc()
    {
        var entity = new Scenario();
        entity.DeaOutputSproc = "sp_Output";
        entity.DeaOutputSproc.ShouldBe("sp_Output");
    }

    [Fact]
    public void Should_Get_CreatedBy_Default()
    {
        var entity = new Scenario();
        entity.CreatedBy.ShouldBe("None");
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new Scenario();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new Scenario();
        entity.CreatedAt.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new Scenario();
        var value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastRunBy_Default()
    {
        var entity = new Scenario();
        entity.LastRunBy.ShouldBe("None");
    }

    [Fact]
    public void Should_Set_LastRunBy()
    {
        var entity = new Scenario();
        entity.LastRunBy = "Admin";
        entity.LastRunBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_LastRunAt()
    {
        var entity = new Scenario();
        entity.LastRunAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastRunAt()
    {
        var entity = new Scenario();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.LastRunAt = value;
        entity.LastRunAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastStarted()
    {
        var entity = new Scenario();
        entity.LastStarted.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastStarted()
    {
        var entity = new Scenario();
        DateTime? value = new DateTime(2024, 6, 1, 8, 0, 0);
        entity.LastStarted = value;
        entity.LastStarted.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastFinished()
    {
        var entity = new Scenario();
        entity.LastFinished.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastFinished()
    {
        var entity = new Scenario();
        DateTime? value = new DateTime(2024, 6, 1, 9, 0, 0);
        entity.LastFinished = value;
        entity.LastFinished.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Locked()
    {
        var entity = new Scenario();
        entity.Locked.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Locked()
    {
        var entity = new Scenario();
        entity.Locked = true;
        entity.Locked.ShouldBe(true);
    }

    [Fact]
    public void Should_Get_Notes_Default()
    {
        var entity = new Scenario();
        entity.Notes.ShouldBe("None");
    }

    [Fact]
    public void Should_Set_Notes()
    {
        var entity = new Scenario();
        entity.Notes = "Some notes";
        entity.Notes.ShouldBe("Some notes");
    }

    [Fact]
    public void Should_Get_Stale()
    {
        var entity = new Scenario();
        entity.Stale.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_Stale()
    {
        var entity = new Scenario();
        entity.Stale = true;
        entity.Stale.ShouldBeTrue();
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new Scenario();
        entity.Id = 5;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(5);
    }
}
