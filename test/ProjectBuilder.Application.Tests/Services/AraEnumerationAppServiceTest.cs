using Autofac.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.AraEnumerations.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class AraEnumerationAppServiceTests : AppServiceTestBase
    {
        private readonly AraEnumerationAppService _service;

        public AraEnumerationAppServiceTests()
        {
            _service = new AraEnumerationAppService(AraEnumerationRepository, ObjectMapper);
        }

        [Fact]
        public void GetAllAraEnumerations_ReturnsAllItemsPositive()
        {
            var items = new List<AraEnumerations>
            {
                new AraEnumerations { Id = "100", EnumName = "Active", EnumInt = 1 },
                new AraEnumerations { Id = "200", EnumName = "Inactive", EnumInt = 0 },
                new AraEnumerations { Id = "300", EnumName = "Normal", EnumInt = 10 }
            }.AsQueryable();

            AraEnumerationRepository.GetQueryable().Returns(items);

            var result = _service.GetAllAraEnumerations();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("100", result[0].EnumFamily);
            Assert.Equal("Active", result[0].EnumName);
            Assert.Equal(1, result[0].EnumInt);
            Assert.Equal("200", result[1].EnumFamily);
            Assert.Equal("Inactive", result[1].EnumName);
            Assert.Equal(0, result[1].EnumInt);
            Assert.Equal("300", result[2].EnumFamily);
            Assert.Equal("Normal", result[2].EnumName);
            Assert.Equal(10, result[2].EnumInt);
            Assert.True(result.Count > 0);
            Assert.NotEmpty(result);
            Assert.Equal("100", result.First().EnumFamily);
            Assert.Equal("Normal", result.Last().EnumName);
            Assert.Contains(result, x => x.EnumInt == 1);
            Assert.Contains(result, x => x.EnumInt == 0);
            Assert.All(result, x => Assert.NotNull(x.EnumName));
        }

        [Fact]
        public void GetAllAraEnumerations_ReturnsEmptyListPositive()
        {
            var items = new List<AraEnumerations>().AsQueryable();
            AraEnumerationRepository.GetQueryable().Returns(items);

            var result = _service.GetAllAraEnumerations();

            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Equal(0, result.Count);
            Assert.True(result.Count == 0);
            Assert.False(result.Any());
            Assert.Equal(result, new List<AraEnumerationDto>());
        }

        [Fact]
        public void GetAllAraEnumerations_HandlesNullValuesPositive()
        {
            var items = new List<AraEnumerations>
            {
                new AraEnumerations { Id = null, EnumName = null, EnumInt = 0 }
            }.AsQueryable();

            AraEnumerationRepository.GetQueryable().Returns(items);

            var result = _service.GetAllAraEnumerations();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Null(result[0].EnumFamily);
            Assert.Null(result[0].EnumName);
            Assert.Equal(0, result[0].EnumInt);
            Assert.True(result.Count == 1);
            Assert.Null(result.First().EnumFamily);
            Assert.Null(result.First().EnumName);
            Assert.Equal(0, result.First().EnumInt);
        }

        [Fact]
        public void GetAllAraEnumerations_ThrowsOnRepoFailureNegative()
        {
            AraEnumerationRepository.GetQueryable().Throws(new Exception("Repo error"));

            var ex = Assert.Throws<Exception>(() => _service.GetAllAraEnumerations());

            Assert.Equal("Repo error", ex.Message);
            Assert.NotNull(ex);
            Assert.IsType<Exception>(ex);
            Assert.False(string.IsNullOrEmpty(ex.Message));
        }

        [Fact]
        public void GetPagedAraEnumerations_ReturnsFirstPagePositive()
        {
            var items = new List<AraEnumerations>
            {
                new AraEnumerations { Id = "1", EnumName = "One", EnumInt = 1 },
                new AraEnumerations { Id = "2", EnumName = "Two", EnumInt = 2 },
                new AraEnumerations { Id = "3", EnumName = "Three", EnumInt = 3 }
            }.AsQueryable();

            var param = new JqueryDatatableParamDto { iDisplayStart = 0, iDisplayLength = 2, sEcho = "1" };
            var mapped = new List<AraEnumerationDto>
            {
                new AraEnumerationDto { EnumFamily = "1", EnumName = "One", EnumInt = 1 },
                new AraEnumerationDto { EnumFamily = "2", EnumName = "Two", EnumInt = 2 }
            };

            AraEnumerationRepository.GetQueryable().Returns(items);
            ObjectMapper.Map<List<AraEnumerations>, List<AraEnumerationDto>>(Arg.Any<List<AraEnumerations>>()).Returns(mapped);

            var result = _service.GetPagedAraEnumerations(param);

            Assert.NotNull(result);
            Assert.Equal("1", result.SEcho);
            Assert.Equal(3, result.ITotalRecords);
            Assert.Equal(2, result.AaData.Count);
            Assert.Equal("1", result.AaData[0].EnumFamily);
            Assert.Equal("2", result.AaData[1].EnumFamily);
            Assert.NotNull(result.AaData);
            Assert.Equal(3, result.ITotalDisplayRecords);
            Assert.True(result.AaData.Count <= result.ITotalRecords);
            Assert.Equal("1", result.AaData.First().EnumFamily);
            Assert.Equal("One", result.AaData.First().EnumName);
            Assert.Equal(2, result.AaData.Last().EnumInt);
        }

        [Fact]
        public void GetPagedAraEnumerations_ReturnsSecondPagePositive()
        {
            var items = new List<AraEnumerations>
            {
                new AraEnumerations { Id = "10", EnumName = "Ten", EnumInt = 10 },
                new AraEnumerations { Id = "20", EnumName = "Twenty", EnumInt = 20 },
                new AraEnumerations { Id = "30", EnumName = "Thirty", EnumInt = 30 }
            }.AsQueryable();

            var param = new JqueryDatatableParamDto { iDisplayStart = 1, iDisplayLength = 1, sEcho = "2" };
            var mapped = new List<AraEnumerationDto>
            {
                new AraEnumerationDto { EnumFamily = "20", EnumName = "Twenty", EnumInt = 20 }
            };

            AraEnumerationRepository.GetQueryable().Returns(items);
            ObjectMapper.Map<List<AraEnumerations>, List<AraEnumerationDto>>(Arg.Any<List<AraEnumerations>>()).Returns(mapped);

            var result = _service.GetPagedAraEnumerations(param);

            Assert.NotNull(result);
            Assert.Equal("2", result.SEcho);
            Assert.Equal(3, result.ITotalRecords);
            Assert.Equal(1, result.AaData.Count);
            Assert.Equal("20", result.AaData[0].EnumFamily);
            Assert.NotNull(result.AaData);
            Assert.True(result.AaData.Count == 1);
            Assert.Equal(3, result.ITotalDisplayRecords);
            Assert.Equal("Twenty", result.AaData[0].EnumName);
        }

        [Fact]
        public void GetPagedAraEnumerations_ReturnsEmptyWhenNoDataPositive()
        {
            var items = new List<AraEnumerations>().AsQueryable();
            var param = new JqueryDatatableParamDto { iDisplayStart = 0, iDisplayLength = 5, sEcho = "3" };

            AraEnumerationRepository.GetQueryable().Returns(items);
            ObjectMapper.Map<List<AraEnumerations>, List<AraEnumerationDto>>(Arg.Any<List<AraEnumerations>>()).Returns(new List<AraEnumerationDto>());

            var result = _service.GetPagedAraEnumerations(param);

            Assert.NotNull(result);
            Assert.Equal(0, result.ITotalRecords);
            Assert.Empty(result.AaData);
            Assert.Equal(0, result.ITotalDisplayRecords);
            Assert.NotNull(result.AaData);
            Assert.True(result.AaData.Count == 0);
            Assert.Equal(result.AaData, new List<AraEnumerationDto>());
        }

        [Fact]
        public void GetPagedAraEnumerations_ThrowsOnRepoFailureNegative()
        {
            var param = new JqueryDatatableParamDto { iDisplayStart = 0, iDisplayLength = 5, sEcho = "4" };
            AraEnumerationRepository.GetQueryable().Throws(new Exception("Database error"));

            var ex = Assert.Throws<Exception>(() => _service.GetPagedAraEnumerations(param));

            Assert.Equal("Database error", ex.Message);
            Assert.NotNull(ex);
            Assert.IsType<Exception>(ex);
        }

        [Fact]
        public void GetPagedAraEnumerations_ThrowsOnMapperFailureNegative()
        {
            var items = new List<AraEnumerations> { new AraEnumerations { Id = "1", EnumName = "One" } }.AsQueryable();
            var param = new JqueryDatatableParamDto { iDisplayStart = 0, iDisplayLength = 1, sEcho = "5" };

            AraEnumerationRepository.GetQueryable().Returns(items);
            ObjectMapper.Map<List<AraEnumerations>, List<AraEnumerationDto>>(Arg.Any<List<AraEnumerations>>())
                .Throws(new Exception("Mapping failed"));

            var ex = Assert.Throws<Exception>(() => _service.GetPagedAraEnumerations(param));

            Assert.Equal("Mapping failed", ex.Message);
            Assert.NotNull(ex);
            Assert.Contains("Mapping", ex.Message);
        }

        [Fact]
        public void GetPagedAraEnumerations_ReturnsCorrectTotalCountPositive()
        {
            var items = new List<AraEnumerations>
            {
                new AraEnumerations { Id = "A" },
                new AraEnumerations { Id = "B" },
                new AraEnumerations { Id = "C" }
            }.AsQueryable();

            var param = new JqueryDatatableParamDto { iDisplayStart = 0, iDisplayLength = 1, sEcho = "6" };
            var mapped = new List<AraEnumerationDto> { new AraEnumerationDto { EnumFamily = "A" } };

            AraEnumerationRepository.GetQueryable().Returns(items);
            ObjectMapper.Map<List<AraEnumerations>, List<AraEnumerationDto>>(Arg.Any<List<AraEnumerations>>()).Returns(mapped);

            var result = _service.GetPagedAraEnumerations(param);

            Assert.Equal(3, result.ITotalRecords);
            Assert.Equal(1, result.AaData.Count);
            Assert.Equal(3, result.ITotalDisplayRecords);
            Assert.NotNull(result.AaData);
            Assert.True(result.ITotalRecords > result.AaData.Count);
            Assert.Equal("6", result.SEcho);
        }
    }
}