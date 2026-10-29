using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class OpenIddictScopeEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new OpenIddictScopeEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Name()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Name.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Name()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Name = "api";
        entity.Name.ShouldBe("api");
    }

    [Fact]
    public void Should_Get_DisplayName()
    {
        var entity = new OpenIddictScopeEntity();
        entity.DisplayName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DisplayName()
    {
        var entity = new OpenIddictScopeEntity();
        entity.DisplayName = "API Access";
        entity.DisplayName.ShouldBe("API Access");
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Description = "API scope";
        entity.Description.ShouldBe("API scope");
    }

    [Fact]
    public void Should_Get_Resources()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Resources.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Resources()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Resources = "resource_server";
        entity.Resources.ShouldBe("resource_server");
    }

    [Fact]
    public void Should_Get_IsDeleted()
    {
        var entity = new OpenIddictScopeEntity();
        entity.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_IsDeleted()
    {
        var entity = new OpenIddictScopeEntity();
        entity.IsDeleted = true;
        entity.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Should_Get_Descriptions()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Descriptions.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Descriptions()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Descriptions = "{}";
        entity.Descriptions.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_DisplayNames()
    {
        var entity = new OpenIddictScopeEntity();
        entity.DisplayNames.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DisplayNames()
    {
        var entity = new OpenIddictScopeEntity();
        entity.DisplayNames = "{}";
        entity.DisplayNames.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_Properties()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Properties.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Properties()
    {
        var entity = new OpenIddictScopeEntity();
        entity.Properties = "{}";
        entity.Properties.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_ExtraProperties()
    {
        var entity = new OpenIddictScopeEntity();
        entity.ExtraProperties.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ExtraProperties()
    {
        var entity = new OpenIddictScopeEntity();
        entity.ExtraProperties = "{}";
        entity.ExtraProperties.ShouldBe("{}");
    }

    [Fact]
    public void Should_Get_ConcurrencyStamp()
    {
        var entity = new OpenIddictScopeEntity();
        entity.ConcurrencyStamp.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ConcurrencyStamp()
    {
        var entity = new OpenIddictScopeEntity();
        entity.ConcurrencyStamp = "stamp-001";
        entity.ConcurrencyStamp.ShouldBe("stamp-001");
    }

    [Fact]
    public void Should_Get_CreationTime()
    {
        var entity = new OpenIddictScopeEntity();
        entity.CreationTime.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Should_Set_CreationTime()
    {
        var entity = new OpenIddictScopeEntity();
        var value = new DateTime(2024, 1, 15);
        entity.CreationTime = value;
        entity.CreationTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CreatorId()
    {
        var entity = new OpenIddictScopeEntity();
        entity.CreatorId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatorId()
    {
        var entity = new OpenIddictScopeEntity();
        var value = Guid.NewGuid();
        entity.CreatorId = value;
        entity.CreatorId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModificationTime()
    {
        var entity = new OpenIddictScopeEntity();
        entity.LastModificationTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModificationTime()
    {
        var entity = new OpenIddictScopeEntity();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.LastModificationTime = value;
        entity.LastModificationTime.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_LastModifierId()
    {
        var entity = new OpenIddictScopeEntity();
        entity.LastModifierId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_LastModifierId()
    {
        var entity = new OpenIddictScopeEntity();
        var value = Guid.NewGuid();
        entity.LastModifierId = value;
        entity.LastModifierId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DeleterId()
    {
        var entity = new OpenIddictScopeEntity();
        entity.DeleterId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeleterId()
    {
        var entity = new OpenIddictScopeEntity();
        var value = Guid.NewGuid();
        entity.DeleterId = value;
        entity.DeleterId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_DeletionTime()
    {
        var entity = new OpenIddictScopeEntity();
        entity.DeletionTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DeletionTime()
    {
        var entity = new OpenIddictScopeEntity();
        DateTime? value = new DateTime(2024, 12, 31);
        entity.DeletionTime = value;
        entity.DeletionTime.ShouldBe(value);
    }
}
