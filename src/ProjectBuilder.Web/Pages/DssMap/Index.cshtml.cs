using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Repositories;

namespace ProjectBuilder.Web.Pages.DssMap
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IDssScenarioRepository _dssScenarioRepository;

        public IndexModel(IDssScenarioRepository dssScenarioRepository)
        {
            _dssScenarioRepository = dssScenarioRepository;
        }

        public Guid? ScenarioId { get; private set; }
        public Guid? PoolId { get; private set; }

        public async Task<IActionResult> OnGetAsync(Guid? scenid)
        {
            if (!scenid.HasValue || scenid.Value == Guid.Empty)
            {
                return RedirectToPage("/Pools/Index");
            }

            ScenarioId = scenid.Value;

            // Powers the "Back to Project" link — the DSS scenario's own PoolId, so the map
            // page can link back to the PC/DS projects report without any extra query params
            // at every place that links into this page (Outputs/Reports/Projects.cshtml, OutputsLayout.cshtml).
            var scenario = await _dssScenarioRepository.GetByIdAsync(scenid.Value);
            PoolId = scenario?.PoolId;

            return Page();
        }
    }
}
