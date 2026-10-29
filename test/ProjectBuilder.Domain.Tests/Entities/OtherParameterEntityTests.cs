using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class OtherParameterEntityTests
{
    [Fact]
    public void Should_Get_Description()
    {
        var entity = new OtherParameterEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new OtherParameterEntity();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_Val()
    {
        var entity = new OtherParameterEntity();
        entity.Val.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Val()
    {
        var entity = new OtherParameterEntity();
        entity.Val = 3.14f;
        entity.Val.ShouldBe(3.14f);
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new OtherParameterEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new OtherParameterEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new OtherParameterEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new OtherParameterEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }
}
