using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class UserTreatmentTests
{
    [Fact]
    public void Should_Get_LibraryId()
    {
        var entity = new UserTreatment();

        entity.LibraryId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_LibraryId()
    {
        var entity = new UserTreatment();
        var value = Guid.NewGuid();

        entity.LibraryId = value;

        entity.LibraryId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ImportedTreatmentId()
    {
        var entity = new UserTreatment();

        entity.ImportedTreatmentId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ImportedTreatmentId()
    {
        var entity = new UserTreatment();
        var value = 42;

        entity.ImportedTreatmentId = value;

        entity.ImportedTreatmentId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new UserTreatment();

        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new UserTreatment();
        var value = Guid.NewGuid();

        entity.Id = value;

        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_SimulationId()
    {
        var entity = new UserTreatment();

        entity.SimulationId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_SimulationId()
    {
        var entity = new UserTreatment();
        var value = Guid.NewGuid();

        entity.SimulationId = value;

        entity.SimulationId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_NetworkId()
    {
        var entity = new UserTreatment();

        entity.NetworkId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_NetworkId()
    {
        var entity = new UserTreatment();
        var value = Guid.NewGuid();

        entity.NetworkId = value;

        entity.NetworkId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_AssetId()
    {
        var entity = new UserTreatment();

        entity.AssetId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_AssetId()
    {
        var entity = new UserTreatment();
        var value = Guid.NewGuid();

        entity.AssetId = value;

        entity.AssetId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new UserTreatment();

        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new UserTreatment();
        var value = "Test_AssetType";

        entity.AssetType = value;

        entity.AssetType.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Asset()
    {
        var entity = new UserTreatment();

        entity.Asset.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Asset()
    {
        var entity = new UserTreatment();
        var value = "Test_Asset";

        entity.Asset = value;

        entity.Asset.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new UserTreatment();

        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new UserTreatment();
        byte? value = 5;

        entity.District = value;

        entity.District.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CountyId()
    {
        var entity = new UserTreatment();

        entity.CountyId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CountyId()
    {
        var entity = new UserTreatment();
        byte? value = 10;

        entity.CountyId = value;

        entity.CountyId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_County()
    {
        var entity = new UserTreatment();

        entity.County.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_County()
    {
        var entity = new UserTreatment();
        var value = new CountyEntity();

        entity.County = value;

        entity.County.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new UserTreatment();

        entity.Route.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new UserTreatment();
        int? value = 42;

        entity.Route = value;

        entity.Route.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Direction()
    {
        var entity = new UserTreatment();

        entity.Direction.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Direction()
    {
        var entity = new UserTreatment();
        byte? value = 1;

        entity.Direction = value;

        entity.Direction.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_FromSection()
    {
        var entity = new UserTreatment();

        entity.FromSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_FromSection()
    {
        var entity = new UserTreatment();
        int? value = 100;

        entity.FromSection = value;

        entity.FromSection.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ToSection()
    {
        var entity = new UserTreatment();

        entity.ToSection.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ToSection()
    {
        var entity = new UserTreatment();
        int? value = 200;

        entity.ToSection = value;

        entity.ToSection.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Offset()
    {
        var entity = new UserTreatment();

        entity.Offset.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Offset()
    {
        var entity = new UserTreatment();
        int? value = 50;

        entity.Offset = value;

        entity.Offset.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Interstate()
    {
        var entity = new UserTreatment();

        entity.Interstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Interstate()
    {
        var entity = new UserTreatment();
        bool? value = true;

        entity.Interstate = value;

        entity.Interstate.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Treatment()
    {
        var entity = new UserTreatment();

        entity.Treatment.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Treatment()
    {
        var entity = new UserTreatment();
        var value = "Test_Treatment";

        entity.Treatment = value;

        entity.Treatment.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Benefit()
    {
        var entity = new UserTreatment();

        entity.Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Benefit()
    {
        var entity = new UserTreatment();
        double? value = 12.5;

        entity.Benefit = value;

        entity.Benefit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new UserTreatment();

        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new UserTreatment();
        double? value = 25.75;

        entity.Cost = value;

        entity.Cost.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Risk()
    {
        var entity = new UserTreatment();

        entity.Risk.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Risk()
    {
        var entity = new UserTreatment();
        double? value = 5.5;

        entity.Risk = value;

        entity.Risk.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_PriorityOrder()
    {
        var entity = new UserTreatment();

        entity.PriorityOrder.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PriorityOrder()
    {
        var entity = new UserTreatment();
        byte? value = 1;

        entity.PriorityOrder = value;

        entity.PriorityOrder.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_PreferredYear()
    {
        var entity = new UserTreatment();

        entity.PreferredYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PreferredYear()
    {
        var entity = new UserTreatment();
        int? value = 2024;

        entity.PreferredYear = value;

        entity.PreferredYear.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_MinYear()
    {
        var entity = new UserTreatment();

        entity.MinYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MinYear()
    {
        var entity = new UserTreatment();
        int? value = 2020;

        entity.MinYear = value;

        entity.MinYear.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_MaxYear()
    {
        var entity = new UserTreatment();

        entity.MaxYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MaxYear()
    {
        var entity = new UserTreatment();
        int? value = 2030;

        entity.MaxYear = value;

        entity.MaxYear.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Brkey()
    {
        var entity = new UserTreatment();

        entity.Brkey.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Brkey()
    {
        var entity = new UserTreatment();
        var value = "Test_BRKEY";

        entity.Brkey = value;

        entity.Brkey.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_BridgeId()
    {
        var entity = new UserTreatment();

        entity.BridgeId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeId()
    {
        var entity = new UserTreatment();
        long? value = 123456789;

        entity.BridgeId = value;

        entity.BridgeId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_RemainingLife()
    {
        var entity = new UserTreatment();

        entity.RemainingLife.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RemainingLife()
    {
        var entity = new UserTreatment();
        double? value = 10.5;

        entity.RemainingLife = value;

        entity.RemainingLife.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_TreatmentFundingIgnoresSpendingLimit()
    {
        var entity = new UserTreatment();

        entity.TreatmentFundingIgnoresSpendingLimit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentFundingIgnoresSpendingLimit()
    {
        var entity = new UserTreatment();
        bool? value = false;

        entity.TreatmentFundingIgnoresSpendingLimit = value;

        entity.TreatmentFundingIgnoresSpendingLimit.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_TreatmentStatus()
    {
        var entity = new UserTreatment();

        entity.TreatmentStatus.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentStatus()
    {
        var entity = new UserTreatment();
        byte? value = 1;

        entity.TreatmentStatus = value;

        entity.TreatmentStatus.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_TreatmentCause()
    {
        var entity = new UserTreatment();

        entity.TreatmentCause.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentCause()
    {
        var entity = new UserTreatment();
        byte? value = 2;

        entity.TreatmentCause = value;

        entity.TreatmentCause.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IsCommitted()
    {
        var entity = new UserTreatment();

        entity.IsCommitted.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsCommitted()
    {
        var entity = new UserTreatment();
        bool? value = true;

        entity.IsCommitted = value;

        entity.IsCommitted.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IsIsolatedBridge()
    {
        var entity = new UserTreatment();

        entity.IsIsolatedBridge.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsIsolatedBridge()
    {
        var entity = new UserTreatment();
        bool? value = false;

        entity.IsIsolatedBridge = value;

        entity.IsIsolatedBridge.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_PopulatedAt()
    {
        var entity = new UserTreatment();

        entity.PopulatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PopulatedAt()
    {
        var entity = new UserTreatment();
        DateTime? value = new DateTime(2024, 1, 15, 10, 30, 0);

        entity.PopulatedAt = value;

        entity.PopulatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IsUserTreatment()
    {
        var entity = new UserTreatment();

        entity.IsUserTreatment.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsUserTreatment()
    {
        var entity = new UserTreatment();
        bool? value = true;

        entity.IsUserTreatment = value;

        entity.IsUserTreatment.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_UserTreatmentTypeNo()
    {
        var entity = new UserTreatment();

        entity.UserTreatmentTypeNo.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UserTreatmentTypeNo()
    {
        var entity = new UserTreatment();
        int? value = 5;

        entity.UserTreatmentTypeNo = value;

        entity.UserTreatmentTypeNo.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_UserTreatmentName()
    {
        var entity = new UserTreatment();

        entity.UserTreatmentName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UserTreatmentName()
    {
        var entity = new UserTreatment();
        var value = "Test_UserTreatmentName";

        entity.UserTreatmentName = value;

        entity.UserTreatmentName.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IndirectCostDesign()
    {
        var entity = new UserTreatment();

        entity.IndirectCostDesign.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IndirectCostDesign()
    {
        var entity = new UserTreatment();
        double? value = 150.50;

        entity.IndirectCostDesign = value;

        entity.IndirectCostDesign.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IndirectCostRow()
    {
        var entity = new UserTreatment();

        entity.IndirectCostRow.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IndirectCostRow()
    {
        var entity = new UserTreatment();
        double? value = 200.75;

        entity.IndirectCostRow = value;

        entity.IndirectCostRow.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IndirectCostUtilities()
    {
        var entity = new UserTreatment();

        entity.IndirectCostUtilities.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IndirectCostUtilities()
    {
        var entity = new UserTreatment();
        double? value = 75.25;

        entity.IndirectCostUtilities = value;

        entity.IndirectCostUtilities.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IndirectCostOther()
    {
        var entity = new UserTreatment();

        entity.IndirectCostOther.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IndirectCostOther()
    {
        var entity = new UserTreatment();
        double? value = 50.00;

        entity.IndirectCostOther = value;

        entity.IndirectCostOther.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new UserTreatment();
        var testId = Guid.NewGuid();
        entity.Id = testId;

        var keys = entity.GetKeys();

        keys.ShouldNotBeEmpty();
        keys[0].ShouldBe(testId);
    }
}
