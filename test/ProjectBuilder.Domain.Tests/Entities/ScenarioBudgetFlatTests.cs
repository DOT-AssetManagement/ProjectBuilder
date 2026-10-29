using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ScenarioBudgetFlatTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new ScenarioBudgetFlat();
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new ScenarioBudgetFlat();
        entity.Id = 99;
        entity.Id.ShouldBe(99);
    }

    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new ScenarioBudgetFlat();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new ScenarioBudgetFlat();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new ScenarioBudgetFlat();
        entity.District.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new ScenarioBudgetFlat();
        entity.District = 3;
        entity.District.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_BridgeInterstateBudget()
    {
        var entity = new ScenarioBudgetFlat();
        entity.BridgeInterstateBudget.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeInterstateBudget()
    {
        var entity = new ScenarioBudgetFlat();
        entity.BridgeInterstateBudget = 500000m;
        entity.BridgeInterstateBudget.ShouldBe(500000m);
    }

    [Fact]
    public void Should_Get_BridgeNonInterstateBudget()
    {
        var entity = new ScenarioBudgetFlat();
        entity.BridgeNonInterstateBudget.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeNonInterstateBudget()
    {
        var entity = new ScenarioBudgetFlat();
        entity.BridgeNonInterstateBudget = 200000m;
        entity.BridgeNonInterstateBudget.ShouldBe(200000m);
    }

    [Fact]
    public void Should_Get_PavementInterstateBudget()
    {
        var entity = new ScenarioBudgetFlat();
        entity.PavementInterstateBudget.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PavementInterstateBudget()
    {
        var entity = new ScenarioBudgetFlat();
        entity.PavementInterstateBudget = 750000m;
        entity.PavementInterstateBudget.ShouldBe(750000m);
    }

    [Fact]
    public void Should_Get_PavementNonInterstateBudget()
    {
        var entity = new ScenarioBudgetFlat();
        entity.PavementNonInterstateBudget.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PavementNonInterstateBudget()
    {
        var entity = new ScenarioBudgetFlat();
        entity.PavementNonInterstateBudget = 300000m;
        entity.PavementNonInterstateBudget.ShouldBe(300000m);
    }

    [Fact]
    public void Should_Get_YearWork()
    {
        var entity = new ScenarioBudgetFlat();
        entity.YearWork.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_YearWork()
    {
        var entity = new ScenarioBudgetFlat();
        entity.YearWork = 2026;
        entity.YearWork.ShouldBe(2026);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new ScenarioBudgetFlat();
        entity.Id = 1;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(1);
    }
}
