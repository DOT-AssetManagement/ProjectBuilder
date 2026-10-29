using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectBuilder.Entities;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Authorization;
using Volo.Abp.Authorization.Permissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account.Settings;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Volo.Abp.Validation;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace ProjectBuilder.Web.Pages.Account
{
    public class CustomLoginModel : LoginModel
    {
        protected IWebHostEnvironment WebHostEnvironment { get; }
        public bool ShowRequireMigrateSeedMessage { get; set; }
        private readonly IDataFilter _dataFilter;
        private readonly IUserRepository _userRepository;
        private readonly IdentityRoleManager _roleManager;
        private readonly PermissionGrantCacheClearer _permissionGrantCacheClearer;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;


        public CustomLoginModel(
            IAuthenticationSchemeProvider schemeProvider,
            IOptions<AbpAccountOptions> accountOptions,
            IOptions<IdentityOptions> identityOptions,
            IdentityDynamicClaimsPrincipalContributorCache identityDynamicClaimsPrincipalContributorCache,
            IWebHostEnvironment webHostEnvironment,
            IDataFilter dataFilter,
            IUserRepository userRepository,
            IdentityRoleManager roleManager,
            PermissionGrantCacheClearer permissionGrantCacheClearer,
            IPermissionChecker permissionChecker,
            IEmailSender emailSender,
            IConfiguration configuration)
            : base(schemeProvider, accountOptions, identityOptions, identityDynamicClaimsPrincipalContributorCache, webHostEnvironment)
        {
            _dataFilter = dataFilter;
            WebHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _roleManager = roleManager;
            _permissionGrantCacheClearer = permissionGrantCacheClearer;
            _permissionChecker = permissionChecker;
            _emailSender = emailSender;
            _configuration = configuration;
        }


        public override async Task<IActionResult> OnGetAsync()
        {
            LoginInput = new LoginInputModel();

            ExternalProviders = await GetExternalProviders();

            EnableLocalLogin = await SettingProvider.IsTrueAsync(AccountSettingNames.EnableLocalLogin);

            if (IsExternalLoginOnly)
            {
                return await OnPostExternalLogin(ExternalProviders.First().AuthenticationScheme);
            }

            return Page();
        }

        public override async Task<IActionResult> OnPostAsync(string action)
        {
            await CheckLocalLoginAsync();

            ValidateModel();

            ExternalProviders = await GetExternalProviders();

            EnableLocalLogin = await SettingProvider.IsTrueAsync(AccountSettingNames.EnableLocalLogin);

            await ReplaceEmailToUsernameOfInputIfNeeds();

            await IdentityOptions.SetAsync();

            var result = await SignInManager.PasswordSignInAsync(
                LoginInput.UserNameOrEmailAddress,
                LoginInput.Password,
                LoginInput.RememberMe,
                true
            );

            await IdentitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext()
            {
                Identity = IdentitySecurityLogIdentityConsts.Identity,
                Action = result.ToIdentitySecurityLogAction(),
                UserName = LoginInput.UserNameOrEmailAddress
            });

            if (result.RequiresTwoFactor)
            {
                return await TwoFactorLoginResultAsync();
            }

            if (result.IsLockedOut)
            {
                Alerts.Warning(L["UserLockedOutMessage"]);
                return Page();
            }

            if (result.IsNotAllowed)
            {
                Alerts.Warning(L["LoginIsNotAllowed"]);
                return Page();
            }

            if (!result.Succeeded)
            {
                if (LoginInput.UserNameOrEmailAddress == IdentityDataSeedContributor.AdminUserNameDefaultValue &&
                    WebHostEnvironment.IsDevelopment())
                {
                    var adminUser = await UserManager.FindByNameAsync(IdentityDataSeedContributor.AdminUserNameDefaultValue);
                    if (adminUser == null)
                    {
                        ShowRequireMigrateSeedMessage = true;
                        return Page();
                    }
                }

                Alerts.Danger(L["InvalidUserNameOrPassword"]);
                return Page();
            }

            //TODO: Find a way of getting user's id from the logged in user and do not query it again like that!
            var user = await UserManager.FindByNameAsync(LoginInput.UserNameOrEmailAddress) ??
                       await UserManager.FindByEmailAsync(LoginInput.UserNameOrEmailAddress);

            Debug.Assert(user != null, nameof(user) + " != null");

            await IdentityDynamicClaimsPrincipalContributorCache.ClearAsync(user.Id, user.TenantId);
            await _permissionGrantCacheClearer.ClearForRoleNamesAsync(
                await UserManager.GetRolesAsync(user));

            return await PermissionAwareRedirect.ToLandingPageAsync(this, _permissionChecker);
        }

        protected override Task<IActionResult> TwoFactorLoginResultAsync()
        {
            throw new NotImplementedException();
        }

        protected override async Task<List<ExternalProviderModel>> GetExternalProviders()
        {
            var schemes = await SchemeProvider.GetAllSchemesAsync();

            return schemes
                .Where(x => x.DisplayName != null || x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                .Select(x => new ExternalProviderModel
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                })
                .ToList();
        }

        public override async Task<IActionResult> OnPostExternalLogin(string provider)
        {
            var redirectUrl = Url.Page("./Login", pageHandler: "ExternalLoginCallback", values: new { ReturnUrl, ReturnUrlHash });
            var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            properties.Items["scheme"] = provider;

            return await Task.FromResult(Challenge(properties, provider));
        }


        //for abp user , as now i am on old database this is for abptables

        //public override async Task<IActionResult> OnGetExternalLoginCallbackAsync(string returnUrl = "", string returnUrlHash = "", string remoteError = null)
        //{
        //    //TODO: Did not implemented Identity Server 4 sample for this method (see ExternalLoginCallback in Quickstart of IDS4 sample)
        //    /* Also did not implement these:
        //     * - Logout(string logoutId)
        //     */
        //    using (_dataFilter.Disable<IMultiTenant>())
        //    {
        //        if (remoteError != null)
        //        {
        //            Logger.LogWarning($"External login callback error: {remoteError}");
        //            return RedirectToPage("./Login");
        //        }

        //        await IdentityOptions.SetAsync();

        //        var loginInfo = await SignInManager.GetExternalLoginInfoAsync();
        //        if (loginInfo == null)
        //        {
        //            Logger.LogWarning("External login info is not available");
        //            return RedirectToPage("./Login");
        //        }

        //        var result = await SignInManager.ExternalLoginSignInAsync(
        //            loginInfo.LoginProvider,
        //            loginInfo.ProviderKey,
        //            isPersistent: false,
        //            bypassTwoFactor: true
        //        );

        //        if (!result.Succeeded)
        //        {
        //            await IdentitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext()
        //            {
        //                Identity = IdentitySecurityLogIdentityConsts.IdentityExternal,
        //                Action = "Login" + result
        //            });
        //        }

        //        if (result.IsLockedOut)
        //        {
        //            Logger.LogWarning($"External login callback error: user is locked out!");
        //            throw new UserFriendlyException("Cannot proceed because user is locked out!");
        //        }

        //        if (result.IsNotAllowed)
        //        {
        //            Logger.LogWarning($"External login callback error: user is not allowed!");
        //            throw new UserFriendlyException("Cannot proceed because user is not allowed!");
        //        }

        //        Volo.Abp.Identity.IdentityUser user;
        //        if (result.Succeeded)
        //        {
        //            user = await UserManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
        //            if (user != null)
        //            {
        //                await IdentityDynamicClaimsPrincipalContributorCache.ClearAsync(user.Id, user.TenantId);
        //            }
        //            if (User.IsInRole("Guest"))
        //            {
        //                // Redirect to GuestIndex
        //                return RedirectToPage("/Scenarios/Index");
        //            }
        //            else if (User.IsInRole("admin"))
        //            {
        //                return RedirectToPage("/Identity/Users");
        //            }
        //            return RedirectToPage("/Scenarios/Index");
        //        }
        //        var email = loginInfo.Principal.FindFirstValue("emails");
        //        if (email.IsNullOrWhiteSpace())
        //        {
        //            return RedirectToPage("./Register", new
        //            {
        //                IsExternalLogin = true,
        //                ExternalLoginAuthSchema = loginInfo.LoginProvider,
        //                ReturnUrl = returnUrl
        //            });
        //        }

        //        user = await UserManager.FindByEmailAsync(email);
        //        if (user == null)
        //        {
        //            return RedirectToPage("./Register", new
        //            {
        //                IsExternalLogin = true,
        //                ExternalLoginAuthSchema = loginInfo.LoginProvider,
        //                ReturnUrl = returnUrl
        //            });
        //        }
        //        await SignInManager.SignInAsync(user, false);

        //        await IdentitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext()
        //        {
        //            Identity = IdentitySecurityLogIdentityConsts.IdentityExternal,
        //            Action = result.ToIdentitySecurityLogAction(),
        //            UserName = user.Name
        //        });
        //        await IdentityDynamicClaimsPrincipalContributorCache.ClearAsync(user.Id, user.TenantId);
        //        if (User.IsInRole("Guest"))
        //        {
        //            // Redirect to GuestIndex
        //            return RedirectToPage("/Scenarios/Index");
        //        }
        //        else if (User.IsInRole("admin"))
        //        {
        //            return RedirectToPage("/Identity/Users");
        //        }
        //        return RedirectToPage("/Scenarios/Index");
        //    }
        //}


public override async Task<IActionResult> OnGetExternalLoginCallbackAsync(
    string returnUrl = "",
    string returnUrlHash = "",
    string remoteError = null)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            if (remoteError != null)
            {
                Logger.LogWarning("External login callback error: {RemoteError}", ProjectBuilder.Web.Common.LogSanitizer.SanitizeForLog(remoteError));
                return RedirectToPage("./Login");
            }

            await IdentityOptions.SetAsync();

            var loginInfo = await SignInManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                Logger.LogWarning("External login info is not available");
                return RedirectToPage("./Login");
            }

            var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)
                        ?? loginInfo.Principal.FindFirst("email")?.Value
                        ?? loginInfo.Principal.FindFirst("preferred_username")?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToPage("./Register", new
                {
                    IsExternalLogin = true,
                    ExternalLoginAuthSchema = loginInfo.LoginProvider,
                    ReturnUrl = returnUrl
                });
            }

            // Find or create the ABP identity user (AbpUsers table).
            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                var userName = loginInfo.Principal.FindFirstValue(ClaimTypes.Name)
                               ?? loginInfo.Principal.FindFirst("name")?.Value
                               ?? email;

                user = new Volo.Abp.Identity.IdentityUser(
                    GuidGenerator.Create(),
                    userName,
                    email,
                    tenantId: CurrentTenant.Id);

                (await UserManager.CreateAsync(user)).CheckErrors();

                // Assign the default "Guest" role only if it exists (no seeding is performed here).
                if (await _roleManager.RoleExistsAsync("Guest"))
                {
                    (await UserManager.AddToRoleAsync(user, "Guest")).CheckErrors();
                }

                await NotifyNewUserSignUpAsync(user, loginInfo.LoginProvider);
            }

            // Link the external login to the ABP user so subsequent logins resolve directly.
            if (await UserManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey) == null)
            {
                (await UserManager.AddLoginAsync(user, new UserLoginInfo(
                    loginInfo.LoginProvider, loginInfo.ProviderKey, loginInfo.ProviderDisplayName))).CheckErrors();
            }

            await SignInManager.SignInAsync(user, isPersistent: false);

            await IdentitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext
            {
                Identity = IdentitySecurityLogIdentityConsts.IdentityExternal,
                Action = "LoginSucceeded",
                UserName = user.UserName
            });

            await IdentityDynamicClaimsPrincipalContributorCache.ClearAsync(user.Id, user.TenantId);
            await _permissionGrantCacheClearer.ClearForRoleNamesAsync(
                await UserManager.GetRolesAsync(user));

            return await PermissionAwareRedirect.ToLandingPageAsync(this, _permissionChecker);
        }
    }




    /// <summary>
    /// Notifies the configured administrator that a new user account was provisioned.
    /// Failures are logged and swallowed — a broken mail server must never block a sign-in.
    /// </summary>
    protected virtual async Task NotifyNewUserSignUpAsync(
        Volo.Abp.Identity.IdentityUser user,
        string loginProvider)
    {
        try
        {
            if (!_configuration.GetValue("NewUserNotification:Enabled", true))
            {
                return;
            }

            var toEmail = _configuration["NewUserNotification:ToEmail"];
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                Logger.LogWarning("New user sign-up notification skipped: NewUserNotification:ToEmail is not configured.");
                return;
            }

            var body =
                $"<p>A new user has signed up for ProjectBuilder.</p>" +
                $"<ul>" +
                $"<li><b>User name:</b> {user.UserName}</li>" +
                $"<li><b>Email:</b> {user.Email}</li>" +
                $"<li><b>User id:</b> {user.Id}</li>" +
                $"<li><b>Login provider:</b> {loginProvider}</li>" +
                $"<li><b>Signed up at (UTC):</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</li>" +
                $"</ul>";

            await _emailSender.SendAsync(
                to: toEmail,
                subject: $"New user sign-up: {user.UserName}",
                body: body,
                isBodyHtml: true);

            Logger.LogInformation(
                "New user sign-up notification sent to {ToEmail} for user {UserName} ({UserId}).",
                toEmail, user.UserName, user.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Failed to send new user sign-up notification for user {UserName} ({UserId}).",
                user.UserName, user.Id);
        }
    }

    protected virtual async Task ReplaceEmailToUsernameOfInputIfNeeds()
        {
            if (!ValidationHelper.IsValidEmailAddress(LoginInput.UserNameOrEmailAddress))
            {
                return;
            }

            var userByUsername = await UserManager.FindByNameAsync(LoginInput.UserNameOrEmailAddress);
            if (userByUsername != null)
            {
                return;
            }

            var userByEmail = await UserManager.FindByEmailAsync(LoginInput.UserNameOrEmailAddress);
            if (userByEmail == null)
            {
                return;
            }

            LoginInput.UserNameOrEmailAddress = userByEmail.UserName;
        }

        protected virtual async Task CheckLocalLoginAsync()
        {
            if (!await SettingProvider.IsTrueAsync(AccountSettingNames.EnableLocalLogin))
            {
                throw new UserFriendlyException(L["LocalLoginDisabledMessage"]);
            }
        }

    }
}
