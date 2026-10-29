using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class AssetTypeParameterDataEntityTests
{
    [Fact]
    public void Should_Get_AssetType()
    {
        var entity = new AssetTypeParameterDataEntity();
        entity.AssetType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_AssetType()
    {
        var entity = new AssetTypeParameterDataEntity();
        entity.AssetType = "Bridge";
        entity.AssetType.ShouldBe("Bridge");
    }

    [Fact]
    public void Should_Get_BenefitScalingFactor()
    {
        var entity = new AssetTypeParameterDataEntity();
        entity.BenefitScalingFactor.ShouldBe(0f);
    }

    [Fact]
    public void Should_Set_BenefitScalingFactor()
    {
        var entity = new AssetTypeParameterDataEntity();
        entity.BenefitScalingFactor = 1.5f;
        entity.BenefitScalingFactor.ShouldBe(1.5f);
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new AssetTypeParameterDataEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new AssetTypeParameterDataEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new AssetTypeParameterDataEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new AssetTypeParameterDataEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ScenId()
    {
        var entity = new AssetTypeParameterDataEntity();
        entity.ScenId.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ScenId()
    {
        var entity = new AssetTypeParameterDataEntity();
        var value = Guid.NewGuid();
        entity.ScenId = value;
        entity.ScenId.ShouldBe(value);
    }
}
