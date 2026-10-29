using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.BridgeReferenceData;
using ProjectBuilder.Models.JqueryDatatableParam;
using System;
using System.Linq;

namespace ProjectBuilder.Web.Pages.SetUp.BridgeReferenceData
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IBridgeReferenceDataAppService _service;

        public IndexModel(IBridgeReferenceDataAppService service)
        {
            _service = service;
        }

        public void OnGet()
        {
            // Table rows are fetched page-by-page by the client via OnGetTable —
            // this table has 50k+ rows, so nothing is preloaded here.
        }

        // Backs the Tabulator remote-pagination request (see Index.cshtml).
        public JsonResult OnGetTable(int page = 1, int size = 25, string search = "", string sortField = "", string sortDir = "")
        {
            var param = new JqueryDatatableParamDto
            {
                iDisplayStart = (page - 1) * size,
                iDisplayLength = size,
                sSearch = search,
                SortField = sortField,
                SortDir = sortDir
            };

            var result = _service.GetPagedBridgeReferenceData(param);

            var rows = result.AaData.Select(t => new
            {
                id = t.Id,
                bridgeId = t.BridgeId,
                district = t.District,
                countyCode = t.CountyCode,
                route = t.Route,
                segment = t.Segment,
                offSet = t.OffSet,
                createdBy = t.CreatedBy,
                createdAt = t.CreatedAt
            });

            var lastPage = Math.Max(1, (int)Math.Ceiling(result.ITotalRecords / (double)size));

            return new JsonResult(new { data = rows, last_page = lastPage });
        }
    }
}
