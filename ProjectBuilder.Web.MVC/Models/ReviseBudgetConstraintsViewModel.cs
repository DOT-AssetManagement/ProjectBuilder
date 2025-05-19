using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
namespace ProjectBuilder.Web.MVC.Models
{
    public class ReviseBudgetConstraintsViewModel
    {
        public int? YearOfWork { get; set; }
        public int? District { get; set; }
        public int? BridgeInterstate { get; set; }
        public int? BridgeNonInterstate { get; set; }
        public int? PavementInterstate { get; set; }
        public int? PavementNonInterstate { get; set; }

    }
}
