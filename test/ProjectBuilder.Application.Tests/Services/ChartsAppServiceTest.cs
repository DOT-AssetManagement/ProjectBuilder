//using AutoMapper;
//using Microsoft.AspNetCore.Http;
//using NSubstitute;
//using ProjectBuilder.Entities;
//using ProjectBuilder.Models.VisualizationModels;
//using ProjectBuilder.Repositories;
//using ProjectBuilder.Services;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Metrics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Volo.Abp;
//using Xunit;

//namespace ProjectBuilder.AppServices
//{
//    public class ChartsAppServiceTest
//    {
//        private readonly IRunScenarioRepository _runScenarioRepository;
//        private readonly ICountyRepository _countyRepository;
//        private readonly IChartsNeedsRepository _chartsNeedsRepository;
//        private readonly IChartsPotentialBenefitsUnitOfWork _chartsPotentialBenefitsUnitOfWork;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly IMapper _mapper;

//        private readonly ChartsAppService _chartsAppService;

//        public ChartsAppServiceTest()
//        {
//            _runScenarioRepository = Substitute.For<IRunScenarioRepository>();
//            _countyRepository = Substitute.For<ICountyRepository>();
//            _chartsNeedsRepository = Substitute.For<IChartsNeedsRepository>();
//            _chartsPotentialBenefitsUnitOfWork = Substitute.For<IChartsPotentialBenefitsUnitOfWork>();
//            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
//            _mapper = Substitute.For<IMapper>();
//            _chartsAppService = new ChartsAppService (_runScenarioRepository, _countyRepository, _chartsNeedsRepository, _chartsPotentialBenefitsUnitOfWork, _httpContextAccessor, _mapper);
//        }

//        [Fact]
//        public async Task LoadChartViewModel_Should_Return_ChartDto_With_Scenario_And_Districts_When_ScenarioId_Provided()
//        {
//            // Arrange
//            int scenarioId = 1;
//            var scenario = new Scenario { Id = scenarioId, ScenarioName = "Test Scenario" };
//            var counties = new List<CountyEntity>
//            {
//                new CountyEntity { District =  1 },
//                new CountyEntity { District =  2 },
//            };

//            _runScenarioRepository.FindAsync(scenarioId).Returns(Task.FromResult(scenario));
//            _countyRepository.GetListAsync().Returns(Task.FromResult(counties));

//            // Act
//            var result = await _chartsAppService.LoadChartViewModel(scenarioId);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(scenarioId, result.ScenarioId);
//            Assert.Equal("Test Scenario", result.ScenarioName);
//            Assert.Equal(2, result.Districts.Count());
//            Assert.Contains(1, result.Districts);
//            Assert.Contains(2, result.Districts);

//            await _runScenarioRepository.Received(1).FindAsync(scenarioId);
//            await _countyRepository.Received(1).GetListAsync();
//        }

//        [Fact]
//        public async Task LoadChartViewModel_Should_Return_Empty_ScenarioName_When_Scenario_Not_Found()
//        {
//            // Arrange
//            int scenarioId = 99;
//            var counties = new List<CountyEntity>
//            {
//                new CountyEntity { District =  1 },
//                new CountyEntity { District =  2 },
//            };

//            _runScenarioRepository.FindAsync(scenarioId).Returns(Task.FromResult<Scenario>(null));
//            _countyRepository.GetListAsync().Returns(Task.FromResult(counties));

//            // Act
//            var result = await _chartsAppService.LoadChartViewModel(scenarioId);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(scenarioId, result.ScenarioId);
//            Assert.Equal(string.Empty, result.ScenarioName);
//            Assert.Equal(2, result.Districts.Count());
//            Assert.Contains(1, result.Districts);
//            Assert.Contains(2, result.Districts);

//            await _runScenarioRepository.Received(1).FindAsync(scenarioId);
//            await _countyRepository.Received(1).GetListAsync();
//        }

//        [Fact]
//        public async Task LoadChartViewModel_Should_Return_Districts_Only_When_ScenarioId_Not_Provided()
//        {
//            // Arrange
//            var counties = new List<CountyEntity>
//            {
//                  new CountyEntity { District =  1 },
//                  new CountyEntity { District =  2 },
//            };

//            _countyRepository.GetListAsync().Returns(Task.FromResult(counties));

//            // Act
//            var result = await _chartsAppService.LoadChartViewModel(null);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Null(result.ScenarioId);
//            Assert.Null(result.ScenarioName); 
//            Assert.Equal(2, result.Districts.Count());
//            Assert.Contains(1, result.Districts);
//            Assert.Contains(2, result.Districts);

//            await _runScenarioRepository.DidNotReceive().FindAsync(Arg.Any<int>());
//            await _countyRepository.Received(1).GetListAsync();
//        }

//        [Fact]
//        public async Task LoadNeedCharts_Should_Throw_Exception_When_No_Scenario_And_No_District()
//        {
//            // Act & Assert
//            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
//                await _chartsAppService.LoadNeedCharts(null, null, null)
//            );

//            Assert.Equal("You need to select a scenario and a district first", exception.Message);
//        }

//        [Fact]
//        public async Task LoadPotentialBenefitsCharts_Should_Throw_Exception_When_No_Scenario_And_No_District()
//        {
//            // Act & Assert
//            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
//                await _chartsAppService.LoadPotentialBenefitsCharts(null, null, null)
//            );

//            Assert.Equal("You need to select a scenario and a district first", exception.Message);
//        }

//    }
//}
