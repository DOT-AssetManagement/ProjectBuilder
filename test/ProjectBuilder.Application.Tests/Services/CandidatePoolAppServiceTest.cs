//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using NSubstitute;
//using Xunit;
//using AutoMapper;
//using ProjectBuilder.Models.CandidatePool;
//using ProjectBuilder.Models.CandidatePool.Dtos;
//using ProjectBuilder.Repositories;
//using ProjectBuilder.Models.Scenario.Dtos;
//using ProjectBuilder.Entities;
//using ProjectBuilder.Services;
//using Volo.Abp.Identity;
//using Volo.Abp.Users;
//using System.Text;
//using ProjectBuilder.Helpers;
//using System.IO;
//using Volo.Abp.ObjectMapping;

//public class CandidatePoolAppServiceTests
//{
//    private readonly ICandidatePoolRepository _candidateRepository;
//    private readonly IRunScenarioRepository _scenarioRepository;
//    private readonly ITreatmentRepository _treatmentRepository;
//    private readonly IHttpContextAccessor _httpContextAccessor;
//    private readonly IObjectMapper _mapper;
//    private readonly CandidatePoolAppService _candidatePoolAppService;

//    public CandidatePoolAppServiceTests()
//    {
//        _candidateRepository = Substitute.For<ICandidatePoolRepository>();
//        _scenarioRepository = Substitute.For<IRunScenarioRepository>();
//        _treatmentRepository = Substitute.For<ITreatmentRepository>();
//        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
//        _mapper = Substitute.For<IObjectMapper>();
//        _candidatePoolAppService = new CandidatePoolAppService(_candidateRepository, _scenarioRepository, _treatmentRepository, _httpContextAccessor, _mapper);
//    }


//        [Fact]
//        public async Task GetCandidatePoolDataAsync_FiltersByScenarioId()
//        {
//            // Arrange
//            var candidatePools = new List<CandidatePool>
//            {
//                new CandidatePool { Id = new Guid("1EFA39C4-7ACA-40C1-8EAF-871BA5D3726A"), Name = "Test Pool" }
//            };
//            var candidatePoolDtos = new List<CandidatePoolDto>
//            {
//                new CandidatePoolDto { Id = new Guid("1EFA39C4-7ACA-40C1-8EAF-871BA5D3726A"), Name = "Test Pool" }
//            };
//            var sources = new Dictionary<Guid, string>
//            {
//                { new Guid("1EFA39C4-7ACA-40C1-8EAF-871BA5D3726A"), "Source Data" } 
//            };

//            _candidateRepository.GetPoolsListAsync().Returns(Task.FromResult(candidatePools));
//            _mapper.Map<List<CandidatePool>, List<CandidatePoolDto>>(candidatePools).Returns(candidatePoolDtos);
//            _candidateRepository.GetSource(Arg.Any<List<Guid>>()).Returns(Task.FromResult(sources));

//            // Act
//            var result = await _candidatePoolAppService.GetCandidatePoolDataAsync(null, null, "Test Pool"); 

//            // Assert
//            Assert.NotNull(result);
//            Assert.NotEmpty(result.Libraries); 
//            Assert.Single(result.Libraries);
//            Assert.Equal("Test Pool", result.Libraries.First().Name);
//            Assert.Equal("Source Data", result.Libraries.First().Source);
//        }


//        [Fact]
//        public async Task GetCandidatePoolDataAsync_ReturnsEmptyWhenNotAuthenticated()
//        {
//            // Arrange
//            var scenid = (int?)null;
//            var importing = (bool?)null;
//            var searchString = "SomeSearch";

//            var mockHttpContext = Substitute.For<HttpContext>();
//            var mockUser = Substitute.For<ClaimsPrincipal>();
//            mockUser.Identity.IsAuthenticated.Returns(false); 
//            mockHttpContext.User.Returns(mockUser);
//            _httpContextAccessor.HttpContext.Returns(mockHttpContext);

//            // Act
//            var result = await _candidatePoolAppService.GetCandidatePoolDataAsync(scenid, importing, searchString);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Empty(result.Libraries);
//        }

//        [Fact]
//        public async Task GetSource_PopulatesSourc()
//        {
//            // Arrange
//            var candidatePool = new CandidatePoolDto
//            {
//                Libraries = new List<CandidatePoolDto>
//                {
//                    new CandidatePoolDto { Id = Guid.Parse("67d4d427-753c-45ce-b954-5a023aeb76cf") },
//                }
//            };

//            var sources = new Dictionary<Guid, string>
//                {
//                    { Guid.Parse("67d4d427-753c-45ce-b954-5a023aeb76cf"), "Source1" }
//                };

//            _candidateRepository.GetSource(Arg.Is<List<Guid>>(ids => ids.SequenceEqual(new List<Guid> { Guid.Parse("67d4d427-753c-45ce-b954-5a023aeb76cf") })))
//                .Returns(Task.FromResult(sources));


//            // Act
//            await _candidatePoolAppService.GetSource(candidatePool);

//            // Assert
//            Assert.Equal("Source1", candidatePool.Libraries.First().Source);
//        }

//        [Fact]
//        public async Task ImportingLibrary_UpdatesCandidatePoolViewModel_WhenScenarioExists()
//        {
//            // Arrange
//            var scenid = 1;
//            var isImporting = (bool?)true;
//            var scenario = new Scenario { LibraryId = Guid.NewGuid() };
//            var candidatePoolViewModel = new CandidatePoolDto
//            {
//                Libraries = new List<CandidatePoolDto>
//            {
//               new CandidatePoolDto { Id = scenario.LibraryId ?? Guid.NewGuid(), Name = "Test Library" }

//            }
//            };

//            _scenarioRepository.FindAsync(scenid).Returns(Task.FromResult(scenario));

//            // Act
//            await _candidatePoolAppService.ImportingLibrary(scenid, isImporting, candidatePoolViewModel);

//            // Assert
//            Assert.Equal(scenario.LibraryId, candidatePoolViewModel.Id);
//            Assert.Equal("Test Library", candidatePoolViewModel.Name);
//            Assert.True(candidatePoolViewModel.IsImporting);
//            Assert.False(candidatePoolViewModel.FromScenario);
//        }

//        [Fact]
//        public async Task ImportingLibrary_DoesNotUpdateCandidatePoolViewModel_WhenScenarioDoesNotExist()
//        {
//            // Arrange
//            var scenid = 1;
//            var isImporting = (bool?)true;
//            var candidatePoolViewModel = new CandidatePoolDto
//            {
//                Id = Guid.NewGuid(),
//                Name = "Original Name",
//                Libraries = new List<CandidatePoolDto>()
//            };

//            _scenarioRepository.FindAsync(scenid).Returns(Task.FromResult<Scenario>(null));

//            // Act
//            await _candidatePoolAppService.ImportingLibrary(scenid, isImporting, candidatePoolViewModel);

//            // Assert
//            Assert.Equal("Original Name", candidatePoolViewModel.Name);
//            Assert.True(candidatePoolViewModel.IsImporting);
//            Assert.False(candidatePoolViewModel.FromScenario);
//        }

//        [Fact]
//        public async Task CreateNewLibrary_CreatesLibraryAndPopulates_WhenValidDataProvided()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var libraryViewModel = new CreateCandidatePoolDto
//            {
//                Name = "Test Library",
//                Description = "Test Description",
//                IsShared = true,
//                IsEmptyLibrary = false,
//                SelectedAsset = "Brdige",
//                MinYear = 2000,
//                MaxYear = 2023
//            };

//            var expectedLibraryId = Guid.NewGuid();
//            _candidateRepository.CreateNewCandidatePool(Arg.Any<CandidatePool>())
//                                .Returns(Task.FromResult(expectedLibraryId));

//            // Act
//            var result = await _candidatePoolAppService.CreateNewLibrary(libraryViewModel, userId, "TestSource");

//            // Assert
//            Assert.NotEqual(Guid.Empty, result);
//            Assert.Equal(expectedLibraryId, result);

//        }

//        [Fact]
//        public async Task CheckLibraryNameAsync_ValidatesLibraryNameCorrectly()
//        {
//            // Arrange
//            var existingLibraries = new List<CandidatePool>
//        {
//            new CandidatePool { Id = Guid.NewGuid(), Name = "Existing Library" },
//            new CandidatePool { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Conflicting Library" }
//        };

//            _candidateRepository.GetListAsync().Returns(Task.FromResult(existingLibraries));

//            // Act & Assert

//            // No duplicate, valid new library
//            var result1 = await _candidatePoolAppService.CheckLibraryNameAsync("New Library", null);
//            Assert.True(result1);
     
//        }
//        [Fact]
//        public async Task ImportTreatmentsAsync_ReturnsNullOrEmpty_WhenImportIsSuccessful()
//        {
//            // Arrange
//            var data = new ImportTreatmentViewModel
//            {
//                ExcelFile = Substitute.For<IFormFile>(),
//                ImportType = "P",
//                TabName = "Sheet1",
//                LibraryId = Guid.NewGuid(),
//                fromScratch = true,
//                keepAll = false
//            };

//            var filePath = Path.GetTempFileName(); 
//            data.ExcelFile.SaveUploadedFile().Returns(Task.FromResult(filePath));

//            _treatmentRepository.ImportTreatmentsFromExcelFile("PAMS", data.ImportType, filePath, data.TabName, data.LibraryId, data.fromScratch, data.keepAll)
//                .Returns(Task.FromResult<string?>(string.Empty)); 

//            // Act
//            var result = await _candidatePoolAppService.ImportTreatmentsAsync(data);

//            // Assert
//            Assert.True(string.IsNullOrEmpty(result));

//            // Ensure DeleteExcelFile is called
//            _treatmentRepository.Received(1).DeleteExcelFile(Arg.Any<string>());
//        }

//        [Fact]
//        public async Task ImportMASAndB2PAsync_ReturnsNullOrEmpty_WhenImportIsSuccessful()
//        {
//            // Arrange
//            var data = new ImportMASAndB2PViewModel
//            {
//                ExcelFile = Substitute.For<IFormFile>(),
//                IsMAS = true,
//                TabName = "Sheet1"
//            };

//            var filePath = Path.GetTempFileName();  
//            data.ExcelFile.SaveUploadedFile().Returns(Task.FromResult(filePath));

//            _treatmentRepository.ImportMASAndB2PFromExcelFile(data.IsMAS, filePath, data.TabName)
//                .Returns(Task.FromResult<string?>(string.Empty)); 

//            // Act
//            var result = await _candidatePoolAppService.ImportMASAndB2PAsync(data);

//            // Assert
//            Assert.True(string.IsNullOrEmpty(result));

//            // Ensure DeleteExcelFile is called
//            _treatmentRepository.Received(1).DeleteExcelFile(Arg.Any<string>());
//        }

//}

