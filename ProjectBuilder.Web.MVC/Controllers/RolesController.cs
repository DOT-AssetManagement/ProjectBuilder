using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class RolesController : Controller
    {
        private readonly ProjectBuilderDbContext _context;
        private readonly IRepository<RoleModel> _repository;
        private readonly IRepository<UserRoleModel> _userRoleRepository;

        public RolesController(ProjectBuilderDbContext context, IRepository<RoleModel> repository, IRepository<UserRoleModel> userRoleRepository)
        {
            _context = context;
            _repository = repository;
            _userRoleRepository = userRoleRepository;
        }
        public async Task<IActionResult> Index()
        {
            var roles = await _repository.GetAllAsync();
            var userRoles = await _userRoleRepository.GetAllAsync();
            var roleViewModel = new AdminRoleViewModel();
            var roleList = new List<RoleModel>();
            foreach (var role in roles)
            {
                RoleModel admin = new RoleModel();
                
                admin.EntityId = role.EntityId;
                admin.Name = role.Name;
                {
                    var userRole = userRoles.Any(x => x.RoleId == admin.EntityId);
                    admin.IsDeleteable = userRole ? true : admin.IsDeleteable;
                }
                roleList.Add(admin);
            }
            roleViewModel.Roles = roleList;
            return View(roleViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _repository.FindAsync(id);
            var roleViewModel = new AdminRoleViewModel
            {
                Name = role.Name,
                RoleId = role.EntityId,
            };
            return Json(roleViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(AdminRoleViewModel model)
        {
            var roles = new RoleModel
            {
                Name = model.Name
            };
            _repository.InsertAndSave(roles);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var role = await _repository.DeleteAsync(roleId);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(AdminRoleViewModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a Role" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.Name), model.Name }
            };
            await _repository.UpdateAsync(model.RoleId, pairs);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
