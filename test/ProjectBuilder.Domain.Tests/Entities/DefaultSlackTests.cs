using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class DefaultSlackTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new DefaultSlack();
        entity.Id.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new DefaultSlack();
        entity.Id = "Bridge";
        entity.Id.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_MoveBefore()
    {
        var entity = new DefaultSlack();
        entity.MoveBefore.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_MoveBefore()
    {
        var entity = new DefaultSlack();
        entity.MoveBefore = 2;
        entity.MoveBefore.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_MoveAfter()
    {
        var entity = new DefaultSlack();
        entity.MoveAfter.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_MoveAfter()
    {
        var entity = new DefaultSlack();
        entity.MoveAfter = 3;
        entity.MoveAfter.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new DefaultSlack();
        entity.Id = "Pavement";
        var keys = entity.GetKeys();
        keys[0].ShouldBe("Pavement");
    }
}
