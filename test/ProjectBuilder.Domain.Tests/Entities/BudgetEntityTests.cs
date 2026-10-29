using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class BudgetEntityTests
{
    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new BudgetEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new BudgetEntity();
        entity.ScenarioId = 3;
        entity.ScenarioId.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new BudgetEntity();
        entity.District.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new BudgetEntity();
        entity.District = 5;
        entity.District.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_BridgeBudgetInterstate()
    {
        var entity = new BudgetEntity();
        entity.BridgeBudgetInterstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeBudgetInterstate()
    {
        var entity = new BudgetEntity();
        entity.BridgeBudgetInterstate = 1000000m;
        entity.BridgeBudgetInterstate.ShouldBe(1000000m);
    }

    [Fact]
    public void Should_Get_BridgeBudgetNonInterstate()
    {
        var entity = new BudgetEntity();
        entity.BridgeBudgetNonInterstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeBudgetNonInterstate()
    {
        var entity = new BudgetEntity();
        entity.BridgeBudgetNonInterstate = 500000m;
        entity.BridgeBudgetNonInterstate.ShouldBe(500000m);
    }

    [Fact]
    public void Should_Get_PavementBudgetInterstate()
    {
        var entity = new BudgetEntity();
        entity.PavementBudgetInterstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PavementBudgetInterstate()
    {
        var entity = new BudgetEntity();
        entity.PavementBudgetInterstate = 2000000m;
        entity.PavementBudgetInterstate.ShouldBe(2000000m);
    }

    [Fact]
    public void Should_Get_PavementBudgetNonInterstate()
    {
        var entity = new BudgetEntity();
        entity.PavementBudgetNonInterstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PavementBudgetNonInterstate()
    {
        var entity = new BudgetEntity();
        entity.PavementBudgetNonInterstate = 1500000m;
        entity.PavementBudgetNonInterstate.ShouldBe(1500000m);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new BudgetEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new BudgetEntity();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new BudgetEntity();
        entity.Id = 2024;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2024);
    }
}

public class BudgetSpentEntityTests
{
    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new BudgetSpentEntity();
        entity.ScenarioId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new BudgetSpentEntity();
        entity.ScenarioId = 3;
        entity.ScenarioId.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new BudgetSpentEntity();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new BudgetSpentEntity();
        entity.District = 5;
        entity.District.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_BridgeBudgetInterstate()
    {
        var entity = new BudgetSpentEntity();
        entity.BridgeBudgetInterstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeBudgetInterstate()
    {
        var entity = new BudgetSpentEntity();
        entity.BridgeBudgetInterstate = 800000.0;
        entity.BridgeBudgetInterstate.ShouldBe(800000.0);
    }

    [Fact]
    public void Should_Get_BridgeBudgetNonInterstate()
    {
        var entity = new BudgetSpentEntity();
        entity.BridgeBudgetNonInterstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeBudgetNonInterstate()
    {
        var entity = new BudgetSpentEntity();
        entity.BridgeBudgetNonInterstate = 400000.0;
        entity.BridgeBudgetNonInterstate.ShouldBe(400000.0);
    }

    [Fact]
    public void Should_Get_PavementBudgetInterstate()
    {
        var entity = new BudgetSpentEntity();
        entity.PavementBudgetInterstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PavementBudgetInterstate()
    {
        var entity = new BudgetSpentEntity();
        entity.PavementBudgetInterstate = 1800000.0;
        entity.PavementBudgetInterstate.ShouldBe(1800000.0);
    }

    [Fact]
    public void Should_Get_PavementBudgetNonInterstate()
    {
        var entity = new BudgetSpentEntity();
        entity.PavementBudgetNonInterstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PavementBudgetNonInterstate()
    {
        var entity = new BudgetSpentEntity();
        entity.PavementBudgetNonInterstate = 1200000.0;
        entity.PavementBudgetNonInterstate.ShouldBe(1200000.0);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new BudgetSpentEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new BudgetSpentEntity();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new BudgetSpentEntity();
        entity.Id = 2024;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2024);
    }
}
