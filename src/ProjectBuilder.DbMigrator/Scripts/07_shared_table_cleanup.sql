-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 7    : SHARED TABLE CLEANUP
-- Run in    : PB database
-- Depends on: Steps 01-06 completed successfully
--
-- Strategy  : PB versions are authoritative.
--             Drop DSS copies; rename PB tables to shared names.
-- ============================================================

---------------------------------------------------------------
-- BridgeToPavements
---------------------------------------------------------------
DROP TABLE [dbo].[tbl_dss_BridgeToPavements];
GO
EXEC sp_rename 'dbo.tbl_pb_BridgeToPavements', 'tbl_BridgeToPavements';
GO

---------------------------------------------------------------
-- Counties
---------------------------------------------------------------
DROP TABLE [dbo].[tbl_dss_Counties];
GO
EXEC sp_rename 'dbo.tbl_pb_Counties', 'tbl_Counties';
GO

---------------------------------------------------------------
-- ImportSessions
---------------------------------------------------------------
DROP TABLE [dbo].[tbl_dss_ImportSessions];
GO
EXEC sp_rename 'dbo.tbl_pb_ImportSessions', 'tbl_ImportSessions';
GO

---------------------------------------------------------------
-- Scenarios
---------------------------------------------------------------
DROP TABLE [dbo].[tbl_dss_Scenarios];
GO
EXEC sp_rename 'dbo.tbl_pb_Scenarios', 'tbl_Scenarios';
GO
