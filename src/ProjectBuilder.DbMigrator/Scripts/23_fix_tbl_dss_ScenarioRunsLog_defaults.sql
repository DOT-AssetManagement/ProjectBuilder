-- Restores DEFAULT constraints on tbl_dss_ScenarioRunsLog that the linked-server SELECT INTO
-- migration didn't preserve (the README already documents that this approach drops IDENTITY;
-- it drops DEFAULT constraints too). UspStartScenarioRun's INSERT only sets (ScenId, ScenNo,
-- Status), relying on Id and StartedAt to auto-populate — without these defaults every DSS
-- scenario run fails immediately with "Cannot insert the value NULL into column 'Id'".
IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('tbl_dss_ScenarioRunsLog') AND c.name = 'Id'
)
    ALTER TABLE tbl_dss_ScenarioRunsLog ADD DEFAULT NEWID() FOR Id;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('tbl_dss_ScenarioRunsLog') AND c.name = 'StartedAt'
)
    ALTER TABLE tbl_dss_ScenarioRunsLog ADD DEFAULT CURRENT_TIMESTAMP FOR StartedAt;
GO
