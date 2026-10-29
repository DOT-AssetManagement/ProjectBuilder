using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Helpers.Logger;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.ScenarioParamater.Dtos;
using ProjectBuilder.Web.Services;
using Volo.Abp;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.EditScenario
{
    public class ParametersModel : EditScenarioPageModelBase
    {
        private readonly IAppLogger<ParametersModel> _logger;

        public ParametersModel(
            IScenarioAppService scenarioAppService,
            IAppLogger<ParametersModel> logger,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService)
            : base(scenarioAppService, currentUser, tourGuideSeenService)
        {
            _logger = logger;
        }

        public override string ActiveMenu => "Parameters";

        public List<ScenarioParameterDto> Parameters { get; set; } = new();
        public List<TreatmentShiftDto> TreatmentShifts { get; set; } = new();
        public bool Saved { get; set; }
        public string ErrorMessage { get; set; }
        public string ActiveTab { get; set; }

        public async Task OnGetAsync(int scenid, string activeTab = null)
        {
            // Read the one-time "saved" flag from TempData (set on the redirect after a
            // successful POST) instead of a `saved=true` query string flag — a query string
            // flag stays in the URL, so refreshing the page kept re-showing the banner forever.
            // TempData is cleared once read, so a refresh of the same URL won't show it again.
            // activeTab stays in the query string since it's real page state (which tab is
            // open), not a one-time flash message.
            Saved = TempData["ParametersSaved"] != null;
            ActiveTab = activeTab;
            await LoadAsync(scenid);
        }

        public async Task<IActionResult> OnPostAsync(int scenid, string activeTab = null)
        {
            var values = ParseParameterValues(Request.Form);
            var shifts = ParseTreatmentShifts(Request.Form);

            try
            {
                await ScenarioAppService.SaveParametersAsync(scenid, new ProjectBuilder.Models.ScenarioParamater.Dtos.SaveParametersInputDto { ParameterValues = values, Shifts = shifts });
                TempData["ParametersSaved"] = true;
                return RedirectToPage(new { scenid, activeTab });
            }
            catch (UserFriendlyException ex)
            {
                ErrorMessage = ex.Message;
                ActiveTab = activeTab;
                await LoadAsync(scenid);
                return Page();
            }
        }

        private async Task LoadAsync(int scenid)
        {
            var sw = Stopwatch.StartNew();

            await LoadScenarioAsync(scenid);
            _logger.Info("[PARAMETERS] LoadScenarioAsync took {Elapsed}ms | scenid: {ScenId}", sw.ElapsedMilliseconds, scenid);

            sw.Restart();
            Parameters = await ScenarioAppService.GetScenarioParametersAsync(scenid);
            _logger.Info("[PARAMETERS] GetScenarioParametersAsync took {Elapsed}ms | scenid: {ScenId} | count: {Count}", sw.ElapsedMilliseconds, scenid, Parameters.Count);

            sw.Restart();
            TreatmentShifts = await ScenarioAppService.GetTreatmentShiftsAsync(scenid);
            _logger.Info("[PARAMETERS] GetTreatmentShiftsAsync took {Elapsed}ms | scenid: {ScenId} | count: {Count}", sw.ElapsedMilliseconds, scenid, TreatmentShifts.Count);
        }

        // Project Logistics fields (excluding the YFST/YLST year dropdowns) are free-typed
        // numbers capped at 0-999 in the UI; clamp server-side too in case that's bypassed.
        private static readonly string[] LogisticsRangeCappedParameterIds =
            { "MXGM", "NXML", "MXGL", "NXMI", "MXGC", "NXSC", "HFTL", "HFTR" };

        private static Dictionary<string, double?> ParseParameterValues(IFormCollection form)
        {
            var values = new Dictionary<string, double?>();
            foreach (var key in form.Keys.Where(k => k.StartsWith("parameter[") && k.EndsWith("]")))
            {
                var id = key.Substring("parameter[".Length, key.Length - "parameter[".Length - 1);
                var raw = form[key].ToString();
                var value = double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (double?)null;

                if (value.HasValue && LogisticsRangeCappedParameterIds.Contains(id))
                {
                    value = Math.Clamp(value.Value, 0, 999);
                }

                values[id] = value;
            }
            return values;
        }

        private static List<TreatmentShiftDto> ParseTreatmentShifts(IFormCollection form)
        {
            var rows = new Dictionary<int, TreatmentShiftDto>();
            foreach (var key in form.Keys.Where(k => k.StartsWith("treatmentShift[")))
            {
                var start = key.IndexOf('[') + 1;
                var end = key.IndexOf(']');
                if (start <= 0 || end <= start) continue;
                if (!int.TryParse(key.Substring(start, end - start), out var index)) continue;

                var dot = key.IndexOf('.', end);
                if (dot < 0 || dot == key.Length - 1) continue;
                var prop = key.Substring(dot + 1);

                if (!rows.TryGetValue(index, out var row))
                {
                    row = new TreatmentShiftDto();
                    rows[index] = row;
                }

                var value = form[key].ToString();
                switch (prop)
                {
                    case nameof(TreatmentShiftDto.Treatment): row.Treatment = value?.Trim(); break;
                    case nameof(TreatmentShiftDto.MaxAccelerate): row.MaxAccelerate = int.TryParse(value, out var a) ? Math.Clamp(a, 0, 999) : 0; break;
                    case nameof(TreatmentShiftDto.MaxDelay): row.MaxDelay = int.TryParse(value, out var dl) ? Math.Clamp(dl, 0, 999) : 0; break;
                }
            }

            return rows.OrderBy(r => r.Key).Select(r => r.Value).Where(r => !string.IsNullOrWhiteSpace(r.Treatment)).ToList();
        }
    }
}
