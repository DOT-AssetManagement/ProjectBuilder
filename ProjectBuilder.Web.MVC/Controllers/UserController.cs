using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers.Administration
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IFilterUnitOfWork _filter;
        private readonly IRepository<RoleModel> _repository;
        private readonly IRepository<UserRoleModel> _userRoleRepository;

        public UserController(IUserRepository userRepository, IFilterUnitOfWork filter, IRepository<RoleModel> repository, IRepository<UserRoleModel> userRoleRepository)
        {
            _userRepository = userRepository;
            _filter = filter;
            _repository = repository;
            _userRoleRepository = userRoleRepository;
        }
        public async Task<IActionResult> Index()
        {
            _filter.UserRepo.ApplyFilter(new Dictionary<string, object> { { "IsActive", true } });
            var users = await _userRepository.GetAllAsync();
            var roles = await _repository.GetAllAsync();
            var userRoles = await _userRoleRepository.GetAllAsync();
            var userViewModel = new AdminUsersViewModel();
            foreach (var user in users)
            {
                var userRole = userRoles.FirstOrDefault(x => x.UserId == user.EntityId);
                if (userRole != default)
                {
                    var role = roles.FirstOrDefault(x => x.EntityId == userRole.RoleId);
                    user.RoleName = role.Name;
                }
            }
            userViewModel.Users = users;

            return View(userViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> GetUser(long id)
        {
            var user = await _userRepository.FindAsync(id);
            var userRoles = await _userRoleRepository.GetAllAsync();
            var roles = await _repository.GetAllAsync();
            var userViewModel = new AdminUsersViewModel
            {
                Name = user.Name,
                EntityId = user.EntityId,
                IsActive = user.IsActive,
                Email = user.Email,
                B2CUserId = user.B2CUserId,
            };
            var userRole = userRoles.FirstOrDefault(x => x.UserId == user.EntityId);
            if (userRole != default)
            {
                var role = roles.FirstOrDefault(x => x.EntityId == userRole.RoleId);
                userViewModel.RoleName = role.Name;
            }
            return Json(userViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _repository.GetAllAsync();
            return Json(roles);
        }
        public async Task<IActionResult> DeleteUser(long userId)
        {
            await _userRepository.UpdateAsync(userId, new Dictionary<string, object> { { "IsActive", false } });
            await _userRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(AdminUsersViewModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a User" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.Name), model.Name }
            };
            await _userRepository.UpdateAsync(model.EntityId, pairs);
            await _userRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(AdminUsersViewModel model)
        {
            if (string.IsNullOrEmpty(model.UpdatedRole))
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a Role" });
            var role = await _userRepository.GetRoleNameAsync(model.UpdatedRole);
            await _userRepository.UpdateUserRoleAsync(model.EntityId, role.EntityId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateIsMapActive(int userId, bool isActive)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.EntityId == userId); 

            if (user == null)
            {
                return NotFound();
            }

            user.IsMapActive = isActive;
            await _userRepository.UpdateAsync(user);

            return Ok(new { success = true, message = "IsMapActive updated successfully!" });
        }

    }
}