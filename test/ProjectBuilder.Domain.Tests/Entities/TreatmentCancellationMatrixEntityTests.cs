using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class TreatmentCancellationMatrixEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.Id = "Overlay";
        entity.Id.ShouldBe("Overlay");
    }

    [Fact]
    public void Should_Get_AssetTypeA()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.AssetTypeA.ShouldBe(default(char));
    }

    [Fact]
    public void Should_Set_AssetTypeA()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.AssetTypeA = 'B';
        entity.AssetTypeA.ShouldBe('B');
    }

    [Fact]
    public void Should_Get_AssetTypeB()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.AssetTypeB.ShouldBe(default(char));
    }

    [Fact]
    public void Should_Set_AssetTypeB()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.AssetTypeB = 'P';
        entity.AssetTypeB.ShouldBe('P');
    }

    [Fact]
    public void Should_Get_TreatmentB()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.TreatmentB.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_TreatmentB()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.TreatmentB = "Crack Seal";
        entity.TreatmentB.ShouldBe("Crack Seal");
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new TreatmentCancellationMatrixEntity();
        entity.Id = "Overlay";
        var keys = entity.GetKeys();
        keys[0].ShouldBe("Overlay");
    }
}
