using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class CountyEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new CountyEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new CountyEntity();
        entity.Id = 5;
        entity.Id.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new CountyEntity();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new CountyEntity();
        entity.District = 3;
        entity.District.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_CountyName()
    {
        var entity = new CountyEntity();
        entity.CountyName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CountyName()
    {
        var entity = new CountyEntity();
        entity.CountyName = "Allegheny";
        entity.CountyName.ShouldBe("Allegheny");
    }

    [Fact]
    public void Should_Return_CountyFullName_When_Id_And_Name_Set()
    {
        var entity = new CountyEntity();
        entity.Id = 2;
        entity.CountyName = "Allegheny";
        entity.CountyFullName.ShouldBe("2-Allegheny");
    }

    [Fact]
    public void Should_Return_Null_CountyFullName_When_Id_Is_Null()
    {
        var entity = new CountyEntity();
        entity.CountyName = "Allegheny";
        entity.CountyFullName.ShouldBeNull();
    }

    [Fact]
    public void Should_Return_Null_CountyFullName_When_Name_Is_Empty()
    {
        var entity = new CountyEntity();
        entity.Id = 2;
        entity.CountyName = "";
        entity.CountyFullName.ShouldBeNull();
    }

    [Fact]
    public void Should_Parse_FullName_Via_Constructor()
    {
        var entity = new CountyEntity("5-Allegheny");
        entity.Id.ShouldBe(5);
        entity.CountyName.ShouldBe("Allegheny");
    }

    [Fact]
    public void Should_Handle_Empty_FullName_In_Constructor()
    {
        var entity = new CountyEntity("");
        entity.Id.ShouldBeNull();
        entity.CountyName.ShouldBeNull();
    }

    [Fact]
    public void Should_Return_ToString()
    {
        var entity = new CountyEntity();
        entity.Id = 5;
        entity.CountyName = "Allegheny";
        entity.ToString().ShouldBe("5-Allegheny");
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new CountyEntity();
        entity.Id = 10;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(10);
    }
}
