using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ScenarioBudgetTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new ScenarioBudget();
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new ScenarioBudget();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new ScenarioBudget();
        entity.District.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new ScenarioBudget();
        entity.District = 5;
        entity.District.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new ScenarioBudget();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new ScenarioBudget();
        entity.ScenarioId = 10;
        entity.ScenarioId.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_IsInterstate()
    {
        var entity = new ScenarioBudget();
        entity.IsInterstate.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_IsInterstate()
    {
        var entity = new ScenarioBudget();
        entity.IsInterstate = true;
        entity.IsInterstate.ShouldBeTrue();
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new ScenarioBudget();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new ScenarioBudget();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_Budget()
    {
        var entity = new ScenarioBudget();
        entity.Budget.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Budget()
    {
        var entity = new ScenarioBudget();
        entity.Budget = 1000000m;
        entity.Budget.ShouldBe(1000000m);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new ScenarioBudget();
        entity.Id = 2026;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2026);
    }
}
