using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.UserTreatmentType.Dtos;
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
    public class UserTreatmentTypeAppServiceTest
    {
        private readonly IUserTreatmentTypeRepository _userTreatmentTypeRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly UserTreatmentTypeAppService _userTreatmentTypeAppService;

        public UserTreatmentTypeAppServiceTest()
        {
            _userTreatmentTypeRepository = Substitute.For<IUserTreatmentTypeRepository>();
            _objectMapper = Substitute.For<IObjectMapper>();
            _userTreatmentTypeAppService = new UserTreatmentTypeAppService(_userTreatmentTypeRepository, _objectMapper);
        }

        [Fact]
        public async Task ApplyPagination_Should_Return_PaginatedDto()
        {
            // Arrange
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 2,
                sEcho = "1"
            };

            var userTreatments = new List<UserTreatmentType>
            {
                new UserTreatmentType { Id = 1, UserTreatmentName = "Treatment A" },
                new UserTreatmentType { Id = 2, UserTreatmentName = "Treatment B" },
                new UserTreatmentType { Id = 3, UserTreatmentName = "Treatment C" }
            }.AsQueryable();

            var expectedMappedResults = new List<UserTreatmentTypeDto>
            {
                new UserTreatmentTypeDto { Id = 1, UserTreatmentName = "Treatment A" },
                new UserTreatmentTypeDto { Id = 2, UserTreatmentName = "Treatment B" }
            };

            _userTreatmentTypeRepository.GetQueryableAsync().Returns(Task.FromResult(userTreatments));
            _objectMapper.Map<List<UserTreatmentType>, List<UserTreatmentTypeDto>>(Arg.Any<List<UserTreatmentType>>()).Returns(expectedMappedResults);

            // Act
            var result = await _userTreatmentTypeAppService.ApplyPagination(param);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(3, result.ITotalRecords); 
            Assert.Equal(3, result.ITotalDisplayRecords);
            Assert.Equal(2, result.AaData.Count);
            Assert.Equal("Treatment A", result.AaData[0].UserTreatmentName);
            Assert.Equal("Treatment B", result.AaData[1].UserTreatmentName);
        }

        [Fact]
        public async Task GetUserTreatment_Should_Return_UserTreatmentTypeDto_When_Id_Exists()
        {
            // Arrange
            var id = 1;
            var userTreatmentEntity = new UserTreatmentType
            {
                Id = id,
                UserTreatmentName = "Treatment A"
            };

            _userTreatmentTypeRepository.FindAsync(id).Returns(Task.FromResult(userTreatmentEntity));

            // Act
            var result = await _userTreatmentTypeAppService.GetUserTreatment(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Treatment A", result.UserTreatmentName);
        }

        [Fact]
        public async Task GetUserTreatment_Should_Return_Null_When_Id_Does_Not_Exist()
        {
            // Arrange
            var id = 99;

            _userTreatmentTypeRepository.FindAsync(id).Returns(Task.FromResult<UserTreatmentType>(null));

            // Act
            var result = await _userTreatmentTypeAppService.GetUserTreatment(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteUserTreatments_Should_Return_True_When_Delete_Succeeds()
        {
            // Arrange
            var userTreatmentDto = new UserTreatmentTypeDto
            {
                Id = 1,
                UserTreatmentName = "Treatment A"
            };

            _userTreatmentTypeRepository.DeleteAsync(userTreatmentDto.Id).Returns(Task.FromResult(true));

            // Act
            var result = await _userTreatmentTypeAppService.DeleteUserTreatments(userTreatmentDto);

            // Assert
            Assert.True(result);
            await _userTreatmentTypeRepository.Received(1).DeleteAsync(userTreatmentDto.Id);
        }

        [Fact]
        public async Task DeleteUserTreatments_Should_Return_False_When_Delete_Fails()
        {
            // Arrange
            var userTreatmentDto = new UserTreatmentTypeDto
            {
                Id = 2,
                UserTreatmentName = "Treatment B"
            };

            _userTreatmentTypeRepository.DeleteAsync(userTreatmentDto.Id).Returns(Task.FromResult(false));

            // Act
            var result = await _userTreatmentTypeAppService.DeleteUserTreatments(userTreatmentDto);

            // Assert
            Assert.False(result);
            await _userTreatmentTypeRepository.Received(1).DeleteAsync(userTreatmentDto.Id);
        }

        [Fact]
        public async Task EditUserTreatments_Should_Throw_Exception_When_Name_Is_Null_Or_Empty()
        {
            // Arrange
            var invalidModel = new UpdateUserTreatmentTypeDto
            {
                Id = 1,
                UserTreatmentName = "" // Invalid name
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(() => _userTreatmentTypeAppService.EditUserTreatments(invalidModel));
            Assert.Equal("the provided information are not valid to edit a User Treatments", exception.Message);
        }

        [Fact]
        public async Task EditUserTreatments_Should_Update_And_Return_Updated_Dto()
        {
            // Arrange
            var updateModel = new UpdateUserTreatmentTypeDto
            {
                Id = 1,
                UserTreatmentName = "Updated Treatment Name"
            };

            var updatedEntity = new UserTreatmentType
            {
                Id = 1,
                UserTreatmentName = "Updated Treatment Name"
            };

            var expectedDto = new UserTreatmentTypeDto
            {
                Id = 1,
                UserTreatmentName = "Updated Treatment Name"
            };

            // Mock repository update and fetch
            _userTreatmentTypeRepository
                .FindAsync(updateModel.Id)
                .Returns(Task.FromResult(updatedEntity));

            _objectMapper
                .Map<UserTreatmentType, UserTreatmentTypeDto>(updatedEntity)
                .Returns(expectedDto);

            // Act
            var result = await _userTreatmentTypeAppService.EditUserTreatments(updateModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.UserTreatmentName, result.UserTreatmentName);

            await _userTreatmentTypeRepository.Received(1)
                .UpdateUserTreatmentTypeAsync(Arg.Is<Dictionary<string, object>>(d =>
                    d.ContainsKey("UserTreatmentName") && (string)d["UserTreatmentName"] == "Updated Treatment Name"
                ), updateModel.Id);

            await _userTreatmentTypeRepository.Received(1).FindAsync(updateModel.Id);
        }
    }
}

