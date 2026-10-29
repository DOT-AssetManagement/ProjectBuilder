using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class PoolParameterDataEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new PoolParameterDataEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new PoolParameterDataEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LibraryId()
    {
        var entity = new PoolParameterDataEntity();
        entity.LibraryId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_LibraryId()
    {
        var entity = new PoolParameterDataEntity();
        var value = Guid.NewGuid();
        entity.LibraryId = value;
        entity.LibraryId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DataSectionId()
    {
        var entity = new PoolParameterDataEntity();
        entity.DataSectionId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DataSectionId()
    {
        var entity = new PoolParameterDataEntity();
        var value = Guid.NewGuid();
        entity.DataSectionId = value;
        entity.DataSectionId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ItemId()
    {
        var entity = new PoolParameterDataEntity();
        entity.ItemId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ItemId()
    {
        var entity = new PoolParameterDataEntity();
        var value = Guid.NewGuid();
        entity.ItemId = value;
        entity.ItemId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ItemValue()
    {
        var entity = new PoolParameterDataEntity();
        entity.ItemValue.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ItemValue()
    {
        var entity = new PoolParameterDataEntity();
        entity.ItemValue = 3.14f;
        entity.ItemValue.ShouldBe(3.14f);
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new PoolParameterDataEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new PoolParameterDataEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new PoolParameterDataEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new PoolParameterDataEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }
}
