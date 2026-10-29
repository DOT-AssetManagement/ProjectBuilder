-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 8    : SCHEMA MISMATCH FIXES
-- Run in    : PB database
-- Run after : Steps 01 - 07 are complete
--
-- Fixes all issues found by 06b_schema_verification.sql:
--   1. tbl_dss_AbpSettingDefinitions  — length, nullability, missing columns
--   2. tbl_dss_AbpSettings            — length + nullability on Value
--   3. tbl_dss_WorkCandidates         — type mismatches + nullability
-- ============================================================


-- ============================================================
-- FIX 1 : tbl_dss_AbpSettingDefinitions
-- ============================================================

-- 1a. Fix DisplayName nullability  (PB=YES, DSS=NO)
ALTER TABLE [dbo].[tbl_dss_AbpSettingDefinitions]
    ALTER COLUMN [DisplayName] NVARCHAR(256) NOT NULL;
GO

-- 1b. Fix DefaultValue length  (PB=MAX, DSS=2048)
ALTER TABLE [dbo].[tbl_dss_AbpSettingDefinitions]
    ALTER COLUMN [DefaultValue] NVARCHAR(2048) NULL;
GO

-- 1c. Fix Description length  (PB=MAX, DSS=512)
ALTER TABLE [dbo].[tbl_dss_AbpSettingDefinitions]
    ALTER COLUMN [Description] NVARCHAR(512) NULL;
GO

-- 1d. Fix Providers length  (PB=MAX, DSS=1024)
ALTER TABLE [dbo].[tbl_dss_AbpSettingDefinitions]
    ALTER COLUMN [Providers] NVARCHAR(1024) NULL;
GO

-- 1e. Add missing column : IsInherited
ALTER TABLE [dbo].[tbl_dss_AbpSettingDefinitions]
    ADD [IsInherited] BIT NOT NULL DEFAULT(0);
GO

-- 1f. Add missing column : IsEncrypted
ALTER TABLE [dbo].[tbl_dss_AbpSettingDefinitions]
    ADD [IsEncrypted] BIT NOT NULL DEFAULT(0);
GO

-- 1g. Add missing column : ExtraProperties
ALTER TABLE [dbo].[tbl_dss_AbpSettingDefinitions]
    ADD [ExtraProperties] NVARCHAR(MAX) NULL;
GO


-- ============================================================
-- FIX 2 : tbl_dss_AbpSettings
-- ============================================================

-- 2a. Fix Value  (PB=MAX nullable, DSS=2048 NOT NULL)
ALTER TABLE [dbo].[tbl_dss_AbpSettings]
    ALTER COLUMN [Value] NVARCHAR(2048) NOT NULL;
GO


-- ============================================================
-- FIX 3 : tbl_dss_WorkCandidates
-- ============================================================

-- 3a. Fix BRIDGE_ID type  (PB=bigint, DSS=nvarchar(50))
ALTER TABLE [dbo].[tbl_dss_WorkCandidates]
    ALTER COLUMN [BRIDGE_ID] NVARCHAR(50) NULL;
GO

-- 3b. Fix ToSection type  (PB=int, DSS=nvarchar(50))
ALTER TABLE [dbo].[tbl_dss_WorkCandidates]
    ALTER COLUMN [ToSection] NVARCHAR(50) NULL;
GO

-- 3c. Fix Description length  (PB=MAX, DSS=255)
ALTER TABLE [dbo].[tbl_dss_WorkCandidates]
    ALTER COLUMN [Description] NVARCHAR(255) NULL;
GO

-- 3d. Fix ImportSessionId nullability  (PB=YES, DSS=NO)
ALTER TABLE [dbo].[tbl_dss_WorkCandidates]
    ALTER COLUMN [ImportSessionId] UNIQUEIDENTIFIER NOT NULL;
GO

-- 3e. Fix ImportTimeGeneratedId nullability  (PB=YES, DSS=NO)
ALTER TABLE [dbo].[tbl_dss_WorkCandidates]
    ALTER COLUMN [ImportTimeGeneratedId] UNIQUEIDENTIFIER NOT NULL;
GO


-- ============================================================
-- VERIFY : Re-run 06b_schema_verification.sql after this
-- Section 1 (Mismatches) should now return 0 rows
-- ============================================================
