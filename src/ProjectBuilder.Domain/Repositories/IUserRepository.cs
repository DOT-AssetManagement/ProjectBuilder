using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IUserRepository : IRepository<UserEntity, long>
    {
        Task<UserEntity> GetByEmailAsync(string email);

        Task<UserEntity> GetUserObjectIdAsync(string objectId);

        Task<RoleModel> GetByNameAsync(string name);

        Task<UserRoleModel> GetUserRoleAsync(long userId);

        Task<RoleModel> GetRoleAsync(int roleId);

        Task<RoleModel> GetRoleNameAsync(string roleName);

        Task<bool> UpdateUserRoleAsync(long userId, int roleId);

        Task<bool> UpdateAsync(UserModel userModel);

        Task<bool> EsesUserExists(string email);

        Task AddEsecUser(UserModel userModel);

        UserModel GetUserByEmailAsync(string email);   

        Task<RoleEntity> GetRoleByName(string roleName);
        Task<RoleModel> GetRoleByUserIdAsync(long userId);
        Task<UserEntity> GetByIdAsync(long userId);
    }
}