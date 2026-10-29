using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.TenantManagement;
using static Volo.Abp.Identity.Web.Pages.Identity.Users.CustomEditModalModel.DetailViewModel;
using static Volo.Abp.TenantManagement.TenantManagementPermissions;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;

namespace Volo.Abp.Identity.Web.Pages.Identity.Users;

[Authorize]
public class CustomEditModalModel : IdentityPageModel
{
    [BindProperty]
    public UserInfoViewModel UserInfo { get; set; }

    [BindProperty]
    public AssignedRoleViewModel[] Roles { get; set; }
    [BindProperty]
    public List<SelectListItem> Teanats { get; set; }
    public DetailViewModel Detail { get; set; }

    protected IIdentityUserAppService _identityUserAppService { get; }
    protected IdentityUserCreateOrUpdateDtoBase IdentityUserCreateOrUpdateDtoBase;
    protected IPermissionChecker PermissionChecker { get; }
    private readonly ITenantAppService _tenantAppService;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;

    public bool IsEditCurrentUser { get; set; }

    public CustomEditModalModel(IIdentityUserAppService identityUserAppService,
        IPermissionChecker permissionChecker, ITenantAppService tenantAppService,
        IIdentityUserRepository userRepository, IDataFilter dataFilter, IdentityUserManager userManager, IdentityRoleManager roleManager, IRepository<IdentityRole, Guid> roleRepository)
    {
        _identityUserAppService = identityUserAppService;
        PermissionChecker = permissionChecker;
        _tenantAppService = tenantAppService;
        _userRepository = userRepository;
        _dataFilter = dataFilter;
        _userManager = userManager;
        _roleManager = roleManager;
        _roleRepository = roleRepository;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _identityUserAppService.GetAsync(id);

            UserInfo = new UserInfoViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                LockoutEnabled = user.LockoutEnabled,
                TenantId = user.TenantId,
            };

            var tenantList = (await _tenantAppService.GetListAsync(new GetTenantsInput())).Items;
            var userTenantId = user.TenantId;
            UserInfo.Teanats = tenantList.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name,
                Selected = t.Id == userTenantId
            }).ToList();
            var userRoles = (await _identityUserAppService.GetRolesAsync(UserInfo.Id)).Items.Select(r => r.Name).ToList();
            var assignableRoles = await _roleRepository.GetListAsync(role => role.TenantId == userTenantId);

            Roles = assignableRoles.Select(roleDto => new AssignedRoleViewModel
            {
                Name = roleDto.Name,
                IsAssigned = userRoles.Contains(roleDto.Name)
            }).ToArray();

            Detail = new DetailViewModel
            {
                CreatedBy = await GetUserNameOrNullAsync(user.CreatorId),
                CreationTime = user.CreationTime,
                ModifiedBy = await GetUserNameOrNullAsync(user.LastModifierId),
                LastModificationTime = user.LastModificationTime,
                LastPasswordChangeTime = user.LastPasswordChangeTime,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount
            };

            return Page();
        }
    }



    private async Task<string> GetUserNameOrNullAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return "";
        }

        var user = await _identityUserAppService.GetAsync(userId.Value);
        return user.UserName;
    }

    private static void EnsureContainsLetterOrDigit(string value, string fieldName)
    {
        if (string.IsNullOrEmpty(value) || !value.Any(char.IsLetterOrDigit))
        {
            throw new UserFriendlyException($"{fieldName} must contain at least one letter or number.");
        }
    }



    public async Task<IActionResult> OnPostUpdateUser(Guid? selectedTenantId)
    {
        try
        {
            IdentityUser user;
            using (_dataFilter.Disable<IMultiTenant>())
            {
                user = await _userManager.GetByIdAsync(UserInfo.Id);
                if (user == null)
                {
                    return NotFound();
                }
            }
            EnsureContainsLetterOrDigit(UserInfo.UserName, "Username");
            EnsureContainsLetterOrDigit(UserInfo.Name, "Name");
            if (!string.IsNullOrEmpty(UserInfo.Surname))
            {
                EnsureContainsLetterOrDigit(UserInfo.Surname, "Surname");
            }

            using (CurrentTenant.Change(UserInfo.TenantId))
            {
                user.Name = UserInfo.Name;
                user.Surname = UserInfo.Surname;
                var selectedRole = Roles.FirstOrDefault(x => x.IsAssigned)?.Name;
                if (selectedRole != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                    {
                        var removeExistingRole = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }
                    var addNewRole = await _userManager.AddToRoleAsync(user, selectedRole.ToLower());
                }
                var username = await _userManager.SetUserNameAsync(user, UserInfo.UserName);
                var property = user.GetType().GetProperty("TenantId");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(user, selectedTenantId);
                }
                var emailUpdateResult = await _userManager.SetEmailAsync(user, UserInfo.Email);
                user.SetPhoneNumber(UserInfo.PhoneNumber, true);
                user.SetIsActive(UserInfo.IsActive);
                await _userRepository.UpdateAsync(user);
                return NoContent();
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            throw;
        }
    }



    public class UserInfoViewModel : ExtensibleObject, IHasConcurrencyStamp
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [HiddenInput]
        public string ConcurrencyStamp { get; set; }

        [Required]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxUserNameLength))]
        [RegularExpression(@".*[a-zA-Z0-9].*", ErrorMessage = "Username must contain at least one letter or number.")]
        public string UserName { get; set; }
        [Required]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxNameLength))]
        [RegularExpression(@".*[a-zA-Z0-9].*", ErrorMessage = "Name must contain at least one letter or number.")]
        public string Name { get; set; }

        public string Surname { get; set; }

        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
        [DataType(DataType.Password)]
        [DisableAuditing]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public bool LockoutEnabled { get; set; }
        public Guid? TenantId { get; set; }
        public List<SelectListItem> Teanats { get; set; }
    }

    public class AssignedRoleViewModel
    {
        [Required]
        [HiddenInput]
        public string Name { get; set; }

        public bool IsAssigned { get; set; }
    }

    public class DetailViewModel
    {
        public string CreatedBy { get; set; }
        public DateTime? CreationTime { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime? LastModificationTime { get; set; }

        public DateTimeOffset? LastPasswordChangeTime { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public int AccessFailedCount { get; set; }

        public class UpdateUser : IdentityUserUpdateDto
        {
            public Guid? TenantId { get; set; }
        }
    }
}