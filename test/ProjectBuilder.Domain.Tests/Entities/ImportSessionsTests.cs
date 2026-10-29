using System;
using ProjectBuilder.Entities;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Entities;

public class ImportSessionsTests
{
    [Fact]
    public void Should_Get_NoId()
    {
        var entity = new ImportSessions();
        entity.NoId.ShouldBe(0);
    }

    [Fact]
    public void Should_Set_NoId()
    {
        var entity = new ImportSessions();
        entity.NoId = 10;
        entity.NoId.ShouldBe(10);
    }

    [Fact]
    public void Should_Get_Id()
    {
        var entity = new ImportSessions();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_Id()
    {
        var entity = new ImportSessions();
        var value = Guid.NewGuid();
        entity.Id = value;
        entity.Id.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_ImportSource()
    {
        var entity = new ImportSessions();
        entity.ImportSource.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_ImportSource()
    {
        var entity = new ImportSessions();
        entity.ImportSource = "ARA";
        entity.ImportSource.ShouldBe("ARA");
    }

    [Fact]
    public void Should_Get_DataSourceType()
    {
        var entity = new ImportSessions();
        entity.DataSourceType.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DataSourceType()
    {
        var entity = new ImportSessions();
        entity.DataSourceType = "CSV";
        entity.DataSourceType.ShouldBe("CSV");
    }

    [Fact]
    public void Should_Get_DataSourceName()
    {
        var entity = new ImportSessions();
        entity.DataSourceName.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_DataSourceName()
    {
        var entity = new ImportSessions();
        entity.DataSourceName = "Import_2024";
        entity.DataSourceName.ShouldBe("Import_2024");
    }

    [Fact]
    public void Should_Get_CreatedBy()
    {
        var entity = new ImportSessions();
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CreatedBy()
    {
        var entity = new ImportSessions();
        entity.CreatedBy = "Admin";
        entity.CreatedBy.ShouldBe("Admin");
    }

    [Fact]
    public void Should_Get_CreatedAt()
    {
        var entity = new ImportSessions();
        entity.CreatedAt.ShouldBe(default(DateTime));
    }

    [Fact]
    public void Should_Set_CreatedAt()
    {
        var entity = new ImportSessions();
        var value = new DateTime(2024, 1, 15);
        entity.CreatedAt = value;
        entity.CreatedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CompletedAt()
    {
        var entity = new ImportSessions();
        entity.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CompletedAt()
    {
        var entity = new ImportSessions();
        DateTime? value = new DateTime(2024, 1, 16);
        entity.CompletedAt = value;
        entity.CompletedAt.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_CompletedStatus()
    {
        var entity = new ImportSessions();
        entity.CompletedStatus.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_CompletedStatus()
    {
        var entity = new ImportSessions();
        entity.CompletedStatus = 1;
        entity.CompletedStatus.ShouldBe(1);
    }

    [Fact]
    public void Should_Get_Notes()
    {
        var entity = new ImportSessions();
        entity.Notes.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Notes()
    {
        var entity = new ImportSessions();
        entity.Notes = "Some notes";
        entity.Notes.ShouldBe("Some notes");
    }

    [Fact]
    public void Should_Get_LibraryId()
    {
        var entity = new ImportSessions();
        entity.LibraryId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Should_Set_LibraryId()
    {
        var entity = new ImportSessions();
        var value = Guid.NewGuid();
        entity.LibraryId = value;
        entity.LibraryId.ShouldBe(value);
    }

    [Fact]
    public void Should_Get_Keys()
    {
        var entity = new ImportSessions();
        var id = Guid.NewGuid();
        entity.Id = id;
        var keys = entity.GetKeys();
        keys[0].ShouldBe(id);
    }
}
