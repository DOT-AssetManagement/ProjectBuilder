using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class OpenIddictApplicationEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new OpenIddictApplicationEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ClientId()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ClientId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ClientId()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ClientId = "my-client";
        entity.ClientId.ShouldBe("my-client");
    }

    [Fact]
    public void Should_Get_DisplayName()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.DisplayName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DisplayName()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.DisplayName = "My App";
        entity.DisplayName.ShouldBe("My App");
    }

    [Fact]
    public void Should_Get_ClientType()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ClientType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ClientType()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ClientType = "public";
        entity.ClientType.ShouldBe("public");
    }

    [Fact]
    public void Should_Get_IsDeleted()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_IsDeleted()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.IsDeleted = true;
        entity.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Should_Get_ConcurrencyStamp()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ConcurrencyStamp.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ConcurrencyStamp()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ConcurrencyStamp = "stamp-001";
        entity.ConcurrencyStamp.ShouldBe("stamp-001");
    }

    [Fact]
    public void Should_Get_CreationTime()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.CreationTime.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Should_Set_CreationTime()
    {
        var entity = new OpenIddictApplicationEntity();
        var value = new DateTime(2024, 1, 15);
        entity.CreationTime = value;
        entity.CreationTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DeletionTime()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.DeletionTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeletionTime()
    {
        var entity = new OpenIddictApplicationEntity();
        DateTime? value = new DateTime(2024, 12, 31);
        entity.DeletionTime = value;
        entity.DeletionTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ApplicationType()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ApplicationType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ApplicationType()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ApplicationType = "web";
        entity.ApplicationType.ShouldBe("web");
    }

    [Fact]
    public void Should_Get_ClientSecret()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ClientSecret.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ClientSecret()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ClientSecret = "secret";
        entity.ClientSecret.ShouldBe("secret");
    }

    [Fact]
    public void Should_Get_ConsentType()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ConsentType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ConsentType()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ConsentType = "implicit";
        entity.ConsentType.ShouldBe("implicit");
    }

    [Fact]
    public void Should_Get_DisplayNames()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.DisplayNames.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DisplayNames()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.DisplayNames = "{}";
        entity.DisplayNames.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_JsonWebKeySet()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.JsonWebKeySet.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_JsonWebKeySet()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.JsonWebKeySet = "{}";
        entity.JsonWebKeySet.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_Permissions()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Permissions.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Permissions()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Permissions = "[]";
        entity.Permissions.ShouldBe("[]");
    }

    [Fact]
    public void Should_Get_PostLogoutRedirectUris()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.PostLogoutRedirectUris.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PostLogoutRedirectUris()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.PostLogoutRedirectUris = "[]";
        entity.PostLogoutRedirectUris.ShouldBe("[]");
    }

    [Fact]
    public void Should_Get_Properties()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Properties.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Properties()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Properties = "{}";
        entity.Properties.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_RedirectUris()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.RedirectUris.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_RedirectUris()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.RedirectUris = "[]";
        entity.RedirectUris.ShouldBe("[]");
    }

    [Fact]
    public void Should_Get_Requirements()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Requirements.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Requirements()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Requirements = "[]";
        entity.Requirements.ShouldBe("[]");
    }

    [Fact]
    public void Should_Get_Settings()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Settings.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Settings()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.Settings = "{}";
        entity.Settings.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_ClientUri()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ClientUri.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ClientUri()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ClientUri = "https://client.example.com";
        entity.ClientUri.ShouldBe("https://client.example.com");
    }

    [Fact]
    public void Should_Get_LogoUri()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.LogoUri.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LogoUri()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.LogoUri = "https://logo.example.com/logo.png";
        entity.LogoUri.ShouldBe("https://logo.example.com/logo.png");
    }

    [Fact]
    public void Should_Get_ExtraProperties()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ExtraProperties.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ExtraProperties()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.ExtraProperties = "{}";
        entity.ExtraProperties.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_CreatorId()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.CreatorId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatorId()
    {
        var entity = new OpenIddictApplicationEntity();
        var value = Guid.NewGuid();
        entity.CreatorId = value;
        entity.CreatorId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModificationTime()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.LastModificationTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModificationTime()
    {
        var entity = new OpenIddictApplicationEntity();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.LastModificationTime = value;
        entity.LastModificationTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModifierId()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.LastModifierId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModifierId()
    {
        var entity = new OpenIddictApplicationEntity();
        var value = Guid.NewGuid();
        entity.LastModifierId = value;
        entity.LastModifierId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DeleterId()
    {
        var entity = new OpenIddictApplicationEntity();
        entity.DeleterId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeleterId()
    {
        var entity = new OpenIddictApplicationEntity();
        var value = Guid.NewGuid();
        entity.DeleterId = value;
        entity.DeleterId.ShouldBe(value);
    }
}
