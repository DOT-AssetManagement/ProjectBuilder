using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class InfoAppServiceTest
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private IProjectRepository _projectRepository;
        private readonly IProjectTreatmentRepository _projectTreatmentsRepo;
        private readonly IObjectMapper _objectMapper;

        private readonly InfoAppService _infoAppService;

        public InfoAppServiceTest()
        {
            _treatmentRepository = Substitute.For<ITreatmentRepository>();
            _projectRepository = Substitute.For<IProjectRepository>();
            _projectTreatmentsRepo = Substitute.For<IProjectTreatmentRepository>();
            _objectMapper = Substitute.For<IObjectMapper>();
            _infoAppService = new InfoAppService(_treatmentRepository, _projectRepository, _projectTreatmentsRepo, _objectMapper);
        }

        [Fact]
        public async Task GetDistricts_Should_Return_Districts_From_Treatments_When_Default_Caller()
        {
            // Arrange
            var treatments = new List<UserTreatment>
            {
                new UserTreatment { District = 1 },
                new UserTreatment { District = 2 },
                new UserTreatment { District = 1 }, 
                new UserTreatment { District = null } 
            };

            _treatmentRepository.GetListAsync().Returns(Task.FromResult(treatments));

            // Act
            var result = await _infoAppService.GetDistricts("AnyOtherCaller", null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); 
            Assert.Contains((byte?)1, result);
            Assert.Contains((byte?)2, result);
            Assert.Contains((byte?)null, result);
        }

        [Fact]
        public async Task GetDistricts_Should_Return_Districts_From_ProjectRepository_When_Caller_Is_Projects()
        {
            // Arrange
            int? scenarioId = 123;
            var expectedDistricts = new List<byte?> { 3, 4 };

            _projectRepository.GetDistricts(scenarioId).Returns(Task.FromResult(expectedDistricts));

            // Act
            var result = await _infoAppService.GetDistricts("/Projects", scenarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains((byte?)3, result);
            Assert.Contains((byte?)4, result);
        }

        [Fact]
        public async Task GetDistricts_Should_Return_Districts_From_ProjectTreatmentsRepo_When_Caller_Is_ProjectTreatments()
        {
            // Arrange
            int? scenarioId = 456;
            var expectedDistricts = new List<byte?> { 5, 6 };

            _projectTreatmentsRepo.GetDistricts(scenarioId).Returns(Task.FromResult(expectedDistricts));

            // Act
            var result = await _infoAppService.GetDistricts("/ProjectTreatments", scenarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains((byte?)5, result);
            Assert.Contains((byte?)6, result);
        }

        [Fact]
        public async Task GetCounties_Should_Return_Counties_From_TreatmentRepository_When_Default_Caller()
        {
            // Arrange
            int? district = 1;

            var counties = new List<CountyEntity>

            {
                new CountyEntity { Id = 1, CountyName = "County A" },
                new CountyEntity { Id = 2, CountyName = "County B" }
            };

            _treatmentRepository.GetCounties(district).Returns(Task.FromResult(counties));

            // Act
            var result = await _infoAppService.GetCounties("AnyOtherCaller", null, district);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("County A", result[0].CountyName);
            Assert.Equal("County B", result[1].CountyName);
        }

        [Fact]
        public async Task GetCounties_Should_Return_Counties_From_ProjectRepository_When_Projects_Caller()
        {
            // Arrange
            int? scenarioId = 100;
            int? district = 2;

            var counties = new List<CountyEntity>
            {
                new CountyEntity { Id = 3, CountyName = "County X" },
                new CountyEntity { Id = 4, CountyName = "County Y" }
            };

            _projectRepository.GetCounties(scenarioId, district).Returns(Task.FromResult(counties));

            // Act
            var result = await _infoAppService.GetCounties("/Projects", scenarioId, district);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("County X", result[0].CountyName);
            Assert.Equal("County Y", result[1].CountyName);
        }

        [Fact]
        public async Task GetCounties_Should_Return_Counties_From_ProjectTreatmentRepository_When_ProjectTreatments_Caller()
        {
            // Arrange
            int? scenarioId = 200;
            int? district = 3;

            var counties = new List<CountyEntity>
            {
                new CountyEntity { Id = 5, CountyName = "County M" },
                new CountyEntity { Id = 6, CountyName = "County N" }
            };

            _projectTreatmentsRepo.GetCounties(scenarioId, district).Returns(Task.FromResult(counties));

            // Act
            var result = await _infoAppService.GetCounties("/ProjectTreatments", scenarioId, district);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("County M", result[0].CountyName);
            Assert.Equal("County N", result[1].CountyName);
        }

        [Fact]
        public async Task GetRoutes_Should_Return_Routes_From_TreatmentRepository_When_Default_Caller()
        {
            // Arrange
            int? district = 1;
            int? county = 10;

            var routes = new List<int?> { 100, 200, 300 };

            _treatmentRepository.GetRoutes(district, county).Returns(Task.FromResult(routes));

            // Act
            var result = await _infoAppService.GetRoutes("AnyOtherCaller", null, district, county);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(100, result);
            Assert.Contains(200, result);
            Assert.Contains(300, result);
        }

        [Fact]
        public async Task GetRoutes_Should_Return_Routes_From_ProjectRepository_When_Projects_Caller()
        {
            // Arrange
            int? scenarioId = 101;
            int? district = 2;
            int? county = 20;

            var routes = new List<int?> { 400, 500 };

            _projectRepository.GetRoutes(scenarioId, district, county).Returns(Task.FromResult(routes));

            // Act
            var result = await _infoAppService.GetRoutes("/Projects", scenarioId, district, county);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(400, result);
            Assert.Contains(500, result);
        }

        [Fact]
        public async Task GetRoutes_Should_Return_Routes_From_ProjectTreatmentsRepository_When_ProjectTreatments_Caller()
        {
            // Arrange
            int? scenarioId = 202;
            int? district = 3;
            int? county = 30;

            var routes = new List<int?> { 600, 700, 800 };

            _projectTreatmentsRepo.GetRoutes(scenarioId, district, county).Returns(Task.FromResult(routes));

            // Act
            var result = await _infoAppService.GetRoutes("/ProjectTreatments", scenarioId, district, county);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(600, result);
            Assert.Contains(700, result);
            Assert.Contains(800, result);
        }

        [Fact]
        public async Task GetSections_Should_Return_Sections_From_TreatmentRepository_When_Default_Caller()
        {
            // Arrange
            int? district = 1;
            int? county = 10;
            int? route = 100;

            var sections = new List<string> { "Section A", "Section B" };

            _treatmentRepository.GetSections(district, county, route).Returns(Task.FromResult(sections));

            // Act
            var result = await _infoAppService.GetSections("AnyOtherCaller", null, district, county, route);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("Section A", result);
            Assert.Contains("Section B", result);
        }

        [Fact]
        public async Task GetSections_Should_Return_Sections_From_ProjectRepository_When_Projects_Caller()
        {
            // Arrange
            int? scenarioId = 101;
            int? district = 2;
            int? county = 20;
            int? route = 200;

            var sections = new List<string> { "Section X", "Section Y" };

            _projectRepository.GetSections(scenarioId, district, county, route).Returns(Task.FromResult(sections));

            // Act
            var result = await _infoAppService.GetSections("/Projects", scenarioId, district, county, route);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("Section X", result);
            Assert.Contains("Section Y", result);
        }

        [Fact]
        public async Task GetSections_Should_Return_Sections_From_ProjectTreatmentsRepository_When_ProjectTreatments_Caller()
        {
            // Arrange
            int? scenarioId = 202;
            int? district = 3;
            int? county = 30;
            int? route = 300;

            var sections = new List<string> { "Section M", "Section N", "Section O" };

            _projectTreatmentsRepo.GetSections(scenarioId, district, county, route).Returns(Task.FromResult(sections));

            // Act
            var result = await _infoAppService.GetSections("/ProjectTreatments", scenarioId, district, county, route);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains("Section M", result);
            Assert.Contains("Section N", result);
            Assert.Contains("Section O", result);
        }

        [Fact]
        public async Task GetDirectionInterstate_Should_Return_Interstate_When_Section_Has_Two_Parts()
        {
            // Arrange
            string section = "100-200";
            int? route = 1;
            int? county = 10;
            int? district = 5;

            var expectedResult = new SegmentationModel { Direction = true, isInterstate = true };

            _treatmentRepository
                .GetDirectionInterstate(district, county, route, 100, 200)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _infoAppService.GetDirectionInterstate(section, route, county, district);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(true, result.Direction);
            Assert.Equal(true, result.isInterstate);
        }

        [Fact]
        public async Task GetDirectionInterstate_Should_Return_Interstate_When_Section_Has_One_Part()
        {
            // Arrange
            string section = "150";
            int? route = 1;
            int? county = 10;
            int? district = 5;

            var expectedResult = new SegmentationModel { Direction = false, isInterstate = false };

            _treatmentRepository
                .GetDirectionInterstate(district, county, route, 150, 150)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _infoAppService.GetDirectionInterstate(section, route, county, district);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(false, result.Direction);
            Assert.Equal(false, result.isInterstate);
        }

        [Fact]
        public async Task GetDefaultSlackValues_Should_Return_DefaultSlackDto_When_AssetType_Is_Valid()
        {
            // Arrange
            string assetType = "Bridge";
            var defaultSlack = new DefaultSlack { Id = "1", MoveBefore = 1  };
            var expectedDto = new DefaultSlackDto { Id = "1", MoveBefore = 1 };

            _treatmentRepository.GetAssetTypeDefaultSlack(assetType).Returns(Task.FromResult(defaultSlack));
            _objectMapper.Map<DefaultSlack, DefaultSlackDto>(defaultSlack).Returns(expectedDto);

            // Act
            var result = await _infoAppService.GetDefaultSlackValues(assetType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.MoveBefore, result.MoveBefore);
        }

    }
}

