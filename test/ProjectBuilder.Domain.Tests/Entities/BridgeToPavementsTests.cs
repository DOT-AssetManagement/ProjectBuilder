using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class BridgeToPavementsTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new BridgeToPavements();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new BridgeToPavements();
        entity.Id = "ABC123";
        entity.Id.ShouldBe("ABC123");
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new BridgeToPavements();
        entity.District.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new BridgeToPavements();
        entity.District = 5;
        entity.District.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_County()
    {
        var entity = new BridgeToPavements();
        entity.County.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_County()
    {
        var entity = new BridgeToPavements();
        entity.County = "Test_County";
        entity.County.ShouldBe("Test_County");
    }

    [Fact]
    public void Should_Get_BridgeId()
    {
        var entity = new BridgeToPavements();
        entity.BridgeId.ShouldBe(0L);
    }

    [Fact]
    public void Should_Set_BridgeId()
    {
        var entity = new BridgeToPavements();
        entity.BridgeId = 123456789L;
        entity.BridgeId.ShouldBe(123456789L);
    }

    [Fact]
    public void Should_Get_CountyCode()
    {
        var entity = new BridgeToPavements();
        entity.CountyCode.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_CountyCode()
    {
        var entity = new BridgeToPavements();
        entity.CountyCode = 42;
        entity.CountyCode.ShouldBe(42);
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new BridgeToPavements();
        entity.Route.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new BridgeToPavements();
        entity.Route = 101;
        entity.Route.ShouldBe(101);
    }

    [Fact]
    public void Should_Get_Segment()
    {
        var entity = new BridgeToPavements();
        entity.Segment.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Segment()
    {
        var entity = new BridgeToPavements();
        entity.Segment = 10;
        entity.Segment.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_OffSet()
    {
        var entity = new BridgeToPavements();
        entity.OffSet.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_OffSet()
    {
        var entity = new BridgeToPavements();
        entity.OffSet = 50;
        entity.OffSet.ShouldBe(50);
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new BridgeToPavements();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new BridgeToPavements();
        entity.CreatedBy = "Test_User";
        entity.CreatedBy.ShouldBe("Test_User");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new BridgeToPavements();
        entity.CreatedAt.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new BridgeToPavements();
        var value = new DateTime(2024, 1, 15, 10, 30, 0);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new BridgeToPavements();
        entity.Id = "KEY001";
        var keys = entity.GetKeys();
        keys.ShouldNotBeEmpty();
        keys[0].ShouldBe("KEY001");
    }
}
