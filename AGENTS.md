# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

ProjectBuilder is an ABP Framework-based enterprise application for PennDOT infrastructure project planning and optimization. It enables project prioritization through scenario analysis, budget optimization, and treatment pool management, integrating with BAMS (Bridge Asset Management System) and PAMS (Pavement Asset Management System) data sources.

## Technology Stack

- **.NET 10.0**, ABP Framework v10.1.0 (ASP.NET Core)
- **Web UI**: Razor Pages + MVC, LeptonXLite theme
- **Database**: SQL Server via EF Core
- **Auth**: OpenIddict (OAuth 2.0/OIDC) + Azure AD B2C
- **Testing**: xUnit, NSubstitute, Shouldly
- **Client packages**: Node v18+ managed via `abp install-libs`

## Build & Development Commands

```bash
# Restore dependencies
dotnet restore

# Install client-side packages
abp install-libs

# Build solution
dotnet build ProjectBuilder.sln

# Apply migrations and seed (required before first run)
cd src/ProjectBuilder.DbMigrator && dotnet run

# Run web application (https://localhost:44315)
cd src/ProjectBuilder.Web && dotnet run

# Run all tests
dotnet test ProjectBuilder.sln

# Run tests in a specific project
dotnet test test/ProjectBuilder.Application.Tests/ProjectBuilder.Application.Tests.csproj

# Run a single test by name
dotnet test --filter "FullyQualifiedName~CandidatePoolAppServiceTest"

# Create a new EF migration
cd src/ProjectBuilder.EntityFrameworkCore
dotnet ef migrations add MigrationName -p . -s ../ProjectBuilder.Web
```

## Solution Structure

ABP's standard layered monolith:

```
src/
  ProjectBuilder.Domain/                  # Entities (64), repository interfaces (32), business logic
  ProjectBuilder.Domain.Shared/           # Enums, constants, localization resources
  ProjectBuilder.Application.Contracts/   # DTOs and application service interfaces
  ProjectBuilder.Application/             # Application services (31) — orchestration & mapping
  ProjectBuilder.EntityFrameworkCore/     # DbContext, EF configurations, migrations
  ProjectBuilder.HttpApi/                 # Auto-generated API controllers
  ProjectBuilder.HttpApi.Client/          # Auto-generated HTTP client proxies
  ProjectBuilder.Web/                     # Razor Pages UI (primary entry point)
  ProjectBuilder.DbMigrator/              # CLI tool for migrations and data seeding
  ProjectBuilder.Engine/
    ├── PBLogic/                          # Core optimization algorithms
    ├── BAMSDataImporter/                 # Bridge asset data import
    ├── PAMSDataImporter/                 # Pavement asset data import
    ├── BulkInserter/                     # Bulk database operations
    ├── ExcelWrapper/                     # Excel file handling (EPPlus)
    ├── GisJsonHandler/                   # GIS/geographic JSON processing
    └── LargeJSONParser/                  # Streaming JSON parser

test/
  ProjectBuilder.Application.Tests/
  ProjectBuilder.Domain.Tests/
  ProjectBuilder.EntityFrameworkCore.Tests/
  ProjectBuilder.Web.Tests/
  ProjectBuilder.TestBase/                # Shared test infrastructure and DI setup
```

## Architecture Patterns

### ABP Module System

Every project has an `*Module` class that declares its dependencies and registers services:

```csharp
[DependsOn(typeof(ProjectBuilderApplicationModule), typeof(ProjectBuilderEntityFrameworkCoreModule))]
public class ProjectBuilderWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context) { ... }
    public override void OnApplicationInitialization(ApplicationInitializationContext context) { ... }
}
```

The initialization order follows the dependency graph:
`Web → HttpApi → Application → EntityFrameworkCore → Domain`

### DDD Layering

- Business logic lives in domain entities and domain services, not in application services.
- Application services (`ApplicationService` subclass) handle DTO ↔ entity mapping and orchestrate repository calls.
- All repository access is through interfaces (`ICandidatePoolRepository`, etc.) defined in the Domain layer and implemented in EntityFrameworkCore.
- AutoMapper profile: `ProjectBuilderApplicationAutoMapperProfile.cs` in the Application project.

### Adding a New Feature

1. Define entity in `ProjectBuilder.Domain/Entities/`
2. Add repository interface in `ProjectBuilder.Domain/Repositories/`
3. Define DTOs in `ProjectBuilder.Application.Contracts/`
4. Implement app service in `ProjectBuilder.Application/Services/`
5. Add Razor Page in `ProjectBuilder.Web/Pages/`
6. Create EF migration and run DbMigrator

### Web Pages Structure

Key pages in `src/ProjectBuilder.Web/Pages/`:
- `/Dashboard` — main entry point
- `/EditScenario` — scenario creation/modification
- `/ImportScreen` — BAMS/PAMS data import
- `/Outputs/Charts`, `/Outputs/Reports` — result visualization
- `/CandidatePools`, `/Pools` — pool management
- `/Configuration/Goals`, `/Configuration/GoalWeights` — system settings

### Multi-Tenancy

ABP's multi-tenancy is enabled. Queries automatically scope to the current tenant context; no manual filtering required in most cases.

### Authentication

- OpenIddict serves as the local OAuth 2.0/OIDC server
- Azure AD B2C is configured as an external identity provider
- Roles used: `operator`, `admin`, `mapuser`
- Permissions managed via ABP's permission system

## Configuration

`appsettings.json` locations: `src/ProjectBuilder.Web/` and `src/ProjectBuilder.DbMigrator/`

Key settings:
- `ConnectionStrings.Default` — SQL Server connection string
- `App.SelfUrl` — application base URL (used for auth redirects)
- `AzureAdB2C` — external IdP credentials (never commit live values to public repos)
- `StringEncryption.DefaultPassPhrase` — data encryption key

For production OpenIddict signing, generate and place `openiddict.pfx` in `src/ProjectBuilder.Web/`:
```bash
dotnet dev-certs https -v -ep openiddict.pfx -p <password>
```

## Testing Patterns

Tests use `ProjectBuilderTestBase` for DI setup and `ProjectBuilderApplicationTestBase` for integration tests with a real DbContext. NSubstitute is used for repository mocks in unit tests.

```csharp
public class CandidatePoolAppServiceTest : ApplicationServiceTestBase
{
    private readonly ICandidatePoolAppService _appService;
    public CandidatePoolAppServiceTest() => _appService = GetRequiredService<ICandidatePoolAppService>();

    [Fact]
    public async Task GetCandidatePoolDataAsync_Should_Return_Data() { ... }
}
```

## Key Notes

- Always run `ProjectBuilder.DbMigrator` before the first run or after adding migrations — `dotnet ef database update` is not used here.
- `appsettings.json` in Web and DbMigrator projects may contain live Azure/database credentials; handle with care.
- Pages use `HttpContext.Session` for role caching — session middleware is configured in `Program.cs`.
- Engine projects (`PBLogic`, `GisJsonHandler`, etc.) contain domain-specific algorithms and are consumed via DI in application services.
