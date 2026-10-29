using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class PoolParameterEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new PoolParameterEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new PoolParameterEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ItemName()
    {
        var entity = new PoolParameterEntity();
        entity.ItemName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ItemName()
    {
        var entity = new PoolParameterEntity();
        entity.ItemName = "Benefit Weight";
        entity.ItemName.ShouldBe("Benefit Weight");
    }

    [Fact]
    public void Should_Get_ItemDescription()
    {
        var entity = new PoolParameterEntity();
        entity.ItemDescription.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ItemDescription()
    {
        var entity = new PoolParameterEntity();
        entity.ItemDescription = "Test Description";
        entity.ItemDescription.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new PoolParameterEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new PoolParameterEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new PoolParameterEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new PoolParameterEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }
}
