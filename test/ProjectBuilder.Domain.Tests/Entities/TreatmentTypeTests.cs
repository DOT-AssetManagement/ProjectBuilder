using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class TreatmentTypeTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new TreatmentType();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new TreatmentType();
        entity.Id = "Overlay";
        entity.Id.ShouldBe("Overlay");
    }

    [Fact]
    public void Should_Get_MoveEarlier()
    {
        var entity = new TreatmentType();
        entity.MoveEarlier.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MoveEarlier()
    {
        var entity = new TreatmentType();
        entity.MoveEarlier = 2;
        entity.MoveEarlier.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_MoveLater()
    {
        var entity = new TreatmentType();
        entity.MoveLater.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MoveLater()
    {
        var entity = new TreatmentType();
        entity.MoveLater = 3;
        entity.MoveLater.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new TreatmentType();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new TreatmentType();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new TreatmentType();
        entity.Id = "Overlay";
        var keys = entity.GetKeys();
        keys[0].ShouldBe("Overlay");
    }
}
