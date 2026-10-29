using Hangfire;
using Hangfire.Dashboard;
using ProjectBuilder.Web.Jobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Localization;
using ProjectBuilder.MultiTenancy;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Authorization;
using ProjectBuilder.Web.Menus;
using ProjectBuilder.Web.Security;
using ProjectBuilder.Web.Security.Interfaces;
using Serilog;
using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Security;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.SecurityLog;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.Uow;
using Volo.Abp.VirtualFileSystem;

namespace ProjectBuilder.Web;

[DependsOn(
    typeof(ProjectBuilderHttpApiModule),
    typeof(ProjectBuilderApplicationModule),
    typeof(ProjectBuilderEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpFeatureManagementWebModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule)
)]
public class ProjectBuilderWebModule : AbpModule
{
    // ESEC auth codes are URL-safe/base64 tokens; anything else is dropped before redirecting
    private static readonly Regex EsecAuthCodePattern = new(@"^[A-Za-z0-9\-._~+/=]{1,2048}$", RegexOptions.Compiled);

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(ProjectBuilderResource),
                typeof(ProjectBuilderDomainModule).Assembly,
                typeof(ProjectBuilderDomainSharedModule).Assembly,
                typeof(ProjectBuilderApplicationModule).Assembly,
                typeof(ProjectBuilderApplicationContractsModule).Assembly,
                typeof(ProjectBuilderWebModule).Assembly
            );

        });


        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("ProjectBuilder");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            var certFile = Path.Combine(hostingEnvironment.ContentRootPath, "openiddict.pfx"); // changed filename

            if (File.Exists(certFile))
            {
                PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
                {
                    options.AddDevelopmentEncryptionAndSigningCertificate = false;
                });

                PreConfigure<OpenIddictServerBuilder>(builder =>
                {
                    var cert = GetSigningCertificate(hostingEnvironment, configuration);
                    builder.AddSigningCertificate(cert);
                    builder.AddEncryptionCertificate(cert);
                    builder.SetIssuer(new Uri(configuration["AuthServer:Authority"]));
                });
            }
            else
            {
                // Fallback if cert file missing - prevents Azure crash
                PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
                {
                    options.AddDevelopmentEncryptionAndSigningCertificate = false;
                });

                PreConfigure<OpenIddictServerBuilder>(builder =>
                {
                    builder.AddEphemeralEncryptionKey();
                    builder.AddEphemeralSigningKey();
                    builder.SetIssuer(new Uri(configuration["AuthServer:Authority"]));
                });
            }
        }
    }

    private X509Certificate2 GetSigningCertificate(IWebHostEnvironment hostingEnv, IConfiguration configuration)
    {
        var fileName = "openiddict.pfx";
        var passPhrase = configuration["AuthServer:CertificatePassPhrase"];
        var file = Path.Combine(hostingEnv.ContentRootPath, fileName);

        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"Signing Certificate couldn't found: {file}");
        }

        return new X509Certificate2(file, passPhrase);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        // Large JSON API responses (e.g. /api/app/map/generate-map, which returns every
        // project + treatment row for a scenario) go over the wire uncompressed otherwise.
        context.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        // Used by FetchBridgeReferenceDataJob to pull bridge data from PennDOT's public
        // PennShare ArcGIS REST service (Admin > Import > Fetch Bridge Reference Data).
        context.Services.AddHttpClient();

        context.Services.Replace(
        ServiceDescriptor.Singleton<ISettingStore, NullSettingStore>()
    );

        // Permission store now uses the ABP database-backed store (AbpPermissionGrants),
        // so the NullPermissionStore replacement is intentionally removed.

        context.Services.Replace(
            ServiceDescriptor.Singleton<IFeatureStore, NullFeatureStore>()
        );

        // Optional but recommended: Disable audit logging to database
        context.Services.Replace(
            ServiceDescriptor.Singleton<IAuditingStore, SimpleLogAuditingStore>()  // Logs to console/file instead of DB
        );
        context.Services.AddHangfire(config =>
    config.UseSqlServerStorage(configuration.GetConnectionString("Default")));
        context.Services.AddHangfireServer();

        Configure<AbpSecurityLogOptions>(options =>
        {
            options.IsEnabled = false;        // ← This disables security logging completely
        });

        // Disable dynamic claims if not needed (prevents extra DB calls)
        Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = false;
        });

        // Disable background jobs entirely (stops AbpBackgroundJobs polling)
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
        });

        // Enable the ABP permission store now that the AbpPermission* tables exist:
        // static permission definitions are persisted and permissions are read from the DB.
        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = true;
            options.SaveStaticPermissionsToDatabase = true;
            options.ManagementProviders.Add<UserPermissionDenyManagementProvider>();
        });

        // Lets a specific user's permission grant be denied even when a Role grants it
        // (ABP's own permission model is grant-only; Prohibited from this provider wins).
        Configure<AbpPermissionOptions>(options =>
        {
            options.ValueProviders.Add<UserPermissionDenyValueProvider>();
        });

        // Disable static feature saving to DB (stops AbpFeatureGroups errors)  
        Configure<FeatureManagementOptions>(options =>
        {
            options.IsDynamicFeatureStoreEnabled = false;
            options.SaveStaticFeaturesToDatabase = false;
        });

        // Disable static setting saving to DB (stops AbpSettingDefinitions errors)
        Configure<SettingManagementOptions>(options =>
        {
            options.SaveStaticSettingsToDatabase = false;
            options.IsDynamicSettingStoreEnabled = false;
        });
        context.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        var dataProtectionKeysPath = configuration["DataProtection:KeysPath"];

        if (string.IsNullOrWhiteSpace(dataProtectionKeysPath))
        {
            dataProtectionKeysPath = Path.Combine(
                hostingEnvironment.ContentRootPath,
                "App_Data",
                "dp-keys");
        }

        Directory.CreateDirectory(dataProtectionKeysPath);

        context.Services.AddDataProtection()
            .SetApplicationName("ProjectBuilder")
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

        ConfigureBundles();
        ConfigureUrls(configuration);
        //ConfigureAuthentication(context);
        ConfigureAuthentication(context, configuration);
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);



        Configure<RazorPagesOptions>(options =>
        {
            ProjectBuilderRazorPagesAuthorizationConfigurer.Configure(options);
        });
    }


    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    //private void ConfigureAuthentication(ServiceConfigurationContext context)
    //{
    //    context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
    //    context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
    //    {
    //        options.IsDynamicClaimsEnabled = true;
    //    });
    //}

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddScoped<IEsecSecurity, EsecSecurity>();
        var securityType = configuration["SecurityType"];
        if (securityType == SecurityConstants.SecurityTypes.Esec)
        {
            return;
        }

        context.Services.AddAuthentication()
            .AddOpenIdConnect("AzureAd", "Microsoft Entra", options =>
            {
                // Tenant-based authority, NO B2C policy segment
                options.Authority = $"{configuration["AzureAd:Instance"]}{configuration["AzureAd:TenantId"]}/v2.0/";
                options.ClientId = configuration["AzureAd:ClientId"];
                options.ClientSecret = configuration["AzureAd:ClientSecret"];
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.CallbackPath = configuration["AzureAd:CallbackPath"];
                options.RequireHttpsMetadata = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
                options.SignInScheme = IdentityConstants.ExternalScheme;

                options.Events = new OpenIdConnectEvents
                {
                    OnTicketReceived = async ticketReceivedContext =>
                    {
                        // Entra v2 emits "email" / "preferred_username", NOT "emails"
                        var userEmailClaim = ticketReceivedContext.Principal.FindFirstValue("email")
                                             ?? ticketReceivedContext.Principal.FindFirstValue("preferred_username");
                        var userName = ticketReceivedContext.Principal.FindFirstValue("name");

                        if (userEmailClaim != null)
                        {
                            var unitOfWorkManager = context.Services.GetRequiredService<IUnitOfWorkManager>();
                            var userRepository = context.Services.GetRequiredService<IUserRepository>();
                            var httpContextAccessor = context.Services.GetRequiredService<IHttpContextAccessor>();
                            var dataFilter = context.Services.GetRequiredService<IDataFilter>();
                            var currentTenant = context.Services.GetRequiredService<ICurrentTenant>();
                            var tenantRepository = context.Services.GetRequiredService<ITenantRepository>();
                            var tenantManager = context.Services.GetRequiredService<ITenantManager>();
                            var dataSeeder = context.Services.GetRequiredService<IDataSeeder>();
                            var distributedEventBus = context.Services.GetRequiredService<IDistributedEventBus>();
                            var identityUserManager = context.Services.GetRequiredService<IdentityUserManager>();
                            var identityRoleManager = context.Services.GetRequiredService<IdentityRoleManager>();
                            var signInManager = context.Services.GetRequiredService<AbpSignInManager>();


                            var guidGenerator = context.Services.GetRequiredService<IGuidGenerator>();

                            using (var uow = unitOfWorkManager.Begin(requiresNew: true))
                            {
                                // Provision the user against ABP Identity (AbpUsers) instead of the custom table.
                                Volo.Abp.Identity.IdentityUser user;
                                using (dataFilter.Disable<IMultiTenant>())
                                {
                                    user = await identityUserManager.FindByEmailAsync(userEmailClaim);
                                }
                                if (user == null)
                                {
                                    user = new Volo.Abp.Identity.IdentityUser(
                                        guidGenerator.Create(),
                                        userEmailClaim,
                                        userEmailClaim,
                                        currentTenant.Id)
                                    {
                                        Name = userName
                                    };

                                    (await identityUserManager.CreateAsync(user)).CheckErrors();

                                    // Assign the default "Guest" role only if it exists (no seeding is performed here).
                                    if (await identityRoleManager.RoleExistsAsync("Guest"))
                                    {
                                        (await identityUserManager.AddToRoleAsync(user, "Guest")).CheckErrors();
                                    }
                                }
                                await uow.CompleteAsync();

                                // Promote the external Entra login into the app's own session. Without this,
                                // the user only ever holds the transient IdentityConstants.ExternalScheme
                                // cookie set by options.SignInScheme above, so SignInManager.IsSignedIn(User) —
                                // which ABP's own Account/Logout page checks — is always false, and Logout 404s.
                                await signInManager.SignInAsync(user, isPersistent: true);

                                // Redirect back to whatever host actually served this request instead of a
                                // fixed config URL, so this works the same on localhost and any deployment.
                                var httpContext = httpContextAccessor.HttpContext;
                                httpContext.Response.Redirect($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/");
                            }
                        }
                        else
                        {
                            // Handle the scenario where the email claim is missing from the token
                        }
                    },

                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        // Same fix as OnTicketReceived: derive the post-logout target from the
                        // request that's actually being served instead of a fixed config URL.
                        var request = context.HttpContext.Request;
                        var postLogoutRedirectUri = $"{request.Scheme}://{request.Host}/";

                        var logoutUri = $"{configuration["AzureAd:Instance"]}{configuration["AzureAd:TenantId"]}/oauth2/v2.0/logout";
                        logoutUri += $"?post_logout_redirect_uri={Uri.EscapeDataString(postLogoutRedirectUri)}";
                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },

                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.Prompt = "login";
                        return Task.CompletedTask;
                    }
                };
            });
    }




    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ProjectBuilderWebModule>();
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ProjectBuilderWebModule>();

            if (hostingEnvironment.IsDevelopment())
            {
                options.FileSets.ReplaceEmbeddedByPhysical<ProjectBuilderDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}ProjectBuilder.Domain.Shared", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<ProjectBuilderDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}ProjectBuilder.Domain", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<ProjectBuilderApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}ProjectBuilder.Application.Contracts", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<ProjectBuilderApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}ProjectBuilder.Application", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<ProjectBuilderHttpApiModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}ProjectBuilder.HttpApi", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<ProjectBuilderWebModule>(hostingEnvironment.ContentRootPath);
            }
        });
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new ProjectBuilderMenuContributor());
        });

        Configure<AbpToolbarOptions>(options =>
        {
            options.Contributors.Add(new ProjectBuilderToolbarContributor());
        });

        // Baseline CSP (Veracode flaw #6779 - no policy was previously set). Pages here load
        // from several CDNs and rely on inline <script> blocks throughout, so this allow-lists
        // those specific hosts plus 'unsafe-inline'/'unsafe-eval' rather than attempting a
        // nonce-based strict policy, which would need a full per-page audit to avoid breakage.
        Configure<AbpSecurityHeadersOptions>(options =>
        {
            options.UseContentSecurityPolicyHeader = true;
            options.ContentSecurityPolicyValue =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com https://use.fontawesome.com https://cdn.datatables.net https://js.arcgis.com https://d3js.org; " +
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com https://cdn.datatables.net https://js.arcgis.com https://fonts.googleapis.com; " +
                "img-src 'self' data: blob: https:; " +
                "font-src 'self' data: https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://use.fontawesome.com https://fonts.gstatic.com https://js.arcgis.com; " +
                "connect-src 'self' https:; " +
                "worker-src 'self' blob:; " +
                "child-src 'self' blob:; " +
                "frame-src 'self'; " +
                "frame-ancestors 'self'; " +
                "object-src 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self' https://*.ciamlogin.com;";
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(ProjectBuilderApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectBuilder API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        Console.WriteLine("🔥 APP STARTED 🔥");
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();
        //app.UseExceptionHandler("/ErrorPage/Error");

        app.UseForwardedHeaders();
        app.UseResponseCompression();

        if (env.IsDevelopment())
        {
            // app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            //app.UseErrorPage();
            app.UseHsts();
        }
        app.UseSession();
        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseRouting();
        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.Use(async (httpContext, next) =>
        {
            Log.Information("Request: {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            await next();

            Log.Information("Response: {StatusCode}",
                httpContext.Response.StatusCode);
        });
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        // app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectBuilder API");
        });
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            DashboardTitle = "Project Builder — Background Jobs",
            AppPath = "/Administration/BackgroundJobs"
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.Use(async (context, next) =>
        {
            if (!context.Request.Path.HasValue || context.Request.Path == "/")
            {
                if (!context.User.Identity?.IsAuthenticated == true)
                {
                    var config = context.RequestServices.GetRequiredService<IConfiguration>();
                    if (config["SecurityType"] == SecurityConstants.SecurityTypes.Esec)
                    {
                        // Forward only a well-formed ESEC auth code, encoded, to the fixed local callback action
                        var code = context.Request.Query["code"].ToString();
                        var target = EsecAuthCodePattern.IsMatch(code)
                            ? "/ESECAccount/AuthorizeESECUser?code=" + Uri.EscapeDataString(code)
                            : "/ESECAccount/AuthorizeESECUser";
                        context.Response.Redirect(target);
                        return;
                    }

                    context.Response.Redirect("/Account/Login");
                    return;
                }

                var permissionChecker = context.RequestServices.GetService(typeof(IPermissionChecker)) as IPermissionChecker;
                if (permissionChecker != null)
                {
                    if (await permissionChecker.IsGrantedAsync(DSSPermissions.TopBarPermissions.Dashboard))
                    {
                        context.Response.Redirect("/Dashboard");
                        return;
                    }
                    if (await permissionChecker.IsGrantedAsync(ScenarioPermissions.Default))
                    {
                        context.Response.Redirect("/Scenarios");
                        return;
                    }
                    if (await permissionChecker.IsGrantedAsync(Volo.Abp.Identity.IdentityPermissions.Users.Default))
                    {
                        context.Response.Redirect("/Identity/Users");
                        return;
                    }
                }

                context.Response.Redirect("/Account/AccessDenied");
                return;
            }

            await next();
        });
        app.UseConfiguredEndpoints();
    }
}