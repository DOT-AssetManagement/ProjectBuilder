using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.TreatmentType.Dtos;
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
    public class TreatmentTypeAppServiceTest
    {
        private readonly ITreatmentTypeRepository _treatmentTypeRepository;
        private readonly IObjectMapper _ObjectMapper;
        private readonly TreatmentTypeAppService _treatmentTypeAppService;

        public TreatmentTypeAppServiceTest()
        {
            _treatmentTypeRepository = Substitute.For<ITreatmentTypeRepository>();
            _ObjectMapper = Substitute.For<IObjectMapper>();
            _treatmentTypeAppService = new TreatmentTypeAppService(_treatmentTypeRepository, _ObjectMapper);
        }

        [Fact]
        public async Task ApplyPagination_Should_Return_Paginated_Result()
        {
            // Arrange
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 2,
                sEcho = "1"
            };

            // Ensure IQueryable is returned properly
            var treatmentTypes = new List<TreatmentType>
            {
                new TreatmentType { Id = "1", AssetType = "Type A" },
                new TreatmentType { Id = "2", AssetType = "Type B" },
                new TreatmentType { Id = "3", AssetType = "Type C" }
            }.AsQueryable();

            var pagedTreatmentTypes = treatmentTypes
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();

            var mappedDtos = new List<TreatmentTypeDto>
            {
                new TreatmentTypeDto { Id = "1", AssetType = "Type A" },
                new TreatmentTypeDto { Id = "2", AssetType = "Type B" }
            };

            // Mock repository properly
            _treatmentTypeRepository.GetQueryableAsync().Returns(Task.FromResult(treatmentTypes));

            // Mock mapper properly
            _ObjectMapper.Map<List<TreatmentType>, List<TreatmentTypeDto>>(Arg.Any<List<TreatmentType>>()).Returns(mappedDtos);

            // Act
            var result = await _treatmentTypeAppService.ApplyPagination(param);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AaData);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(3, result.ITotalRecords);
            Assert.Equal(3, result.ITotalDisplayRecords);
            Assert.Equal(2, result.AaData.Count);
            Assert.Equal("Type A", result.AaData[0].AssetType);
            Assert.Equal("Type B", result.AaData[1].AssetType);
        }

        [Fact]
        public async Task GetTreatmentType_Should_Return_Correct_Dto_When_Found()
        {
            // Arrange
            var id = "123"; 
            var treatmentTypeEntity = new TreatmentType
            {
                Id = id,
                AssetType = "Type A",
                MoveEarlier = 1,
                MoveLater = 2
            };

            // Mock repository to return the treatmentTypeEntity when searched with 'id'
            _treatmentTypeRepository.FindAssetType(id).Returns(Task.FromResult(treatmentTypeEntity));

            // Act
            var result = await _treatmentTypeAppService.GetTreatmentType(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Type A", result.AssetType);
            Assert.Equal(1, result.MoveEarlier); 
            Assert.Equal(2, result.MoveLater);  

        }

        [Fact]
        public async Task GetTreatmentType_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var id = "not-found-id";

            // Mock repository to return null
            _treatmentTypeRepository.FindAssetType(id).Returns((TreatmentType)null);

            // Act
            var result = await _treatmentTypeAppService.GetTreatmentType(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task EditTreatmentType_Should_Update_And_Return_Updated_Entity()
        {
            // Arrange
            var updateDto = new UpdateTreatmentTypeDto
            {
                Id = "1",
                MoveEarlier = 1,
                MoveLater = 0,
                AssetType = "Updated Type"
            };

            var updatedTreatmentType = new TreatmentType
            {
                Id = "1",
                MoveEarlier = 1,
                MoveLater = 0,
                AssetType = "Updated Type"
            };

            var expectedDto = new TreatmentTypeDto
            {
                Id = "1",
                MoveEarlier = 1,
                MoveLater = 0,
                AssetType = "Updated Type"
            };

            _treatmentTypeRepository
                .UpdateTreatmnetTypeAsync(Arg.Any<Dictionary<string, object>>(), updateDto.Id)
                .Returns(Task.FromResult(updatedTreatmentType));

 
            _treatmentTypeRepository.FindAssetType(updateDto.Id)
                .Returns(Task.FromResult(updatedTreatmentType));

            // Mock ObjectMapper for mapping entity to DTO
            _ObjectMapper.Map<TreatmentType, TreatmentTypeDto>(updatedTreatmentType)
                .Returns(expectedDto);

            // Act
            var result = await _treatmentTypeAppService.EditTreatmentType(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.MoveEarlier, result.MoveEarlier);
            Assert.Equal(expectedDto.MoveLater, result.MoveLater);
            Assert.Equal(expectedDto.AssetType, result.AssetType);
        }

        [Fact]
        public async Task EditTreatmentType_Should_Throw_Exception_When_TreatmentType_Not_Found()
        {
            // Arrange
            var updateDto = new UpdateTreatmentTypeDto
            {
                Id = "invalid-id",
                MoveEarlier = 1,
                MoveLater = 0,
                AssetType = "Non-Existent Type"
            };

            _treatmentTypeRepository
                .UpdateTreatmnetTypeAsync(Arg.Any<Dictionary<string, object>>(), updateDto.Id)
                .Returns<Task<TreatmentType>>(x => throw new UserFriendlyException("TreatmentType record not found."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(() => _treatmentTypeAppService.EditTreatmentType(updateDto));

            // Validate exception message
            Assert.Equal("TreatmentType record not found.", exception.Message);
        }

    }
}
