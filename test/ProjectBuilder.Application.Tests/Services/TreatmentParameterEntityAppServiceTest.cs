using Autofac.Core;
using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class TreatmentParameterEntityAppServiceTest
    {
        private readonly ITreatmentParameterRepositorty _treatmentParameterRepositorty;
        private readonly IObjectMapper _ObjectMapper;
        private readonly TreatmentParameterEntityAppService _treatmentParameterEntityAppService;

        public TreatmentParameterEntityAppServiceTest()
        {
            _treatmentParameterRepositorty = Substitute.For<ITreatmentParameterRepositorty>();
            _ObjectMapper = Substitute.For<IObjectMapper>();
            _treatmentParameterEntityAppService = new TreatmentParameterEntityAppService(_treatmentParameterRepositorty, _ObjectMapper);
        }

        [Fact]
        public async Task ApplyPagination_Should_Return_PaginatedDto()
        {
            // Arrange
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 10,
                sEcho = "1"
            };

            var entities = Enumerable.Range(1, 20).Select(i => new TreatmentParameterEntity { Id = i, UserTreatmentBenefitWeight = i * 10 }).AsQueryable();
            _treatmentParameterRepositorty.GetQueryableAsync().Returns(Task.FromResult(entities));

            var mappedDtos = entities.Take(10).Select(e => new TreatmentParameterDto { Id = e.Id, UserTreatmentBenefitWeight = e.UserTreatmentBenefitWeight }).ToList();
            _ObjectMapper.Map<List<TreatmentParameterEntity>, List<TreatmentParameterDto>>(Arg.Any<List<TreatmentParameterEntity>>()).Returns(mappedDtos);

            // Act
            var result = await _treatmentParameterEntityAppService.ApplyPagination(param);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.ITotalRecords);
            Assert.Equal(10, result.AaData.Count);
            Assert.Equal("1", result.SEcho);
        }

        [Fact]
        public async Task GetTreatmentParameter_Should_Return_Correct_Entity()
        {
            // Arrange
            int id = 1;
            var entity = new TreatmentParameterEntity { Id = id, UserTreatmentBenefitWeight = 50 };
            _treatmentParameterRepositorty.FindTreatmentParameterAsync(id).Returns(Task.FromResult(entity));

            // Act
            var result = await _treatmentParameterEntityAppService.GetTreatmentParameter(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(50, result.UserTreatmentBenefitWeight);
        }

        [Fact]
        public async Task EditTreatmentParameter_Should_Update_And_Return_Entity()
        {
            // Arrange
            var dto = new TreatmentParameterDto { Id = 1, UserTreatmentBenefitWeight = 75 };
            var updatedEntity = new TreatmentParameterEntity { Id = 1, UserTreatmentBenefitWeight = 75 };

            _treatmentParameterRepositorty
                .UpdateDeTreatmentParameter(Arg.Any<Dictionary<string, object>>(), dto.Id)
                .Returns(Task.CompletedTask);

            _treatmentParameterRepositorty
                .FindTreatmentParameterAsync(dto.Id)
                .Returns(Task.FromResult(updatedEntity));

            _ObjectMapper.Map<TreatmentParameterEntity, TreatmentParameterDto>(updatedEntity)
                .Returns(new TreatmentParameterDto { Id = updatedEntity.Id, UserTreatmentBenefitWeight = updatedEntity.UserTreatmentBenefitWeight });

            // Act
            var result = await _treatmentParameterEntityAppService.EditTreatmentParameter(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(75, result.UserTreatmentBenefitWeight);
        }

        [Fact]
        public async Task EditTreatmentParameter_Should_Throw_Exception_When_UserTreatmentBenefitWeight_Null()
        {
            // Arrange
            var dto = new TreatmentParameterDto { Id = 1, UserTreatmentBenefitWeight = null };

            // Act & Assert
            await Assert.ThrowsAsync<UserFriendlyException>(() => _treatmentParameterEntityAppService.EditTreatmentParameter(dto));
        }

        [Fact]
        public async Task DeleteTreatmentParameter_Should_Return_True_If_Success()
        {
            // Arrange
            var dto = new TreatmentParameterDto { Id = 1 };
            _treatmentParameterRepositorty.DeleteTreatmentParameter(dto.Id).Returns(Task.FromResult(true));

            // Act
            var result = await _treatmentParameterEntityAppService.DeleteTreatmentParameter(dto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteTreatmentParameter_Should_Return_False_If_Failure()
        {
            // Arrange
            var dto = new TreatmentParameterDto { Id = 2 };
            _treatmentParameterRepositorty.DeleteTreatmentParameter(dto.Id).Returns(Task.FromResult(false));

            // Act
            var result = await _treatmentParameterEntityAppService.DeleteTreatmentParameter(dto);

            // Assert
            Assert.False(result);
        }
    }
}
