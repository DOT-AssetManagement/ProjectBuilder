using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class UserRepository : ProjectBuilderRepository<UserEntity, UserModel, long>, IUserRepository
    {     
        public UserRepository(ProjectBuilderDbContext projectBuilderDbContext, IMapper mapper, ILogger<ProjectBuilderRepository<UserEntity, UserModel, long>> logger) :
                             base(projectBuilderDbContext, mapper, logger)
        {
        }
        protected override IQueryable<UserEntity> InitializeCurrrentQuery(Expression<Func<UserEntity, bool>> filter = null)
        {
            if (filter is null)
                filter = PredicateBuilder.New<UserEntity>(true);
            return ProjectBuilderDbContext.Users
                                          .Include(x => x.UserClaims)
                                          .Include(x => x.UserRoles)
                                          .Where(filter)
                                          .OrderBy(e => e.EntityId);
        }
        public async Task<UserModel> GetByEmailAsync(string email)
        {
            return await CurrentQuery.AsNoTracking()
                                     .ProjectTo<UserModel>(Mapper.ConfigurationProvider)
                                     .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task<UserModel> GetUserObjectIdAsync(string objectId)
        {
           if(!Guid.TryParse(objectId, out var userGuid))
                return null;
            return await CurrentQuery.AsNoTracking()
                           .ProjectTo<UserModel>(Mapper.ConfigurationProvider)
                           .FirstOrDefaultAsync(x => x.B2CUserId == userGuid);
        }

        public async Task<RoleModel> GetByNameAsync(string name)
        {
            var roleEntity = await ProjectBuilderDbContext.Roles.FirstOrDefaultAsync(x => x.Name == name);

            var roleModel = new RoleModel
            {
                EntityId = roleEntity.EntityId,
                Name = roleEntity.Name,
            };

            return roleModel;
        }

         public async Task<UserRoleModel> GetUserRoleAsync(long userId)
        {
            var userRole = await ProjectBuilderDbContext.UserRoles.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userRole == null) return null;

            var role = await ProjectBuilderDbContext.Roles.FirstOrDefaultAsync(r => r.EntityId == userRole.RoleId);
            if (role == null) return null;

            return new UserRoleModel
            {
                UserId = userRole.UserId,
                RoleId = userRole.RoleId,
            };
        }

        public async Task<RoleModel> GetRoleAsync(int roleId)
        {
            var role = await ProjectBuilderDbContext.Roles.FirstOrDefaultAsync(r => r.EntityId == roleId);
            if (role == null) return null;

            return new RoleModel
            {
                EntityId = role.EntityId,
                Name = role.Name,
            };
        }

        public async Task<RoleModel> GetRoleNameAsync(string roleName)
        {
            var role = await ProjectBuilderDbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return null;

            return new RoleModel
            {
                EntityId = role.EntityId,
                Name = role.Name,
            };

        }

        public async Task<bool> UpdateUserRoleAsync(long userId, int roleId)
        {
            var userRole = await ProjectBuilderDbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId);
            var user = await ProjectBuilderDbContext.Users.FindAsync(userId);

            if (user.IsActive != true)
            {
                user.IsActive = true;
                ProjectBuilderDbContext.Users.Update(user);
                ProjectBuilderDbContext.SaveChanges();
            }

            if (userRole == null)
            {
                var usermodel = new UserRoleEntity(roleId)
                {
                    UserId = userId,
                    RoleId = roleId
                };
                await ProjectBuilderDbContext.UserRoles.AddAsync(usermodel);
                await ProjectBuilderDbContext.SaveChangesAsync();
                return true;
            }

            userRole.RoleId = roleId;
            ProjectBuilderDbContext.UserRoles.Update(userRole);
            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(UserModel userModel)
        {
            if (userModel == null)
            {
                throw new ArgumentNullException(nameof(userModel));
            }

            var existingUser = await ProjectBuilderDbContext.Users.FindAsync(userModel.EntityId);

            if (existingUser != null)
            {
                // Manually map properties from UserModel to UserEntity
                existingUser.Name = userModel.Name;
                existingUser.Email = userModel.Email;
                existingUser.IsActive = userModel.IsActive;
                existingUser.IsMapActive = userModel.IsMapActive;

                ProjectBuilderDbContext.Users.Update(existingUser);
            }
            else
            {
                // Convert UserModel to UserEntity before adding
                var userEntity = new UserEntity
                {
                    EntityId = userModel.EntityId,
                    Name = userModel.Name,
                    Email = userModel.Email,
                    IsActive = userModel.IsActive,
                    IsMapActive = userModel.IsMapActive
                };

                await ProjectBuilderDbContext.Users.AddAsync(userEntity);
            }

            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }

    }
}
