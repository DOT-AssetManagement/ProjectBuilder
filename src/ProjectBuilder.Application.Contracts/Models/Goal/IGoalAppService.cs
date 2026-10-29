using ProjectBuilder.Models.Goal.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.Goal
{
    public interface IGoalAppService : ICrudAppService<GoalDto, Guid, GetGoalInput, CreateGoalDto, UpdateGoalDto>
    {
        Task<List<GoalDto>> GetAllAsync();
    }
}
