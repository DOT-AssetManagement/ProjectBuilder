using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Pool;
using ProjectBuilder.Models.Pool.Dtos;
using ProjectBuilder.Models.ProjectCandidate;
using ProjectBuilder.Models.ProjectCandidate.Dtos;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Scenario.Dtos;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.Outputs.Charts
{
    public class ProjectsAndGoalsPercentageContributionModel : PageModel
    {
        private readonly IProjectCandidateAppService _projectCandidateAppService;
        private readonly IScenarioAppService _scenarioAppService;
        private readonly IPoolAppService _poolAppService;

        public List<ScenarioDto> Scenarios { get; set; }
        public List<CandidatePoolDto> Pools { get; set; }
        public string PoolName { get; set; }
        public Guid PoolId { get; set; }
        public string json { get; set; }
        public List<ProjectCandidateDto> ProjectCandidates { get; set; }

        public ProjectsAndGoalsPercentageContributionModel(IProjectCandidateAppService projectCandidateAppService, IScenarioAppService scenarioAppService, IPoolAppService poolAppService)
        {
            _projectCandidateAppService = projectCandidateAppService;
            _scenarioAppService = scenarioAppService;
            _poolAppService = poolAppService;
        }
        public async Task OnGet(Guid? poolId)
        {
            if (poolId != null)
            {
                ProjectCandidates = await _projectCandidateAppService.GetAllProjectsAsync((Guid)poolId);

                var pool = await _poolAppService.GetAsync(poolId.Value);
                PoolId = poolId.Value;
                PoolName = pool.Name;

                foreach (var candidate in ProjectCandidates)
                {
                    double? totalScore = candidate.SafetyScore +
                                        candidate.MobilityAndEconomyScore +
                                        candidate.EquityAndAccessScore +
                                        candidate.ResilienceAndEnvironmentScore +
                                        candidate.ConditionAndPerformanceScore;

                    if (!totalScore.HasValue || totalScore.Value == 0)
                    {
                        // An all-manual/no-score candidate has nothing to divide into — 0/0 would
                        // otherwise produce NaN/Infinity, which System.Text.Json can't serialize.
                        candidate.SafetyScorePercentage = 0;
                        candidate.MobilityAndEconomyScorePercentage = 0;
                        candidate.EquityAndAccessScorePercentage = 0;
                        candidate.ResilienceAndEnvironmentScorePercentage = 0;
                        candidate.ConditionAndPerformanceScorePercentage = 0;
                    }
                    else
                    {
                        candidate.SafetyScorePercentage = (candidate.SafetyScore / totalScore) * 100;
                        candidate.MobilityAndEconomyScorePercentage = (candidate.MobilityAndEconomyScore / totalScore) * 100;
                        candidate.EquityAndAccessScorePercentage = (candidate.EquityAndAccessScore / totalScore) * 100;
                        candidate.ResilienceAndEnvironmentScorePercentage = (candidate.ResilienceAndEnvironmentScore / totalScore) * 100;
                        candidate.ConditionAndPerformanceScorePercentage = (candidate.ConditionAndPerformanceScore / totalScore) * 100;
                    }
                }

                json = System.Text.Json.JsonSerializer.Serialize(ProjectCandidates, new JsonSerializerOptions
                {
                    WriteIndented = true 
                });
            }

            Scenarios = await _scenarioAppService.GetAllAsync("");
            Pools = await _poolAppService.GetAllAsync("", "");
        }
    }
}
