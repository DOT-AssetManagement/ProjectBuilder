using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.ParameterEntity.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class ParameterEntityAppServiceTest
    {
        private readonly IParametersRepository _parametersRepository;
        private readonly IScenarioParametersRepository _scenarioParametersRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ParameterEntityAppService _service;

        public ParameterEntityAppServiceTest()
        {
            _parametersRepository = Substitute.For<IParametersRepository>();
            _scenarioParametersRepository = Substitute.For<IScenarioParametersRepository>();
            _objectMapper = Substitute.For<IObjectMapper>();
            _service = new ParameterEntityAppService(_parametersRepository, _scenarioParametersRepository, _objectMapper);
        }

        // ── ApplyPagination ──────────────────────────────────────────────────────

        [Fact]
        public async Task ApplyPagination_Should_Return_Paged_Results()
        {
            // Arrange
            var entities = Enumerable.Range(1, 5)
                .Select(i => new ParameterEntity { Id = $"P{i}", Parmfamily = "FAM", ParmName = $"Name{i}" })
                .AsQueryable();

            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 3,
                sEcho = "1"
            };

            _parametersRepository.GetQueryableAsync().Returns(Task.FromResult(entities));

            var mappedDtos = entities.Take(3)
                .Select(e => new ParameterDto { Id = e.Id, ParmName = e.ParmName })
                .ToList();
            _objectMapper
                .Map<List<ParameterEntity>, List<ParameterDto>>(Arg.Any<List<ParameterEntity>>())
                .Returns(mappedDtos);

            // Act
            var result = await _service.ApplyPagination(param);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(5, result.ITotalRecords);
            Assert.Equal(5, result.ITotalDisplayRecords);
            Assert.Equal(3, result.AaData.Count);
        }

        [Fact]
        public async Task ApplyPagination_Should_Return_Empty_When_No_Parameters()
        {
            // Arrange
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 10,
                sEcho = "2"
            };

            _parametersRepository.GetQueryableAsync()
                .Returns(Task.FromResult(Enumerable.Empty<ParameterEntity>().AsQueryable()));

            _objectMapper
                .Map<List<ParameterEntity>, List<ParameterDto>>(Arg.Any<List<ParameterEntity>>())
                .Returns(new List<ParameterDto>());

            // Act
            var result = await _service.ApplyPagination(param);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.ITotalRecords);
            Assert.Empty(result.AaData);
        }

        // ── GetAllparameters ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllparameters_Should_Return_All_Mapped_Parameters()
        {
            // Arrange
            var entities = new List<ParameterEntity>
            {
                new ParameterEntity { Id = "P1", Parmfamily = "FAM", ParmName = "Alpha" },
                new ParameterEntity { Id = "P2", Parmfamily = "FAM", ParmName = "Beta" }
            };
            var dtos = entities.Select(e => new ParameterDto { Id = e.Id, ParmName = e.ParmName }).ToList();

            _parametersRepository.GetListAsync().Returns(Task.FromResult(entities));
            _objectMapper
                .Map<List<ParameterEntity>, List<ParameterDto>>(entities)
                .Returns(dtos);

            // Act
            var result = await _service.GetAllparameters();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Alpha", result[0].ParmName);
        }

        [Fact]
        public async Task GetAllparameters_Should_Return_Empty_List_When_None_Exist()
        {
            // Arrange
            _parametersRepository.GetListAsync().Returns(Task.FromResult(new List<ParameterEntity>()));
            _objectMapper
                .Map<List<ParameterEntity>, List<ParameterDto>>(Arg.Any<List<ParameterEntity>>())
                .Returns(new List<ParameterDto>());

            // Act
            var result = await _service.GetAllparameters();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ── GetParameter ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetParameter_Should_Return_Correct_ParameterDto()
        {
            // Arrange
            var entity = new ParameterEntity
            {
                Id = "P1",
                Parmfamily = "FAM",
                ParmName = "Alpha",
                ParmDescription = "Desc",
                DefaultValue = 1.5
            };
            _parametersRepository.FindAsync("P1", "FAM").Returns(Task.FromResult(entity));

            // Act
            var result = await _service.GetParameter("P1", "FAM");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P1", result.Id);
            Assert.Equal("FAM", result.Parmfamily);
            Assert.Equal("Alpha", result.ParmName);
            Assert.Equal("Desc", result.ParmDescription);
            Assert.Equal(1.5, result.DefaultValue);
        }

        // ── EditParameter ────────────────────────────────────────────────────────

        [Fact]
        public async Task EditParameter_Should_Update_And_Return_Mapped_Dto()
        {
            // Arrange
            var updateDto = new UpdateParameterDto
            {
                Id = "P1",
                Parmfamily = "FAM",
                ParmName = "Updated",
                ParmDescription = "New Desc",
                DefaultValue = 3.0
            };
            var updatedEntity = new ParameterEntity
            {
                Id = "P1",
                Parmfamily = "FAM",
                ParmName = "Updated",
                ParmDescription = "New Desc",
                DefaultValue = 3.0
            };
            var expectedDto = new ParameterDto { Id = "P1", ParmName = "Updated" };

            _parametersRepository
                .UpdateAsync(Arg.Any<Dictionary<string, object>>(), "P1", "FAM")
                .Returns(Task.FromResult(updatedEntity));
            _objectMapper
                .Map<ParameterEntity, ParameterDto>(updatedEntity)
                .Returns(expectedDto);

            // Act
            var result = await _service.EditParameter(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P1", result.Id);
            Assert.Equal("Updated", result.ParmName);
        }

        [Fact]
        public async Task EditParameter_Should_Throw_When_Description_And_Name_Empty_But_DefaultValue_Set()
        {
            // Arrange — the guard fires when BOTH description and name are empty AND DefaultValue is non-null
            var updateDto = new UpdateParameterDto
            {
                Id = "P1",
                Parmfamily = "FAM",
                ParmDescription = "",
                ParmName = "",
                DefaultValue = 5.0
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EditParameter(updateDto));
        }

        // ── InsertParameterAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task InsertParameterAsync_Should_Insert_Each_Parameter_Into_ScenarioRepository()
        {
            // Arrange
            var models = new List<ParameterDto>
            {
                new ParameterDto { Id = "P1", ParmName = "Alpha", ParmDescription = "Desc A", DefaultValue = 1.0 },
                new ParameterDto { Id = "P2", ParmName = "Beta",  ParmDescription = "Desc B", DefaultValue = 2.0 }
            };
            var scenarioId = 42;

            _objectMapper
                .Map<Models.ScenarioParamater.Dtos.ScenarioParameterDto, ScenarioParamater>(
                    Arg.Any<Models.ScenarioParamater.Dtos.ScenarioParameterDto>())
                .Returns(ci => new ScenarioParamater
                {
                    Id = ci.Arg<Models.ScenarioParamater.Dtos.ScenarioParameterDto>().ParameterId,
                    ScenarioId = scenarioId
                });

            // Act
            await _service.InsertParameterAsync(models, scenarioId);

            // Assert — InsertAsync called once per model
            await _scenarioParametersRepository
                .Received(2)
                .InsertAsync(Arg.Any<ScenarioParamater>());
        }

        [Fact]
        public async Task InsertParameterAsync_Should_Throw_When_Description_And_Name_Empty_But_DefaultValue_Set()
        {
            // Arrange
            var models = new List<ParameterDto>
            {
                new ParameterDto { Id = "P1", ParmDescription = "", ParmName = "", DefaultValue = 9.0 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<UserFriendlyException>(() => _service.InsertParameterAsync(models, 1));
        }

        // ── DeleteParameter ──────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteParameter_Should_Return_True_When_Deletion_Succeeds()
        {
            // Arrange
            _parametersRepository.DeleteAsync("P1", "FAM").Returns(Task.FromResult(true));

            // Act
            var result = await _service.DeleteParameter("P1", "FAM");

            // Assert
            Assert.True(result);
            await _parametersRepository.Received(1).DeleteAsync("P1", "FAM");
        }

        [Fact]
        public async Task DeleteParameter_Should_Return_False_When_Deletion_Fails()
        {
            // Arrange
            _parametersRepository.DeleteAsync("P99", "FAM").Returns(Task.FromResult(false));

            // Act
            var result = await _service.DeleteParameter("P99", "FAM");

            // Assert
            Assert.False(result);
            await _parametersRepository.Received(1).DeleteAsync("P99", "FAM");
        }
    }
}
