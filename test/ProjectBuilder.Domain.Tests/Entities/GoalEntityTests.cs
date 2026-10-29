using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class GoalEntityTests
{
    [Fact]
    public void Should_Get_Name()
    {
        var entity = new GoalEntity();
        entity.Name.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Name()
    {
        var entity = new GoalEntity();
        entity.Name = "Safety Goal";
        entity.Name.ShouldBe("Safety Goal");
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new GoalEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new GoalEntity();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_Status()
    {
        var entity = new GoalEntity();
        entity.Status.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Status()
    {
        var entity = new GoalEntity();
        entity.Status = 1;
        entity.Status.ShouldBe((byte?)1);
    }

    [Fact]
    public void Should_Get_CreatorId()
    {
        var entity = new GoalEntity();
        entity.CreatorId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatorId()
    {
        var entity = new GoalEntity();
        var value = Guid.NewGuid();
        entity.CreatorId = value;
        entity.CreatorId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModificationTime()
    {
        var entity = new GoalEntity();
        entity.LastModificationTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModificationTime()
    {
        var entity = new GoalEntity();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.LastModificationTime = value;
        entity.LastModificationTime.ShouldBe(value);
    }
}
