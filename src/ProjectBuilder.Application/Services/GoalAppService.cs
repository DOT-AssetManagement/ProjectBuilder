using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.Goal;
using ProjectBuilder.Models.Goal.Dtos;
using ProjectBuilder.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(ConfigurationPermissions.ConfigurationPageActions.Default)]
    public class GoalAppService : ApplicationService, IGoalAppService
    {
        private readonly IGoalRepository _goalRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;

        public GoalAppService(IGoalRepository goalRepository, IObjectMapper objectMapper, ProjectBuilderMapper mapper)
        {
            _goalRepository = goalRepository;
            _objectMapper = objectMapper;
            _mapper = mapper;
        }

        public async Task<GoalDto> CreateAsync(CreateGoalDto input)
        {
            var goal = _mapper.Map(input);
            var response = await _goalRepository.InsertAsync(goal);
            return _mapper.Map(response);
        }

        [HttpGet("api/app/goal/delete")]
        public async Task DeleteAsync(Guid id)
        {
            await _goalRepository.DeleteAsync(id);
        }

        [HttpGet("api/app/goal/getAll")]
        public async Task<List<GoalDto>> GetAllAsync()
        {
            var goals = await _goalRepository.GetListAsync();
            return _mapper.Map(goals);
        }

        public async Task<GoalDto> GetAsync(Guid id)
        {
            var goal = await _goalRepository.GetAsync(id);
            return _objectMapper.Map<GoalEntity, GoalDto>(goal);
        }

        public async Task<PagedResultDto<GoalDto>> GetListAsync(GetGoalInput input)
        {
            var goals = await _goalRepository.GetListAsync();
            var paged = goals.Skip(input.SkipCount).Take(input.MaxResultCount);
            var dtos = _mapper.Map(new List<GoalEntity>(paged));
            return new PagedResultDto<GoalDto>(goals.Count, dtos);
        }

        public async Task<GoalDto> UpdateAsync(Guid id, UpdateGoalDto input)
        {
            var goal = await _goalRepository.GetAsync(id);
            goal.Name = input.Name;
            goal.Description = input.Description;
            goal.Status = input.Status;
            var response = await _goalRepository.UpdateAsync(goal);
            return _mapper.Map(response);
        }
    }
}
