using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class AraEnumerationsTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new AraEnumerations();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new AraEnumerations();
        entity.Id = "DISTRICT";
        entity.Id.ShouldBe("DISTRICT");
    }

    [Fact]
    public void Should_Get_EnumName()
    {
        var entity = new AraEnumerations();
        entity.EnumName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_EnumName()
    {
        var entity = new AraEnumerations();
        entity.EnumName = "District 1";
        entity.EnumName.ShouldBe("District 1");
    }

    [Fact]
    public void Should_Get_EnumInt()
    {
        var entity = new AraEnumerations();
        entity.EnumInt.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_EnumInt()
    {
        var entity = new AraEnumerations();
        entity.EnumInt = 1;
        entity.EnumInt.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new AraEnumerations();
        entity.Id = "COUNTY";
        var keys = entity.GetKeys();
        keys[0].ShouldBe("COUNTY");
    }
}
