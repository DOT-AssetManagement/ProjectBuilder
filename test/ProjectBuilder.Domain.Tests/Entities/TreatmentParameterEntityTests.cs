using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class TreatmentParameterEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new TreatmentParameterEntity();
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new TreatmentParameterEntity();
        entity.Id = 5;
        entity.Id.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_UserTreatmentBenefitWeight()
    {
        var entity = new TreatmentParameterEntity();
        entity.UserTreatmentBenefitWeight.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UserTreatmentBenefitWeight()
    {
        var entity = new TreatmentParameterEntity();
        entity.UserTreatmentBenefitWeight = 0.75;
        entity.UserTreatmentBenefitWeight.ShouldBe(0.75);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new TreatmentParameterEntity();
        entity.Id = 3;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(3);
    }
}
