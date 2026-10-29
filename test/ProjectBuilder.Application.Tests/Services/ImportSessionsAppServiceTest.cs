using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.ImportSessions.Dtos;
using ProjectBuilder.Models.ImportSessions;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class ImportSessionsAppServiceTest : AppServiceTestBase
    {
        private readonly ImportSessionsAppService _service;
        private readonly IImportSessionRepository _importSessionRepository;

        public ImportSessionsAppServiceTest()
        {
            _service = new ImportSessionsAppService(ImportSessionsRepository, _importSessionRepository, ObjectMapper);
        }

        [Fact]
        public void GetPagedImportSessions()
        {
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 2,
                sEcho = "1"
            };

            var importSessionsList = new List<ImportSessions>
            {
                new ImportSessions { Id = Guid.NewGuid(), ImportSource = "Session 1" },
                new ImportSessions { Id = Guid.NewGuid(), ImportSource = "Session 2" },
                new ImportSessions { Id = Guid.NewGuid(), ImportSource = "Session 3" }
            };

            ImportSessionsRepository.GetQueryable().Returns(importSessionsList.AsQueryable());

            ObjectMapper
                .Map<List<ImportSessions>, List<ImportSessionDto>>(Arg.Any<List<ImportSessions>>())
                .Returns(call =>
                {
                    var input = call.Arg<List<ImportSessions>>();
                    return input.Select(x => new ImportSessionDto
                    {
                        ImportSource = x.ImportSource
                    }).ToList();
                });

            var result = _service.GetPagedImportSessions(param);

            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(3, result.ITotalRecords);
            Assert.Equal(3, result.ITotalDisplayRecords);
            Assert.Equal(2, result.AaData.Count);
            Assert.Equal("Session 1", result.AaData[0].ImportSource);
            Assert.Equal("Session 2", result.AaData[1].ImportSource);
        }

        [Fact]
        public void GetPagedImportSessions_Empty()
        {
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 2,
                sEcho = "1"
            };

            ImportSessionsRepository.GetQueryable().Returns(new List<ImportSessions>().AsQueryable());

            ObjectMapper
                .Map<List<ImportSessions>, List<ImportSessionDto>>(Arg.Any<List<ImportSessions>>())
                .Returns(new List<ImportSessionDto>());

            var result = _service.GetPagedImportSessions(param);

            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(0, result.ITotalRecords);
            Assert.Equal(0, result.ITotalDisplayRecords);
            Assert.Empty(result.AaData);
        }

        [Fact]
        public void GetPagedImportSessions_Pagination()
        {
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 1,
                iDisplayLength = 1,
                sEcho = "1"
            };

            var importSessionsList = new List<ImportSessions>
            {
                new ImportSessions { Id = Guid.NewGuid(), ImportSource = "Session 1" },
                new ImportSessions { Id = Guid.NewGuid(), ImportSource = "Session 2" },
                new ImportSessions { Id = Guid.NewGuid(), ImportSource = "Session 3" }
            };

            ImportSessionsRepository.GetQueryable().Returns(importSessionsList.AsQueryable());

            ObjectMapper
                .Map<List<ImportSessions>, List<ImportSessionDto>>(Arg.Any<List<ImportSessions>>())
                .Returns(call =>
                {
                    var input = call.Arg<List<ImportSessions>>();
                    return input.Select(x => new ImportSessionDto
                    {
                        ImportSource = x.ImportSource
                    }).ToList();
                });

            var result = _service.GetPagedImportSessions(param);

            Assert.NotNull(result);
            Assert.Equal(3, result.ITotalRecords);
            Assert.Single(result.AaData);
            Assert.Equal("Session 2", result.AaData[0].ImportSource);
        }

        [Fact]
        public void GetPagedImportSessions_Echo()
        {
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 1,
                sEcho = "999"
            };

            var importSessionsList = new List<ImportSessions>
            {
                new ImportSessions { Id = Guid.NewGuid(), ImportSource = "Session A" }
            };

            ImportSessionsRepository.GetQueryable().Returns(importSessionsList.AsQueryable());

            ObjectMapper
                .Map<List<ImportSessions>, List<ImportSessionDto>>(Arg.Any<List<ImportSessions>>())
                .Returns(new List<ImportSessionDto>
                {
                    new ImportSessionDto { ImportSource = "Session A" }
                });

            var result = _service.GetPagedImportSessions(param);

            Assert.NotNull(result);
            Assert.Equal("999", result.SEcho);
            Assert.Single(result.AaData);
            Assert.Equal("Session A", result.AaData[0].ImportSource);
        }
    }
}