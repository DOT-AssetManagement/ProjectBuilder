using NSubstitute;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.WorkCandidates.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using Xunit;

namespace ProjectBuilder.AppServices
{
    public class WorkCandidatesAppServiceTest
    {
        private readonly IWorkCandidatesRepository _workCandidatesRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;
        private readonly WorkCandidatesAppService _service;

        public WorkCandidatesAppServiceTest()
        {
            _workCandidatesRepository = Substitute.For<IWorkCandidatesRepository>();
            _currentUser = Substitute.For<ICurrentUser>();
            _objectMapper = Substitute.For<IObjectMapper>();
            _mapper = new ProjectBuilderMapper();
            _service = new WorkCandidatesAppService(_workCandidatesRepository, _currentUser, _objectMapper, _mapper);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenEntityNotFound()
        {
            var id = Guid.NewGuid();
            _workCandidatesRepository.GetAsync(id).Returns((WorkCandidateEntity)null);

            var result = await _service.GetAsync(id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_ReturnsMappedDto_WhenEntityFound()
        {
            var id = Guid.NewGuid();
            var entity = new WorkCandidateEntity { AssetType = "Bridge", BRKEY = "B123" };
            _workCandidatesRepository.GetAsync(id).Returns(entity);

            var result = await _service.GetAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Bridge", result.AssetType);
            Assert.Equal("B123", result.BRKEY);
        }

        [Fact]
        public async Task GetAllByPoolAndWorkCandidateIds_ReturnsNull_WhenEntitiesNull()
        {
            var libraryId = Guid.NewGuid();
            var ids = new List<Guid> { Guid.NewGuid() };
            _workCandidatesRepository.GetAllByPoolAndWorkCandidateIds(libraryId, ids).Returns((List<WorkCandidateEntity>)null);

            var result = await _service.GetAllByPoolAndWorkCandidateIds(libraryId, ids);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllByPoolAndWorkCandidateIds_ReturnsMappedList_WhenEntitiesFound()
        {
            var libraryId = Guid.NewGuid();
            var ids = new List<Guid> { Guid.NewGuid() };
            var entities = new List<WorkCandidateEntity>
            {
                new WorkCandidateEntity { AssetType = "Bridge" },
                new WorkCandidateEntity { AssetType = "Pavement" }
            };
            _workCandidatesRepository.GetAllByPoolAndWorkCandidateIds(libraryId, ids).Returns(entities);

            var result = await _service.GetAllByPoolAndWorkCandidateIds(libraryId, ids);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Bridge", result[0].AssetType);
            Assert.Equal("Pavement", result[1].AssetType);
        }

        [Fact]
        public async Task GetListAsync_ThrowsNotImplementedException()
        {
            var input = new GetWorkCandidatesInput();

            await Assert.ThrowsAsync<NotImplementedException>(() => _service.GetListAsync(input));
        }

        [Fact]
        public async Task GetAllByPoolOrProjectIdAsync_ReturnsNull_WhenEntitiesNull()
        {
            var libraryId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            _workCandidatesRepository.GetListByPoolOrProjectId(libraryId, projectId).Returns((List<WorkCandidateEntity>)null);

            var result = await _service.GetAllByPoolOrProjectIdAsync(libraryId, projectId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllByPoolOrProjectIdAsync_ReturnsMappedList_WhenEntitiesFound()
        {
            var libraryId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var entities = new List<WorkCandidateEntity>
            {
                new WorkCandidateEntity { BRKEY = "B001" }
            };
            _workCandidatesRepository.GetListByPoolOrProjectId(libraryId, projectId).Returns(entities);

            var result = await _service.GetAllByPoolOrProjectIdAsync(libraryId, projectId);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("B001", result[0].BRKEY);
        }

        [Fact]
        public async Task GetByProjectId_ReturnsNull_WhenEntityNotFound()
        {
            var projectId = Guid.NewGuid();
            _workCandidatesRepository.GetByProjectId(projectId).Returns((WorkCandidateEntity)null);

            var result = await _service.GetByProjectId(projectId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByProjectId_ReturnsMappedDto_WhenEntityFound()
        {
            var projectId = Guid.NewGuid();
            var entity = new WorkCandidateEntity { ProjectName = "Test Project" };
            _workCandidatesRepository.GetByProjectId(projectId).Returns(entity);

            var result = await _service.GetByProjectId(projectId);

            Assert.NotNull(result);
            Assert.Equal("Test Project", result.ProjectName);
        }

        [Fact]
        public async Task DeleteAsync_CallsRepositoryDelete()
        {
            var id = Guid.NewGuid();

            await _service.DeleteAsync(id);

            await _workCandidatesRepository.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task CreateAsync_ThrowsNotImplementedException()
        {
            var input = new CreateWorkCandidatesDto();

            await Assert.ThrowsAsync<NotImplementedException>(() => _service.CreateAsync(input));
        }

        [Fact]
        public async Task CreateWorkCandidateAsync_ReturnsTrue_AndUpdatesPool_WhenCreateSucceeds()
        {
            var libraryId = Guid.NewGuid();
            var input = new CreateWorkCandidatesDto { AssetType = "Bridge", BRKEY = "B123" };
            _workCandidatesRepository.CreateWorkCandidate(Arg.Any<WorkCandidateEntity>()).Returns(true);
            _workCandidatesRepository.UpdatePool(libraryId, false).Returns(true);

            var result = await _service.CreateWorkCandidateAsync(input, libraryId);

            Assert.True(result);
            await _workCandidatesRepository.Received(1).CreateWorkCandidate(Arg.Is<WorkCandidateEntity>(e => e.AssetType == "Bridge" && e.BRKEY == "B123"));
            await _workCandidatesRepository.Received(1).UpdatePool(libraryId, false);
        }

        [Fact]
        public async Task CreateWorkCandidateAsync_ReturnsFalse_WhenCreateFails()
        {
            var libraryId = Guid.NewGuid();
            var input = new CreateWorkCandidatesDto { AssetType = "Bridge" };
            _workCandidatesRepository.CreateWorkCandidate(Arg.Any<WorkCandidateEntity>()).Returns(false);
            _workCandidatesRepository.UpdatePool(libraryId, false).Returns(true);

            var result = await _service.CreateWorkCandidateAsync(input, libraryId);

            Assert.False(result);
            await _workCandidatesRepository.Received(1).UpdatePool(libraryId, false);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsNotImplementedException()
        {
            var input = new UpdateWorkCandidatesDto();

            await Assert.ThrowsAsync<NotImplementedException>(() => _service.UpdateAsync(Guid.NewGuid(), input));
        }

        [Fact]
        public async Task UpdateWorkCandidateAsync_UpdatesEntityFieldsAndReturnsDto()
        {
            var id = Guid.NewGuid();
            var libraryId = Guid.NewGuid();
            var existingEntity = new WorkCandidateEntity { AssetType = "Old", District = 1 };
            var input = new UpdateWorkCandidatesDto
            {
                AssetType = "Bridge",
                District = 5,
                Cnty = 10,
                Route = 100,
                Description = "Updated description",
                BRKEY = "B999",
                Cost = 123.45f,
                IsIsolatedBridge = true
            };
            var updatedDto = new WorkCandidatesDto { Id = id, AssetType = "Bridge" };

            _workCandidatesRepository.GetAsync(id).Returns(existingEntity);
            _workCandidatesRepository.UpdateAsync(existingEntity).Returns(existingEntity);
            _objectMapper.Map<WorkCandidateEntity, WorkCandidatesDto>(existingEntity).Returns(updatedDto);
            _workCandidatesRepository.UpdateWorkCandidate(id).Returns(true);
            _workCandidatesRepository.UpdatePool(libraryId, false).Returns(true);

            var result = await _service.UpdateWorkCandidateAsync(id, input, libraryId);

            Assert.Equal("Bridge", existingEntity.AssetType);
            Assert.Equal((byte)5, existingEntity.District);
            Assert.Equal((byte)10, existingEntity.Cnty);
            Assert.Equal(100, existingEntity.Route);
            Assert.Equal("Updated description", existingEntity.Description);
            Assert.Equal("B999", existingEntity.BRKEY);
            Assert.Equal(123.45f, existingEntity.Cost);
            Assert.True(existingEntity.IsIsolatedBridge);

            await _workCandidatesRepository.Received(1).UpdateWorkCandidate(id);
            await _workCandidatesRepository.Received(1).UpdatePool(libraryId, false);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Bridge", result.AssetType);
        }
    }
}
