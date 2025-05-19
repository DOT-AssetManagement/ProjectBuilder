using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface IUserRepository : IRepository<UserModel>
    {
        Task<UserModel> GetByEmailAsync(string email);
        Task<UserModel> GetUserObjectIdAsync(string objectId);
        Task<RoleModel> GetByNameAsync(string name);
        Task<UserRoleModel> GetUserRoleAsync(long userId);
        Task<RoleModel> GetRoleAsync(int roleId);
        Task<RoleModel> GetRoleNameAsync(string roleName);
        Task<bool> UpdateUserRoleAsync(long userId, int roleId);

        Task<bool> UpdateAsync(UserModel userModel);
    }
}
