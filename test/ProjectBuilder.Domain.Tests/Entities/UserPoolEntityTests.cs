using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class UserPoolEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new UserPoolEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new UserPoolEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LibNo()
    {
        var entity = new UserPoolEntity();
        entity.LibNo.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_LibNo()
    {
        var entity = new UserPoolEntity();
        entity.LibNo = 7;
        entity.LibNo.ShouldBe(7);
    }

    [Fact]
    public void Should_Get_UserId()
    {
        var entity = new UserPoolEntity();
        entity.UserId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_UserId()
    {
        var entity = new UserPoolEntity();
        var value = Guid.NewGuid();
        entity.UserId = value;
        entity.UserId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Name()
    {
        var entity = new UserPoolEntity();
        entity.Name.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Name()
    {
        var entity = new UserPoolEntity();
        entity.Name = "My Pool";
        entity.Name.ShouldBe("My Pool");
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new UserPoolEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new UserPoolEntity();
        entity.Description = "Test Description";
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new UserPoolEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new UserPoolEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new UserPoolEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new UserPoolEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IsActive()
    {
        var entity = new UserPoolEntity();
        entity.IsActive.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsActive()
    {
        var entity = new UserPoolEntity();
        entity.IsActive = true;
        entity.IsActive.ShouldBe(true);
    }

    [Fact]
    public void Should_Get_UpdatedBy()
    {
        var entity = new UserPoolEntity();
        entity.UpdatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UpdatedBy()
    {
        var entity = new UserPoolEntity();
        entity.UpdatedBy = "Editor";
        entity.UpdatedBy.ShouldBe("Editor");
    }

    [Fact]
    public void Should_Get_UpdatedAt()
    {
        var entity = new UserPoolEntity();
        entity.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UpdatedAt()
    {
        var entity = new UserPoolEntity();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.UpdatedAt = value;
        entity.UpdatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IsSharedLibrary()
    {
        var entity = new UserPoolEntity();
        entity.IsSharedLibrary.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsSharedLibrary()
    {
        var entity = new UserPoolEntity();
        entity.IsSharedLibrary = false;
        entity.IsSharedLibrary.ShouldBe(false);
    }

    [Fact]
    public void Should_Get_SourcePoolId()
    {
        var entity = new UserPoolEntity();
        entity.SourcePoolId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_SourcePoolId()
    {
        var entity = new UserPoolEntity();
        var value = Guid.NewGuid();
        entity.SourcePoolId = value;
        entity.SourcePoolId.ShouldBe(value);
    }
}
