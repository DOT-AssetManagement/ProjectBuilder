using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.ImportSessions;
using ProjectBuilder.Models.ImportSessions.Dtos;
using System.Collections.Generic;

namespace ProjectBuilder.Web.Pages.SetUp.ImportSessions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IImportSessionsAppService _service;

        public IndexModel(IImportSessionsAppService service)
        {
            _service = service;
        }

        public List<ImportSessionDto> ImportSessions { get; set; } = new();

        public void OnGet()
        {
            ImportSessions = _service.GetAllAsync();
        }
    }
}
