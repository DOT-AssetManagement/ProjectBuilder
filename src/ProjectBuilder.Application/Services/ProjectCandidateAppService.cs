using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.ProjectCandidate;
using ProjectBuilder.Models.ProjectCandidate.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace ProjectBuilder.Services
{
    public class ProjectCandidateAppService : ApplicationService, IProjectCandidateAppService
    {
        private readonly IProjectCandidateRepository _projectCandidateRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;

        public ProjectCandidateAppService(IProjectCandidateRepository projectCandidateRepository, ICurrentUser currentUser, IObjectMapper objectMapper, ProjectBuilderMapper mapper)
        {
            _projectCandidateRepository = projectCandidateRepository;
            _currentUser = currentUser;
            _objectMapper = objectMapper;
            _mapper = mapper;
        }

        public async Task<ProjectCandidateDto> CreateAsync(CreateProjectCandidateDto input)
        {
            var entity = _mapper.Map(input);
            entity.CreatedBy = _currentUser.UserName;
            var response = await _projectCandidateRepository.InsertAsync(entity);
            return _mapper.Map(response);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _projectCandidateRepository.DeleteAsync(id);
        }

        [HttpGet("api/app/project-candidate/getAll")]
        public async Task<List<ProjectCandidateDto>> GetAllAsync()
        {
            var entities = await _projectCandidateRepository.GetAll();
            return _mapper.Map(entities);
        }

        public async Task<List<ProjectCandidateDto>> GetAllProjectsAsync(Guid libraryId)
        {
            var entities = await _projectCandidateRepository.GetProjectsByPoolId(libraryId);
            return _mapper.Map(entities);
        }

        public async Task<ProjectCandidateDto> GetAsync(Guid id)
        {
            var entity = await _projectCandidateRepository.GetAsync(id);
            return _mapper.Map(entity);
        }

        public async Task<PagedResultDto<ProjectCandidateDto>> GetListAsync(GetProjectCandidateInput input)
        {
            var entities = await _projectCandidateRepository.GetListAsync();
            var paged = entities.Skip(input.SkipCount).Take(input.MaxResultCount);
            var dtos = _mapper.Map(new List<ProjectCandidateEntity>(paged));
            return new PagedResultDto<ProjectCandidateDto>(entities.Count, dtos);
        }

        public async Task<List<string>> GetDistinctDescriptions()
        {
            return await _projectCandidateRepository.GetDistinctDescriptions();
        }

        public async Task<string> GetGisJsonStringOutput(Guid scenarioId, double relativeEfficiencyThreshold)
        {
            return await _projectCandidateRepository.GetGisJsonStringOutput(scenarioId, relativeEfficiencyThreshold);
        }

        public async Task<ProjectCandidateDto> UpdateAsync(Guid id, UpdateProjectCandidateDto input)
        {
            var entity = _mapper.Map(input);
            entity.CreatedBy = _currentUser.UserName;
            var response = await _projectCandidateRepository.UpdateAsync(entity);
            return _mapper.Map(response);
        }
    }
}
