using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class BenefitParameterEntityTests
{
    [Fact]
    public void Should_Get_Family()
    {
        var entity = new BenefitParameterEntity();
        entity.Family.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Family()
    {
        var entity = new BenefitParameterEntity();
        entity.Family = "Safety";
        entity.Family.ShouldBe("Safety");
    }

    [Fact]
    public void Should_Get_Code()
    {
        var entity = new BenefitParameterEntity();
        entity.Code.ShouldBe((byte)0);
    }

    [Fact]
    public void Should_Set_Code()
    {
        var entity = new BenefitParameterEntity();
        entity.Code = 5;
        entity.Code.ShouldBe((byte)5);
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new BenefitParameterEntity();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new BenefitParameterEntity();
        entity.Description = "Test_Description";
        entity.Description.ShouldBe("Test_Description");
    }

    [Fact]
    public void Should_Get_Val()
    {
        var entity = new BenefitParameterEntity();
        entity.Val.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Val()
    {
        var entity = new BenefitParameterEntity();
        entity.Val = 3.14f;
        entity.Val.ShouldBe(3.14f);
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new BenefitParameterEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new BenefitParameterEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new BenefitParameterEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new BenefitParameterEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }
}
