using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.DefaultSlack;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class DefaultSlackAppServiceTest
    {
        private readonly IDefaultSlackrepository _defaultSlackrepository;
        private readonly IObjectMapper _objectMapper;

        private readonly DefaultSlackAppService _defaultSlackAppService;
        public DefaultSlackAppServiceTest()
        {
            _defaultSlackrepository = Substitute.For<IDefaultSlackrepository>();
            _objectMapper = Substitute.For<IObjectMapper>();
            _defaultSlackAppService = new DefaultSlackAppService(_defaultSlackrepository, _objectMapper);
        }

        [Fact]
        public async Task ApplyPagination_Should_Return_Paginated_And_Mapped_Results()
        {
            // Arrange
            var defaultSlacks = new List<DefaultSlack>
            {
                new DefaultSlack { Id = "1", MoveBefore = 1 },
                new DefaultSlack { Id = "2", MoveBefore = 2 },
                new DefaultSlack { Id = "3", MoveBefore = 3 }
            };

            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,     
                iDisplayLength = 2,     
                sEcho = "1"
            };

            var defaultSlackDtos = new List<DefaultSlackDto>
            {
                new DefaultSlackDto { Id = "1", MoveBefore = 1 },
                new DefaultSlackDto {  Id = "2", MoveBefore = 2  }
            };

            // Mock the repository call to return a list of DefaultSlacks
            _defaultSlackrepository.GetQueryableAsync().Returns(Task.FromResult(defaultSlacks.AsQueryable()));

            // Mock the ObjectMapper to return mapped DefaultSlackDto objects
            _objectMapper.Map<List<DefaultSlack>, List<DefaultSlackDto>>(Arg.Any<List<DefaultSlack>>())
                        .Returns(defaultSlackDtos);

            // Act
            var result = await _defaultSlackAppService.ApplyPagination(param);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(3, result.ITotalRecords); 
            Assert.Equal(3, result.ITotalDisplayRecords); 
            Assert.Equal(2, result.AaData.Count);

            // Ensure the expected data is present
            Assert.Equal(1, result.AaData[0].MoveBefore);
            Assert.Equal(2, result.AaData[1].MoveBefore);

            // Verify repository and mapping method were called
            await _defaultSlackrepository.Received(1).GetQueryableAsync();
            _objectMapper.Received(1).Map<List<DefaultSlack>, List<DefaultSlackDto>>(Arg.Any<List<DefaultSlack>>());
        }

        [Fact]
        public async Task GetDefaultSlack_Should_Return_DefaultSlackDto_When_Found()
        {
            // Arrange
            var assetType = "Bridge"; // Example AssetType
            var defaultSlack = new DefaultSlack
            {
                Id = "1",
                MoveAfter = 10,
                MoveBefore = 5
            };

            var expectedDefaultSlackDto = new DefaultSlackDto
            {
                Id = "1",
                MoveAfter = 10,
                MoveBefore = 5
            };

            // Mock the repository to return the DefaultSlack object when FindAsync is called
            _defaultSlackrepository.FindAsync(assetType).Returns(Task.FromResult(defaultSlack));

            // Act
            var result = await _defaultSlackAppService.GetDefaultSlack(assetType);

            // Assert
            Assert.NotNull(result); 
            Assert.Equal(expectedDefaultSlackDto.Id, result.Id);
            Assert.Equal(expectedDefaultSlackDto.MoveAfter, result.MoveAfter);
            Assert.Equal(expectedDefaultSlackDto.MoveBefore, result.MoveBefore); 

            await _defaultSlackrepository.Received(1).FindAsync(assetType);
        }

        [Fact]
        public async Task DeleteDefaultSlack_Should_Return_True_When_Deleted_Successfully()
        {
            // Arrange
            var assetType = "Bridge"; 
            bool deleteResult = true; 

            _defaultSlackrepository.DeleteDefaultSlackAsync(assetType).Returns(Task.FromResult(deleteResult));

            // Act
            var result = await _defaultSlackAppService.DeleteDefaultSlack(assetType);

            // Assert
            Assert.True(result);

            // Verify that the repository method was called exactly once with the correct parameter
            await _defaultSlackrepository.Received(1).DeleteDefaultSlackAsync(assetType);
        }

        [Fact]
        public async Task DeleteDefaultSlack_Should_Return_False_When_Delete_Fails()
        {
            // Arrange
            var assetType = "Bridge"; 
            bool deleteResult = false; 

            _defaultSlackrepository.DeleteDefaultSlackAsync(assetType).Returns(Task.FromResult(deleteResult));

            // Act
            var result = await _defaultSlackAppService.DeleteDefaultSlack(assetType);

            // Assert
            Assert.False(result); 

            // Verify that the repository method was called exactly once with the correct parameter
            await _defaultSlackrepository.Received(1).DeleteDefaultSlackAsync(assetType);
        }

    }
}
