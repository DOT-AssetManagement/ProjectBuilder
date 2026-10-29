using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.OutPut;
using ProjectBuilder.Models.OutPut.Dtos;
using ProjectBuilder.Models.Pool;
using ProjectBuilder.Models.Pool.Dtos;
using ProjectBuilder.Models.ProjectCandidate;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;

namespace ProjectBuilder.Web.Pages.Outputs
{
    [Authorize]
    public class MapsModel : PageModel
    {
        private readonly IOutPutAppService _outPutAppService;
        private readonly IScenarioAppService _scenarioAppService;
        private readonly IPoolAppService _poolAppService;
        private readonly IProjectCandidateAppService _projectCandidateAppService;
        private readonly IDssScenarioRepository _dssScenarioRepository;

        public string MapViewerUrl { get; set; }
        public string ErrorMessage { get; set; }
        public List<ScenarioDto> Scenarios { get; set; }
        public List<CandidatePoolDto> Pools { get; set; }
        public string PoolName { get; set; }
        public string ScenarioName { get; set; }
        public Guid PoolId { get; set; }

        public MapsModel(
            IOutPutAppService outPutAppService,
            IScenarioAppService scenarioAppService,
            IPoolAppService poolAppService,
            IProjectCandidateAppService projectCandidateAppService,
            IDssScenarioRepository dssScenarioRepository)
        {
            _outPutAppService = outPutAppService;
            _scenarioAppService = scenarioAppService;
            _poolAppService = poolAppService;
            _projectCandidateAppService = projectCandidateAppService;
            _dssScenarioRepository = dssScenarioRepository;
        }
        public async Task OnGet(Guid? poolId)
        {
            if (poolId != null)
            {
                // + Candidates are a DSS-legacy concept: the scenario tied to a pool lives in
                // tbl_dss_Scenarios (created alongside the pool), not tbl_pb_Scenarios — so the map
                // data must come from the DSS GIS pipeline (GetGisJsonStringOutput), not the PB
                // ExportScenarioResultsToJson pipeline used by /Scenarios.
                var dssScenario = await _dssScenarioRepository.GetByPoolIdAsync((Guid)poolId);
                if (dssScenario != null)
                {
                    ScenarioName = dssScenario.ScenName;

                    try
                    {
                        var gisJson = await _projectCandidateAppService.GetGisJsonStringOutput(dssScenario.Id, 0);
                        if (string.IsNullOrWhiteSpace(gisJson))
                        {
                            ErrorMessage = "No map data is available for this candidate + yet. Run it from the Candidates + page first.";
                        }
                        else
                        {
                            var mapsResult = await _outPutAppService.SendMapRequest(gisJson);
                            if (mapsResult.HasError)
                            {
                                ErrorMessage = mapsResult.Error;
                            }
                            else
                            {
                                var parsed = JObject.Parse(mapsResult.Result);
                                MapViewerUrl = parsed.Value<string>("MapViewerUrl");
                                if (string.IsNullOrWhiteSpace(MapViewerUrl))
                                {
                                    ErrorMessage = "The map service did not return a viewer URL for this scenario.";
                                }
                            }
                        }
                    }
                    catch (UserFriendlyException ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                    catch (Exception)
                    {
                        ErrorMessage = "Unable to load the map for this scenario. Please try again later.";
                    }
                }
                else
                {
                    ErrorMessage = "This candidate + has no scenario yet. Run it from the Candidates + page first.";
                }

                var pool = await _poolAppService.GetAsync(poolId.Value);
                PoolId = poolId.Value;
                PoolName = pool.Name;
            }

            Scenarios = await _scenarioAppService.GetAllAsync("");
            Pools = await _poolAppService.GetAllAsync("", "");
        }

    }
}
