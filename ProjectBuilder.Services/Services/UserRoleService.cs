using log4net.Filter;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Services.Services
{
    public class UserRoleService
    {
        public IRepository<UserRoleModel> Repository { get; set; }
        //private readonly IRepository<UserRoleModel> _userRoleRepository;
        //public UserRoleService(IRepository<UserRoleModel> userRoleRepository)
        //{
        //    _userRoleRepository = userRoleRepository;
        //}
        public async Task<bool> CheckRole(int RoleId)
        {

            var userRoles =  await Repository.GetAllAsync();
            var userRole = userRoles.Any(x => x.RoleId == RoleId);
            return true;
        }
    }
}
