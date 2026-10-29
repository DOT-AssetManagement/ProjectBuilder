-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 1    : ALTER PB TABLE
-- Run in    : PB database
-- ============================================================

ALTER TABLE [dbo].[tbl_pb_Scenarios]
ADD
    [ScenNo]         INT            NULL,
    [Description]    NVARCHAR(MAX)  NULL,
    [DeaInputSproc]  NVARCHAR(255)  NULL,
    [DeaOutputSproc] NVARCHAR(255)  NULL;
GO
