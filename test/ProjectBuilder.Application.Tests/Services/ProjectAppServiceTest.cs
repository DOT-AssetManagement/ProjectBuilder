using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using NSubstitute;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;
using Xunit;
using Microsoft.Extensions.Primitives;
using ProjectBuilder.Models.Project.Dtos;
using System.Data;
using System.IO;

namespace ProjectBuilder.AppServices
{
    public class ProjectAppServiceTest
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IObjectMapper _ObjectMapper;
        private readonly ProjectBuilderMapper _mapper;
        private readonly ProjectAppService _projectAppService;
        public ProjectAppServiceTest()
        {
            _projectRepository = Substitute.For<IProjectRepository>();
            _treatmentRepository = Substitute.For<ITreatmentRepository>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _ObjectMapper = Substitute.For<IObjectMapper>();
            _mapper = new ProjectBuilderMapper();
            _projectAppService = new ProjectAppService(_projectRepository, _treatmentRepository, _httpContextAccessor, _ObjectMapper, _mapper);
        }

        [Fact]
        public async Task ExportProjects_Should_Export_Correct_Excel_File()
        {
            // Arrange
            var scenarioId = 101;
            var exportType = "BAMS";
            var currentDate = DateTime.Now.ToString("yyyyMMdd");

            var exportDto = new ExportProjectDto
            {
                ScenarioId = scenarioId,
                ExportType = exportType
            };

            // Mock HttpContext Response
            var response = Substitute.For<HttpResponse>();

            var responseBody = new MemoryStream(); 
            response.Body.Returns(responseBody);
            var headers = new HeaderDictionary();
            response.Headers.Returns(headers);

            var httpContext = Substitute.For<HttpContext>();
            httpContext.Response.Returns(response);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Mock repository methods returning DataTable
            var projectDataTable = new DataTable();
            projectDataTable.Columns.Add("ProjectId", typeof(int));
            projectDataTable.Columns.Add("Name", typeof(string));
            projectDataTable.Rows.Add(1, "Project A");

            var treatmentDataTable = new DataTable();
            treatmentDataTable.Columns.Add("TreatmentId", typeof(int));
            treatmentDataTable.Columns.Add("Description", typeof(string));
            treatmentDataTable.Rows.Add(1, "Treatment X");

            _projectRepository.GetProjectData("sp_pb_ExportBridgeProjects", scenarioId)
                              .Returns(Task.FromResult(projectDataTable));

            _projectRepository.GetTreatmentData("sp_pb_ExportNarrowBridgeTreatments", scenarioId)
                              .Returns(Task.FromResult(treatmentDataTable));

            // Instantiate service
            var projectAppService = new ProjectAppService(
                _projectRepository,
                _treatmentRepository,
                _httpContextAccessor,
                _ObjectMapper,
                _mapper
            );

            // Act
            await projectAppService.ExportProjects(exportDto);

            // Assert - Verify headers are set correctly
            Assert.True(headers.ContainsKey("content-disposition"));
            Assert.Contains($"attachment;filename=BAMS-{currentDate}.xlsx", headers["content-disposition"].ToString());


            await _projectRepository.Received(1).GetProjectData("sp_pb_ExportBridgeProjects", scenarioId);
            await _projectRepository.Received(1).GetTreatmentData("sp_pb_ExportNarrowBridgeTreatments", scenarioId);

            Assert.Throws<ObjectDisposedException>(() => { var _ = responseBody.Length; });
        }

        [Fact]
        public async Task DeleteProject_Should_UnassignTreatments_And_DeleteProject_When_ValidIds_AreProvided()
        {
            // Arrange
            int projectId = 1;
            int scenarioId = 101;

            // Mock treatment unassignment to return true (successful unassignment)
            _treatmentRepository.UnAssignTreatmentsFromProject(projectId)
                .Returns(Task.FromResult(true));

            // Mock successful project deletion
            _projectRepository.DeleteProject(projectId, scenarioId)
                .Returns(Task.FromResult(true)); 

            var projectAppService = new ProjectAppService(
                _projectRepository,
                _treatmentRepository,
                _httpContextAccessor,
                _ObjectMapper,
                _mapper
            );

            // Act
            var result = await projectAppService.DeleteProject(projectId, scenarioId);

            // Assert
            Assert.True(result);

            await _treatmentRepository.Received(1).UnAssignTreatmentsFromProject(projectId);
            await _projectRepository.Received(1).DeleteProject(projectId, scenarioId);
        }

        [Fact]
        public async Task DeleteProject_Should_ThrowException_When_Project_Not_Deleted()
        {
            // Arrange
            int projectId = 1;
            int scenarioId = 101;

            // Mock successful treatment unassignment
            _treatmentRepository.UnAssignTreatmentsFromProject(projectId)
                .Returns(Task.FromResult(true)); 

            // Mock failed project deletion
            _projectRepository.DeleteProject(projectId, scenarioId)
                .Returns(Task.FromResult(false));

            var projectAppService = new ProjectAppService(
                _projectRepository,
                _treatmentRepository,
                _httpContextAccessor,
                _ObjectMapper,
                _mapper
            );

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => projectAppService.DeleteProject(projectId, scenarioId));

            Assert.Equal("Could not delete the selected Project because it was not found, make sure that you have selected a scenario and a project then try again.", exception.Message);

            // Verify methods called with correct IDs
            await _treatmentRepository.Received(1).UnAssignTreatmentsFromProject(projectId);
            await _projectRepository.Received(1).DeleteProject(projectId, scenarioId);
        }



    }
}
