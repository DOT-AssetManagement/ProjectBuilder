using Azure.Core.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using NuGet.LibraryModel;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace ProjectBuilder.Web.MVC.Controllers
{
    
    public class HomeController : LibraryBaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISession _session;

        public HomeController(ILogger<HomeController> logger,
        IUserRepository userRepository,
        IRepository<UserRoleModel> userRoleRepository,
        IHttpContextAccessor httpContextAccessor,
        IFilterUnitOfWork filterUnitOfWork) : base(userRepository, filterUnitOfWork, userRoleRepository) 
        {
            _logger = logger;
            _session = httpContextAccessor.HttpContext.Session;
        }

        public async Task<IActionResult> Index(string id)
        
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await GetOrCreateUser();
                var userRole = await GetUserRole(user.EntityId);
                if(userRole == default)
                {
                    throw new Exception("No Role is Assigned to User Yet!");
                }
                var role = await GetRole(userRole.RoleId);
                if(role == default)
                {
                    throw new Exception("No Role is Found against RoleId: " + userRole.RoleId);
                }
                HttpContext.Session.SetString("RoleName", role.Name);
                if (role.Name == "Admin")
                    return View("AdminIndex");
                if (role.Name == "Guest")
                    return View("GuestIndex", "CandidatePools");
                if (role.Name == "Operator")
                    return RedirectToAction("Index", "DashBoard");
            }
            return View();
        }
        public IActionResult GuestIndex()
        {
            return View();
        }
        public IActionResult AdminIndex()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}