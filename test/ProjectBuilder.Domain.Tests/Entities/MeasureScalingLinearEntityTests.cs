using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class MeasureScalingLinearEntityTests
{
    [Fact]
    public void Should_Get_MeasureId()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MeasureId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_MeasureId()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MeasureId = 5;
        entity.MeasureId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_MinScore()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MinScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MinScore()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MinScore = 0;
        entity.MinScore.ShouldBe(0);
    }

    [Fact]
    public void Should_Get_MeasureScoreA()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MeasureScoreA.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MeasureScoreA()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MeasureScoreA = 10f;
        entity.MeasureScoreA.ShouldBe(10f);
    }

    [Fact]
    public void Should_Get_MaxScore()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MaxScore.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MaxScore()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MaxScore = 100f;
        entity.MaxScore.ShouldBe(100f);
    }

    [Fact]
    public void Should_Get_MeasureScoreB()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MeasureScoreB.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_MeasureScoreB()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.MeasureScoreB = 50f;
        entity.MeasureScoreB.ShouldBe(50f);
    }

    [Fact]
    public void Should_Get_Slope()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.Slope.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Slope()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.Slope = 1.5f;
        entity.Slope.ShouldBe(1.5f);
    }

    [Fact]
    public void Should_Get_Intercept()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.Intercept.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Intercept()
    {
        var entity = new MeasureScalingLinearEntity();
        entity.Intercept = 0.5f;
        entity.Intercept.ShouldBe(0.5f);
    }
}
