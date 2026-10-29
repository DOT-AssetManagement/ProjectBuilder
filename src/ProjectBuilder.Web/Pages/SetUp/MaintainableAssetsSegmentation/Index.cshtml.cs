using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PAMSSectionSegment;
using System;
using System.Linq;

namespace ProjectBuilder.Web.Pages.SetUp.MaintainableAssetsSegmentation
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPAMSSectionSegmentAppService _service;

        public IndexModel(IPAMSSectionSegmentAppService service)
        {
            _service = service;
        }

        public void OnGet()
        {
            // Table rows are fetched page-by-page by the client via OnGetTable —
            // this table has 700k+ rows, so nothing is preloaded here.
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

            var result = _service.GetPagedMaintainableAssetsSegmentation(param);

            var rows = result.AaData.Select(t => new
            {
                district = t.District,
                countyId = t.CountyId,
                route = t.Route,
                asset = t.Asset,
                direction = t.Direction ? 1 : 0,
                fromSection = t.FromSection,
                toSection = t.ToSection,
                area = t.Area,
                interstate = t.Interstate ? 1 : 0,
                lanes = t.Lanes,
                width = t.Width,
                length = t.Length,
                surfaceName = t.SurfaceName,
                risk = t.Risk
            });

            var lastPage = Math.Max(1, (int)Math.Ceiling(result.ITotalRecords / (double)size));

            return new JsonResult(new { data = rows, last_page = lastPage });
        }
    }
}
