-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 3    : CREATE DSS TABLES
-- Run in    : PB database
-- Source    : Schemas mirrored from DSS_DB via SELECT * INTO WHERE 1=0
--
-- ⚠️  tbl_dss_WorkCandidates ← DSS_DB.dbo.TblWorkCandidates only
--
-- ⚠️  tbl_dss_OtherParameters appears twice in source script
--     (from TblOtherParameters AND from OtherParameters).
--     Only one CREATE is included here. Verify which source is correct.
-- ============================================================


-- ------------------------------------------------------------
-- DSS Core Tables
-- ------------------------------------------------------------

CREATE TABLE [dbo].[tbl_dss_Goals]
(
    [Id]                   UNIQUEIDENTIFIER NULL,
    [Name]                 NVARCHAR(50)     NULL,
    [Description]          TEXT             NULL,
    [Status]               TINYINT          NULL,
    [CreatorId]            UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME         NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_GoalWeights]
(
    [GoalId] INT   NULL,
    [Weight] FLOAT NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_Measures]
(
    [Id]                   UNIQUEIDENTIFIER NULL,
    [MeasureName]          NVARCHAR(50)     NULL,
    [MeasureDescription]   TEXT             NULL,
    [Status]               TINYINT          NULL,
    [CreatorId]            UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME         NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_MeasureScalingLinear]
(
    [MeasureId]     INT   NULL,
    [MinScore]      INT   NULL,
    [MeasureScoreA] FLOAT NULL,
    [MaxScore]      FLOAT NULL,
    [MeasureScoreB] FLOAT NULL,
    [slope]         FLOAT NULL,
    [intercept]     FLOAT NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_MeasureWeights]
(
    [MeasureId] INT   NULL,
    [GoalId]    INT   NULL,
    [Weight]    FLOAT NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_Projects]
(
    [Id]                            INT   NULL,
    [PlanningPartner]               INT   NULL,
    [District]                      INT   NULL,
    [County]                        INT   NULL,
    [Description]                   TEXT  NULL,
    [EarliestYear]                  INT   NULL,
    [LatestYear]                    INT   NULL,
    [Cost]                          FLOAT NULL,
    [SafetyScore]                   FLOAT NULL,
    [MobilityAndEconomyScore]       FLOAT NULL,
    [EquityAndAccessScore]          FLOAT NULL,
    [ResilienceAndEnvironmentScore] FLOAT NULL,
    [ConditionAndPerformanceScore]  FLOAT NULL,
    [TotalScore]                    FLOAT NULL,
    [RelativeEfficiency]            FLOAT NULL
);
GO




-- ------------------------------------------------------------
-- Tbl* Tables
-- ------------------------------------------------------------

CREATE TABLE [dbo].[tbl_dss_AssetTypeParametersData]
(
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [AssetType]            NVARCHAR(1)      NOT NULL,
    [BenefitScalingFactor] FLOAT            NOT NULL,
    [CreatedBy]            NVARCHAR(50)     NULL,
    [CreatedAt]            DATETIME         NULL,
    [ScenId]               UNIQUEIDENTIFIER NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_BenefitParameters]
(
    [Family]      NVARCHAR(50) NOT NULL,
    [Code]        TINYINT      NOT NULL,
    [Description] NVARCHAR(50) NULL,
    [Val]         FLOAT        NULL,
    [CreatedBy]   NVARCHAR(50) NULL,
    [CreatedAt]   DATETIME     NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_BridgeToPavements]
(
    [BRKEY]       NVARCHAR(15) NOT NULL,
    [DISTRICT]    INT          NOT NULL,
    [COUNTY]      NVARCHAR(50) NOT NULL,
    [BRIDGE_ID]   BIGINT       NOT NULL,
    [County_Code] INT          NOT NULL,
    [Route]       INT          NOT NULL,
    [Segment]     INT          NOT NULL,
    [Offset]      INT          NOT NULL,
    [CreatedBy]   NVARCHAR(50) NULL,
    [CreatedAt]   DATETIME     NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_Counties]
(
    [District] INT          NULL,
    [Cnty]     INT          NULL,
    [County]   NVARCHAR(50) NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_DataSections]
(
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [Name]         NVARCHAR(100)    NOT NULL,
    [Notes]        NVARCHAR(MAX)    NULL,
    [CreatedBy]    NVARCHAR(50)     NULL,
    [CreatedAt]    DATETIME         NULL,
    [ShortName]    NVARCHAR(20)     NULL,
    [ExportOrdNum] TINYINT          NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_ImportSessions]
(
    [Id]               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [SessionNo]        INT              IDENTITY(1,1) NOT NULL,
    [ImportSourceType] NVARCHAR(10)     NULL,
    [ImportSourceName] NVARCHAR(50)     NULL,
    [DataSourceType]   VARCHAR(10)      NOT NULL,
    [DataSourceName]   NVARCHAR(255)    NOT NULL,
    [CreatedBy]        NVARCHAR(50)     NOT NULL,
    [CreatedAt]        DATETIME         NOT NULL,
    [CompletedAt]      DATETIME         NULL,
    [CompletedStatus]  INT              NULL,
    [Notes]            NVARCHAR(MAX)    NULL,
    [PoolId]           UNIQUEIDENTIFIER NULL
);
GO

ALTER TABLE [dbo].[tbl_dss_ImportSessions] ADD CONSTRAINT [DF_tbl_dss_ImportSessions_CreatedBy] DEFAULT (user_name()) FOR [CreatedBy];
GO
ALTER TABLE [dbo].[tbl_dss_ImportSessions] ADD CONSTRAINT [DF_tbl_dss_ImportSessions_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt];
GO

-- ⚠️  Source conflict: TblOtherParameters vs OtherParameters in DSS_DB.
--     Schema below is from TblOtherParameters. Verify before running.
CREATE TABLE [dbo].[tbl_dss_OtherParameters]
(
    [Code]        NVARCHAR(10)  NULL,
    [Description] NVARCHAR(100) NULL,
    [Val]         FLOAT         NULL,
    [CreatedBy]   NVARCHAR(50)  NULL,
    [CreatedAt]   DATETIME      NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_PoolParameters]
(
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [ItemName]        NVARCHAR(50)     NOT NULL,
    [ItemDescription] NVARCHAR(255)    NULL,
    [CreatedBy]       NVARCHAR(50)     NULL,
    [CreatedAt]       DATETIME         NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_PoolParametersData]
(
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [PoolId]        UNIQUEIDENTIFIER NOT NULL,
    [DataSectionId] UNIQUEIDENTIFIER NULL,
    [ItemId]        UNIQUEIDENTIFIER NULL,
    [ItemValue]     FLOAT            NULL,
    [CreatedBy]     NVARCHAR(50)     NULL,
    [CreatedAt]     DATETIME         NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_ProjectCandidates]
(
    [Id]                            UNIQUEIDENTIFIER NOT NULL,
    [PoolId]                        UNIQUEIDENTIFIER NOT NULL,
    [District]                      TINYINT          NULL,
    [Cnty]                          TINYINT          NULL,
    [Description]                   NVARCHAR(250)    NULL,
    [NumberOfWorkCandidates]        INT              NULL,
    [Treatment]                     NVARCHAR(MAX)    NULL,
    [YearEarliest]                  INT              NULL,
    [YearLatest]                    INT              NULL,
    [TotalCost]                     FLOAT            NULL,
    [TotalScaledBenefit]            FLOAT            NULL,
    [SafetyScore]                   FLOAT            NULL,
    [MobilityAndEconomyScore]       FLOAT            NULL,
    [EquityAndAccessScore]          FLOAT            NULL,
    [ResilienceAndEnvironmentScore] FLOAT            NULL,
    [ConditionAndPerformanceScore]  FLOAT            NULL,
    [TotalScore]                    FLOAT            NULL,
    [RelativeEfficiency]            FLOAT            NULL,
    [CreatedBy]                     NVARCHAR(50)     NULL,
    [CreatedAt]                     DATETIME         NULL,
    [RelativeEfficiencySetAt]       DATETIME         NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_RawImportedWorkCandidates]
(
    [ProjectID]                                                        NVARCHAR(50)  NULL,
    [District]                                                         TINYINT       NULL,
    [County]                                                           NVARCHAR(50)  NULL,
    [Route]                                                            INT           NULL,
    [FromSection]                                                      INT           NULL,
    [ToSection]                                                        NVARCHAR(50)  NULL,
    [BRKEY]                                                            NVARCHAR(50)  NULL,
    [BRIDGE_ID]                                                        NVARCHAR(50)  NULL,
    [AssetType]                                                        NVARCHAR(1)   NULL,
    [Description]                                                      NVARCHAR(255) NULL,
    [Cost]                                                             FLOAT         NULL,
    [HSIP_Eligible]                                                    FLOAT         NULL,
    [Excess_Safety_Value]                                              FLOAT         NULL,
    [Supported_by_RSA_Safety_Study]                                    INT           NULL,
    [Degree_of_Safety_Improvement]                                     INT           NULL,
    [Supports_Hazard_Mitigation]                                       INT           NULL,
    [Improves_Access_to_Emergency_Services]                            INT           NULL,
    [ADT]                                                              FLOAT         NULL,
    [Percent_Trucks]                                                   FLOAT         NULL,
    [LFAR_BOF_Eligible]                                                INT           NULL,
    [BPN]                                                              INT           NULL,
    [Length]                                                           FLOAT         NULL,
    [Speed_Limit]                                                      FLOAT         NULL,
    [Population_Density]                                               FLOAT         NULL,
    [Bike_Commute_Share]                                               FLOAT         NULL,
    [Functional_Classification]                                        INT           NULL,
    [National_Highway_System]                                          INT           NULL,
    [Pennsylvania_Byway]                                               NVARCHAR(1)   NULL,
    [Pavement_Rehabilitated_or_Reconstructed]                          FLOAT         NULL,
    [Bridges_Rehabilitated_or_Replaced]                                FLOAT         NULL,
    [Culverts_Rehabilitated_or_Replaced]                               FLOAT         NULL,
    [Guardrail_Rehabilitated_or_Replaced]                              FLOAT         NULL,
    [Geotechnical_Assets_Rehabilitated_or_Replaced]                    FLOAT         NULL,
    [Facilities_Rehabilitated_or_Reconstructed]                        FLOAT         NULL,
    [Sidewalks_Rehabilitated_or_Reconstructed]                         FLOAT         NULL,
    [Degree_of_Environmental_Improvement]                              FLOAT         NULL,
    [Reduced_Flood_Closure_Risk]                                       FLOAT         NULL,
    [Detour_Distance]                                                  FLOAT         NULL,
    [Wetlands_Improved]                                                FLOAT         NULL,
    [Wildlife_Crossings]                                               FLOAT         NULL,
    [Reduced_Fuel_Consumption]                                         FLOAT         NULL,
    [Consistency_with_Land_Use_Plans]                                  NVARCHAR(1)   NULL,
    [Sidewalks_and_Curb_Ramps_Constructed_or_Improved]                 FLOAT         NULL,
    [Change_in_Accessible_Destinations]                                INT           NULL,
    [Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations] FLOAT         NULL,
    [Support_for_Environmental_Justice]                                NVARCHAR(1)   NULL,
    [Travel_Time_Savings]                                              FLOAT         NULL,
    [Operating_Cost_Savings]                                           FLOAT         NULL,
    [Degree_of_Mobility_Improvement]                                   FLOAT         NULL,
    [Freight_Improvement]                                              FLOAT         NULL,
    [Facilitates_Transit_Use]                                          INT           NULL,
    [Facilitates_Active_Transportation]                                INT           NULL,
    [Bike_Lanes_Constructed]                                           INT           NULL,
    [Support_for_Job_Growth]                                           INT           NULL,
    [Support_for_Tourism]                                              INT           NULL,
    [Support_for_Economic_Goals]                                       INT           NULL,
    [Support_for_Recreation]                                           INT           NULL,
    [Consistency_with_Existing_Plans]                                  INT           NULL,
    [Treatment]                                                        NVARCHAR(50)  NULL,
    [MinYear]                                                          INT           NULL,
    [MaxYear]                                                          INT           NULL,
    [Year]                                                             INT           NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_ScenarioProjectObjectiveValues]
(
    [ScenId]             UNIQUEIDENTIFIER NOT NULL,
    [ProjectId]          UNIQUEIDENTIFIER NOT NULL,
    [RelativeEfficiency] FLOAT            NULL,
    [CreatedBy]          NVARCHAR(50)     NULL,
    [CreatedAt]          DATETIME         NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_ScenarioRunsLog]
(
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [RunNo]     INT              IDENTITY(1,1) NOT NULL,
    [ScenId]    UNIQUEIDENTIFIER NULL,
    [ScenNo]    INT              NULL,
    [StartedAt] DATETIME         NOT NULL,
    [StartedBy] NVARCHAR(50)     NULL,
    [Status]    NVARCHAR(50)     NULL,
    [EndedAt]   DATETIME         NULL,
    [Notes]     NVARCHAR(MAX)    NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_Scenarios]
(
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [ScenNo]         INT              IDENTITY(1,1) NOT NULL,
    [ScenName]       NVARCHAR(50)     NOT NULL,
    [Description]    NVARCHAR(255)    NULL,
    [DeaInputSproc]  NVARCHAR(255)    NULL,
    [DeaOutputSproc] NVARCHAR(255)    NULL,
    [CreatedBy]      NVARCHAR(50)     NULL,
    [CreatedAt]      DATETIME         NULL,
    [PoolId]         UNIQUEIDENTIFIER NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_UserPools]
(
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [LibNo]           INT              IDENTITY(1,1) NOT NULL,
    [UserId]          UNIQUEIDENTIFIER NOT NULL,
    [Name]            NVARCHAR(50)     NOT NULL,
    [Description]     NVARCHAR(MAX)    NULL,
    [CreatedBy]       NVARCHAR(50)     NULL,
    [CreatedAt]       DATETIME         NULL,
    [IsActive]        BIT              NULL,
    [UpdatedBy]       NVARCHAR(50)     NULL,
    [UpdatedAt]       DATETIME         NULL,
    [IsSharedLibrary] BIT              NULL,
    [SourcePoolId]    UNIQUEIDENTIFIER NULL
);
GO

-- Source: DSS_DB.dbo.TblWorkCandidates
CREATE TABLE [dbo].[tbl_dss_WorkCandidates]
(
    [Id]                            UNIQUEIDENTIFIER NOT NULL,
    [ImportSessionId]               UNIQUEIDENTIFIER NULL,
    [ImportTimeGeneratedId]         UNIQUEIDENTIFIER NULL,
    [UserLibraryId]                 UNIQUEIDENTIFIER NULL,
    [ProjectCandidateId]            UNIQUEIDENTIFIER NULL,
    [AssetType]                     NVARCHAR(1)      NULL,
    [District]                      TINYINT          NULL,
    [Cnty]                          TINYINT          NULL,
    [Route]                         INT              NULL,
    [Direction]                     TINYINT          NULL,
    [FromSection]                   INT              NULL,
    [ToSection]                     INT              NULL,
    [Offset]                        INT              NULL,
    [BRKEY_]                        NVARCHAR(15)     NULL,
    [BRIDGE_ID]                     BIGINT           NULL,
    [Description]                   NVARCHAR(MAX)    NULL,
    [Cost]                          FLOAT            NULL,
    [Benefit]                       FLOAT            NULL,
    [IsIsolatedBridge]              BIT              NOT NULL,
    [SafetyScore]                   FLOAT            NULL,
    [MobilityAndEconomyScore]       FLOAT            NULL,
    [EquityAndAccessScore]          FLOAT            NULL,
    [ResilienceAndEnvironmentScore] FLOAT            NULL,
    [ConditionAndPerformanceScore]  FLOAT            NULL,
    [TotalScore]                    FLOAT            NULL,
    [RelativeEfficiency]            FLOAT            NULL,
    [CreatedBy]                     NVARCHAR(50)     NULL,
    [CreatedAt]                     DATETIME         NULL
);
GO


-- ------------------------------------------------------------
-- Standalone Tables
-- ------------------------------------------------------------

CREATE TABLE [dbo].[tbl_dss_SpoofProjects_NorthwestRPO]
(
    [ProjectID]                                                          NVARCHAR(50)  NULL,
    [District]                                                           TINYINT       NULL,
    [County]                                                             NVARCHAR(50)  NULL,
    [Route]                                                              INT           NULL,
    [FromSection]                                                        INT           NULL,
    [ToSection]                                                          NVARCHAR(50)  NULL,
    [BRKEY]                                                              NVARCHAR(50)  NULL,
    [BRIDGE_ID]                                                          NVARCHAR(50)  NULL,
    [AssetType]                                                          NVARCHAR(1)   NULL,
    [Description]                                                        NVARCHAR(250) NULL,
    [Cost]                                                               FLOAT         NULL,
    [HSIP_Eligible]                                                      FLOAT         NULL,
    [Excess_Safety_Value]                                                FLOAT         NULL,
    [Supported_by_RSA_Safety_Study]                                      NVARCHAR(1)   NULL,
    [Degree_of_Safety_Improvement]                                       FLOAT         NULL,
    [Supports_Hazard_Mitigation]                                         NVARCHAR(1)   NULL,
    [Improves_Access_to_Emergency_Sevices]                               NVARCHAR(1)   NULL,
    [AADT]                                                               FLOAT         NULL,
    [Percent_Trucks]                                                     FLOAT         NULL,
    [LFAR_BOF_Eligible]                                                  NVARCHAR(1)   NULL,
    [BPN]                                                                NVARCHAR(50)  NULL,
    [Population_Density]                                                 FLOAT         NULL,
    [Bike_Commute_Share]                                                 NVARCHAR(1)   NULL,
    [Functional_Classification]                                          INT           NULL,
    [National_Highway_System]                                            NVARCHAR(1)   NULL,
    [Pennsylvania_Byway]                                                 NVARCHAR(1)   NULL,
    [Pavement_Rehabilitated_or_Reconstructed]                            FLOAT         NULL,
    [Bridges_Rehabilitated_or_Replaced]                                  FLOAT         NULL,
    [Culverts_Rehabilitated_or_Replaced]                                 FLOAT         NULL,
    [Guardrail_Rehabilitated_or_Replaced]                                FLOAT         NULL,
    [Geotechnical_Assets_Rehabilitated_or_Replaced]                      FLOAT         NULL,
    [Facilities_Rehabilitated_or_Reconstructed]                          FLOAT         NULL,
    [Sidewalks_Rehabilitated_or_Reconstructed]                           FLOAT         NULL,
    [Environmental_Benefit]                                              FLOAT         NULL,
    [Reduced_Flood_Closure_Risk]                                         FLOAT         NULL,
    [Wetlands_Improved]                                                  FLOAT         NULL,
    [Wildlife_Crossings]                                                 FLOAT         NULL,
    [Reduced_Fuel_Consumption]                                           FLOAT         NULL,
    [Consistency_with_Land_Use_Plans]                                    NVARCHAR(1)   NULL,
    [Sidewalks_and_Curb_Ramps_Constructed_or_Improved]                   FLOAT         NULL,
    [Change_in_Accessible_Destinations]                                  SMALLINT      NULL,
    [Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations]   FLOAT         NULL,
    [Support_for_Environmental_Justice]                                  NVARCHAR(1)   NULL,
    [Travel_Time_Savings]                                                FLOAT         NULL,
    [Operating_Cost_Savings]                                             FLOAT         NULL,
    [Degree_of_Mobility_Improvement]                                     FLOAT         NULL,
    [Freight_Improvement]                                                FLOAT         NULL,
    [Facilitiates_Transit_Use]                                           NVARCHAR(1)   NULL,
    [Facilitiates_Active_Transportation]                                 NVARCHAR(1)   NULL,
    [Bike_Lanes_Constructed]                                             INT           NULL,
    [Support_for_Job_Growth]                                             NVARCHAR(1)   NULL,
    [Support_for_Tourism]                                                NVARCHAR(1)   NULL,
    [Support_for_Economic_Goals]                                         NVARCHAR(1)   NULL,
    [Support_for_Recreation]                                             NVARCHAR(1)   NULL,
    [Consistency_with_Existing_Plans]                                    NVARCHAR(1)   NULL
);
GO


-- ------------------------------------------------------------
-- OpenIddict Tables
-- ------------------------------------------------------------

CREATE TABLE [dbo].[tbl_dss_OpenIddictApplications]
(
    [Id]                     UNIQUEIDENTIFIER NOT NULL,
    [ApplicationType]        NVARCHAR(50)     NULL,
    [ClientId]               NVARCHAR(100)    NULL,
    [ClientSecret]           NVARCHAR(MAX)    NULL,
    [ClientType]             NVARCHAR(50)     NULL,
    [ConsentType]            NVARCHAR(50)     NULL,
    [DisplayName]            NVARCHAR(MAX)    NULL,
    [DisplayNames]           NVARCHAR(MAX)    NULL,
    [JsonWebKeySet]          NVARCHAR(MAX)    NULL,
    [Permissions]            NVARCHAR(MAX)    NULL,
    [PostLogoutRedirectUris] NVARCHAR(MAX)    NULL,
    [Properties]             NVARCHAR(MAX)    NULL,
    [RedirectUris]           NVARCHAR(MAX)    NULL,
    [Requirements]           NVARCHAR(MAX)    NULL,
    [Settings]               NVARCHAR(MAX)    NULL,
    [ClientUri]              NVARCHAR(MAX)    NULL,
    [LogoUri]                NVARCHAR(MAX)    NULL,
    [ExtraProperties]        NVARCHAR(MAX)    NOT NULL,
    [ConcurrencyStamp]       NVARCHAR(40)     NOT NULL,
    [CreationTime]           DATETIME2        NOT NULL,
    [CreatorId]              UNIQUEIDENTIFIER NULL,
    [LastModificationTime]   DATETIME2        NULL,
    [LastModifierId]         UNIQUEIDENTIFIER NULL,
    [IsDeleted]              BIT              NOT NULL,
    [DeleterId]              UNIQUEIDENTIFIER NULL,
    [DeletionTime]           DATETIME2        NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_OpenIddictAuthorizations]
(
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [ApplicationId]        UNIQUEIDENTIFIER NULL,
    [CreationDate]         DATETIME2        NULL,
    [Properties]           NVARCHAR(MAX)    NULL,
    [Scopes]               NVARCHAR(MAX)    NULL,
    [Status]               NVARCHAR(50)     NULL,
    [Subject]              NVARCHAR(400)    NULL,
    [Type]                 NVARCHAR(50)     NULL,
    [ExtraProperties]      NVARCHAR(MAX)    NOT NULL,
    [ConcurrencyStamp]     NVARCHAR(40)     NOT NULL,
    [CreationTime]         DATETIME2        NOT NULL,
    [CreatorId]            UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME2        NULL,
    [LastModifierId]       UNIQUEIDENTIFIER NULL,
    [IsDeleted]            BIT              NOT NULL,
    [DeleterId]            UNIQUEIDENTIFIER NULL,
    [DeletionTime]         DATETIME2        NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_OpenIddictScopes]
(
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [Description]          NVARCHAR(MAX)    NULL,
    [Descriptions]         NVARCHAR(MAX)    NULL,
    [DisplayName]          NVARCHAR(MAX)    NULL,
    [DisplayNames]         NVARCHAR(MAX)    NULL,
    [Name]                 NVARCHAR(200)    NULL,
    [Properties]           NVARCHAR(MAX)    NULL,
    [Resources]            NVARCHAR(MAX)    NULL,
    [ExtraProperties]      NVARCHAR(MAX)    NOT NULL,
    [ConcurrencyStamp]     NVARCHAR(40)     NOT NULL,
    [CreationTime]         DATETIME2        NOT NULL,
    [CreatorId]            UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME2        NULL,
    [LastModifierId]       UNIQUEIDENTIFIER NULL,
    [IsDeleted]            BIT              NOT NULL,
    [DeleterId]            UNIQUEIDENTIFIER NULL,
    [DeletionTime]         DATETIME2        NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_OpenIddictTokens]
(
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [ApplicationId]        UNIQUEIDENTIFIER NULL,
    [AuthorizationId]      UNIQUEIDENTIFIER NULL,
    [CreationDate]         DATETIME2        NULL,
    [ExpirationDate]       DATETIME2        NULL,
    [Payload]              NVARCHAR(MAX)    NULL,
    [Properties]           NVARCHAR(MAX)    NULL,
    [RedemptionDate]       DATETIME2        NULL,
    [ReferenceId]          NVARCHAR(100)    NULL,
    [Status]               NVARCHAR(50)     NULL,
    [Subject]              NVARCHAR(400)    NULL,
    [Type]                 NVARCHAR(50)     NULL,
    [ExtraProperties]      NVARCHAR(MAX)    NOT NULL,
    [ConcurrencyStamp]     NVARCHAR(40)     NOT NULL,
    [CreationTime]         DATETIME2        NOT NULL,
    [CreatorId]            UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME2        NULL,
    [LastModifierId]       UNIQUEIDENTIFIER NULL,
    [IsDeleted]            BIT              NOT NULL,
    [DeleterId]            UNIQUEIDENTIFIER NULL,
    [DeletionTime]         DATETIME2        NULL
);
GO
