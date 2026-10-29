using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class TreatmentTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new Treatment();
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new Treatment();
        entity.Id = 42;
        entity.Id.ShouldBe(42);
    }

    [Fact]
    public void Should_Get_ImportTimeGeneratedId()
    {
        var entity = new Treatment();
        entity.ImportTimeGeneratedId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_ImportTimeGeneratedId()
    {
        var entity = new Treatment();
        var value = Guid.NewGuid();
        entity.ImportTimeGeneratedId = value;
        entity.ImportTimeGeneratedId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_SimulationId()
    {
        var entity = new Treatment();
        entity.SimulationId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_SimulationId()
    {
        var entity = new Treatment();
        var value = Guid.NewGuid();
        entity.SimulationId = value;
        entity.SimulationId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_NetworkId()
    {
        var entity = new Treatment();
        entity.NetworkId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_NetworkId()
    {
        var entity = new Treatment();
        var value = Guid.NewGuid();
        entity.NetworkId = value;
        entity.NetworkId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_AssetId()
    {
        var entity = new Treatment();
        entity.AssetId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_AssetId()
    {
        var entity = new Treatment();
        var value = Guid.NewGuid();
        entity.AssetId = value;
        entity.AssetId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new Treatment();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new Treatment();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_Asset()
    {
        var entity = new Treatment();
        entity.Asset.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Asset()
    {
        var entity = new Treatment();
        entity.Asset = "Test_Asset";
        entity.Asset.ShouldBe("Test_Asset");
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new Treatment();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new Treatment();
        entity.District = 5;
        entity.District.ShouldBe((byte?)5);
    }

    [Fact]
    public void Should_Get_CountyId()
    {
        var entity = new Treatment();
        entity.CountyId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CountyId()
    {
        var entity = new Treatment();
        entity.CountyId = 10;
        entity.CountyId.ShouldBe((byte?)10);
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new Treatment();
        entity.Route.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new Treatment();
        entity.Route = 42;
        entity.Route.ShouldBe(42);
    }

    [Fact]
    public void Should_Get_Direction()
    {
        var entity = new Treatment();
        entity.Direction.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Direction()
    {
        var entity = new Treatment();
        entity.Direction = 1;
        entity.Direction.ShouldBe((byte?)1);
    }

    [Fact]
    public void Should_Get_FromSection()
    {
        var entity = new Treatment();
        entity.FromSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_FromSection()
    {
        var entity = new Treatment();
        entity.FromSection = 100;
        entity.FromSection.ShouldBe(100);
    }

    [Fact]
    public void Should_Get_ToSection()
    {
        var entity = new Treatment();
        entity.ToSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ToSection()
    {
        var entity = new Treatment();
        entity.ToSection = 200;
        entity.ToSection.ShouldBe(200);
    }

    [Fact]
    public void Should_Get_Offset()
    {
        var entity = new Treatment();
        entity.Offset.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Offset()
    {
        var entity = new Treatment();
        entity.Offset = 50;
        entity.Offset.ShouldBe(50);
    }

    [Fact]
    public void Should_Get_Interstate()
    {
        var entity = new Treatment();
        entity.Interstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Interstate()
    {
        var entity = new Treatment();
        entity.Interstate = true;
        entity.Interstate.ShouldBe(true);
    }

    [Fact]
    public void Should_Get_TreatmentName()
    {
        var entity = new Treatment();
        entity.TreatmentName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentName()
    {
        var entity = new Treatment();
        entity.TreatmentName = "Overlay";
        entity.TreatmentName.ShouldBe("Overlay");
    }

    [Fact]
    public void Should_Get_Benefit()
    {
        var entity = new Treatment();
        entity.Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Benefit()
    {
        var entity = new Treatment();
        entity.Benefit = 12.5;
        entity.Benefit.ShouldBe(12.5);
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new Treatment();
        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new Treatment();
        entity.Cost = 25.75;
        entity.Cost.ShouldBe(25.75);
    }

    [Fact]
    public void Should_Get_Risk()
    {
        var entity = new Treatment();
        entity.Risk.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Risk()
    {
        var entity = new Treatment();
        entity.Risk = 5.5;
        entity.Risk.ShouldBe(5.5);
    }

    [Fact]
    public void Should_Get_PriorityOrder()
    {
        var entity = new Treatment();
        entity.PriorityOrder.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PriorityOrder()
    {
        var entity = new Treatment();
        entity.PriorityOrder = 1;
        entity.PriorityOrder.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_PreferredYear()
    {
        var entity = new Treatment();
        entity.PreferredYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PreferredYear()
    {
        var entity = new Treatment();
        entity.PreferredYear = 2025;
        entity.PreferredYear.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_MinYear()
    {
        var entity = new Treatment();
        entity.MinYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MinYear()
    {
        var entity = new Treatment();
        entity.MinYear = 2020;
        entity.MinYear.ShouldBe(2020);
    }

    [Fact]
    public void Should_Get_MaxYear()
    {
        var entity = new Treatment();
        entity.MaxYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MaxYear()
    {
        var entity = new Treatment();
        entity.MaxYear = 2030;
        entity.MaxYear.ShouldBe(2030);
    }

    [Fact]
    public void Should_Get_Brkey()
    {
        var entity = new Treatment();
        entity.Brkey.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Brkey()
    {
        var entity = new Treatment();
        entity.Brkey = "BR001";
        entity.Brkey.ShouldBe("BR001");
    }

    [Fact]
    public void Should_Get_BridgeId()
    {
        var entity = new Treatment();
        entity.BridgeId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeId()
    {
        var entity = new Treatment();
        entity.BridgeId = 123456789L;
        entity.BridgeId.ShouldBe(123456789L);
    }

    [Fact]
    public void Should_Get_RemainingLife()
    {
        var entity = new Treatment();
        entity.RemainingLife.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RemainingLife()
    {
        var entity = new Treatment();
        entity.RemainingLife = 10.5;
        entity.RemainingLife.ShouldBe(10.5);
    }

    [Fact]
    public void Should_Get_TreatmentFundingIgnoresSpendingLimit()
    {
        var entity = new Treatment();
        entity.TreatmentFundingIgnoresSpendingLimit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentFundingIgnoresSpendingLimit()
    {
        var entity = new Treatment();
        entity.TreatmentFundingIgnoresSpendingLimit = false;
        entity.TreatmentFundingIgnoresSpendingLimit.ShouldBe(false);
    }

    [Fact]
    public void Should_Get_TreatmentStatus()
    {
        var entity = new Treatment();
        entity.TreatmentStatus.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentStatus()
    {
        var entity = new Treatment();
        entity.TreatmentStatus = 1;
        entity.TreatmentStatus.ShouldBe((byte?)1);
    }

    [Fact]
    public void Should_Get_TreatmentCause()
    {
        var entity = new Treatment();
        entity.TreatmentCause.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentCause()
    {
        var entity = new Treatment();
        entity.TreatmentCause = 2;
        entity.TreatmentCause.ShouldBe((byte?)2);
    }

    [Fact]
    public void Should_Get_IsCommitted()
    {
        var entity = new Treatment();
        entity.IsCommitted.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsCommitted()
    {
        var entity = new Treatment();
        entity.IsCommitted = true;
        entity.IsCommitted.ShouldBe(true);
    }

    [Fact]
    public void Should_Get_IsIsolatedBridge()
    {
        var entity = new Treatment();
        entity.IsIsolatedBridge.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsIsolatedBridge()
    {
        var entity = new Treatment();
        entity.IsIsolatedBridge = false;
        entity.IsIsolatedBridge.ShouldBe(false);
    }

    [Fact]
    public void Should_Get_PopulatedBy()
    {
        var entity = new Treatment();
        entity.PopulatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PopulatedBy()
    {
        var entity = new Treatment();
        entity.PopulatedBy = "System";
        entity.PopulatedBy.ShouldBe("System");
    }

    [Fact]
    public void Should_Get_PopulatedAt()
    {
        var entity = new Treatment();
        entity.PopulatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PopulatedAt()
    {
        var entity = new Treatment();
        DateTime? value = new DateTime(2024, 1, 15, 10, 30, 0);
        entity.PopulatedAt = value;
        entity.PopulatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_County()
    {
        var entity = new Treatment();
        entity.County.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_County()
    {
        var entity = new Treatment();
        var county = new CountyEntity();
        entity.County = county;
        entity.County.ShouldBe(county);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new Treatment();
        entity.Id = 77;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(77);
    }
}
