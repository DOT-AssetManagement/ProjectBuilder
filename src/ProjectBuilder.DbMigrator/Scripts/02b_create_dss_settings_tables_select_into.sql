-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 2b   : CREATE DSS SETTINGS TABLES  —  SELECT INTO approach
-- Run in    : PB database
-- Requires  : Linked server [DSS_DB] to be active
--
-- This approach copies the schema directly from the live DSS_DB.
-- Types, lengths, and nullability are guaranteed to match exactly.
-- Use this instead of 02a if you have linked server access.
--
-- ⚠️  IDENTITY columns are NOT preserved by SELECT INTO.
--     Plain int is used instead. This is intentional here.
-- ============================================================

SELECT * INTO [dbo].[tbl_dss_AbpSettings]
    FROM [DSS_DB].[dbo].[AbpSettings]           WHERE 1=0;
GO

SELECT * INTO [dbo].[tbl_dss_AbpSettingDefinitions]
    FROM [DSS_DB].[dbo].[AbpSettingDefinitions]  WHERE 1=0;
GO
