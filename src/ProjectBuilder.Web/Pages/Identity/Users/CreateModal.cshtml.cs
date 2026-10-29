using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Validation;
using System;
using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;
using static Volo.Abp.Identity.Web.Pages.Identity.Users.CreateModalModel;
using Microsoft.AspNetCore.Authorization;

namespace Volo.Abp.Identity.Web.Pages.Identity.Users;

[Authorize]
public class CustomCreateModalModel : IdentityPageModel
{
    [BindProperty]
    public UserInfoViewModel UserInfo { get; set; }

    [BindProperty]
    public AssignedRoleViewModel[] Roles { get; set; }

    protected IIdentityUserAppService IdentityUserAppService { get; }

    public CustomCreateModalModel(IIdentityUserAppService identityUserAppService)
    {
        IdentityUserAppService = identityUserAppService;
    }

    public virtual async Task<IActionResult> OnGetAsync()
    {
        try
        {
            UserInfo = new UserInfoViewModel();

            var roleDtoList = (await IdentityUserAppService.GetAssignableRolesAsync())?.Items;

            if (roleDtoList != null)
            {
                Roles = roleDtoList.Select(roleDto =>
                    new AssignedRoleViewModel
                    {
                        Name = roleDto.Name,
                        IsAssigned = roleDto.IsDefault,
                        IsDefault = roleDto.IsDefault
                    }).ToArray();
            }
            else
            {
                // Handle case when roleDtoList is null or empty
                // Log or handle the error appropriately
            }

            return Page();
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }



    public virtual async Task<NoContentResult> OnPostCreate()
    {
        try
        {

            var input = new IdentityUserCreateDto
            {
                UserName = UserInfo.UserName,
                Name= UserInfo.Name,
                Surname= UserInfo.Surname,
                Password= UserInfo.Password,
                Email = UserInfo.Email,
                PhoneNumber = UserInfo.PhoneNumber,
                IsActive = UserInfo.IsActive,
                LockoutEnabled = UserInfo.LockoutEnabled,
            };

            input.RoleNames = Roles.Where(r => r.IsAssigned).Select(r => r.Name).ToArray();

            await IdentityUserAppService.CreateAsync(input);

            return NoContent();
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            throw;
        }
    }


    public class  UserInfoViewModel : ExtensibleObject
    {
        [Required]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxUserNameLength))]
        [RegularExpression(@".*[a-zA-Z0-9].*", ErrorMessage = "Username must contain at least one letter or number.")]
        public string UserName { get; set; }

        [Required]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxNameLength))]
        [RegularExpression(@".*[a-zA-Z0-9].*", ErrorMessage = "Name must contain at least one letter or number.")]
        public string Name { get; set; }

        public string Surname { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisableAuditing]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
        public string Email { get; set; }


        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public bool LockoutEnabled { get; set; } = true;
    }

    public class AssignedRoleViewModel
    {
        [Required]
        [HiddenInput]
        public string Name { get; set; }

        public bool IsAssigned { get; set; }

        public bool IsDefault { get; set; }
    }
}
