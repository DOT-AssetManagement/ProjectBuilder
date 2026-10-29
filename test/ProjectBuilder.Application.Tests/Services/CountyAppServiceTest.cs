using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class CountyAppServiceTest
    {
        private readonly ICountyRepository _countyRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;

        private readonly CountyAppService _countyAppService;
        public CountyAppServiceTest()
        {
            _countyRepository = Substitute.For<ICountyRepository>();
            _treatmentRepository = Substitute.For<ITreatmentRepository>();
            _objectMapper = Substitute.For<IObjectMapper>();
            _mapper = new ProjectBuilderMapper();
            _countyAppService = new CountyAppService (_countyRepository, _treatmentRepository, _objectMapper, _mapper);
        }

        [Fact]
        public async Task ApplyPagination_Should_Return_Paginated_And_Filtered_Results()
        {
            // Arrange
            var counties = new List<CountyEntity>
            {
                new CountyEntity { Id = 1, CountyName = "County1", District = 1},
                new CountyEntity { Id = 2, CountyName = "County2", District = 2},
                new CountyEntity { Id = 3, CountyName = "County3", District = 3}
            };

            var treatments = new List<UserTreatment>
            {
                new UserTreatment { CountyId = 1 },
                new UserTreatment { CountyId = 2 }
            };

            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 2,
                sSearch = "County1", 
                sEcho = "1"
            };

            // Mock repositories
            _countyRepository.GetQueryableAsync().Returns(Task.FromResult(counties.AsQueryable()));
            _treatmentRepository.GetQueryableAsync().Returns(Task.FromResult(treatments.AsQueryable()));

            // Act
            var result = await _countyAppService.ApplyPagination(param);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(1, result.ITotalRecords); 
            Assert.Equal(1, result.ITotalDisplayRecords); 
            Assert.Single(result.AaData); 
            Assert.Equal("County1", result.AaData[0].CountyName);

            // Verifying that the repository methods were called
            await _countyRepository.Received(1).GetQueryableAsync();
            await _treatmentRepository.Received(1).GetQueryableAsync();
        }

        [Fact]
        public async Task ApplyPagination_Should_Return_Empty_When_No_Match()
        {
            var counties = new List<CountyEntity>
            {
                new CountyEntity { Id = 1, CountyName = "County1", District = 1},
                new CountyEntity { Id = 2, CountyName = "County2", District = 2},
                new CountyEntity { Id = 3, CountyName = "County3", District = 3}
            };

            var treatments = new List<UserTreatment>
            {
                new UserTreatment { CountyId = 1 },
                new UserTreatment { CountyId = 2 }
            };

            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 2,
                sSearch = "NonExistentCounty",  
                sEcho = "1"
            };

            // Mock repositories
            _countyRepository.GetQueryableAsync().Returns(Task.FromResult(counties.AsQueryable()));
            _treatmentRepository.GetQueryableAsync().Returns(Task.FromResult(treatments.AsQueryable()));

            // Act
            var result = await _countyAppService.ApplyPagination(param);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(0, result.ITotalRecords); 
            Assert.Equal(0, result.ITotalDisplayRecords); 
            Assert.Empty(result.AaData); 

            // Verifying that the repository methods were called
            await _countyRepository.Received(1).GetQueryableAsync();
            await _treatmentRepository.Received(1).GetQueryableAsync();
        }

        [Fact]
        public async Task NewCounty_Should_Return_Correct_CountyDtos()
        {
            // Arrange
            var counties = new List<CountyEntity>
            {
                new CountyEntity { Id = 1, CountyName = "County1", District = 1},
                new CountyEntity { Id = 2, CountyName = "County2", District = 2},
                new CountyEntity { Id = 3, CountyName = "County3", District = 3}
            };

            var treatments = new List<UserTreatment>
            {
                new UserTreatment { CountyId = 1 },
                new UserTreatment { CountyId = 2 }
            };

            // Mock repositories
            _countyRepository.GetListAsync().Returns(Task.FromResult(counties));
            _treatmentRepository.GetListAsync().Returns(Task.FromResult(treatments));

            // Act
            var result = await _countyAppService.NewCounty();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            Assert.Equal("County1", result[0].CountyName);
            Assert.True(result[0].IsDeleteable);

            Assert.Equal("County2", result[1].CountyName);
            Assert.True(result[1].IsDeleteable);

            Assert.Equal("County3", result[2].CountyName);
            Assert.False(result[2].IsDeleteable);

            // Verifying that the repository methods were called
            await _countyRepository.Received(1).GetListAsync();
            await _treatmentRepository.Received(1).GetListAsync();
        }

        [Fact]
        public async Task NewCounty_Should_Return_Empty_List_When_No_Counties()
        {
            // Arrange
            var counties = new List<CountyEntity>(); 
            var treatments = new List<UserTreatment>(); 

            // Mock repositories
            _countyRepository.GetListAsync().Returns(Task.FromResult(counties));
            _treatmentRepository.GetListAsync().Returns(Task.FromResult(treatments));

            // Act
            var result = await _countyAppService.NewCounty();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); 

            // Verifying that the repository methods were called
            await _countyRepository.Received(1).GetListAsync();
            await _treatmentRepository.Received(1).GetListAsync();
        }

        [Fact]
        public async Task GetCountyAsync_Should_Return_CountyDto_When_County_Exists()
        {
            // Arrange
            var countyId = 1;
            var districtId = 2;
            var county = new CountyEntity
            {
                Id = countyId,
                CountyName = "TestCounty",
                District = districtId
            };

            // Mock repository to return a valid county
            _countyRepository.FindAsync(districtId, countyId).Returns(Task.FromResult(county));

            // Act
            var result = await _countyAppService.GetCountyAsync(districtId, countyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(countyId, result.Id);
            Assert.Equal("TestCounty", result.CountyName);
            Assert.Equal(districtId, result.District);

            // Verify repository call
            await _countyRepository.Received(1).FindAsync(districtId, countyId);
        }

        [Fact]
        public async Task GetCountyAsync_Should_Throw_Exception_When_County_Not_Found()
        {
            // Arrange
            var countyId = 1;
            var districtId = 2;

            // Mock repository to return null (county not found)
            _countyRepository.FindAsync(districtId, countyId).Returns(Task.FromResult<CountyEntity>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(() =>
                _countyAppService.GetCountyAsync(districtId, countyId));

            // Assert exception message
            Assert.Equal("County not found", exception.Message);

            // Verify repository call
            await _countyRepository.Received(1).FindAsync(districtId, countyId);
        }

        [Fact]
        public async Task CheckCounty_Should_Return_True_When_County_Exists_In_Treatments()
        {
            // Arrange
            var countyId = 1;
            var treatments = new List<UserTreatment>
            {
                new UserTreatment { CountyId = 1 },
                new UserTreatment { CountyId = 2 }
            };


            // Mock _treatmentRepository to return a list of treatments
            _treatmentRepository.GetListAsync().Returns(Task.FromResult(treatments));

            // Act
            var result = await _countyAppService.CheckCounty(countyId);

            // Assert
            Assert.True(result); 

            // Verify repository call
            await _treatmentRepository.Received(1).GetListAsync();
        }

        [Fact]
        public async Task DeleteCounty_Should_Return_True_When_Deletion_Succeeds()
        {
            // Arrange
            var countyId = 1;
            var district = 2;

            // Mock _countyRepository to return true (indicating successful deletion)
            _countyRepository.DeleteCountyAsync(district, countyId).Returns(Task.FromResult(true));

            // Act
            var result = await _countyAppService.DeleteCounty(countyId, district);

            // Assert
            Assert.True(result); 

            // Verify repository call
            await _countyRepository.Received(1).DeleteCountyAsync(district, countyId);
        }

        [Fact]
        public async Task DeleteCounty_Should_Return_False_When_Deletion_Fails()
        {
            // Arrange
            var countyId = 1;
            var district = 2;

            _countyRepository.DeleteCountyAsync(district, countyId).Returns(Task.FromResult(false));

            // Act
            var result = await _countyAppService.DeleteCounty(countyId, district);

            // Assert
            Assert.False(result); 

            // Verify repository call
            await _countyRepository.Received(1).DeleteCountyAsync(district, countyId);
        }

    }
}
