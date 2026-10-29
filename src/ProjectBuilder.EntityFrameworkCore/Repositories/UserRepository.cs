using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Helpers.Logger;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Repositories
{

    public class UserRepository : EfCoreRepository<ProjectBuilderDbContext, UserEntity, long>, IUserRepository
    {
        private readonly IAppLogger<UserRepository> _logger;
        private readonly ProjectBuilderDbContext _context;
        private readonly IObjectMapper _objectMapper;
        private readonly IConfiguration _configuration;
        private readonly IdentityUserManager _identityUserManager;
        private readonly IdentityRoleManager _identityRoleManager;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentTenant _currentTenant;
        private readonly IDataFilter _dataFilter;

        public UserRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, IConfiguration configuration, ProjectBuilderDbContext dbContext, IObjectMapper objectMapper, IAppLogger<UserRepository> logger,
            IdentityUserManager identityUserManager, IdentityRoleManager identityRoleManager, IGuidGenerator guidGenerator, ICurrentTenant currentTenant, IDataFilter dataFilter)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
            _objectMapper = objectMapper;
            _configuration = configuration;
            _logger = logger;
            _identityUserManager = identityUserManager;
            _identityRoleManager = identityRoleManager;
            _guidGenerator = guidGenerator;
            _currentTenant = currentTenant;
            _dataFilter = dataFilter;
        }
        public async Task<UserEntity> GetByEmailAsync(string email)
        {
            //return await _context.Users.AsNoTracking()
            //                         .ProjectTo<UserModel>(Mapper.ConfigurationProvider)
            //                         .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
            // return _objectMapper.Map<UserEntity, UserModel>(user);

            _logger.DatabaseInfo("Fetching user by email: {Email}", email);
            var user = await _context.AllUsers
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                _logger.Warning("User not found for email: {Email}", email);
            }
            return user;
        }
        public async Task<UserEntity> GetUserObjectIdAsync(string objectId)
        {
            if (!Guid.TryParse(objectId, out var userGuid))
                return null;
            //return await _context.AsNoTracking()
            //               .ProjectTo<UserModel>(Mapper.ConfigurationProvider)
            //               .FirstOrDefaultAsync(x => x.B2CUserId == userGuid);
            var user = await _context.AllUsers
                           .AsNoTracking()
                           .FirstOrDefaultAsync(x => x.B2CUserId == userGuid);
            return user;
        }
        public async Task<RoleModel> GetByNameAsync(string name)
        {
            var roleEntity = await _context.AllRoles.FirstOrDefaultAsync(x => x.Name == name);

            var roleModel = new RoleModel
            {
                EntityId = roleEntity.Id,
                Name = roleEntity.Name,
            };

            return roleModel;
        }
        public async Task<UserRoleModel> GetUserRoleAsync(long userId)
        {
            try
            {


                var userRole = await _context.UserRoles.FirstOrDefaultAsync(u => u.UserId == userId);
                if (userRole == null) return null;

                //var role = await _context.AllRoles.FirstOrDefaultAsync(r => r.Id == userRole.RoleId);
                //if (role == null) return null;

                return new UserRoleModel
                {
                    UserId = userRole.UserId,
                    RoleId = userRole.RoleId,
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<RoleModel> GetRoleAsync(int roleId)
        {
            _logger.DatabaseInfo("Fetching user by roleId: {RoleId}", roleId);
            var role = await _context.AllRoles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
            {
                _logger.Warning("User not found for roleId: {RoleId}", roleId);
            };

            return new RoleModel
            {
                EntityId = role.Id,
                Name = role.Name,
            };
        }
        public async Task<RoleModel> GetRoleNameAsync(string roleName)
        {
            var role = await _context.AllRoles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return null;

            return new RoleModel
            {
                EntityId = role.Id,
                Name = role.Name,
            };

        }
        public async Task<RoleModel> GetRoleByUserIdAsync(long userId)
        {
            var userRole = await _context.UserRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (userRole == null)
                return null;

            var role = await _context.AllRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == userRole.RoleId);

            if (role == null)
                return null;

            return new RoleModel
            {
                EntityId = role.Id,
                Name = role.Name
            };
        }
        public async Task<bool> UpdateUserRoleAsync(long userId, int roleId)
        {
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId);
            var user = await _context.AllUsers.FindAsync(userId);

            if (user.IsActive != true)
            {
                user.IsActive = true;
                _context.AllUsers.Update(user);
                _context.SaveChanges();
            }

            if (userRole == null)
            {
                var usermodel = new UserRoleEntity(roleId)
                {
                    UserId = userId,
                    RoleId = roleId
                };
                await _context.UserRoles.AddAsync(usermodel);
                await _context.SaveChangesAsync();
                return true;
            }

            userRole.RoleId = roleId;
            _context.UserRoles.Update(userRole);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateAsync(UserModel userModel)
        {
            if (userModel == null)
            {
                throw new ArgumentNullException(nameof(userModel));
            }

            var existingUser = await _context.AllUsers.FindAsync(userModel.EntityId);

            if (existingUser != null)
            {
                // Manually map properties from UserModel to UserEntity
                existingUser.Name = userModel.Name;
                existingUser.Email = userModel.Email;
                existingUser.IsActive = userModel.IsActive;
               // existingUser.IsMapActive = userModel.IsMapActive;

                _context.AllUsers.Update(existingUser);
            }
            else
            {
                // Convert UserModel to UserEntity before adding
                var userEntity = new UserEntity
                {
                    Id = userModel.EntityId,
                    Name = userModel.Name,
                    Email = userModel.Email,
                    IsActive = userModel.IsActive,
                    //IsMapActive = userModel.IsMapActive
                };

                await _context.AllUsers.AddAsync(userEntity);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> EsesUserExists(string email)
        {
            try
            {
                email = NormalizeEmail(email);

                return await _context.Users.AnyAsync(x => x.Email == email);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task AddEsecUser(UserModel userModel)
        {
            try
            {
                // Map the ESEC LDAP role to the application role (Admin/Operator/Guest).
                var roleMappingSection = _configuration.GetSection("RoleMappings");
                var companyKeyword = roleMappingSection["Company"] ?? string.Empty;
                var rolesSection = roleMappingSection.GetSection("Roles");

                string mappedRole = "Guest";

                foreach (var child in rolesSection.GetChildren())
                {
                    var roleKey = child.Key;
                    var appRole = child.Value;

                    if (!string.IsNullOrEmpty(userModel.RoleName) &&
                        userModel.RoleName.Contains(companyKeyword, StringComparison.OrdinalIgnoreCase) &&
                        userModel.RoleName.Contains(roleKey, StringComparison.OrdinalIgnoreCase))
                    {
                        mappedRole = appRole;
                        break;
                    }
                }

                using (_dataFilter.Disable<IMultiTenant>())
                {
                    var email = NormalizeEmail(userModel.Email);

                    var user = await _identityUserManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new IdentityUser(
                            _guidGenerator.Create(),
                            email,
                            email,
                            _currentTenant.Id)
                        {
                            Name = userModel.Name
                        };

                        ThrowIfFailed(await _identityUserManager.CreateAsync(user), "create user");
                    }

                    if (!string.IsNullOrWhiteSpace(mappedRole))
                    {
                        if (!await _identityRoleManager.RoleExistsAsync(mappedRole))
                        {
                            ThrowIfFailed(
                                await _identityRoleManager.CreateAsync(
                                    new IdentityRole(_guidGenerator.Create(), mappedRole, _currentTenant.Id)),
                                "create role");
                        }

                        if (!await _identityUserManager.IsInRoleAsync(user, mappedRole))
                        {
                            ThrowIfFailed(
                                await _identityUserManager.AddToRoleAsync(user, mappedRole),
                                "assign role");
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void ThrowIfFailed(Microsoft.AspNetCore.Identity.IdentityResult result, string action)
        {
            if (!result.Succeeded)
            {
                throw new Exception($"ESEC provisioning failed to {action}: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            return new EmailAddressAttribute().IsValid(email)
                ? email
                : $"{email}@projectbuilder.com";
        }

        public UserModel GetUserByEmailAsync(string email)
        {
            try
            {
                var user = _context.AllUsers.FirstOrDefault(x => x.Email == email);
                return new UserModel
                {
                    EntityId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    B2CUserId = user.B2CUserId,
                };
            }
            catch (Exception e)
            {
                throw new Exception("Entity not found");
            }
        }
        public async Task<RoleEntity> GetRoleByName(string roleName)
        {
            return await _context.AllRoles.FirstOrDefaultAsync(x => x.Name == roleName);
        }
        public async Task<UserEntity> GetByIdAsync(long userId)
        {
            _logger.DatabaseInfo("Fetching user by id: {UserId}", userId);
            var user = await _context.AllUsers
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                _logger.Warning("User not found for id: {UserId}", userId);
            }
            return user;
        }
    }
}
