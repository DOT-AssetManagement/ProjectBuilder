-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 4    : CREATE INDEXES
-- Run in    : PB database
-- ============================================================

CREATE NONCLUSTERED INDEX [IX_tbl_dss_AbpSettings_Name]
    ON [dbo].[tbl_dss_AbpSettings] ([Name]);
GO

CREATE NONCLUSTERED INDEX [IX_tbl_dss_AbpSettingDefinitions_Name]
    ON [dbo].[tbl_dss_AbpSettingDefinitions] ([Name]);
GO
