using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectBuilder.DataAccess.Entities;
using System;
using System.Diagnostics.SymbolStore;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public partial class ProjectBuilderDbContext : DbContext
    {   
        #region DbSets
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserClaimEntity> UserClaims { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<UserRoleEntity> UserRoles { get; set; }
        public DbSet<Scenario> Scenarios { get; set; }
        public DbSet<CandidatePool> Libraries { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<CountyEntity> Counties { get; set; }
        public DbSet<PAMSSectionSegment> PAMS { get; set; }
        public DbSet<ParameterEntity> Parameters { get; set; }
        public DbSet<TreatmentParameterEntity> TreatmentParameters { get; set; }
        public DbSet<TreatmentCancellationMatrixEntity> TreatmentCancellationMatrix { get; set; }
        public DbSet<TreatmentType> TreatmentTypes { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ScenarioParamater> ScenarioParameters { get; set; }
        public DbSet<ScenarioBudget> ScenariosBudgets { get; set; }
        public DbSet<DefaultSlack> DefaultSlacksValues{ get; set; }
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
        public DbSet<CargoData> CargoData { get; set; }
        public DbSet<CargoAttributes> CargoAttributes { get; set; }
        #endregion

        public ProjectBuilderDbContext(DbContextOptions options) : base(options)
        {
           
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CargoData>(entity =>
            {
                entity.ToTable("tbl_pb_CargoData");
                entity.HasKey(e => new { e.EntityId, e.AttributeNo  });
            });
            modelBuilder.Entity<CargoAttributes>(entity =>
            {
                entity.ToTable("tbl_pb_CargoAttributes");
                entity.HasKey(e => new { e.EntityId });
            });
            modelBuilder.Entity<UserTreatmentType>(entity =>
            {
                entity.ToTable("tbl_pb_UserTreatments");
                entity.HasKey(e => new { e.EntityId });
            });
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("tbl_identity_User");
                entity.HasKey(e => new { e.EntityId });
            });
            modelBuilder.Entity<RoleEntity>(entity =>
            {
                entity.ToTable("tbl_identity_Role");
                entity.HasKey(e => new { e.EntityId });
            });
            modelBuilder.Entity<UserRoleEntity>(entity =>
            {
                entity.ToTable("tbl_identity_UserRoles");
                entity.HasKey(e => new { e.EntityId });
            });
            modelBuilder.Entity<UserClaimEntity>(entity =>
            {
                entity.ToTable("tbl_identity_UserClaims");
                entity.HasKey(e => new { e.EntityId });
            });
             modelBuilder.Entity<TreatmentParameterEntity>(entity =>
            {
                entity.ToTable("tbl_pb_UserTreatmentParameters");
                entity.HasKey(e => new { e.EntityId });
            });
            modelBuilder.Entity<TreatmentCancellationMatrixEntity>(entity =>
            {
                entity.HasKey(e => new { e.AssetTypeA, e.EntityId })
                      .IsClustered();
            });
            modelBuilder.Entity<CandidatePool>(entity =>
            {
                entity.HasKey(e => e.EntityId).HasName("PK_tbl_lib_TreatmentLibraries");
                entity.ToTable("tbl_lib_UserLibraries");
                entity.Property(e => e.EntityId).HasDefaultValueSql("(newid())");
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
            modelBuilder.Entity<UserTreatment>(entity =>
            {
                entity.HasKey(e => new { e.LibraryId, e.EntityId });

                entity.ToTable("tbl_lib_LibraryTreatments");

                entity.Property(e => e.Asset).HasMaxLength(50);
                entity.Property(e => e.AssetType)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false);     
                entity.Property(e => e.Treatment).HasMaxLength(100);
                entity.Property(e => e.UserTreatmentTypeNo).HasDefaultValueSql("((0))");

            });
            modelBuilder.Entity<Scenario>(entity =>
            {
                entity.HasKey(e => e.EntityId);
                
            });
           modelBuilder.Entity<CountyEntity>(entity =>
            {
                entity.HasKey(e => new { e.District, e.EntityId })
                      .IsClustered();
            });
            modelBuilder.Entity<ParameterEntity>(entity =>
            {
                entity.HasKey(e => new { e.EntityId, e.Parmfamily})
                      .IsClustered();
            });
            modelBuilder.Entity<Treatment>(entity =>
            {
                entity.HasKey(e => e.EntityId).HasName("PK_tbl_ImportedTreatments");

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
            modelBuilder.Entity<ProjectTreatment>(entity =>
            {
                entity.HasKey(e => e.EntityId).HasName("PK_tbl_pb_ExtendedImportedTreatments");
                entity.ToView("vw_pb_ui_ScenarioTreatments")
                      .ToTable("tbl_pb_ExtendedImportedTreatments");
            });
            modelBuilder.Entity<PAMSSectionSegment>(entity =>
            {
                entity.HasKey(e => e.EntityId);
                entity.HasIndex(e => new { e.District, e.CountyId, e.Route, e.FromSection }, "IX_tbl_pams_MaintainableAssetsSegmentation_DCRS");
                entity.Property(e => e.EntityId).ValueGeneratedNever();
            });
            modelBuilder.Entity<TreatmentType>(entity =>
            {
                entity.HasKey(e => e.EntityId)
                      .HasName("PK__Treatmen__1015D11E07EE5045");
                entity.HasIndex(e => e.AssetType, "IX_TreatmentTypes_AssetType");

            });       
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => new { e.ScenarioId, e.EntityId });
                entity.ToView("vw_pb_ui_ScenarioProjects");
                entity.ToTable("tbl_pb_ExtendedTimewiseConstrainedProjects");
            });      
            modelBuilder.Entity<ScenarioParamater>(entity =>
            {
                entity.HasKey(e => new { e.ScenarioId, e.EntityId });
                entity.ToView("vw_pb_ScenarioParameters");
                entity.ToTable("tbl_pb_ScenParm");
                entity.Property(e => e.ParmDescription).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.ParmName).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            });
            modelBuilder.Entity<ScenarioBudget>(entity =>
            {
                entity.HasKey(e => new { e.ScenarioId, e.District, e.EntityId, e.IsInterstate, e.AssetType })
                      .HasName("PK_tbl_pb_ExtendedScenBudget");
            });
            modelBuilder.Entity<DefaultSlack>(entity =>
            {
                entity.HasKey(e => e.EntityId);
            });
            modelBuilder.Entity<AllNeedsEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_AllNeeds");
            });
            modelBuilder.Entity<BridgeNeedsEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_BridgeNeeds");
            });
            modelBuilder.Entity<PavementNeedsEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_PavementNeeds");
            });
            modelBuilder.Entity<AllPotentialBenefitEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_AllPotentialBenefits");
            });
            modelBuilder.Entity<BridgePotentialBenefitEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_BridgePotentialBenefits");
            });
            modelBuilder.Entity<PavementPotentialBenefitEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_PavementPotentialBenefits");
            });
            modelBuilder.Entity<ProjectSummaryEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_ProjectSummary");
            });
            modelBuilder.Entity<TreatmentSummaryEntity>(entity =>
            {
               entity.HasNoKey();
               entity.ToView("vw_pb_ui_TreatmentSummary");
            });
            modelBuilder.Entity<BudgetEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_BudgetAvailable");
            });
            modelBuilder.Entity<BudgetSpentEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_BudgetSpent");
            });
            modelBuilder.Entity<CombinedProjectSummaryEntity>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_pb_ui_Projects");
            });
             modelBuilder.Entity<AraEnumerations>(entity =>
            {
                entity.HasNoKey();
                entity.HasIndex(e => new { e.EntityId, e.EnumName }, "tbl_ara_Enumerations")
                     .IsUnique()
                     .IsClustered();
            });
            modelBuilder.Entity<BridgeToPavements>(entity =>
            {
                entity.HasNoKey();
                entity.HasIndex(e => new { e.EntityId, e.BridgeId }, "tbl_pb_BridgeToPavements")
                     .IsUnique()
                     .IsClustered();
            });
            modelBuilder.Entity<ImportSessions>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("tbl_pb_ImportSessions");
            });
           
            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
