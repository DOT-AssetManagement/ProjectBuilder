using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using System.Security.Claims;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class LibraryBaseController : Controller
    {
        private readonly IFilterUnitOfWork _filterUnitOfWork;
        private readonly IUserRepository _users;
        private readonly IRepository<UserRoleModel> _userRoleRepository;
        public LibraryBaseController(IUserRepository userRepository, IFilterUnitOfWork filterUnitOfWork, IRepository<UserRoleModel> userRoleRepository)
        {
            _filterUnitOfWork = filterUnitOfWork;
            _users = userRepository;
            _userRoleRepository = userRoleRepository;
        }
        public IFilterUnitOfWork FilterUnitOfWork { get => _filterUnitOfWork; }
        public async Task<UserModel> GetOrCreateUser()
        {
            var user = await _users.GetByEmailAsync(User.FindFirstValue("emails"));
            if(user is null)
            {
                user = User.ToUserModel();
                user = await _users.InsertAndSave(user);    
                var guestRole = await _users.GetByNameAsync("Guest");
                var roleId = guestRole.EntityId;
                var userRole = new UserRoleModel { RoleId = roleId, UserId = user.EntityId };
                await _userRoleRepository.InsertAndSave(userRole);
            }
            return user;
        }

        public async Task<UserRoleModel> GetUserRole(long userId)
        {
            var userRole = await _users.GetUserRoleAsync(userId);
            return userRole;
        }
        public async Task<RoleModel> GetRole(int roleId)
        {
            var roles = await _users.GetRoleAsync(roleId);
            return roles;
        }
        public async Task<List<CandidatePoolModel>> LoadCandidatePoolsForUser()
        {
            var user = await GetOrCreateUser();
            return await GetCurrentUserCandidatePools(user.EntityId);           
        }
        private async Task<List<CandidatePoolModel>> GetCurrentUserCandidatePools(long userId)
        {
            _filterUnitOfWork.CandidatePoolRepo.ApplyFilter(new Dictionary<string, object> { { "UserId", userId }, { "IsActive", true } });
            var droplist = await _filterUnitOfWork.CandidatePoolRepo.GetAllAsync();
            return droplist;
        }
    }
}
