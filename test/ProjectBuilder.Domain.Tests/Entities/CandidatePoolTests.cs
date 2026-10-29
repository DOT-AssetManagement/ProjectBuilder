using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class CandidatePoolTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new CandidatePool();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new CandidatePool();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CandidatePoolNumber()
    {
        var entity = new CandidatePool();
        entity.CandidatePoolNumber.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_CandidatePoolNumber()
    {
        var entity = new CandidatePool();
        entity.CandidatePoolNumber = 7;
        entity.CandidatePoolNumber.ShouldBe(7);
    }

    [Fact]
    public void Should_Get_UserId()
    {
        var entity = new CandidatePool();
        entity.UserId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_UserId()
    {
        var entity = new CandidatePool();
        var userId = Guid.NewGuid();

        entity.UserId = userId;

        entity.UserId.ShouldBe(userId);
    }

    [Fact]
    public void Should_Get_Owner()
    {
        var entity = new CandidatePool();
        entity.Owner.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Owner()
    {
        var entity = new CandidatePool();
        entity.Owner = "Test_Owner";
        entity.Owner.ShouldBe("Test_Owner");
    }

    [Fact]
    public void Should_Get_Name()
    {
        var entity = new CandidatePool();
        entity.Name.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Name()
    {
        var entity = new CandidatePool();
        entity.Name = "Test_Pool";
        entity.Name.ShouldBe("Test_Pool");
    }

    [Fact]
    public void Should_Get_Description()
    {
        var entity = new CandidatePool();
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Description()
    {
        var entity = new CandidatePool();
        entity.Description = "Test_Description";
        entity.Description.ShouldBe("Test_Description");
    }

    [Fact]
    public void Should_Get_TreatmentsCount()
    {
        var entity = new CandidatePool();
        entity.TreatmentsCount.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_TreatmentsCount()
    {
        var entity = new CandidatePool();
        entity.TreatmentsCount = 10;
        entity.TreatmentsCount.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_BridgeTreatmentsCount()
    {
        var entity = new CandidatePool();
        entity.BridgeTreatmentsCount.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_BridgeTreatmentsCount()
    {
        var entity = new CandidatePool();
        entity.BridgeTreatmentsCount = 5;
        entity.BridgeTreatmentsCount.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_PavementTreatmentsCount()
    {
        var entity = new CandidatePool();
        entity.PavementTreatmentsCount.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_PavementTreatmentsCount()
    {
        var entity = new CandidatePool();
        entity.PavementTreatmentsCount = 3;
        entity.PavementTreatmentsCount.ShouldBe(3);
    }

    [Fact]
    public void Should_Get_ScenarioCount()
    {
        var entity = new CandidatePool();
        entity.ScenarioCount.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_ScenarioCount()
    {
        var entity = new CandidatePool();
        entity.ScenarioCount = 2;
        entity.ScenarioCount.ShouldBe(2);
    }

    [Fact]
    public void Should_Get_PopulatedAt()
    {
        var entity = new CandidatePool();
        entity.PopulatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_PopulatedAt()
    {
        var entity = new CandidatePool();
        DateTime? value = new DateTime(2024, 6, 1);
        entity.PopulatedAt = value;
        entity.PopulatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Source()
    {
        var entity = new CandidatePool();
        entity.Source.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Source()
    {
        var entity = new CandidatePool();
        entity.Source = "ARA";
        entity.Source.ShouldBe("ARA");
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new CandidatePool();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new CandidatePool();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new CandidatePool();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new CandidatePool();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_IsActive()
    {
        var entity = new CandidatePool();
        entity.IsActive.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_IsActive()
    {
        var entity = new CandidatePool();
        entity.IsActive = true;
        entity.IsActive.ShouldBe(true);
    }

    [Fact]
    public void Should_Get_IsShared()
    {
        var entity = new CandidatePool();
        entity.IsShared.ShouldBeFalse();
    }

    [Fact]
    public void Should_Set_IsShared()
    {
        var entity = new CandidatePool();
        entity.IsShared = true;
        entity.IsShared.ShouldBe(true);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new CandidatePool();
        var id = Guid.NewGuid();
        entity.Id = id;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(id);
    }
}
