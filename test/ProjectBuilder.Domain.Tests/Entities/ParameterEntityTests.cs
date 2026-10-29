using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ParameterEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new ParameterEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new ParameterEntity();
        entity.Id = "PARM001";
        entity.Id.ShouldBe("PARM001");
    }

    [Fact]
    public void Should_Get_Parmfamily()
    {
        var entity = new ParameterEntity();
        entity.Parmfamily.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Parmfamily()
    {
        var entity = new ParameterEntity();
        entity.Parmfamily = "Test_Family";
        entity.Parmfamily.ShouldBe("Test_Family");
    }

    [Fact]
    public void Should_Get_ParmName()
    {
        var entity = new ParameterEntity();
        entity.ParmName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ParmName()
    {
        var entity = new ParameterEntity();
        entity.ParmName = "Test_Name";
        entity.ParmName.ShouldBe("Test_Name");
    }

    [Fact]
    public void Should_Get_ParmDescription()
    {
        var entity = new ParameterEntity();
        entity.ParmDescription.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ParmDescription()
    {
        var entity = new ParameterEntity();
        entity.ParmDescription = "Test_Description";
        entity.ParmDescription.ShouldBe("Test_Description");
    }

    [Fact]
    public void Should_Get_DefaultValue()
    {
        var entity = new ParameterEntity();
        entity.DefaultValue.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DefaultValue()
    {
        var entity = new ParameterEntity();
        entity.DefaultValue = 3.14;
        entity.DefaultValue.ShouldBe(3.14);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new ParameterEntity();
        entity.Id = "KEY001";
        var keys = entity.GetKeys();
        keys[0].ShouldBe("KEY001");
    }
}
