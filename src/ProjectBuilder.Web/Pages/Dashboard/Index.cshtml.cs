using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.DataAccess;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectBuilderDbContext _context;

        public string DashboardMapUrl { get; set; } = "/Scenarios";

        public IndexModel(
            IConfiguration configuration,
            ProjectBuilderDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var mapUrl = Url.Page("/Scenarios/Index") ?? "/Scenarios";
            var email = GetCurrentUserEmail();

            if (!string.IsNullOrWhiteSpace(email))
            {
                //var latestScenarioId = await _context.Scenarios
                //    .AsNoTracking()
                //    .Where(s => s.CreatedBy == email || s.IsShared)
                //    .OrderByDescending(s => s.CreatedAt)
                //    .Select(s => (int?)s.EntityId)
                //    .FirstOrDefaultAsync();
                var latestScenarioId = await _context.Scenarios
                   .AsNoTracking()
                   .Where(s => s.CreatedBy == email)
                   .OrderByDescending(s => s.CreatedAt)
                   .Select(s => (int?)s.Id)
                   .FirstOrDefaultAsync();

                if (latestScenarioId.HasValue)
                {
                    mapUrl = Url.Page("/GisMap/Index", new { scenid = latestScenarioId.Value }) ?? mapUrl;
                }
            }

            DashboardMapUrl = mapUrl;
            return Page();
        }

        private string? GetCurrentUserEmail()
        {
            var securityType = _configuration["SecurityType"];

            if (string.Equals(securityType, "ESEC", StringComparison.OrdinalIgnoreCase))
            {
                return User.FindFirstValue(ClaimTypes.Email);
            }

            return User.FindFirstValue(ClaimTypes.Email);
        }
    }
}