using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class AllNeedsEntityTests
{
    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new AllNeedsEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new AllNeedsEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new AllNeedsEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new AllNeedsEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new AllNeedsEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new AllNeedsEntity();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_InterstateCost()
    {
        var entity = new AllNeedsEntity();
        entity.InterstateCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_InterstateCost()
    {
        var entity = new AllNeedsEntity();
        entity.InterstateCost = 1000000.0;
        entity.InterstateCost.ShouldBe(1000000.0);
    }

    [Fact]
    public void Should_Get_NonInterstateCost()
    {
        var entity = new AllNeedsEntity();
        entity.NonInterstateCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_NonInterstateCost()
    {
        var entity = new AllNeedsEntity();
        entity.NonInterstateCost = 500000.0;
        entity.NonInterstateCost.ShouldBe(500000.0);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new AllNeedsEntity();
        entity.Id = 2024;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2024);
    }
}

public class BridgeNeedsEntityTests
{
    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new BridgeNeedsEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new BridgeNeedsEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new BridgeNeedsEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new BridgeNeedsEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new BridgeNeedsEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new BridgeNeedsEntity();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_InterstateCost()
    {
        var entity = new BridgeNeedsEntity();
        entity.InterstateCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_InterstateCost()
    {
        var entity = new BridgeNeedsEntity();
        entity.InterstateCost = 2000000.0;
        entity.InterstateCost.ShouldBe(2000000.0);
    }

    [Fact]
    public void Should_Get_NonInterstateCost()
    {
        var entity = new BridgeNeedsEntity();
        entity.NonInterstateCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_NonInterstateCost()
    {
        var entity = new BridgeNeedsEntity();
        entity.NonInterstateCost = 800000.0;
        entity.NonInterstateCost.ShouldBe(800000.0);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new BridgeNeedsEntity();
        entity.Id = 2024;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2024);
    }
}

public class PavementNeedsEntityTests
{
    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new PavementNeedsEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new PavementNeedsEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new PavementNeedsEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new PavementNeedsEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new PavementNeedsEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new PavementNeedsEntity();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_InterstateCost()
    {
        var entity = new PavementNeedsEntity();
        entity.InterstateCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_InterstateCost()
    {
        var entity = new PavementNeedsEntity();
        entity.InterstateCost = 3000000.0;
        entity.InterstateCost.ShouldBe(3000000.0);
    }

    [Fact]
    public void Should_Get_NonInterstateCost()
    {
        var entity = new PavementNeedsEntity();
        entity.NonInterstateCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_NonInterstateCost()
    {
        var entity = new PavementNeedsEntity();
        entity.NonInterstateCost = 1200000.0;
        entity.NonInterstateCost.ShouldBe(1200000.0);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new PavementNeedsEntity();
        entity.Id = 2024;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2024);
    }
}

public class AllPotentialBenefitEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_InterBenefit()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.InterBenefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_InterBenefit()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.InterBenefit = 750000.0;
        entity.InterBenefit.ShouldBe(750000.0);
    }

    [Fact]
    public void Should_Get_NonInterBenefit()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.NonInterBenefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_NonInterBenefit()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.NonInterBenefit = 300000.0;
        entity.NonInterBenefit.ShouldBe(300000.0);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new AllPotentialBenefitEntity();
        entity.Id = 2024;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2024);
    }
}

public class BridgePotentialBenefitEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_InterBenefit()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.InterBenefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_InterBenefit()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.InterBenefit = 900000.0;
        entity.InterBenefit.ShouldBe(900000.0);
    }

    [Fact]
    public void Should_Get_NonInterBenefit()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.NonInterBenefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_NonInterBenefit()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.NonInterBenefit = 400000.0;
        entity.NonInterBenefit.ShouldBe(400000.0);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new BridgePotentialBenefitEntity();
        entity.Id = 2024;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2024);
    }
}

public class PavementPotentialBenefitEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.Id = 2025;
        entity.Id.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_InterBenefit()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.InterBenefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_InterBenefit()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.InterBenefit = 1100000.0;
        entity.InterBenefit.ShouldBe(1100000.0);
    }

    [Fact]
    public void Should_Get_NonInterBenefit()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.NonInterBenefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_NonInterBenefit()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.NonInterBenefit = 600000.0;
        entity.NonInterBenefit.ShouldBe(600000.0);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new PavementPotentialBenefitEntity();
        entity.Id = 2024;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(2024);
    }
}
