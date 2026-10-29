using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class UserClaimEntityTests
{
    [Fact]
    public void Should_Set_Name_And_Value_Via_Constructor()
    {
        var entity = new UserClaimEntity("district", "5");
        entity.Name.ShouldBe("district");
        entity.Value.ShouldBe("5");
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new UserClaimEntity("name", "value");
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new UserClaimEntity("name", "value");
        entity.Id = 10;
        entity.Id.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_UserId()
    {
        var entity = new UserClaimEntity("name", "value");
        entity.UserId.ShouldBe(0L);
    }

    [Fact]
    public void Should_Set_UserId()
    {
        var entity = new UserClaimEntity("name", "value");
        entity.UserId = 42L;
        entity.UserId.ShouldBe(42L);
    }

    [Fact]
    public void Should_Set_Name()
    {
        var entity = new UserClaimEntity("name", "value");
        entity.Name = "role";
        entity.Name.ShouldBe("role");
    }

    [Fact]
    public void Should_Set_Value()
    {
        var entity = new UserClaimEntity("name", "value");
        entity.Value = "Admin";
        entity.Value.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_User()
    {
        var entity = new UserClaimEntity("name", "value");
        entity.User.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_User()
    {
        var entity = new UserClaimEntity("name", "value");
        var user = new UserEntity();
        entity.User = user;
        entity.User.ShouldBe(user);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new UserClaimEntity("name", "value");
        entity.Id = 7;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(7);
    }
}
