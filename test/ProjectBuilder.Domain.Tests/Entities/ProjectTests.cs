using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ProjectTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new Project();
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new Project();
        entity.Id = 42;
        entity.Id.ShouldBe(42);
    }

    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new Project();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new Project();
        entity.ScenarioId = 10;
        entity.ScenarioId.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new Project();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new Project();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new Project();
        entity.District.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new Project();
        entity.District = 5;
        entity.District.ShouldBe((byte?)5);
    }

    [Fact]
    public void Should_Get_CountyId()
    {
        var entity = new Project();
        entity.CountyId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CountyId()
    {
        var entity = new Project();
        entity.CountyId = 10;
        entity.CountyId.ShouldBe((byte?)10);
    }

    [Fact]
    public void Should_Get_County()
    {
        var entity = new Project();
        entity.County.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_County()
    {
        var entity = new Project();
        entity.County = "Allegheny";
        entity.County.ShouldBe("Allegheny");
    }

    [Fact]
    public void Should_Get_Route()
    {
        var entity = new Project();
        entity.Route.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Route()
    {
        var entity = new Project();
        entity.Route = 30;
        entity.Route.ShouldBe(30);
    }

    [Fact]
    public void Should_Get_Direction()
    {
        var entity = new Project();
        entity.Direction.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Direction()
    {
        var entity = new Project();
        entity.Direction = 1;
        entity.Direction.ShouldBe((byte?)1);
    }

    [Fact]
    public void Should_Get_PreferredStartingYear()
    {
        var entity = new Project();
        entity.PreferredStartingYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PreferredStartingYear()
    {
        var entity = new Project();
        entity.PreferredStartingYear = 2025;
        entity.PreferredStartingYear.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_TotalCost()
    {
        var entity = new Project();
        entity.TotalCost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TotalCost()
    {
        var entity = new Project();
        entity.TotalCost = 1500000.0;
        entity.TotalCost.ShouldBe(1500000.0);
    }

    [Fact]
    public void Should_Get_Benefit()
    {
        var entity = new Project();
        entity.Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Benefit()
    {
        var entity = new Project();
        entity.Benefit = 50000.0;
        entity.Benefit.ShouldBe(50000.0);
    }

    [Fact]
    public void Should_Get_NumberOfTreatments()
    {
        var entity = new Project();
        entity.NumberOfTreatments.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_NumberOfTreatments()
    {
        var entity = new Project();
        entity.NumberOfTreatments = 3;
        entity.NumberOfTreatments.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_Section()
    {
        var entity = new Project();
        entity.Section.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Section()
    {
        var entity = new Project();
        entity.Section = "A1-B2";
        entity.Section.ShouldBe("A1-B2");
    }

    [Fact]
    public void Should_Get_CommitmentStatus()
    {
        var entity = new Project();
        entity.CommitmentStatus.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CommitmentStatus()
    {
        var entity = new Project();
        entity.CommitmentStatus = "Committed";
        entity.CommitmentStatus.ShouldBe("Committed");
    }

    [Fact]
    public void Should_Get_Selected()
    {
        var entity = new Project();
        entity.Selected.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Selected()
    {
        var entity = new Project();
        entity.Selected = "Yes";
        entity.Selected.ShouldBe("Yes");
    }

    [Fact]
    public void Should_Get_SelectedFirstYear()
    {
        var entity = new Project();
        entity.SelectedFirstYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_SelectedFirstYear()
    {
        var entity = new Project();
        entity.SelectedFirstYear = 2026;
        entity.SelectedFirstYear.ShouldBe(2026);
    }

    [Fact]
    public void Should_Get_UserCreated()
    {
        var entity = new Project();
        entity.UserCreated.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UserCreated()
    {
        var entity = new Project();
        entity.UserCreated = "Admin";
        entity.UserCreated.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new Project();
        entity.Id = 99;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(99);
    }
}
