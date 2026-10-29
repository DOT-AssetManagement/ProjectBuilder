using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class DataSectionEntityTests
{
    [Fact]
    public void Should_Get_Id()
    {
        var entity = new DataSectionEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new DataSectionEntity();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Name()
    {
        var entity = new DataSectionEntity();
        entity.Name.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Name()
    {
        var entity = new DataSectionEntity();
        entity.Name = "Section A";
        entity.Name.ShouldBe("Section A");
    }

    [Fact]
    public void Should_Get_Notes()
    {
        var entity = new DataSectionEntity();
        entity.Notes.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Notes()
    {
        var entity = new DataSectionEntity();
        entity.Notes = "Some notes";
        entity.Notes.ShouldBe("Some notes");
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new DataSectionEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new DataSectionEntity();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new DataSectionEntity();
        entity.CreatedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new DataSectionEntity();
        DateTime? value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ShortName()
    {
        var entity = new DataSectionEntity();
        entity.ShortName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ShortName()
    {
        var entity = new DataSectionEntity();
        entity.ShortName = "SA";
        entity.ShortName.ShouldBe("SA");
    }

    [Fact]
    public void Should_Get_ExportOrdNum()
    {
        var entity = new DataSectionEntity();
        entity.ExportOrdNum.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ExportOrdNum()
    {
        var entity = new DataSectionEntity();
        entity.ExportOrdNum = 3;
        entity.ExportOrdNum.ShouldBe((byte?)3);
    }
}
