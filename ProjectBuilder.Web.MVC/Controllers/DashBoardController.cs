using Microsoft.AspNetCore.Mvc;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class DashBoardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
