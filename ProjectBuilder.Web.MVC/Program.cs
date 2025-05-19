using ExcelWrapper;
using GisJsonHandler;
using log4net;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using PBLogic;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Repositories;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Services;
using ProjectBuilder.Services.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<ProjectBuilderDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 128 * 1024 * 1024; // 128 MB limit
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 128 * 1024 * 1024; // 128 MB limit for Kestrel
});

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddRazorPages().AddMicrosoftIdentityUI();   
builder.Services.AddSession(x=>
{
    x.IdleTimeout = TimeSpan.FromDays(30);
});
builder.Services.AddHttpContextAccessor();
builder.Services.RegisterAutoMapper();
builder.Services.AddLogging();
builder.Services.RegisterRepositories();
builder.Services.AddScoped<ICandidatePoolUnitOfWork, CandidatePoolUnitOfWork>();
builder.Services.AddScoped<IRunScenarioUnitOfWork,RunScenarioUnitOfWork>();
builder.Services.AddScoped<ITreatmentUnitOfWork, TreatmentUnitOfWork>();
builder.Services.AddScoped<IChartsNeedsUnitOfWork, ChartsNeedsUnitOfWork>();
builder.Services.AddScoped<IChartsPotentialBenefitsUnitOfWork, ChartsPotentialBenefitsUnitOfWork>();
DataManager.Log = LogManager.GetLogger("ProjectBuilder");
SymphonyConductor.Log = LogManager.GetLogger("ProjectBuilder");
ExcelHandler.Log = LogManager.GetLogger("ProjectBuilder");
JsonExporter.Log = LogManager.GetLogger("ProjectBuilder");

builder.Services.AddCors(options =>
                    options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(x => true)
                    .AllowCredentials();
                }));



// Enable Swagger (Optional for testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ProjectBuilder API", Version = "v1" });
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectBuilder API V1");
});


app.MapRazorPages();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseCors("CorsPolicy");
app.UseStaticFiles();
app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always
});
//app.UseSession(new SessionOptions
//{
    
//});
app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
