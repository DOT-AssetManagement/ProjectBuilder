-- Makes sp_pb_AddExtendedProjectsForIsolatedTreatments honor tbl_pb_ScenarioFilters, same pattern already used in sp_pb_PopulateExtendedTreatmentsV2 (falls back to "include all" when no filters are defined for the scenario).
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_pb_AddExtendedProjectsForIsolatedTreatments]
	@ScenId INT
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @YearFirst INT;
	DECLARE @YearLast INT;
	DECLARE @RowCount BIGINT = 0;

	PRINT 'sp_pb_AddExtendedProjectsForIsolatedTreatments @ScenId=' + CONVERT(VARCHAR(10), @ScenId) + ' - started...';

	SELECT @YearFirst = COALESCE(CONVERT(INT,ParmValue), 0) FROM vw_pb_ScenarioParameters WITH (NOLOCK)
	WHERE ScenId=@ScenId AND ParmId='YFST';

	SELECT @YearLast = COALESCE(CONVERT(INT,ParmValue), 0) FROM vw_pb_ScenarioParameters WITH (NOLOCK)
	WHERE ScenId=@ScenId AND ParmId='YLST';

	PRINT '@YearFirst=' + CONVERT(VARCHAR(10), @YearFirst) + ', @YearLast=' + CONVERT(VARCHAR(10), @YearLast);

	DECLARE @NumFilters BIGINT = 0;
	SELECT @NumFilters = COUNT(1) FROM tbl_pb_ScenarioFilteredTreatments WITH (NOLOCK) WHERE ScenId=@ScenId;
	PRINT 'Number of filtering records for scenario ' + CONVERT(VARCHAR(10), @ScenId) + ': ' + CONVERT(VARCHAR(10),@NumFilters);

	INSERT INTO tbl_pb_ExtendedImportedTreatments (ScenId,ImportTimeGeneratedId, AssetType,
		ImportedTreatmentId,
		District, Cnty, [Route], Direction,
		FromSection, ToSection,
		Interstate,
		Treatment, Benefit, Cost, Risk, IsCommitted, PriorityOrder, PreferredYear,
		MinYear, MaxYear,
		BRKEY, BRIDGE_ID, IsolatedBridge)
	SELECT @ScenId, ImportTimeGeneratedId, AssetType,
		ImportedTreatmentId,
		District, Cnty, [Route],  [Direction],
		FromSection, ToSection,
		Interstate,
		Treatment, Benefit, Cost, Risk, IsCommitted, PriorityOrder, PreferredYear,
		CASE WHEN MinYear > @YearFirst THEN MinYear ELSE @YearFirst END AS MinYear,
		CASE WHEN MaxYear < @YearLast THEN MaxYear ELSE @YearLast END AS MaxYear,
		BRKEY, BRIDGE_ID, IsIsolatedBridge
	FROM tbl_lib_LibraryTreatments WITH  (NOLOCK)
		WHERE LibraryId = (SELECT LibraryId FROM tbl_pb_Scenarios WITH (NOLOCK) WHERE ScenId=@ScenId)
		  AND MaxYear >= @YearFirst
		  AND MinYear <= @YearLast
		  AND PreferredYear BETWEEN @YearFirst AND @YearLast
		  AND IsIsolatedBridge = 1
		  AND (@NumFilters = 0
				OR
				ImportTimeGeneratedId IN (SELECT ImportTimeGeneratedId
										FROM tbl_pb_ScenarioFilteredTreatments WITH (NOLOCK)
										WHERE ScenId = @ScenId)
			  )
	ORDER BY ImportTimeGeneratedId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records inserted into tbl_pb_ExtendedImportedTreatments for isolated treatments: ' +
		CONVERT(VARCHAR(10), @RowCount);

	DECLARE @IsolatedBridge BIT = 1;
	EXEC sp_pb_ProcessMultipleTreatmentsOnSameAsset @ScenId, @IsolatedBridge;

	DECLARE @MaxId INT = 0;
	SELECT @MaxId=MAX(ProjectId)
	FROM tbl_pb_ExtendedTimewiseConstrainedProjects WITH (NOLOCK)
	WHERE ScenId=@ScenId;

	SET @MaxId = COALESCE(@MaxId,0);

	PRINT '@MaxId=' + CONVERT(VARCHAR(10), @MaxId);

	CREATE TABLE #X (
		Id INT IDENTITY(1,1),
		ExtendedTreatmentId INT
		)
	CREATE INDEX IX_#X ON #X (ExtendedTreatmentid);

	INSERT INTO #X (ExtendedTreatmentId)
	SELECT ExtendedTreatmentId
	FROM tbl_pb_ExtendedImportedTreatments x WITH (NOLOCK)
	WHERE x.ScenId=@ScenId
	  AND x.IsolatedBridge=1
	  AND x.IsPartOfMultiYear = 0
	ORDER BY ExtendedTreatmentId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records inserted into #X: ' + CONVERT(VARCHAR(10), @RowCount);

	UPDATE tbl_pb_ExtendedImportedTreatments
		SET AssignedTimewiseConstrainedProjectId = @MaxId + x.Id
	FROM tbl_pb_ExtendedImportedTreatments t WITH (ROWLOCK)
	INNER JOIN #X x ON x.ExtendedTreatmentId = t.ExtendedTreatmentId
	WHERE ScenId=@ScenId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records updated in tbl_pb_ExtendedImportedTreatments (AssignedTimewiseConstrainedProjectId set): ' + CONVERT(VARCHAR(10), @RowCount);

	INSERT INTO tbl_pb_ExtendedTimewiseConstrainedProjects (ScenId, ProjectId, District, Cnty, [Route],
		Direction, MinYear, MaxYear, MinSection, MaxSection, MiscellaneousNotes, IsolatedBridge)
	SELECT @ScenId, AssignedTimewiseConstrainedProjectId, District, Cnty, [Route],
		Direction, MinYear, MaxYear, FromSection, ToSection, 'Isolated treatment', t.IsolatedBridge
	FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
	INNER JOIN #X x ON x.ExtendedTreatmentId = t.ExtendedTreatmentId
	WHERE t.ScenId=@ScenId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records added to tbl_pb_ExtendedTimewiseConstrainedProjects: ' + CONVERT(VARCHAR(10), @RowCount);

	DROP TABLE #X;

	PRINT 'sp_pb_AddExtendedProjectsForIsolatedTreatments @ScenId=' + CONVERT(VARCHAR(10), @ScenId) + ' - ended.';

END
GO
