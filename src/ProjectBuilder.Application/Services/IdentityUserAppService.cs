using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.TenantManagement;
using Volo.Abp;
using Volo.Abp.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace ProjectBuilder.Services
{
    [RemoteService(IsEnabled = false)]
    [Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(IIdentityUserAppService), typeof(Volo.Abp.Identity.IdentityUserAppService))]
    public class IdentityUserAppService : Volo.Abp.Identity.IdentityUserAppService
    {
        private readonly IDataFilter _dataFilter;
        private readonly ITenantRepository _tenantRepository;
        private readonly IdentityUserManager _userManager;

        public IdentityUserAppService(
            IdentityUserManager userManager,
            IIdentityUserRepository userRepository,
            IIdentityRoleRepository roleRepository,
            IOptions<IdentityOptions> identityOptions,
            IPermissionChecker permissionChecker,
            IDataFilter dataFilter,
            ITenantRepository tenantRepository)
            : base(userManager, userRepository, roleRepository, identityOptions, permissionChecker)
        {
            _dataFilter = dataFilter;
            _tenantRepository = tenantRepository;
            _userManager = userManager;
        }

        public override async Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetIdentityUsersInput input)
        {
            if (CurrentTenant.Id == null)
            {
                using (_dataFilter.Disable<IMultiTenant>())
                {
                    var count = await UserRepository.GetCountAsync(input.Filter);
                    var list = await UserRepository.GetListAsync(input.Sorting, input.MaxResultCount, input.SkipCount, input.Filter);

                    var userDtos = ObjectMapper.Map<List<Volo.Abp.Identity.IdentityUser>, List<IdentityUserDto>>(list);
                    foreach (var userDto in userDtos)
                    {
                        userDto.ExtraProperties["TenantName"] = await GetTenantNameAsync(userDto.TenantId);
                        userDto.ExtraProperties["Roles"] = await GetUserRolesAsync(userDto.Id);
                    }

                    return new PagedResultDto<IdentityUserDto>(count, userDtos);
                }
            }
            else
            {
                var count = await UserRepository.GetCountAsync(input.Filter);
                var list = await UserRepository.GetListAsync(input.Sorting, input.MaxResultCount, input.SkipCount, input.Filter);

                var userDtos = ObjectMapper.Map<List<Volo.Abp.Identity.IdentityUser>, List<IdentityUserDto>>(list);
                foreach (var userDto in userDtos)
                {
                    userDto.ExtraProperties["TenantName"] = await GetTenantNameAsync(userDto.TenantId);
                    userDto.ExtraProperties["Roles"] = await GetUserRolesAsync(userDto.Id);
                }

                return new PagedResultDto<IdentityUserDto>(count, userDtos);
            }
        }

        private async Task<string> GetTenantNameAsync(Guid? tenantId)
        {
            if (tenantId == null)
                return null;

            var tenant = await _tenantRepository.FindAsync(tenantId.Value);
            return tenant?.Name;
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userManager.GetByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public override async Task DeleteAsync(Guid id)
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var user = await UserManager.FindByIdAsync(id.ToString());
                if (user == null)
                    return;

                (await UserManager.DeleteAsync(user)).CheckErrors();
            }
        }

        public override async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
        {
            EnsureContainsLetterOrDigit(input.UserName, "Username");
            EnsureContainsLetterOrDigit(input.Name, "Name");
            if (!string.IsNullOrEmpty(input.Surname))
            {
                EnsureContainsLetterOrDigit(input.Surname, "Surname");
            }

            return await base.CreateAsync(input);
        }

        private static void EnsureContainsLetterOrDigit(string value, string fieldName)
        {
            if (string.IsNullOrEmpty(value) || !value.Any(char.IsLetterOrDigit))
            {
                throw new UserFriendlyException($"{fieldName} must contain at least one letter or number.");
            }
        }
    }
}
