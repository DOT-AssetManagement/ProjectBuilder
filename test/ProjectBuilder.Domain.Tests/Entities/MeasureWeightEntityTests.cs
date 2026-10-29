using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class MeasureWeightEntityTests
{
    [Fact]
    public void Should_Get_MeasureId()
    {
        var entity = new MeasureWeightEntity();
        entity.MeasureId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MeasureId()
    {
        var entity = new MeasureWeightEntity();
        entity.MeasureId = 5;
        entity.MeasureId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_GoalId()
    {
        var entity = new MeasureWeightEntity();
        entity.GoalId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_GoalId()
    {
        var entity = new MeasureWeightEntity();
        entity.GoalId = 2;
        entity.GoalId.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_Weight()
    {
        var entity = new MeasureWeightEntity();
        entity.Weight.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Weight()
    {
        var entity = new MeasureWeightEntity();
        entity.Weight = 0.3f;
        entity.Weight.ShouldBe(0.3f);
    }
}
