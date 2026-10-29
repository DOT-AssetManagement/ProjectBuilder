using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class UserTreatmentTypeTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new UserTreatmentType();
        entity.Id.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new UserTreatmentType();
        entity.Id = 5;
        entity.Id.ShouldBe(5);
    }

    [Fact]
    public void Should_Get_UserTreatmentName()
    {
        var entity = new UserTreatmentType();
        entity.UserTreatmentName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_UserTreatmentName()
    {
        var entity = new UserTreatmentType();
        entity.UserTreatmentName = "Custom Treatment";
        entity.UserTreatmentName.ShouldBe("Custom Treatment");
    }

    [Fact]
    public void Should_Return_UserTreatmentName_From_ToString()
    {
        var entity = new UserTreatmentType();
        entity.UserTreatmentName = "Custom Treatment";
        entity.ToString().ShouldBe("Custom Treatment");
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new UserTreatmentType();
        entity.Id = 3;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(3);
    }
}
