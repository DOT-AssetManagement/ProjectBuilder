-- Fixes stale, pre-rename table names left over in the DEA (Data Envelopment Analysis)
-- stored procedures that the DSS "Run" pipeline depends on (DataManager.RunScenario ->
-- UspStartScenarioRun -> DEAContext.LoadFromDatabase/RunDEA -> UspUpdateRelativeEfficiencies).
--
-- These procedures were migrated from the original DSS_DB verbatim and never updated when
-- the DSS tables were renamed to the tbl_dss_* convention (see README.md), so they still
-- reference TblScenarios / TblProjectCandidates / TblScenarioProjectObjectiveValues, which
-- don't exist under those names in this database. Running a DSS pool's scenario would fail
-- with "Invalid object name 'TblScenarios'" the moment UspGetProjectCandidates or
-- UspUpdateRelativeEfficiencies executed.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[UspGetProjectCandidates]
	@ScenId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @PoolId UNIQUEIDENTIFIER = NULL;
	IF @ScenId IS NOT NULL BEGIN
		SET @PoolId = (SELECT PoolId FROM tbl_dss_Scenarios WITH (NOLOCK) WHERE Id=@ScenId);
	END
	PRINT '@PoolId=' + CONVERT(VARCHAR(50), @PoolId);
	IF @ScenId IS NOT NULL BEGIN
		SELECT p.ID AS PId, ROW_NUMBER() OVER(ORDER BY p.ID) AS PNo, TotalCost AS C1,
			S1 = p.SafetyScore, S2 = p.MobilityAndEconomyScore, S3 = p.EquityAndAccessScore,
			S4 = p.ResilienceAndEnvironmentScore, S5 = p.ConditionAndPerformanceScore
		FROM tbl_dss_ProjectCandidates p WITH (NOLOCK)
		WHERE (@PoolId IS NULL OR PoolId = @PoolId);
	END ELSE BEGIN
		SELECT TOP 1 p.ID AS PId, ROW_NUMBER() OVER(ORDER BY p.ID) AS PNo, TotalCost AS C1,
			S1 = p.SafetyScore, S2 = p.MobilityAndEconomyScore, S3 = p.EquityAndAccessScore,
			S4 = p.ResilienceAndEnvironmentScore, S5 = p.ConditionAndPerformanceScore
		FROM tbl_dss_ProjectCandidates p WITH (NOLOCK)
		WHERE (@PoolId IS NULL OR PoolId = @PoolId)
	END
END
GO

ALTER PROCEDURE [dbo].[UspUpdateRelativeEfficiencies]
	@ScenId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RowCount BIGINT;
	PRINT 'UspUpdateRelativeEfficiencies - started...';
	DECLARE @PoolId UNIQUEIDENTIFIER = (SELECT PoolId FROM tbl_dss_Scenarios WITH (NOLOCK) WHERE Id=@ScenId);

	UPDATE tbl_dss_ProjectCandidates
		SET RelativeEfficiency = b.RelativeEfficiency, RelativeEfficiencySetAt = CURRENT_TIMESTAMP
	FROM tbl_dss_ProjectCandidates a WITH (ROWLOCK)
	INNER JOIN tbl_dss_ScenarioProjectObjectiveValues b WITH (NOLOCK) ON b.ScenId=@ScenId AND b.ProjectId=a.Id
	WHERE a.PoolId=@PoolId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Number of records updated: ' + CONVERT(VARCHAR(10),@RowCount);
	PRINT 'UspUpdateRelativeEfficiencies - ended.';
END
GO
