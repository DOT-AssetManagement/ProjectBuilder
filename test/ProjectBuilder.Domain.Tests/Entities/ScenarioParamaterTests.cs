using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ScenarioParamaterTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new ScenarioParamater();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new ScenarioParamater();
        entity.Id = "PARM001";
        entity.Id.ShouldBe("PARM001");
    }

    [Fact]
    public void Should_Get_ParmName()
    {
        var entity = new ScenarioParamater();
        entity.ParmName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ParmName()
    {
        var entity = new ScenarioParamater();
        entity.ParmName = "Test_ParmName";
        entity.ParmName.ShouldBe("Test_ParmName");
    }

    [Fact]
    public void Should_Get_ParmDescription()
    {
        var entity = new ScenarioParamater();
        entity.ParmDescription.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ParmDescription()
    {
        var entity = new ScenarioParamater();
        entity.ParmDescription = "Test_Description";
        entity.ParmDescription.ShouldBe("Test_Description");
    }

    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new ScenarioParamater();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new ScenarioParamater();
        entity.ScenarioId = 10;
        entity.ScenarioId.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_ParameterValue()
    {
        var entity = new ScenarioParamater();
        entity.ParameterValue.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ParameterValue()
    {
        var entity = new ScenarioParamater();
        entity.ParameterValue = 1.5;
        entity.ParameterValue.ShouldBe(1.5);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new ScenarioParamater();
        entity.Id = "KEY001";
        var keys = entity.GetKeys();
        keys[0].ShouldBe("KEY001");
    }
}
