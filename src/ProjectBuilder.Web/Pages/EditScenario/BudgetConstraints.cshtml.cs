using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.ScenarioBudgetConstraint.Dtos;
using ProjectBuilder.Web.Services;
using Volo.Abp;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.EditScenario
{
    public class BudgetConstraintsModel : EditScenarioPageModelBase
    {
        public BudgetConstraintsModel(
            IScenarioAppService scenarioAppService,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService)
            : base(scenarioAppService, currentUser, tourGuideSeenService)
        {
        }

        public override string ActiveMenu => "BudgetConstraints";

        public BudgetConstraintsGridDto Grid { get; set; } = new();
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task OnGetAsync(int scenid)
        {
            await LoadScenarioAsync(scenid);
            Grid = await ScenarioAppService.GetBudgetConstraintsAsync(scenid);
            ErrorMessage = TempData["Error"] as string;
            SuccessMessage = TempData["Success"] as string;
        }

        public async Task<IActionResult> OnPostUpdateAsync([FromBody] BudgetBulkUpdateInput input)
        {
            try
            {
                await ScenarioAppService.UpdateBudgetConstraintsAsync(input.ScenId, input.Cells ?? new List<BudgetCellDto>());
                return new JsonResult(new { success = true });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostCreateCriteriaAsync([FromBody] CriteriaCreateInput input)
        {
            try
            {
                var columnId = await ScenarioAppService.CreateBudgetCriteriaAsync(input.ScenarioId, input.ColumnName, input.Criteria);
                return new JsonResult(new { success = true, columnId });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostUpdateCriteriaAsync([FromBody] CriteriaUpdateInput input)
        {
            try
            {
                await ScenarioAppService.UpdateBudgetCriteriaAsync(input.ColumnId, input.ScenarioId, input.ColumnName, input.Criteria);
                return new JsonResult(new { success = true });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteCriteriaAsync(int id)
        {
            try
            {
                await ScenarioAppService.DeleteBudgetCriteriaAsync(id);
                return new JsonResult(new { success = true });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteRowAsync(int scenId, int yearWork, int rowNumber)
        {
            try
            {
                await ScenarioAppService.DeleteBudgetRowAsync(scenId, yearWork, rowNumber);
                return new JsonResult(new { success = true });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostAddRowAsync(int scenId, int year)
        {
            if (year < 1000 || year > 9999)
                return new JsonResult(new { success = false, message = "Year must be a 4-digit number (for example 2027)." });

            try
            {
                await ScenarioAppService.AddBudgetRowAsync(scenId, year);
                return new JsonResult(new { success = true });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostClearAsync(int scenid)
        {
            try
            {
                await ScenarioAppService.ClearBudgetConstraintsAsync(scenid);
                return new JsonResult(new { success = true });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetExportAsync(int scenid)
        {
            var (content, fileName) = await ScenarioAppService.ExportBudgetConstraintsAsync(scenid);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> OnGetTemplateAsync(int scenid)
        {
            var (content, fileName) = await ScenarioAppService.DownloadBudgetTemplateAsync(scenid);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> OnPostImportAsync(int scenid, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid .xlsx file.";
                return RedirectToPage(new { scenid });
            }

            try
            {
                using var stream = new System.IO.MemoryStream();
                await file.CopyToAsync(stream);
                await ScenarioAppService.ImportBudgetConstraintsAsync(scenid, stream.ToArray());
                TempData["Success"] = "Budget constraints imported successfully!";
            }
            catch (UserFriendlyException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { scenid });
        }

        public class BudgetBulkUpdateInput
        {
            public int ScenId { get; set; }
            public List<BudgetCellDto> Cells { get; set; } = new();
        }

        public class CriteriaCreateInput
        {
            public int ScenarioId { get; set; }
            public string ColumnName { get; set; }
            public string Criteria { get; set; }
        }

        public class CriteriaUpdateInput
        {
            public int ColumnId { get; set; }
            public int ScenarioId { get; set; }
            public string ColumnName { get; set; }
            public string Criteria { get; set; }
        }
    }
}
