using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class PAMSSectionSegmentTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new PAMSSectionSegment();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new PAMSSectionSegment();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new PAMSSectionSegment();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new PAMSSectionSegment();
        entity.District = 5;
        entity.District.ShouldBe((byte)5);
    }

    [Fact]
    public void Should_Get_CountyId()
    {
        var entity = new PAMSSectionSegment();
        entity.CountyId.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_CountyId()
    {
        var entity = new PAMSSectionSegment();
        entity.CountyId = 10;
        entity.CountyId.ShouldBe((byte)10);
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new PAMSSectionSegment();
        entity.Route.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new PAMSSectionSegment();
        entity.Route = 42;
        entity.Route.ShouldBe(42);
    }

    [Fact]
    public void Should_Get_FromSection()
    {
        var entity = new PAMSSectionSegment();
        entity.FromSection.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_FromSection()
    {
        var entity = new PAMSSectionSegment();
        entity.FromSection = 100;
        entity.FromSection.ShouldBe(100);
    }

    [Fact]
    public void Should_Get_ToSection()
    {
        var entity = new PAMSSectionSegment();
        entity.ToSection.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ToSection()
    {
        var entity = new PAMSSectionSegment();
        entity.ToSection = 200;
        entity.ToSection.ShouldBe(200);
    }

    [Fact]
    public void Should_Get_Direction()
    {
        var entity = new PAMSSectionSegment();
        entity.Direction.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_Direction()
    {
        var entity = new PAMSSectionSegment();
        entity.Direction = 1;
        entity.Direction.ShouldBe((byte)1);
    }

    [Fact]
    public void Should_Get_Interstate()
    {
        var entity = new PAMSSectionSegment();
        entity.Interstate.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_Interstate()
    {
        var entity = new PAMSSectionSegment();
        entity.Interstate = true;
        entity.Interstate.ShouldBeTrue();
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new PAMSSectionSegment();
        var id = Guid.NewGuid();
        entity.Id = id;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(id);
    }
}
