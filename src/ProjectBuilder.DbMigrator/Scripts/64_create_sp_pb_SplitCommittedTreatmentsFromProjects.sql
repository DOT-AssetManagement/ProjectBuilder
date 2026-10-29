-- Moves committed treatments out of projects that also hold uncommitted ones into their own project,
-- so HasCommitted=1 means every treatment in the project is committed and the engine can fund
-- just those, while the uncommitted remainder keeps competing for budget.
-- Multi-year (cash-flow) projects are left intact. Called from sp_pb_CreateExtendedProjectsV2
-- before each sp_pb_SetAggregatedFieldsOfTimewiseConstrainedProjects (see script 65).
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_pb_SplitCommittedTreatmentsFromProjects]
	@ScenId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @RowCount BIGINT;
	DECLARE @MaxId INT;

	PRINT 'sp_pb_SplitCommittedTreatmentsFromProjects @ScenId=' + CONVERT(VARCHAR(10), @ScenId) + ' - started...';

	CREATE TABLE #Mixed (
		Id INT IDENTITY(1,1),
		ParentProjectId INT NOT NULL PRIMARY KEY
		);

	INSERT INTO #Mixed (ParentProjectId)
	SELECT p.ProjectId
	FROM tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
	INNER JOIN tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
		ON t.ScenId = p.ScenId
	   AND t.AssignedTimewiseConstrainedProjectId = p.ProjectId
	WHERE p.ScenId = @ScenId
	  AND COALESCE(p.IsSuppressed, 0) = 0
	  AND COALESCE(p.IsMultiYear, 0) = 0
	GROUP BY p.ProjectId
	HAVING MIN(CONVERT(INT, COALESCE(t.IsCommitted, 0))) = 0
	   AND MAX(CONVERT(INT, COALESCE(t.IsCommitted, 0))) = 1
	ORDER BY p.ProjectId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Projects with both committed and uncommitted treatments: ' + CONVERT(VARCHAR(10), @RowCount);

	IF @RowCount > 0
	BEGIN
		SELECT @MaxId = COALESCE(MAX(ProjectId), 0)
		FROM tbl_pb_ExtendedTimewiseConstrainedProjects WITH (NOLOCK)
		WHERE ScenId = @ScenId;

		PRINT '@MaxId=' + CONVERT(VARCHAR(10), @MaxId);

		-- Aggregated fields (sections, years, costs, HasCommitted, ProjectSchemaId) are filled in
		-- by sp_pb_SetAggregatedFieldsOfTimewiseConstrainedProjects
		INSERT INTO tbl_pb_ExtendedTimewiseConstrainedProjects (ScenId, ProjectId,
			RefUnconstrainedProjectId, RefSpatiallyConstrainedProjectId,
			District, Cnty, [Route], Direction, MinSection, MaxSection, MinYear, MaxYear,
			IsSuppressed, MiscellaneousNotes, IsolatedBridge, IsMultiYear, IsUserCreated, SuperAssetId)
		SELECT p.ScenId, @MaxId + m.Id,
			p.RefUnconstrainedProjectId, p.RefSpatiallyConstrainedProjectId,
			p.District, p.Cnty, p.[Route], p.Direction, p.MinSection, p.MaxSection, p.MinYear, p.MaxYear,
			0, 'Committed treatments split from project ' + CONVERT(VARCHAR(10), p.ProjectId),
			p.IsolatedBridge, p.IsMultiYear, p.IsUserCreated, p.SuperAssetId
		FROM #Mixed m
		INNER JOIN tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
			ON p.ScenId = @ScenId
		   AND p.ProjectId = m.ParentProjectId
		ORDER BY m.Id;

		SET @RowCount = @@ROWCOUNT;
		PRINT 'Committed-only projects added to tbl_pb_ExtendedTimewiseConstrainedProjects: ' + CONVERT(VARCHAR(10), @RowCount);

		UPDATE tbl_pb_ExtendedImportedTreatments
			SET AssignedTimewiseConstrainedProjectId = @MaxId + m.Id
		FROM tbl_pb_ExtendedImportedTreatments t WITH (ROWLOCK)
		INNER JOIN #Mixed m
			ON m.ParentProjectId = t.AssignedTimewiseConstrainedProjectId
		WHERE t.ScenId = @ScenId
		  AND t.IsCommitted = 1;

		SET @RowCount = @@ROWCOUNT;
		PRINT 'Committed treatments moved to their own project: ' + CONVERT(VARCHAR(10), @RowCount);

		-- The aggregation proc only recalculates rows whose ProjectSchemaId is NULL
		UPDATE tbl_pb_ExtendedTimewiseConstrainedProjects
			SET ProjectSchemaId = NULL
		FROM tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (ROWLOCK)
		INNER JOIN #Mixed m
			ON m.ParentProjectId = p.ProjectId
		WHERE p.ScenId = @ScenId;

		SET @RowCount = @@ROWCOUNT;
		PRINT 'Parent projects reset for re-aggregation: ' + CONVERT(VARCHAR(10), @RowCount);
	END

	DROP TABLE #Mixed;

	PRINT 'sp_pb_SplitCommittedTreatmentsFromProjects @ScenId=' + CONVERT(VARCHAR(10), @ScenId) + ' - ended.';
END
GO
