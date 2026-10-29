using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class OpenIddictTokenEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new OpenIddictTokenEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ApplicationId()
    {
        var entity = new OpenIddictTokenEntity();
        entity.ApplicationId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ApplicationId()
    {
        var entity = new OpenIddictTokenEntity();
        var value = Guid.NewGuid();
        entity.ApplicationId = value;
        entity.ApplicationId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_AuthorizationId()
    {
        var entity = new OpenIddictTokenEntity();
        entity.AuthorizationId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AuthorizationId()
    {
        var entity = new OpenIddictTokenEntity();
        var value = Guid.NewGuid();
        entity.AuthorizationId = value;
        entity.AuthorizationId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Status()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Status.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Status()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Status = "valid";
        entity.Status.ShouldBe("valid");
    }

    [Fact]
    public void Should_Get_Subject()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Subject.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Subject()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Subject = "user@example.com";
        entity.Subject.ShouldBe("user@example.com");
    }

    [Fact]
    public void Should_Get_Type()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Type.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Type()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Type = "access_token";
        entity.Type.ShouldBe("access_token");
    }

    [Fact]
    public void Should_Get_Payload()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Payload.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Payload()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Payload = "token_payload";
        entity.Payload.ShouldBe("token_payload");
    }

    [Fact]
    public void Should_Get_ReferenceId()
    {
        var entity = new OpenIddictTokenEntity();
        entity.ReferenceId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ReferenceId()
    {
        var entity = new OpenIddictTokenEntity();
        entity.ReferenceId = "ref-001";
        entity.ReferenceId.ShouldBe("ref-001");
    }

    [Fact]
    public void Should_Get_ExpirationDate()
    {
        var entity = new OpenIddictTokenEntity();
        entity.ExpirationDate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ExpirationDate()
    {
        var entity = new OpenIddictTokenEntity();
        DateTime? value = new DateTime(2025, 1, 1);
        entity.ExpirationDate = value;
        entity.ExpirationDate.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IsDeleted()
    {
        var entity = new OpenIddictTokenEntity();
        entity.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_IsDeleted()
    {
        var entity = new OpenIddictTokenEntity();
        entity.IsDeleted = true;
        entity.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Should_Get_CreationDate()
    {
        var entity = new OpenIddictTokenEntity();
        entity.CreationDate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreationDate()
    {
        var entity = new OpenIddictTokenEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreationDate = value;
        entity.CreationDate.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_RedemptionDate()
    {
        var entity = new OpenIddictTokenEntity();
        entity.RedemptionDate.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RedemptionDate()
    {
        var entity = new OpenIddictTokenEntity();
        DateTime? value = new DateTime(2024, 2, 1);
        entity.RedemptionDate = value;
        entity.RedemptionDate.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Properties()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Properties.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Properties()
    {
        var entity = new OpenIddictTokenEntity();
        entity.Properties = "{}";
        entity.Properties.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_ExtraProperties()
    {
        var entity = new OpenIddictTokenEntity();
        entity.ExtraProperties.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ExtraProperties()
    {
        var entity = new OpenIddictTokenEntity();
        entity.ExtraProperties = "{}";
        entity.ExtraProperties.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_ConcurrencyStamp()
    {
        var entity = new OpenIddictTokenEntity();
        entity.ConcurrencyStamp.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ConcurrencyStamp()
    {
        var entity = new OpenIddictTokenEntity();
        entity.ConcurrencyStamp = "stamp-001";
        entity.ConcurrencyStamp.ShouldBe("stamp-001");
    }

    [Fact]
    public void Should_Get_CreationTime()
    {
        var entity = new OpenIddictTokenEntity();
        entity.CreationTime.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Should_Set_CreationTime()
    {
        var entity = new OpenIddictTokenEntity();
        var value = new DateTime(2024, 1, 15);
        entity.CreationTime = value;
        entity.CreationTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CreatorId()
    {
        var entity = new OpenIddictTokenEntity();
        entity.CreatorId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatorId()
    {
        var entity = new OpenIddictTokenEntity();
        var value = Guid.NewGuid();
        entity.CreatorId = value;
        entity.CreatorId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModificationTime()
    {
        var entity = new OpenIddictTokenEntity();
        entity.LastModificationTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModificationTime()
    {
        var entity = new OpenIddictTokenEntity();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.LastModificationTime = value;
        entity.LastModificationTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModifierId()
    {
        var entity = new OpenIddictTokenEntity();
        entity.LastModifierId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModifierId()
    {
        var entity = new OpenIddictTokenEntity();
        var value = Guid.NewGuid();
        entity.LastModifierId = value;
        entity.LastModifierId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DeleterId()
    {
        var entity = new OpenIddictTokenEntity();
        entity.DeleterId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeleterId()
    {
        var entity = new OpenIddictTokenEntity();
        var value = Guid.NewGuid();
        entity.DeleterId = value;
        entity.DeleterId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DeletionTime()
    {
        var entity = new OpenIddictTokenEntity();
        entity.DeletionTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeletionTime()
    {
        var entity = new OpenIddictTokenEntity();
        DateTime? value = new DateTime(2024, 12, 31);
        entity.DeletionTime = value;
        entity.DeletionTime.ShouldBe(value);
    }
}
