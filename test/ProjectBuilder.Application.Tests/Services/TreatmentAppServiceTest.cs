using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models;
using ProjectBuilder.Models.Treatment.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Xunit;
using ProjectBuilder.Models.UserTreatment.Dtos;
using Microsoft.AspNetCore.Http.Internal;

namespace ProjectBuilder.AppServices
{
    public class TreatmentAppServiceTest
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IRepository<UserTreatmentType, int> _userTreatmentTypeRepository;
        private readonly IRepository<BridgeToPavements, string> _bridgeToPavementsRepository;
        private readonly IRepository<CargoData> _cargoDataRepository;
        private readonly IRepository<CargoAttribute> _cargoAttributeRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;
        private readonly TreatmentAppService _treatmentAppService;

        public TreatmentAppServiceTest()
        {
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _treatmentRepository = Substitute.For<ITreatmentRepository>();
            _userTreatmentTypeRepository = Substitute.For<IRepository<UserTreatmentType, int>>();
            _bridgeToPavementsRepository = Substitute.For<IRepository<BridgeToPavements, string>>();
            _cargoDataRepository = Substitute.For<IRepository<CargoData>>();
            _cargoAttributeRepository = Substitute.For<IRepository<CargoAttribute>>();
            _objectMapper = Substitute.For<IObjectMapper>();
            _mapper = new ProjectBuilderMapper();
            _treatmentAppService = new TreatmentAppService(_httpContextAccessor, _treatmentRepository, _userTreatmentTypeRepository, _bridgeToPavementsRepository, _cargoDataRepository, _cargoAttributeRepository, _objectMapper, _mapper);
        }

        [Fact]
        public async Task LoadSearchModel_Should_Return_Valid_Model()
        {
            // Arrange
            var libraryId = Guid.NewGuid();
            var treatmentDto = new TreatmentDto
            {
                AssetType = "P",
                Id = 1,
                District = 2,
                Route = 3,
                FilterDirection = true,
                Year = 2025,
                Section = "10-20"
            };
            var service = new TreatmentAppService(_httpContextAccessor, _treatmentRepository, _userTreatmentTypeRepository, _bridgeToPavementsRepository, _cargoDataRepository, _cargoAttributeRepository, _objectMapper, _mapper);

            // Act
            var result = await service.LoadSearchModel(libraryId, treatmentDto);

            // Assert
            Assert.Equal("P", result.AssetType);
            Assert.Equal((byte)1, result.Cnty);
            Assert.Equal(10, result.FromSection);
            Assert.Equal(20, result.ToSection);
            Assert.Equal(libraryId, result.LibraryId);
        }

        [Fact]
        public async Task EditTreatmentAsync_Should_Return_True_When_Updated()
        {
            // Arrange
            // EditTreatmentAsync resolves the active library from session, so set it up.
            var libraryId = Guid.NewGuid();
            var httpContext = Substitute.For<HttpContext>();
            var session = Substitute.For<ISession>();
            session.TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]>()!)
                .Returns(callInfo =>
                {
                    callInfo[1] = System.Text.Encoding.UTF8.GetBytes(libraryId.ToString());
                    return true;
                });
            httpContext.Session.Returns(session);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            var updateDto = new UpdateUserTreatmentDto { Id = Guid.NewGuid(), AssetType = "B" };
            // EditTreatmentAsync builds a fresh entity by hand, so match on any instance.
            _treatmentRepository.EditTreatments(Arg.Any<UserTreatment>()).Returns(Task.FromResult(true));
            var service = new TreatmentAppService(_httpContextAccessor, _treatmentRepository, _userTreatmentTypeRepository, _bridgeToPavementsRepository, _cargoDataRepository, _cargoAttributeRepository, _objectMapper, _mapper);

            // Act
            var result = await service.EditTreatmentAsync(updateDto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateTreatment_Should_Return_Success_Message()
        {
            // Arrange
            // CreateTreatment runs ValidateRequiredForCreate, so populate all required fields.
            var createDto = new CreateUserTreatmentDto
            {
                AssetType = "P",
                Section = "101-102",
                Treatment = "Overlay",
                TreatmentType = 1,
                District = 2,
                CountyId = 3,
                Route = 4,
                Interstate = false,
                PreferredYear = 2025,
                Cost = 100000
            };
            var httpContext = Substitute.For<HttpContext>();
            var session = Substitute.For<ISession>();
            var libraryId = Guid.NewGuid();

            // Mock session properly for TryGetValue
            session.TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]>()!)
                .Returns(callInfo =>
                {
                    callInfo[1] = System.Text.Encoding.UTF8.GetBytes(libraryId.ToString());
                    return true;
                });

            httpContext.Session.Returns(session);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            var userTreatment = new UserTreatment { Id = Guid.NewGuid(), LibraryId = libraryId };

            // Correctly mock repository to return Guid?
            _treatmentRepository.CreateUserTreatment(Arg.Any<UserTreatment>())
                .Returns(callInfo => Task.FromResult<Guid?>(userTreatment.Id));

            var service = new TreatmentAppService(_httpContextAccessor, _treatmentRepository, _userTreatmentTypeRepository, _bridgeToPavementsRepository, _cargoDataRepository, _cargoAttributeRepository, _objectMapper, _mapper);

            // Act
            var result = await service.CreateTreatment(createDto);

            // Assert
            Assert.Equal("Treatment was created successfully", result);
        }

        [Fact]
        public async Task GetTreatmentAsync_Should_Return_Treatment()
        {
            // Arrange
            var treatmentId = Guid.NewGuid();
            var libraryId = Guid.NewGuid();
            var httpContext = Substitute.For<HttpContext>();
            var session = Substitute.For<ISession>();

            // Properly mock TryGetValue
            session.TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]>()!)
                .Returns(callInfo =>
                {
                    callInfo[1] = System.Text.Encoding.UTF8.GetBytes(libraryId.ToString());
                    return true;
                });

            httpContext.Session.Returns(session);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            var mockTreatment = new UserTreatment { Id = treatmentId, LibraryId = libraryId };

            // GetTreatmentAsync resolves the treatment via FilterTreatments, then maps with
            // the real ProjectBuilderMapper (which copies Id across).
            _treatmentRepository.FilterTreatments(Arg.Any<TreatmentSearchModel>())
                .Returns(Task.FromResult(new List<UserTreatment> { mockTreatment }));

            var service = new TreatmentAppService(_httpContextAccessor, _treatmentRepository, _userTreatmentTypeRepository, _bridgeToPavementsRepository, _cargoDataRepository, _cargoAttributeRepository, _objectMapper, _mapper);

            // Act
            var result = await service.GetTreatmentAsync(treatmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(treatmentId, result.Id);
        }

        [Fact]
        public async Task DeleteTreatmentAsync_Should_Return_True_When_Deleted()
        {
            // Arrange
            var treatmentId = Guid.NewGuid();
            var libraryId = Guid.NewGuid();
            var httpContext = Substitute.For<HttpContext>();
            var session = Substitute.For<ISession>();

            // ✅ Properly mock TryGetValue for session
            session.TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]>()!)
                .Returns(callInfo =>
                {
                    callInfo[1] = System.Text.Encoding.UTF8.GetBytes(libraryId.ToString());
                    return true;
                });

            httpContext.Session.Returns(session);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // ✅ Mock repository delete action
            _treatmentRepository.DeleteTreatmentAsync(libraryId, treatmentId).Returns(Task.FromResult(true));

            var service = new TreatmentAppService(_httpContextAccessor, _treatmentRepository, _userTreatmentTypeRepository, _bridgeToPavementsRepository, _cargoDataRepository, _cargoAttributeRepository, _objectMapper, _mapper);

            // Act
            var result = await service.DeleteTreatmentAsync(treatmentId);

            // Assert
            Assert.True(result);
        }


    }
}
