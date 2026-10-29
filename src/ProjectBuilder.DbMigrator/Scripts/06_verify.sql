-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 6    : VERIFY — Shared table name check
-- Run in    : PB database
-- This is a read-only check; safe to re-run at any time.
-- ============================================================

SELECT
    dss.name                           AS DssTableName,
    pb.name                            AS PbTableName,
    REPLACE(dss.name, 'tbl_dss_', '') AS CommonTableName
FROM sys.tables dss
INNER JOIN sys.tables pb
    ON REPLACE(dss.name, 'tbl_dss_', '') = REPLACE(pb.name, 'tbl_pb_', '')
WHERE dss.name LIKE 'tbl_dss_%'
  AND pb.name  LIKE 'tbl_pb_%'
ORDER BY CommonTableName;
