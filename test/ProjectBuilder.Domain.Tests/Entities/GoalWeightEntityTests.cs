using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class GoalWeightEntityTests
{
    [Fact]
    public void Should_Get_GoalId()
    {
        var entity = new GoalWeightEntity();
        entity.GoalId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_GoalId()
    {
        var entity = new GoalWeightEntity();
        entity.GoalId = 5;
        entity.GoalId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_Weight()
    {
        var entity = new GoalWeightEntity();
        entity.Weight.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Weight()
    {
        var entity = new GoalWeightEntity();
        entity.Weight = 0.25f;
        entity.Weight.ShouldBe(0.25f);
    }
}
