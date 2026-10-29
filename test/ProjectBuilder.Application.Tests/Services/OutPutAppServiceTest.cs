using NSubstitute;
using ProjectBuilder.Models;
using ProjectBuilder.Models.OutPut;
using ProjectBuilder.Models.OutPut.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class OutPutAppServiceTest
    {
        private readonly IRunScenarioRepository _runScenarioRepository;
        private readonly OutPutAppService _outPutAppService;

        public OutPutAppServiceTest()
        {
            _runScenarioRepository = Substitute.For<IRunScenarioRepository>();
            _outPutAppService = Substitute.ForPartsOf<OutPutAppService>(_runScenarioRepository);
        }

        [Fact]
        public async Task GenerateMapsResponse_Should_Return_Result_When_Success()
        {
            // Arrange
            var mapsDto = new MapsDto
            {
                SelectedScenario = 1,
                SelectedDistrict = 10,
                SeletedCounty = 20,
                SelectedRoute = 30,
                TreatementType = "Type A"
            };

            var mapExportResult = new MapsResultModel(null, "map-json-data", false);
            var finalMapResult = new MapsResultDto(null, "final-map-data", false);

            // Mock repository method returning scenario data
            _runScenarioRepository.ExportScenarioResultsToJsonAsync(
                mapsDto.SelectedScenario.Value,
                false,
                mapsDto.SelectedDistrict,
                mapsDto.SeletedCounty,
                mapsDto.SelectedRoute,
                null,
                mapsDto.TreatementType
            ).Returns(Task.FromResult(mapExportResult));

            // Mock SendMapRequest method on the substitute
            _outPutAppService.SendMapRequest(Arg.Any<string>()).Returns(Task.FromResult(finalMapResult));

            // Act
            var result = await _outPutAppService.GenerateMapsResponse(mapsDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasError);
            Assert.Equal("final-map-data", result.Result);
        }
    }
}
