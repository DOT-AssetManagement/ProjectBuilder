using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.Web.MVC.Models;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class OutputsController : Controller
    {
        private readonly ILogger<OutputsController> _logger;
        IFilterUnitOfWork _filterunitofwork;
        IRunScenarioUnitOfWork _runScenarioUnitOfWork;
        HttpClient _httpClient;

        private readonly IRepository<CountyModel> _counties;
        public OutputsController(ILogger<OutputsController> logger, IFilterUnitOfWork filterunitofwork, IRunScenarioUnitOfWork runScenarioUnitOfWork, IHttpClientFactory httpClientFactory, IRepository<CountyModel> counties)
        {
            _logger = logger;
            _filterunitofwork = filterunitofwork;
            _runScenarioUnitOfWork = runScenarioUnitOfWork;
            _httpClient = httpClientFactory.CreateClient();
            _counties = counties;
        }

        public async Task<IActionResult> Maps(int? scenid)
        {
            var role = HttpContext.Session.GetString("RoleName");
            if (User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {

                string scenarioName = "";
            if (scenid.HasValue)
            {
                var scenario = await _filterunitofwork.ScenarioRepo.FindAsync(scenid);
                scenarioName = scenario is null ? "" : scenario.ScenarioName;
            }
                var counties = await _counties.GetAllAsync();
                var distinctDistricts = counties.Select(c => c.District).Distinct().OrderBy(d => d);

                return View("~/Views/Outputs/Maps.cshtml", new { ScenarioName = scenarioName, ScenarioId = scenid, Districts = distinctDistricts });
            }
            else
            {
                return RedirectToAction("Index", "Scenarios");
            }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateMapsResponse(MapsViewModel mapsViewModel)
        {
            if (!mapsViewModel.SelectedScenario.HasValue)
                return BadRequest("you must select a scenario before loading the map.");
            var mapresult = await _runScenarioUnitOfWork.ExportScenarioResultsToJson(mapsViewModel.SelectedScenario!.Value, false, mapsViewModel.SelectedDistrict, mapsViewModel.SeletedCounty, mapsViewModel.SelectedRoute, null, mapsViewModel.TreatementType);
            if (mapresult.HasError)
                return BadRequest(mapresult.Error);
            var result = await SendMapRequest(mapresult.Result);
            return result.HasError ? BadRequest(result.Error) : Ok(result.Result);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMapJson(MapsViewModel mapsViewModel)
        {
            if (!mapsViewModel.SelectedScenario.HasValue)
                return BadRequest("you must select a scenario before loading the map.");
            var mapresult = await _runScenarioUnitOfWork.ExportScenarioResultsToJson(mapsViewModel.SelectedScenario!.Value, false, mapsViewModel.SelectedDistrict, mapsViewModel.SeletedCounty, mapsViewModel.SelectedRoute, null, mapsViewModel.TreatementType);
            if (mapresult.HasError)
                return BadRequest(mapresult.Error);

            var json = JsonSerializer.Serialize(mapresult.Result, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Prevent escaping
            });

            return new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        private async Task<MapsResultModel> SendMapRequest(string data)
        {
            try
            {        
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                using var response = await httpClient.PostAsync("https://gis.bergmannpc.com/ProjectBuilderMap/api/scenarios", content);
                var mapresult = await response.Content.ReadAsStringAsync();

                return new MapsResultModel("", mapresult, false);
            }
            catch (HttpRequestException)
            {
                return new MapsResultModel("Conncetion timed out, the server didn't respond please try again later.", "", true);
            }
        }
    }
}