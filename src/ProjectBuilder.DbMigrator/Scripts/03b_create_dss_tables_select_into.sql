-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 3b   : CREATE DSS TABLES  —  SELECT INTO approach
-- Run in    : PB database
-- Requires  : Linked server [DSS_DB] to be active
--
-- This approach copies the schema directly from the live DSS_DB.
-- Types, lengths, and nullability are guaranteed to match exactly.
-- Use this instead of 03a if you have linked server access.
--
-- ⚠️  IDENTITY columns are NOT preserved by SELECT INTO.
--     Columns like SessionNo, RunNo, LibNo become plain int.
--     Add IDENTITY back manually if the application requires it.
--
-- Source table name mapping (actual DSS_DB names):
--   _Dss* prefix  = DSS core model tables
--   _Tbl* prefix  = internal parameter/pool tables
--   Tbl*          = standard operational tables
-- ============================================================


-- ------------------------------------------------------------
-- DSS Core Tables  (underscore-prefixed in DSS_DB)
-- ------------------------------------------------------------

SELECT * INTO [dbo].[tbl_dss_Goals]
    FROM [DSS_DB].[dbo].[_DssGoals]               WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_GoalWeights]
    FROM [DSS_DB].[dbo].[_DssGoalWeights]          WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_Measures]
    FROM [DSS_DB].[dbo].[_DssMeasures]             WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_MeasureScalingLinear]
    FROM [DSS_DB].[dbo].[_DssMeasureScalingLinear] WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_MeasureWeights]
    FROM [DSS_DB].[dbo].[_DssMeasureWeights]       WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_Projects]
    FROM [DSS_DB].[dbo].[_DssProjects]             WHERE 1=0;
GO

-- Source: TblWorkCandidates (not _DssWorkCandidates — confirmed by team)
SELECT * INTO [dbo].[tbl_dss_WorkCandidates]
    FROM [DSS_DB].[dbo].[TblWorkCandidates]        WHERE 1=0;
GO


-- ------------------------------------------------------------
-- Tbl* Tables
-- ------------------------------------------------------------

SELECT * INTO [dbo].[tbl_dss_AssetTypeParametersData]
    FROM [DSS_DB].[dbo].[TblAssetTypeParametersData]         WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_BenefitParameters]
    FROM [DSS_DB].[dbo].[TblBenefitParameters]               WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_BridgeToPavements]
    FROM [DSS_DB].[dbo].[TblBridgeToPavements]               WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_Counties]
    FROM [DSS_DB].[dbo].[TblCounties]                        WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_DataSections]
    FROM [DSS_DB].[dbo].[TblDataSections]                    WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_ImportSessions]
    FROM [DSS_DB].[dbo].[TblImportSessions]                  WHERE 1=0;
GO

ALTER TABLE [dbo].[tbl_dss_ImportSessions]
ADD CONSTRAINT [DF_tbl_dss_ImportSessions_Id] DEFAULT NEWID() FOR [Id];
GO

SELECT * INTO [dbo].[tbl_dss_OtherParameters]
    FROM [DSS_DB].[dbo].[TblOtherParameters]                 WHERE 1=0;
GO

-- Source: _TblPoolParameters (underscore-prefixed in DSS_DB)
SELECT * INTO [dbo].[tbl_dss_PoolParameters]
    FROM [DSS_DB].[dbo].[_TblPoolParameters]                 WHERE 1=0;
GO

-- Source: _TblPoolParametersData (underscore-prefixed in DSS_DB)
SELECT * INTO [dbo].[tbl_dss_PoolParametersData]
    FROM [DSS_DB].[dbo].[_TblPoolParametersData]             WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_ProjectCandidates]
    FROM [DSS_DB].[dbo].[TblProjectCandidates]               WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_RawImportedWorkCandidates]
    FROM [DSS_DB].[dbo].[TblRawImportedWorkCandidates]       WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_ScenarioProjectObjectiveValues]
    FROM [DSS_DB].[dbo].[TblScenarioProjectObjectiveValues]  WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_ScenarioRunsLog]
    FROM [DSS_DB].[dbo].[TblScenarioRunsLog]                 WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_Scenarios]
    FROM [DSS_DB].[dbo].[TblScenarios]                       WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_UserPools]
    FROM [DSS_DB].[dbo].[TblUserPools]                       WHERE 1=0;
GO


-- ------------------------------------------------------------
-- Standalone Tables
-- ------------------------------------------------------------

SELECT * INTO [dbo].[tbl_dss_SpoofProjects_NorthwestRPO]
    FROM [DSS_DB].[dbo].[SpoofProjects_NorthwestRPO]         WHERE 1=0;
GO


-- ------------------------------------------------------------
-- OpenIddict Tables
-- ------------------------------------------------------------

SELECT * INTO [dbo].[tbl_dss_OpenIddictApplications]
    FROM [DSS_DB].[dbo].[OpenIddictApplications]             WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_OpenIddictAuthorizations]
    FROM [DSS_DB].[dbo].[OpenIddictAuthorizations]           WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_OpenIddictScopes]
    FROM [DSS_DB].[dbo].[OpenIddictScopes]                   WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_OpenIddictTokens]
    FROM [DSS_DB].[dbo].[OpenIddictTokens]                   WHERE 1=0;
GO
