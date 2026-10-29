using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Entities;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace ProjectBuilder.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ReplaceDbContext(typeof(ISettingManagementDbContext))]
[ConnectionStringName("Default")]
public class ProjectBuilderDbContext :
    AbpDbContext<ProjectBuilderDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext,
    ISettingManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    public DbSet<UserClaimEntity> UserClaims { get; set; }
    public DbSet<UserEntity> AllUsers { get; set; }
    public DbSet<RoleEntity> AllRoles { get; set; }
    public DbSet<UserRoleEntity> UserRoles { get; set; }
    public DbSet<Scenario> Scenarios { get; set; }
    public DbSet<DssScenario> DssScenarios { get; set; }
    public DbSet<CandidatePool> Libraries { get; set; }
    public DbSet<Treatment> Treatments { get; set; }
    public DbSet<CountyEntity> Counties { get; set; }
    public DbSet<PAMSSectionSegment> PAMS { get; set; }
    public DbSet<BridgeReferenceData> BridgeReferenceData { get; set; }
    public DbSet<ParameterEntity> Parameters { get; set; }
    public DbSet<TreatmentParameterEntity> TreatmentParameters { get; set; }
    public DbSet<TreatmentCancellationMatrixEntity> TreatmentCancellationMatrix { get; set; }
    public DbSet<TreatmentType> TreatmentTypes { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ScenarioParamater> ScenarioParameters { get; set; }
    public DbSet<ScenarioBudget> ScenariosBudgets { get; set; }
    public DbSet<ScenarioBudgetFlat> ScenariosBudgetFalts { get; set; }
    public DbSet<DefaultSlack> DefaultSlacksValues { get; set; }
    public DbSet<AllNeedsEntity> AllNeeds { get; set; }
    public DbSet<BridgeNeedsEntity> BridgeNeeds { get; set; }
    public DbSet<PavementNeedsEntity> PavementNeeds { get; set; }
    public DbSet<AllPotentialBenefitEntity> AllPotentialBenefits { get; set; }
    public DbSet<BridgePotentialBenefitEntity> BridgePotentialBenefits { get; set; }
    public DbSet<PavementPotentialBenefitEntity> PavementPotentialBenefits { get; set; }
    public DbSet<ProjectSummaryEntity> ProjectSummaryEntities { get; set; }
    public DbSet<TreatmentSummaryEntity> TreatmentSummaryEntities { get; set; }
    public DbSet<BudgetEntity> Budgets { get; set; }
    public DbSet<UserTreatment> CustomTreatments { get; set; }
    public DbSet<ProjectTreatment> ProjectTreatments { get; set; }
    public DbSet<BudgetSpentEntity> BudgetSpentEntities { get; set; }
    public DbSet<CombinedProjectSummaryEntity> CombinedProjectSummaryEntities { get; set; }
    public DbSet<UserTreatmentType> UserTreatmentTypes { get; set; }
    public DbSet<BenefitParameterEntity> BenefitParameters { get; set; }
    public DbSet<DataSectionEntity> DataSections { get; set; }
    public DbSet<GoalEntity> Goals { get; set; }
    public DbSet<GoalWeightEntity> GoalWeights { get; set; }
    public DbSet<MeasureEntity> Measures { get; set; }
    public DbSet<MeasureScalingLinearEntity> MeasureScalingLinear { get; set; }
    public DbSet<MeasureWeightEntity> MeasureWeights { get; set; }
    public DbSet<OpenIddictApplicationEntity> OpenIddictApplications { get; set; }
    public DbSet<OpenIddictAuthorizationEntity> OpenIddictAuthorizations { get; set; }
    public DbSet<OpenIddictScopeEntity> OpenIddictScopes { get; set; }
    public DbSet<OpenIddictTokenEntity> OpenIddictTokens { get; set; }
    public DbSet<OtherParameterEntity> OtherParameters { get; set; }
    public DbSet<PoolParameterEntity> PoolParameters { get; set; }
    public DbSet<PoolParameterDataEntity> PoolParametersData { get; set; }
    public DbSet<ProjectCandidateEntity> ProjectCandidates { get; set; }
    public DbSet<DssProjectEntity> DssProjects { get; set; }
    public DbSet<RawImportedWorkCandidateEntity> RawImportedWorkCandidates { get; set; }
    public DbSet<ScenarioProjectObjectiveValueEntity> ScenarioProjectObjectiveValues { get; set; }
    public DbSet<ScenarioRunsLogEntity> ScenarioRunsLog { get; set; }
    public DbSet<SpoofProjectNorthwestRPOEntity> SpoofProjectsNorthwestRPO { get; set; }
    public DbSet<UserPoolEntity> UserPools { get; set; }
    public DbSet<ImportSessions> ImportSessions { get; set; }
    public DbSet<PoolScoreParameterEntity> PoolScoreParameters { get; set; }
    public DbSet<WorkCandidateEntity> WorkCandidates { get; set; }
    public DbSet<AssetTypeParameterDataEntity> AssetTypeParametersData { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; }

    // EditScenario (Analysis Method / Parameters / Budget Constraints)
    public DbSet<ScenarioFilter> ScenarioFilters { get; set; }
    public DbSet<ScenarioTreatmentShift> ScenarioTreatmentShifts { get; set; }
    public DbSet<BudgetConstraint> BudgetConstraints { get; set; }
    public DbSet<BudgetConstraintColumn> BudgetConstraintsColumns { get; set; }
    public DbSet<BudgetConstraintCriteria> BudgetConstraintsCriteria { get; set; }
    public DbSet<ScenarioBudgetWithAttribute> ScenarioBudgetWithAttributes { get; set; }
    public DbSet<CargoAttribute> CargoAttributes { get; set; }
    public DbSet<CargoData> CargoData { get; set; }
    public DbSet<FilteringAttribute> FilteringAttributes { get; set; }
    public DbSet<CargoAttributeOption> CargoAttributeOptions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public ProjectBuilderDbContext(DbContextOptions<ProjectBuilderDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();

        // This app is not multi-tenant, so ABP's IdentityUser.Leaved flag (a "user left the tenant"
        // marker, added in a newer ABP version) is never used. Ignore it so EF does not map/require a
        // Leaved column on AbpUsers. Remove this line if you later add the column or use multi-tenancy.
        builder.Entity<Volo.Abp.Identity.IdentityUser>().Ignore(u => u.Leaved);

        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

  
        builder.Entity<UserTreatmentType>(entity =>
        {
            entity.ToTable("tbl_pb_UserTreatments");
            entity.HasKey(e => new { e.Id });
        });
        builder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("tbl_identity_User");
            entity.HasKey(e => new { e.Id });
        });
        builder.Entity<RoleEntity>(entity =>
        {
            entity.ToTable("tbl_identity_Role");
            entity.HasKey(e => e.Id);

            // Tell EF the PK column is named "RoleId" in the DB
            entity.Property(e => e.Id).HasColumnName("RoleId");
        });
        builder.Entity<UserRoleEntity>(entity =>
        {
            entity.ToTable("tbl_identity_UserRoles");
            entity.HasKey(e => new { e.Id });
        });
        builder.Entity<UserClaimEntity>(entity =>
        {
            entity.ToTable("tbl_identity_UserClaims");
            entity.HasKey(e => new { e.Id });
        });
        builder.Entity<TreatmentParameterEntity>(entity =>
        {
            entity.ToTable("tbl_pb_UserTreatmentParameters");
            entity.HasKey(e => new { e.Id });
        });
        builder.Entity<TreatmentCancellationMatrixEntity>(entity =>
        {
            entity.HasKey(e => new { e.AssetTypeA, e.Id })
                  .IsClustered();
        });
        builder.Entity<CandidatePool>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_lib_TreatmentLibraries");
            entity.ToTable("tbl_lib_UserLibraries");
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("(getdate())")
                  .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50)
                  .HasDefaultValueSql("(user_name())");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.CandidatePoolNumber).ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50);
        });
        builder.Entity<UserTreatment>(entity =>
        {
            entity.HasKey(e => new { e.LibraryId, e.Id });

            entity.ToTable("tbl_lib_LibraryTreatments");

            entity.Property(e => e.Asset).HasMaxLength(50);
            entity.Property(e => e.AssetType)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.Treatment).HasMaxLength(100);
            entity.Property(e => e.UserTreatmentTypeNo).HasDefaultValueSql("((0))");

        });
        builder.Entity<Scenario>(entity =>
        {
            entity.HasKey(e => e.Id);

        });
        builder.Entity<CountyEntity>(entity =>
        {
            entity.HasKey(e => new { e.District, e.Id })
                  .IsClustered();
        });
        builder.Entity<ParameterEntity>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Parmfamily })
                  .IsClustered();
        });
        builder.Entity<Treatment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_ImportedTreatments");

            entity.ToTable("tbl_pb_ImportedTreatments");

            entity.Property(e => e.Asset).HasMaxLength(50);
            entity.Property(e => e.AssetType)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.IsCommitted).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsIsolatedBridge).HasDefaultValueSql("((0))");
            entity.Property(e => e.PopulatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PopulatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("(user_name())");
        });
        builder.Entity<ProjectTreatment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_pb_ExtendedImportedTreatments");
            entity.ToView("vw_pb_ui_ScenarioTreatments")
                  .ToTable("tbl_pb_ExtendedImportedTreatments");
        });
        builder.Entity<PAMSSectionSegment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.District, e.CountyId, e.Route, e.FromSection }, "IX_tbl_pams_MaintainableAssetsSegmentation_DCRS");
            entity.Property(e => e.Id).ValueGeneratedNever();
        });
        builder.Entity<TreatmentType>(entity =>
        {
            entity.ToTable("TreatmentTypes");
            entity.HasKey(e => e.Id)
                  .HasName("PK__Treatmen__1015D11E07EE5045");
            entity.HasIndex(e => e.AssetType, "IX_TreatmentTypes_AssetType");
        });
        builder.Entity<Project>(entity =>
        {
            entity.HasKey(e => new { e.ScenarioId, e.Id });
            entity.ToView("vw_pb_ui_ScenarioProjects");
            entity.ToTable("tbl_pb_ExtendedTimewiseConstrainedProjects");
            // The view stopped filtering out unfunded projects (script 60); this keeps every query
            // funded-only unless it opts out with IgnoreQueryFilters().
            entity.HasQueryFilter(p => p.Selected == "Y");
        });
        builder.Entity<ScenarioParamater>(entity =>
        {
            entity.HasKey(e => new { e.ScenarioId, e.Id });
            entity.ToView("vw_pb_ScenarioParameters");
            entity.ToTable("tbl_pb_ScenParm");
            entity.Property(e => e.ParmDescription).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(e => e.ParmName).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        });
        builder.Entity<ScenarioBudget>(entity =>
        {
            entity.HasKey(e => new { e.ScenarioId, e.District, e.Id, e.IsInterstate, e.AssetType })
                  .HasName("PK_tbl_pb_ExtendedScenBudget");
        });
        // ── EditScenario (Analysis Method / Parameters / Budget Constraints) ──
        builder.Entity<ScenarioFilter>(entity =>
        {
            // FilterNo identifies a criteria GROUP, not a single row — a group can hold multiple
            // AND-ed conditions, so the real uniqueness (and the DB's PK_tbl_pb_ScenarioFilters)
            // spans every column, matching one row per condition.
            entity.HasKey(e => new { e.ScenId, e.FilterNo, e.AssetType, e.AttributeName, e.Operation, e.TextValue });
        });
        builder.Entity<ScenarioTreatmentShift>(entity =>
        {
            entity.HasKey(e => new { e.ScenId, e.Treatment });
        });
        builder.Entity<BudgetConstraint>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        builder.Entity<BudgetConstraintColumn>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        builder.Entity<BudgetConstraintCriteria>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        builder.Entity<ScenarioBudgetWithAttribute>(entity =>
        {
            entity.HasKey(e => new { e.ScenId, e.YearWork, e.District, e.IsInterstate, e.AssetType, e.AttributeName, e.AttributeValue });
        });
        builder.Entity<CargoAttribute>(entity =>
        {
            entity.HasKey(e => new { e.LibraryId, e.AttributeNo, e.AssetType });
        });
        builder.Entity<CargoData>(entity =>
        {
            entity.HasKey(e => new { e.ImportSessionId, e.LibraryId, e.AssetType, e.ImportTimeGeneratedGuid, e.AttributeNo });
        });
        builder.Entity<FilteringAttribute>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_lib_FilteringAttributesLookUp");
        });
        builder.Entity<CargoAttributeOption>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_CargoAttributeOptions");
        });
        builder.Entity<DefaultSlack>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        builder.Entity<AllNeedsEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_AllNeeds");
        });
        builder.Entity<BridgeNeedsEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_BridgeNeeds");
        });
        builder.Entity<PavementNeedsEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_PavementNeeds");
        });
        builder.Entity<AllPotentialBenefitEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_AllPotentialBenefits");
        });
        builder.Entity<BridgePotentialBenefitEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_BridgePotentialBenefits");
        });
        builder.Entity<PavementPotentialBenefitEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_PavementPotentialBenefits");
        });
        builder.Entity<ProjectSummaryEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_ProjectSummary");
        });
        builder.Entity<TreatmentSummaryEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_TreatmentSummary");
        });
        builder.Entity<BudgetEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_BudgetAvailable");
        });
        builder.Entity<BudgetSpentEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_BudgetSpent");
        });
        builder.Entity<CombinedProjectSummaryEntity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_pb_ui_Projects");
        });
        builder.Entity<AraEnumerations>(entity =>
        {
            entity.HasNoKey();
            entity.HasIndex(e => new { e.Id, e.EnumName }, "tbl_ara_Enumerations")
                 .IsUnique()
                 .IsClustered();
        });
        builder.Entity<BridgeToPavements>(entity =>
        {
            entity.HasNoKey();
            entity.HasIndex(e => new { e.Id, e.BridgeId }, "tbl_pb_BridgeToPavements")
                 .IsUnique()
                 .IsClustered();
        });
        builder.Entity<BridgeReferenceData>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_pb_BridgeReferenceDataFromPennshare");
            entity.HasIndex(e => new { e.District, e.CountyCode, e.Route, e.Segment }, "IX_tbl_pb_BridgeReferenceDataFromPennshare_DCRS");
        });
        builder.Entity<ImportSessions>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("tbl_pb_ImportSessions");
        });
        builder.Entity<Setting>(b =>
        {
            b.ToTable("tbl_dss_AbpSettings");
        });
        builder.Entity<SettingDefinitionRecord>(b =>
        {
            b.ToTable("tbl_dss_AbpSettingDefinitions");
        });
        builder.Entity<BenefitParameterEntity>(entity =>
        {
            entity.ToTable("tbl_dss_BenefitParameters");
            entity.HasKey(e => new { e.Family, e.Code });

            entity.Property(e => e.Family)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Code)
                  .IsRequired();

            entity.Property(e => e.Description)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");
        });
        builder.Entity<DataSectionEntity>(entity =>
        {
            entity.ToTable("tbl_dss_DataSections");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Notes)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");

            entity.Property(e => e.ShortName)
                  .HasMaxLength(20);
        });
        builder.Entity<GoalEntity>(entity =>
        {
            entity.ToTable("tbl_dss_Goals");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.Name)
                  .HasMaxLength(50);

            entity.Property(e => e.Description)
                  .HasColumnType("text");

            entity.Property(e => e.LastModificationTime)
                  .HasColumnType("datetime");
        });
        builder.Entity<GoalWeightEntity>(entity =>
        {
            entity.ToTable("tbl_dss_GoalWeights");
            entity.HasNoKey();

            entity.Property(e => e.GoalId)
                  .HasColumnType("int");

            entity.Property(e => e.Weight)
                  .HasColumnType("float");
        });
        builder.Entity<MeasureEntity>(entity =>
        {
            entity.ToTable("tbl_dss_Measures");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.Name)
                  .HasColumnName("MeasureName")
                  .HasMaxLength(50);

            entity.Property(e => e.Description)
                  .HasColumnName("MeasureDescription")
                  .HasColumnType("text");

            entity.Property(e => e.LastModificationTime)
                  .HasColumnType("datetime");
        });
        builder.Entity<MeasureScalingLinearEntity>(entity =>
        {
            entity.ToTable("tbl_dss_MeasureScalingLinear");
            entity.HasKey(e => e.MeasureId);

            entity.Property(e => e.MeasureId)
                  .HasColumnType("int")
                  .ValueGeneratedNever();

            entity.Property(e => e.MinScore)
                  .HasColumnType("int");

            entity.Property(e => e.MeasureScoreA)
                  .HasColumnType("float");

            entity.Property(e => e.MaxScore)
                  .HasColumnType("float");

            entity.Property(e => e.MeasureScoreB)
                  .HasColumnType("float");

            entity.Property(e => e.Slope)
                  .HasColumnType("float");

            entity.Property(e => e.Intercept)
                  .HasColumnType("float");
        });
        builder.Entity<MeasureWeightEntity>(entity =>
        {
            entity.ToTable("tbl_dss_MeasureWeights");
            entity.HasNoKey();

            entity.Property(e => e.MeasureId)
                  .HasColumnType("int");

            entity.Property(e => e.GoalId)
                  .HasColumnType("int");

            entity.Property(e => e.Weight)
                  .HasColumnType("float");
        });
        builder.Entity<OpenIddictApplicationEntity>(entity =>
        {
            entity.ToTable("tbl_dss_OpenIddictApplications");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.ApplicationType)
                  .HasMaxLength(50);

            entity.Property(e => e.ClientId)
                  .HasMaxLength(100);

            entity.Property(e => e.ClientSecret)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ClientType)
                  .HasMaxLength(50);

            entity.Property(e => e.ConsentType)
                  .HasMaxLength(50);

            entity.Property(e => e.DisplayName)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.DisplayNames)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.JsonWebKeySet)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Permissions)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.PostLogoutRedirectUris)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Properties)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.RedirectUris)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Requirements)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Settings)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ClientUri)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.LogoUri)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ExtraProperties)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ConcurrencyStamp)
                  .IsRequired()
                  .HasMaxLength(40);

            entity.Property(e => e.CreationTime)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.LastModificationTime)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.DeletionTime)
                  .HasColumnType("datetime2(7)");
        });
        builder.Entity<OpenIddictAuthorizationEntity>(entity =>
        {
            entity.ToTable("tbl_dss_OpenIddictAuthorizations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.CreationDate)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.Properties)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Scopes)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Status)
                  .HasMaxLength(50);

            entity.Property(e => e.Subject)
                  .HasMaxLength(400);

            entity.Property(e => e.Type)
                  .HasMaxLength(50);

            entity.Property(e => e.ExtraProperties)
                  .IsRequired()
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ConcurrencyStamp)
                  .IsRequired()
                  .HasMaxLength(40);

            entity.Property(e => e.CreationTime)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.LastModificationTime)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.DeletionTime)
                  .HasColumnType("datetime2(7)");
        });
        builder.Entity<OpenIddictScopeEntity>(entity =>
        {
            entity.ToTable("tbl_dss_OpenIddictScopes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Descriptions)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.DisplayName)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.DisplayNames)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Name)
                  .HasMaxLength(200);

            entity.Property(e => e.Properties)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Resources)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ExtraProperties)
                  .IsRequired()
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ConcurrencyStamp)
                  .IsRequired()
                  .HasMaxLength(40);

            entity.Property(e => e.CreationTime)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.LastModificationTime)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.DeletionTime)
                  .HasColumnType("datetime2(7)");
        });
        builder.Entity<OpenIddictTokenEntity>(entity =>
        {
            entity.ToTable("tbl_dss_OpenIddictTokens");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.CreationDate)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.ExpirationDate)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.Payload)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.Properties)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.RedemptionDate)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.ReferenceId)
                  .HasMaxLength(100);

            entity.Property(e => e.Status)
                  .HasMaxLength(50);

            entity.Property(e => e.Subject)
                  .HasMaxLength(400);

            entity.Property(e => e.Type)
                  .HasMaxLength(50);

            entity.Property(e => e.ExtraProperties)
                  .IsRequired()
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.ConcurrencyStamp)
                  .IsRequired()
                  .HasMaxLength(40);

            entity.Property(e => e.CreationTime)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.LastModificationTime)
                  .HasColumnType("datetime2(7)");

            entity.Property(e => e.DeletionTime)
                  .HasColumnType("datetime2(7)");
        });
        builder.Entity<OtherParameterEntity>(entity =>
        {
            entity.ToTable("tbl_dss_OtherParameters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Code").HasMaxLength(10);

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Val).HasColumnType("float");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });
        builder.Entity<PoolParameterEntity>(entity =>
        {
            entity.ToTable("tbl_dss_PoolParameters");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.ItemName)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.ItemDescription)
                  .HasMaxLength(255);

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");
        });
        builder.Entity<PoolParameterDataEntity>(entity =>
        {
            entity.ToTable("tbl_dss_PoolParametersData");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.LibraryId)
                  .IsRequired();

            entity.Property(e => e.DataSectionId);

            entity.Property(e => e.ItemId);

            entity.Property(e => e.ItemValue)
                  .HasColumnType("float");

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");
        });
        builder.Entity<PoolScoreParameterEntity>(entity =>
        {
            entity.ToTable("tbl_dss_PoolScoreParameters");
            entity.HasKey(e => new { e.PoolId, e.Measure });

            entity.Property(e => e.Measure).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Min).HasColumnType("float");
            entity.Property(e => e.ScoreAtMin).HasColumnType("float");
            entity.Property(e => e.Max).HasColumnType("float");
            entity.Property(e => e.ScoreAtMax).HasColumnType("float");
            entity.Property(e => e.Slope).HasColumnType("float");
            entity.Property(e => e.SafetyWeight).HasColumnType("float");
            entity.Property(e => e.MobilityWeight).HasColumnType("float");
            entity.Property(e => e.EquityWeight).HasColumnType("float");
            entity.Property(e => e.EnvWeight).HasColumnType("float");
            entity.Property(e => e.CondWeight).HasColumnType("float");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });
        builder.Entity<ProjectCandidateEntity>(entity =>
        {
            entity.ToTable("tbl_dss_ProjectCandidates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.LibraryId)
                  .HasColumnName("PoolId")
                  .IsRequired();

            entity.Property(e => e.District)
                  .HasColumnType("tinyint");

            entity.Property(e => e.Cnty)
                  .HasColumnType("tinyint");

            entity.Property(e => e.Description)
                  .HasMaxLength(250);

            entity.Property(e => e.NumberOfWorkCandidates)
                  .HasColumnType("int");

            entity.Property(e => e.Treatment)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.YearEarliest)
                  .HasColumnType("int");

            entity.Property(e => e.YearLatest)
                  .HasColumnType("int");

            entity.Property(e => e.TotalCost)
                  .HasColumnType("float");

            entity.Property(e => e.TotalScaledBenefit)
                  .HasColumnType("float");

            entity.Property(e => e.SafetyScore)
                  .HasColumnType("float");

            entity.Property(e => e.MobilityAndEconomyScore)
                  .HasColumnName("MobilityAndEconomyScore")
                  .HasColumnType("float");

            entity.Property(e => e.EquityAndAccessScore)
                  .HasColumnType("float");

            entity.Property(e => e.ResilienceAndEnvironmentScore)
                  .HasColumnName("ResilienceAndEnvironmentScore")
                  .HasColumnType("float");

            entity.Property(e => e.ConditionAndPerformanceScore)
                  .HasColumnName("ConditionAndPerformanceScore")
                  .HasColumnType("float");

            entity.Property(e => e.TotalScore)
                  .HasColumnType("float");

            entity.Property(e => e.RelativeEfficiency)
                  .HasColumnType("float");

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");

            entity.Property(e => e.RelativeEfficiencySetAt)
                  .HasColumnType("datetime");
        });

        builder.Entity<DssProjectEntity>(entity =>
        {
            entity.ToTable("tbl_dss_Projects");
            entity.HasNoKey();

            entity.Property(e => e.Id)
                  .HasColumnType("int");

            entity.Property(e => e.PlanningPartner)
                  .HasColumnType("int");

            entity.Property(e => e.District)
                  .HasColumnType("int");

            entity.Property(e => e.County)
                  .HasColumnType("int");

            entity.Property(e => e.Description)
                  .HasColumnType("text");

            entity.Property(e => e.EarliestYear)
                  .HasColumnType("int");

            entity.Property(e => e.LatestYear)
                  .HasColumnType("int");

            entity.Property(e => e.Cost)
                  .HasColumnType("float");

            entity.Property(e => e.SafetyScore)
                  .HasColumnType("float");

            entity.Property(e => e.MobilityAndEconomyScore)
                  .HasColumnName("MobilityAndEconomySc...")
                  .HasColumnType("float");

            entity.Property(e => e.EquityAndAccessScore)
                  .HasColumnType("float");

            entity.Property(e => e.ResilienceAndEnvironmentScore)
                  .HasColumnName("ResilienceAndEnvironme...")
                  .HasColumnType("float");

            entity.Property(e => e.ConditionAndPerformanceScore)
                  .HasColumnName("ConditionAndPerforman...")
                  .HasColumnType("float");

            entity.Property(e => e.TotalScore)
                  .HasColumnType("float");

            entity.Property(e => e.RelativeEfficiency)
                  .HasColumnType("float");
        });
        builder.Entity<RawImportedWorkCandidateEntity>(entity =>
        {
            entity.ToTable("tbl_dss_RawImportedWorkCandidates");
            entity.HasNoKey();

            entity.Property(e => e.ProjectID)
                  .HasMaxLength(50);

            entity.Property(e => e.District)
                  .HasColumnType("tinyint");

            entity.Property(e => e.County)
                  .HasMaxLength(50);

            entity.Property(e => e.ToSection)
                  .HasMaxLength(50);

            entity.Property(e => e.BRKEY)
                  .HasMaxLength(50);

            entity.Property(e => e.BRIDGE_ID)
                  .HasMaxLength(50);

            entity.Property(e => e.AssetType)
                  .HasMaxLength(1);

            entity.Property(e => e.Description)
                  .HasMaxLength(255);

            entity.Property(e => e.Pennsylvania_Byway)
                  .HasMaxLength(1);

            entity.Property(e => e.Consistency_with_Land_Use_Plans)
                  .HasMaxLength(1);

            entity.Property(e => e.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations)
                  .HasColumnType("float");

            entity.Property(e => e.Support_for_Environmental_Justice)
                  .HasMaxLength(1);

            entity.Property(e => e.Treatment)
                  .HasMaxLength(50);
        });
        builder.Entity<ScenarioProjectObjectiveValueEntity>(entity =>
        {
            entity.ToTable("tbl_dss_ScenarioProjectObjectiveValues");
            entity.HasKey(e => new { e.ScenId, e.ProjectId });

            entity.Property(e => e.RelativeEfficiency)
                  .HasColumnType("float");

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");
        });
        builder.Entity<ScenarioRunsLogEntity>(entity =>
        {
            entity.ToTable("tbl_dss_ScenarioRunsLog");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.RunNo)
                  .IsRequired()
                  .HasColumnType("int");

            entity.Property(e => e.ScenNo)
                  .HasColumnType("int");

            entity.Property(e => e.StartedAt)
                  .IsRequired()
                  .HasColumnType("datetime");

            entity.Property(e => e.StartedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.Status)
                  .HasMaxLength(50);

            entity.Property(e => e.EndedAt)
                  .HasColumnType("datetime");

            entity.Property(e => e.Notes)
                  .HasColumnType("nvarchar(MAX)");
        });
        builder.Entity<SpoofProjectNorthwestRPOEntity>(entity =>
        {
            entity.ToTable("tbl_dss_SpoofProjects_NorthwestRPO");
            entity.HasNoKey();

            entity.Property(e => e.ProjectID).HasMaxLength(50);
            entity.Property(e => e.District).HasColumnType("tinyint");
            entity.Property(e => e.County).HasMaxLength(50);
            entity.Property(e => e.ToSection).HasMaxLength(50);
            entity.Property(e => e.BRKEY).HasMaxLength(50);
            entity.Property(e => e.BRIDGE_ID).HasMaxLength(50);
            entity.Property(e => e.AssetType).HasMaxLength(1);
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.Supported_by_RSA_Safety_Study).HasMaxLength(1);
            entity.Property(e => e.Supports_Hazard_Mitigation).HasMaxLength(1);
            entity.Property(e => e.Improves_Access_to_Emergency_Sevices).HasMaxLength(1);
            entity.Property(e => e.LFAR_BOF_Eligible).HasMaxLength(1);
            entity.Property(e => e.BPN).HasMaxLength(50);
            entity.Property(e => e.Bike_Commute_Share).HasMaxLength(1);
            entity.Property(e => e.National_Highway_System).HasMaxLength(1);
            entity.Property(e => e.Pennsylvania_Byway).HasMaxLength(1);
            entity.Property(e => e.Consistency_with_Land_Use_Plans).HasMaxLength(1);
            entity.Property(e => e.Change_in_Accessible_Destinations).HasColumnType("smallint");
            entity.Property(e => e.Support_for_Environmental_Justice).HasMaxLength(1);
            entity.Property(e => e.Facilitiates_Transit_Use).HasMaxLength(1);
            entity.Property(e => e.Facilitiates_Active_Transportation).HasMaxLength(1);
            entity.Property(e => e.Support_for_Job_Growth).HasMaxLength(1);
            entity.Property(e => e.Support_for_Tourism).HasMaxLength(1);
            entity.Property(e => e.Support_for_Economic_Goals).HasMaxLength(1);
            entity.Property(e => e.Support_for_Recreation).HasMaxLength(1);
            entity.Property(e => e.Consistency_with_Existing_Plans).HasMaxLength(1);
        });
        builder.Entity<UserPoolEntity>(entity =>
        {
            entity.ToTable("tbl_dss_UserPools");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.LibNo)
                  .UseIdentityColumn()
                  .IsRequired()
                  .HasColumnType("int");

            entity.Property(e => e.UserId)
                  .IsRequired();

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(MAX)");

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");

            entity.Property(e => e.IsActive)
                  .HasColumnType("bit");

            entity.Property(e => e.UpdatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("datetime");

            entity.Property(e => e.IsSharedLibrary)
                  .HasColumnType("bit");
        });
        builder.Entity<WorkCandidateEntity>(entity =>
        {
            entity.ToTable("tbl_dss_WorkCandidates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.UserLibraryId)
                  .HasColumnName("PoolId");

            entity.Property(e => e.ImportSessionId)
                  .IsRequired();

            entity.Property(e => e.ImportTimeGeneratedId)
                  .IsRequired();

            entity.Property(e => e.AssetType)
                  .HasMaxLength(1);

            entity.Property(e => e.District)
                  .HasColumnType("tinyint");

            entity.Property(e => e.Cnty)
                  .HasColumnType("tinyint");

            entity.Property(e => e.Route)
                  .HasColumnType("int");

            entity.Ignore(e => e.Direction);
            entity.Ignore(e => e.Offset);
            entity.Ignore(e => e.IsIsolatedBridge);
            entity.Ignore(e => e.RelativeEfficiency);
            entity.Ignore(e => e.Safety_Score);
            entity.Ignore(e => e.Mobility_and_Access_Score);
            entity.Ignore(e => e.Equity_and_Access_Score);
            entity.Ignore(e => e.Resilience_and_Environment_Score);
            entity.Ignore(e => e.Condition_and_Performance_Score);

            entity.Property(e => e.FromSection)
                  .HasColumnType("int");

            entity.Property(e => e.ToSection)
                  .HasMaxLength(50);

            entity.Property(e => e.BRKEY)
                  .HasMaxLength(50);

            entity.Property(e => e.BRIDGE_ID)
                  .HasMaxLength(50);

            entity.Property(e => e.Description)
                  .HasMaxLength(255);

            entity.Property(e => e.Cost)
                  .HasColumnType("float");

            entity.Property(e => e.Benefit)
                  .HasColumnName("Predicted_Scaled_Benefit")
                  .HasColumnType("float");

            entity.Property(e => e.SafetyScore)
                  .HasColumnName("Safety_Score")
                  .HasColumnType("float");

            entity.Property(e => e.MobilityAndEconomyScore)
                  .HasColumnName("Mobility_and_Access_Score")
                  .HasColumnType("float");

            entity.Property(e => e.EquityAndAccessScore)
                  .HasColumnName("Equity_and_Access_Score")
                  .HasColumnType("float");

            entity.Property(e => e.ResilienceAndEnvironmentScore)
                  .HasColumnName("Resilience_and_Environment_Score")
                  .HasColumnType("float");

            entity.Property(e => e.ConditionAndPerformanceScore)
                  .HasColumnName("Condition_and_Performance_Score")
                  .HasColumnType("float");

            entity.Property(e => e.TotalScore)
                  .HasColumnName("Total_Score")
                  .HasColumnType("float");

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");
        });
        builder.Entity<AssetTypeParameterDataEntity>(entity =>
        {
            entity.ToTable("tbl_dss_AssetTypeParametersData");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("(newid())");

            entity.Property(e => e.AssetType)
                  .IsRequired()
                  .HasMaxLength(1);

            entity.Property(e => e.BenefitScalingFactor)
                  .IsRequired()
                  .HasColumnType("float");

            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime");
        });

        OnModelCreatingPartial(builder);
    }
    public void OnModelCreatingPartial(ModelBuilder builder)
    {

    }

    /* Configure your own tables/entities inside here */

    //builder.Entity<YourEntity>(b =>
    //{
    //    b.ToTable(ProjectBuilderConsts.DbTablePrefix + "YourEntities", ProjectBuilderConsts.DbSchema);
    //    b.ConfigureByConvention(); //auto configure for the base class props
    //    //...
    //});
}


