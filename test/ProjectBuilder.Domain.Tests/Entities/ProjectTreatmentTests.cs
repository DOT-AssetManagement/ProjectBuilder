using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ProjectTreatmentTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new ProjectTreatment();
        entity.Id.ShouldBe(0L);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new ProjectTreatment();
        entity.Id = 999L;
        entity.Id.ShouldBe(999L);
    }

    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new ProjectTreatment();
        entity.ScenarioId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new ProjectTreatment();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_ProjectId()
    {
        var entity = new ProjectTreatment();
        entity.ProjectId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ProjectId()
    {
        var entity = new ProjectTreatment();
        entity.ProjectId = 10;
        entity.ProjectId.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new ProjectTreatment();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new ProjectTreatment();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new ProjectTreatment();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new ProjectTreatment();
        entity.District = 3;
        entity.District.ShouldBe((byte?)3);
    }

    [Fact]
    public void Should_Get_CountyId()
    {
        var entity = new ProjectTreatment();
        entity.CountyId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CountyId()
    {
        var entity = new ProjectTreatment();
        entity.CountyId = 7;
        entity.CountyId.ShouldBe((byte?)7);
    }

    [Fact]
    public void Should_Get_County()
    {
        var entity = new ProjectTreatment();
        entity.County.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_County()
    {
        var entity = new ProjectTreatment();
        entity.County = "Allegheny";
        entity.County.ShouldBe("Allegheny");
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new ProjectTreatment();
        entity.Route.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new ProjectTreatment();
        entity.Route = 30;
        entity.Route.ShouldBe(30);
    }

    [Fact]
    public void Should_Get_Direction()
    {
        var entity = new ProjectTreatment();
        entity.Direction.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Direction()
    {
        var entity = new ProjectTreatment();
        entity.Direction = 1;
        entity.Direction.ShouldBe((byte?)1);
    }

    [Fact]
    public void Should_Get_Section()
    {
        var entity = new ProjectTreatment();
        entity.Section.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Section()
    {
        var entity = new ProjectTreatment();
        entity.Section = "A1-B2";
        entity.Section.ShouldBe("A1-B2");
    }

    [Fact]
    public void Should_Get_Brkey()
    {
        var entity = new ProjectTreatment();
        entity.Brkey.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Brkey()
    {
        var entity = new ProjectTreatment();
        entity.Brkey = "BR001";
        entity.Brkey.ShouldBe("BR001");
    }

    [Fact]
    public void Should_Get_BridgeId()
    {
        var entity = new ProjectTreatment();
        entity.BridgeId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_BridgeId()
    {
        var entity = new ProjectTreatment();
        entity.BridgeId = 123456789L;
        entity.BridgeId.ShouldBe(123456789L);
    }

    [Fact]
    public void Should_Get_Interstate()
    {
        var entity = new ProjectTreatment();
        entity.Interstate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Interstate()
    {
        var entity = new ProjectTreatment();
        entity.Interstate = true;
        entity.Interstate.ShouldBe(true);
    }

    [Fact]
    public void Should_Get_IndirectCostDesign()
    {
        var entity = new ProjectTreatment();
        entity.IndirectCostDesign.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IndirectCostDesign()
    {
        var entity = new ProjectTreatment();
        entity.IndirectCostDesign = 150.50;
        entity.IndirectCostDesign.ShouldBe(150.50);
    }

    [Fact]
    public void Should_Get_IndirectCostRow()
    {
        var entity = new ProjectTreatment();
        entity.IndirectCostRow.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IndirectCostRow()
    {
        var entity = new ProjectTreatment();
        entity.IndirectCostRow = 200.75;
        entity.IndirectCostRow.ShouldBe(200.75);
    }

    [Fact]
    public void Should_Get_IndirectCostUtilities()
    {
        var entity = new ProjectTreatment();
        entity.IndirectCostUtilities.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IndirectCostUtilities()
    {
        var entity = new ProjectTreatment();
        entity.IndirectCostUtilities = 75.25;
        entity.IndirectCostUtilities.ShouldBe(75.25);
    }

    [Fact]
    public void Should_Get_IndirectCostOther()
    {
        var entity = new ProjectTreatment();
        entity.IndirectCostOther.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IndirectCostOther()
    {
        var entity = new ProjectTreatment();
        entity.IndirectCostOther = 50.00;
        entity.IndirectCostOther.ShouldBe(50.00);
    }

    [Fact]
    public void Should_Get_TreatmentType()
    {
        var entity = new ProjectTreatment();
        entity.TreatmentType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentType()
    {
        var entity = new ProjectTreatment();
        entity.TreatmentType = "Overlay";
        entity.TreatmentType.ShouldBe("Overlay");
    }

    [Fact]
    public void Should_Get_TotalCost()
    {
        var entity = new ProjectTreatment();
        entity.TotalCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TotalCost()
    {
        var entity = new ProjectTreatment();
        entity.TotalCost = 500000.0;
        entity.TotalCost.ShouldBe(500000.0);
    }

    [Fact]
    public void Should_Get_DirectCost()
    {
        var entity = new ProjectTreatment();
        entity.DirectCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DirectCost()
    {
        var entity = new ProjectTreatment();
        entity.DirectCost = 300000.0;
        entity.DirectCost.ShouldBe(300000.0);
    }

    [Fact]
    public void Should_Get_Benefit()
    {
        var entity = new ProjectTreatment();
        entity.Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Benefit()
    {
        var entity = new ProjectTreatment();
        entity.Benefit = 12.5;
        entity.Benefit.ShouldBe(12.5);
    }

    [Fact]
    public void Should_Get_Risk()
    {
        var entity = new ProjectTreatment();
        entity.Risk.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Risk()
    {
        var entity = new ProjectTreatment();
        entity.Risk = 5.5;
        entity.Risk.ShouldBe(5.5);
    }

    [Fact]
    public void Should_Get_IsCommitted()
    {
        var entity = new ProjectTreatment();
        entity.IsCommitted.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsCommitted()
    {
        var entity = new ProjectTreatment();
        entity.IsCommitted = true;
        entity.IsCommitted.ShouldBe(true);
    }

    [Fact]
    public void Should_Get_PriorityOrder()
    {
        var entity = new ProjectTreatment();
        entity.PriorityOrder.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PriorityOrder()
    {
        var entity = new ProjectTreatment();
        entity.PriorityOrder = 1;
        entity.PriorityOrder.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_IsUserCreated()
    {
        var entity = new ProjectTreatment();
        entity.IsUserCreated.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsUserCreated()
    {
        var entity = new ProjectTreatment();
        entity.IsUserCreated = false;
        entity.IsUserCreated.ShouldBe(false);
    }

    [Fact]
    public void Should_Get_PreferredYear()
    {
        var entity = new ProjectTreatment();
        entity.PreferredYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PreferredYear()
    {
        var entity = new ProjectTreatment();
        entity.PreferredYear = 2025;
        entity.PreferredYear.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_MinYear()
    {
        var entity = new ProjectTreatment();
        entity.MinYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MinYear()
    {
        var entity = new ProjectTreatment();
        entity.MinYear = 2020;
        entity.MinYear.ShouldBe(2020);
    }

    [Fact]
    public void Should_Get_MaxYear()
    {
        var entity = new ProjectTreatment();
        entity.MaxYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MaxYear()
    {
        var entity = new ProjectTreatment();
        entity.MaxYear = 2030;
        entity.MaxYear.ShouldBe(2030);
    }

    [Fact]
    public void Should_Get_SelectedYear()
    {
        var entity = new ProjectTreatment();
        entity.SelectedYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_SelectedYear()
    {
        var entity = new ProjectTreatment();
        entity.SelectedYear = 2027;
        entity.SelectedYear.ShouldBe(2027);
    }

    [Fact]
    public void Should_Get_UserTreatmentTypeNo()
    {
        var entity = new ProjectTreatment();
        entity.UserTreatmentTypeNo.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UserTreatmentTypeNo()
    {
        var entity = new ProjectTreatment();
        entity.UserTreatmentTypeNo = 3;
        entity.UserTreatmentTypeNo.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_UserTreatmentName()
    {
        var entity = new ProjectTreatment();
        entity.UserTreatmentName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UserTreatmentName()
    {
        var entity = new ProjectTreatment();
        entity.UserTreatmentName = "Custom Treatment";
        entity.UserTreatmentName.ShouldBe("Custom Treatment");
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new ProjectTreatment();
        entity.Id = 777L;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(777L);
    }
}
