using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.BridgeToPavements;
using ProjectBuilder.Models.CreateBridgeToPavements.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class BridgeToPavmentsAppServiceTests : AppServiceTestBase
    {
        private readonly BridgeToPavmentsAppService _service;
        public BridgeToPavmentsAppServiceTests()
        {
            _service = new BridgeToPavmentsAppService(BridgeToPavementsRepository, ObjectMapper);
        }

        [Fact]
        public void GetAllBridgeToPavements_ReturnsAllItemsPositive()
        {
            var items = new List<BridgeToPavements>
            {
                new BridgeToPavements { Id = "1" },
                new BridgeToPavements { Id = "2" }
            };

            BridgeToPavementsRepository.GetList().Returns(items);

            var result = _service.GetAllBridgeToPavements();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("1", result[0].Id);
            Assert.Equal("2", result[1].Id);
            Assert.True(result.Count > 0);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetAllBridgeToPavements_ReturnsEmptyListPositive()
        {
            BridgeToPavementsRepository.GetList().Returns(new List<BridgeToPavements>());

            var result = _service.GetAllBridgeToPavements();

            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Equal(0, result.Count);
            Assert.False(result.Any());
        }

        [Fact]
        public void GetPagedBridgeToPavements_ReturnsFirstPagePositive()
        {
            var items = new List<BridgeToPavements>
            {
                new BridgeToPavements { Id = "1", County = "One", BridgeId = 101, District = 1, CountyCode = 1, Route = 1, Segment = 1, OffSet = 10, CreatedBy = "User1", CreatedAt = DateTime.Today },
                new BridgeToPavements { Id = "2", County = "Two", BridgeId = 102, District = 2, CountyCode = 2, Route = 2, Segment = 2, OffSet = 20, CreatedBy = "User2", CreatedAt = DateTime.Today },
                new BridgeToPavements { Id = "3", County = "Three", BridgeId = 103, District = 3, CountyCode = 3, Route = 3, Segment = 3, OffSet = 30, CreatedBy = "User3", CreatedAt = DateTime.Today }
            }.AsQueryable();

            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 2,
                sEcho = "1"
            };

            var mapped = new List<BridgeToPavementDto>
            {
                new BridgeToPavementDto { Id = "1", County = "One" },
                new BridgeToPavementDto { Id = "2", County = "Two" }
            };

            BridgeToPavementsRepository.GetQueryable().Returns(items);
            ObjectMapper.Map<List<BridgeToPavements>, List<BridgeToPavementDto>>(Arg.Any<List<BridgeToPavements>>()).Returns(mapped);

            var result = _service.GetPagedBridgeToPavements(param);

            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(3, result.ITotalRecords);
            Assert.Equal(2, result.AaData.Count);
            Assert.Equal("1", result.AaData[0].Id);
            Assert.Equal("2", result.AaData[1].Id);
            Assert.NotNull(result.AaData);
            Assert.Equal(3, result.ITotalDisplayRecords);
            Assert.True(result.AaData.Count <= result.ITotalRecords);
            Assert.Equal("1", result.AaData.First().Id);
            Assert.Equal("One", result.AaData.First().County);
            Assert.Equal("Two", result.AaData.Last().County);
        }

        [Fact]
        public void GetPagedBridgeToPavements_FiltersByCountyPositive()
        {
            var items = new List<BridgeToPavements>
            {
                new BridgeToPavements { Id = "1", County = "TestCounty1", BridgeId = 101, District = 1, CountyCode = 1, Route = 1, Segment = 1, OffSet = 10, CreatedBy = "User1", CreatedAt = DateTime.Today },
                new BridgeToPavements { Id = "2", County = "TestCounty2", BridgeId = 102, District = 2, CountyCode = 2, Route = 2, Segment = 2, OffSet = 20, CreatedBy = "User2", CreatedAt = DateTime.Today },
                new BridgeToPavements { Id = "3", County = "OtherCounty", BridgeId = 103, District = 3, CountyCode = 3, Route = 3, Segment = 3, OffSet = 30, CreatedBy = "User3", CreatedAt = DateTime.Today }
            }.AsQueryable();

            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 2,
                sSearch = "Test",
                sEcho = "1"
            };

            var mapped = new List<BridgeToPavementDto>
            {
                new BridgeToPavementDto { Id = "1", County = "TestCounty1" },
                new BridgeToPavementDto { Id = "2", County = "TestCounty2" }
            };

            BridgeToPavementsRepository.GetQueryable().Returns(items);
            ObjectMapper.Map<List<BridgeToPavements>, List<BridgeToPavementDto>>(Arg.Any<List<BridgeToPavements>>()).Returns(mapped);

            var result = _service.GetPagedBridgeToPavements(param);

            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(2, result.ITotalRecords);
            Assert.Equal(2, result.AaData.Count);
            Assert.Equal("1", result.AaData[0].Id);
            Assert.Equal("2", result.AaData[1].Id);
            Assert.NotNull(result.AaData);
            Assert.Equal(2, result.ITotalDisplayRecords);
            Assert.True(result.AaData.Count <= result.ITotalRecords);
            Assert.Equal("1", result.AaData.First().Id);
            Assert.Equal("TestCounty1", result.AaData.First().County);
            Assert.Equal("TestCounty2", result.AaData.Last().County);
        }

        [Fact]
        public void GetPagedBridgeToPavements_ThrowsOnRepositoryFailureNegative()
        {
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = 0,
                iDisplayLength = 5,
                sEcho = "1"
            };

            BridgeToPavementsRepository.GetQueryable().Throws(new Exception("Repository failure"));

            var ex = Assert.Throws<Exception>(() => _service.GetPagedBridgeToPavements(param));

            Assert.NotNull(ex);
            Assert.Equal("Repository failure", ex.Message);
            Assert.IsType<Exception>(ex);
        }
    }
}