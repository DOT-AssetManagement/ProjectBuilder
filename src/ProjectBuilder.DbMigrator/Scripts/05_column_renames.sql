-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 5    : COLUMN RENAMES  (PoolId -> LibraryId)
-- Run in    : PB database
-- Depends on: Steps 03 (tables must exist)
-- ============================================================

-- tbl_dss_WorkCandidates (TblWorkCandidates source) already uses UserLibraryId — no rename needed.
EXEC sp_rename '[dbo].[tbl_dss_ProjectCandidates].[PoolId]',  'LibraryId', 'COLUMN';
GO
EXEC sp_rename '[dbo].[tbl_dss_PoolParametersData].[PoolId]', 'LibraryId', 'COLUMN';
GO
