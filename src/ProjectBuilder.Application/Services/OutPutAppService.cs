using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Models;
using ProjectBuilder.Models.OutPut;
using ProjectBuilder.Models.OutPut.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    [Authorize(ReportsPermissions.Reports.Default)]
    public class OutPutAppService : ApplicationService , IOutPutAppService
    {
        private readonly IRunScenarioRepository _runScenarioRepository;

        public OutPutAppService(IRunScenarioRepository runScenarioRepository)
        {
            _runScenarioRepository = runScenarioRepository;
        }
        public async Task<MapsResultDto> GenerateMapsResponse(MapsDto mapsViewModel)
        {
            if (!mapsViewModel.SelectedScenario.HasValue)
            {
                throw new UserFriendlyException("You must select a scenario before loading the map.");
            }

            var mapResult = await _runScenarioRepository.ExportScenarioResultsToJsonAsync(
                mapsViewModel.SelectedScenario.Value,
                false,
                mapsViewModel.SelectedDistrict,
                mapsViewModel.SeletedCounty,
                mapsViewModel.SelectedRoute,
                null,
                mapsViewModel.TreatementType
            );

            if (mapResult.HasError)
            {
                throw new UserFriendlyException(mapResult.Error);
            }

            var result = await SendMapRequest(mapResult.Result);

            if (result.HasError)
            {
                throw new UserFriendlyException(result.Error);
            }

            return result;
        }
        public virtual async Task<MapsResultDto> SendMapRequest(string data)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                using var response = await httpClient.PostAsync("https://gis.bergmannpc.com/ProjectBuilderMap/api/scenarios", content);
                var mapresult = await response.Content.ReadAsStringAsync();

                return new MapsResultDto("", mapresult, false);
            }
            catch (HttpRequestException)
            {
                return new MapsResultDto("Conncetion timed out, the server didn't respond please try again later.", "", true);
            }
        }
    }
}
