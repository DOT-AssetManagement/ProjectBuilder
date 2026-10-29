using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class OpenIddictAuthorizationEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new OpenIddictAuthorizationEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ApplicationId()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.ApplicationId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ApplicationId()
    {
        var entity = new OpenIddictAuthorizationEntity();
        var value = Guid.NewGuid();
        entity.ApplicationId = value;
        entity.ApplicationId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Status()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Status.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Status()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Status = "valid";
        entity.Status.ShouldBe("valid");
    }

    [Fact]
    public void Should_Get_Subject()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Subject.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Subject()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Subject = "user@example.com";
        entity.Subject.ShouldBe("user@example.com");
    }

    [Fact]
    public void Should_Get_Type()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Type.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Type()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Type = "permanent";
        entity.Type.ShouldBe("permanent");
    }

    [Fact]
    public void Should_Get_IsDeleted()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_IsDeleted()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.IsDeleted = true;
        entity.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Should_Get_CreationDate()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.CreationDate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreationDate()
    {
        var entity = new OpenIddictAuthorizationEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreationDate = value;
        entity.CreationDate.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Properties()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Properties.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Properties()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Properties = "{}";
        entity.Properties.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_Scopes()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Scopes.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Scopes()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.Scopes = "[]";
        entity.Scopes.ShouldBe("[]");
    }

    [Fact]
    public void Should_Get_ExtraProperties()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.ExtraProperties.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ExtraProperties()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.ExtraProperties = "{}";
        entity.ExtraProperties.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_ConcurrencyStamp()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.ConcurrencyStamp.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ConcurrencyStamp()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.ConcurrencyStamp = "stamp-001";
        entity.ConcurrencyStamp.ShouldBe("stamp-001");
    }

    [Fact]
    public void Should_Get_CreationTime()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.CreationTime.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Should_Set_CreationTime()
    {
        var entity = new OpenIddictAuthorizationEntity();
        var value = new DateTime(2024, 1, 15);
        entity.CreationTime = value;
        entity.CreationTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CreatorId()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.CreatorId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatorId()
    {
        var entity = new OpenIddictAuthorizationEntity();
        var value = Guid.NewGuid();
        entity.CreatorId = value;
        entity.CreatorId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModificationTime()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.LastModificationTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModificationTime()
    {
        var entity = new OpenIddictAuthorizationEntity();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.LastModificationTime = value;
        entity.LastModificationTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModifierId()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.LastModifierId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModifierId()
    {
        var entity = new OpenIddictAuthorizationEntity();
        var value = Guid.NewGuid();
        entity.LastModifierId = value;
        entity.LastModifierId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DeleterId()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.DeleterId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeleterId()
    {
        var entity = new OpenIddictAuthorizationEntity();
        var value = Guid.NewGuid();
        entity.DeleterId = value;
        entity.DeleterId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DeletionTime()
    {
        var entity = new OpenIddictAuthorizationEntity();
        entity.DeletionTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeletionTime()
    {
        var entity = new OpenIddictAuthorizationEntity();
        DateTime? value = new DateTime(2024, 12, 31);
        entity.DeletionTime = value;
        entity.DeletionTime.ShouldBe(value);
    }
}
