using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class RoleEntityTests
{
    [Fact]
    public void Should_Set_Name_Via_Constructor()
    {
        var entity = new RoleEntity("Admin");
        entity.Name.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new RoleEntity("Admin");
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new RoleEntity("Admin");
        entity.Id = 5;
        entity.Id.ShouldBe(5);
    }

    [Fact]
    public void Should_Set_Name()
    {
        var entity = new RoleEntity("Admin");
        entity.Name = "Editor";
        entity.Name.ShouldBe("Editor");
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new RoleEntity("Admin");
        entity.Id = 1;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(1);
    }
}
