using System;
using System.Linq;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class UserEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new UserEntity();
        entity.Id.ShouldBe(0L);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new UserEntity();
        entity.Id = 42L;
        entity.Id.ShouldBe(42L);
    }

    [Fact]
    public void Should_Get_B2CUserId()
    {
        var entity = new UserEntity();
        entity.B2CUserId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_B2CUserId()
    {
        var entity = new UserEntity();
        var value = Guid.NewGuid();
        entity.B2CUserId = value;
        entity.B2CUserId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Name()
    {
        var entity = new UserEntity();
        entity.Name.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Name()
    {
        var entity = new UserEntity();
        entity.Name = "John Doe";
        entity.Name.ShouldBe("John Doe");
    }

    [Fact]
    public void Should_Get_Email()
    {
        var entity = new UserEntity();
        entity.Email.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Email()
    {
        var entity = new UserEntity();
        entity.Email = "john@example.com";
        entity.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public void Should_Get_IsActive()
    {
        var entity = new UserEntity();
        entity.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_IsActive()
    {
        var entity = new UserEntity();
        entity.IsActive = true;
        entity.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_AddRole()
    {
        var entity = new UserEntity();
        entity.AddRole(1);
        entity.UserRoles.ShouldNotBeNull();
        entity.UserRoles.Count.ShouldBe(1);
        entity.UserRoles.First().RoleId.ShouldBe(1);
    }

    [Fact]
    public void Should_AddClaim()
    {
        var entity = new UserEntity();
        entity.AddClaim("district", "5");
        entity.UserClaims.ShouldNotBeNull();
        entity.UserClaims.Count.ShouldBe(1);
        entity.UserClaims.First().Name.ShouldBe("district");
        entity.UserClaims.First().Value.ShouldBe("5");
    }

    [Fact]
    public void Should_AddMultipleRoles()
    {
        var entity = new UserEntity();
        entity.AddRole(1);
        entity.AddRole(2);
        entity.UserRoles.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new UserEntity();
        entity.Id = 99L;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(99L);
    }
}
