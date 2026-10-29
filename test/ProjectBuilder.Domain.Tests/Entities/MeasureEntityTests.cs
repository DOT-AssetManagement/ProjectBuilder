using System;
using ProjectBuilder.Entities;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace ProjectBuilder.Entities;

public class MeasureEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new MeasureEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new MeasureEntity();
        var value = Guid.NewGuid();
        EntityHelper.TrySetId(entity, () => value);
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_MeasureName()
    {
        var entity = new MeasureEntity();
        entity.Name.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MeasureName()
    {
        var entity = new MeasureEntity();
        entity.Name = "Safety Score";
        entity.Name.ShouldBe("Safety Score");
    }

    [Fact]
    public void Should_Get_MeasureDescription()
    {
        var entity = new MeasureEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MeasureDescription()
    {
        var entity = new MeasureEntity();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_Status()
    {
        var entity = new MeasureEntity();
        entity.Status.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Status()
    {
        var entity = new MeasureEntity();
        entity.Status = 1;
        entity.Status.ShouldBe((byte?)1);
    }

    [Fact]
    public void Should_Get_CreatorId()
    {
        var entity = new MeasureEntity();
        entity.CreatorId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatorId()
    {
        var entity = new MeasureEntity();
        var value = Guid.NewGuid();
        entity.CreatorId = value;
        entity.CreatorId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModificationTime()
    {
        var entity = new MeasureEntity();
        entity.LastModificationTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModificationTime()
    {
        var entity = new MeasureEntity();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.LastModificationTime = value;
        entity.LastModificationTime.ShouldBe(value);
    }
}
