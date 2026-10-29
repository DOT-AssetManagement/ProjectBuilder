using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class UserRoleEntityTests
{
    [Fact]
    public void Should_Set_RoleId_Via_Constructor()
    {
        var entity = new UserRoleEntity(3);
        entity.RoleId.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new UserRoleEntity(1);
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new UserRoleEntity(1);
        entity.Id = 10;
        entity.Id.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_UserId()
    {
        var entity = new UserRoleEntity(1);
        entity.UserId.ShouldBe(0L);
    }

    [Fact]
    public void Should_Set_UserId()
    {
        var entity = new UserRoleEntity(1);
        entity.UserId = 42L;
        entity.UserId.ShouldBe(42L);
    }

    [Fact]
    public void Should_Set_RoleId()
    {
        var entity = new UserRoleEntity(1);
        entity.RoleId = 5;
        entity.RoleId.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_User()
    {
        var entity = new UserRoleEntity(1);
        entity.User.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_User()
    {
        var entity = new UserRoleEntity(1);
        var user = new UserEntity();
        entity.User = user;
        entity.User.ShouldBe(user);
    }

    [Fact]
    public void Should_Get_Role()
    {
        var entity = new UserRoleEntity(1);
        entity.Role.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Role()
    {
        var entity = new UserRoleEntity(1);
        var role = new RoleEntity("Admin");
        entity.Role = role;
        entity.Role.ShouldBe(role);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new UserRoleEntity(1);
        entity.Id = 7;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(7);
    }
}
