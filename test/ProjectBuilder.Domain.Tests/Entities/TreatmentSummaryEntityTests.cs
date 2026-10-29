using ProjectBuilder.DataAccess;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class TreatmentSummaryEntityTests
{
    [Fact]
    public void Should_Get_ScenarioId()
    {
        var entity = new TreatmentSummaryEntity();
        entity.ScenarioId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioId()
    {
        var entity = new TreatmentSummaryEntity();
        entity.ScenarioId = 5;
        entity.ScenarioId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_District()
    {
        var entity = new TreatmentSummaryEntity();
        entity.District.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_District()
    {
        var entity = new TreatmentSummaryEntity();
        entity.District = 3;
        entity.District.ShouldBe((byte)3);
    }

    [Fact]
    public void Should_Get_SelectedYear()
    {
        var entity = new TreatmentSummaryEntity();
        entity.SelectedYear.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_SelectedYear()
    {
        var entity = new TreatmentSummaryEntity();
        entity.SelectedYear = 2025;
        entity.SelectedYear.ShouldBe(2025);
    }

    [Fact]
    public void Should_Get_Cost()
    {
        var entity = new TreatmentSummaryEntity();
        entity.Cost.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Cost()
    {
        var entity = new TreatmentSummaryEntity();
        entity.Cost = 250000.0;
        entity.Cost.ShouldBe(250000.0);
    }

    [Fact]
    public void Should_Get_Benefit()
    {
        var entity = new TreatmentSummaryEntity();
        entity.Benefit.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Benefit()
    {
        var entity = new TreatmentSummaryEntity();
        entity.Benefit = 300000.0;
        entity.Benefit.ShouldBe(300000.0);
    }

    [Fact]
    public void Should_Get_ProjectId()
    {
        var entity = new TreatmentSummaryEntity();
        entity.ProjectId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ProjectId()
    {
        var entity = new TreatmentSummaryEntity();
        entity.ProjectId = 10;
        entity.ProjectId.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_TreatmentDescription()
    {
        var entity = new TreatmentSummaryEntity();
        entity.TreatmentDescription.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentDescription()
    {
        var entity = new TreatmentSummaryEntity();
        entity.TreatmentDescription = "Bridge Deck Overlay";
        entity.TreatmentDescription.ShouldBe("Bridge Deck Overlay");
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new TreatmentSummaryEntity();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new TreatmentSummaryEntity();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new TreatmentSummaryEntity();
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new TreatmentSummaryEntity();
        entity.Id = 77;
        entity.Id.ShouldBe(77);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new TreatmentSummaryEntity();
        entity.Id = 99;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(99);
    }
}
