-- Calls sp_pb_SplitCommittedTreatmentsFromProjects (script 64) before each aggregation pass:
-- once for the initial projects and again after the V3 bridge regrouping, which can re-mix
-- committed and uncommitted treatments. Run script 64 first.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_pb_CreateExtendedProjectsV2]
	@ScenId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @RowCount BIGINT;

	PRINT 'sp_pb_CreateExtendedProjectV2 ScenId=' + CONVERT(VARCHAR(10), @ScenId) + ' started...';

	EXEC dbo.sp_pb_PopulateExtendedTreatmentsV2 @ScenId;
	EXEC dbo.sp_pb_SetIndirectCostsForExtendedTreatments @ScenId;

	DELETE FROM tbl_pb_AssetsWithMultipleTreatments WHERE ScenId=@ScenId;
	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records deleted from table tbl_pb_AssetsWithMultipleTreatments: ' + CONVERT(VARCHAR(10), @RowCount);

	DELETE FROM tbl_pb_MultiYearTreatments WHERE ScenId=@ScenId;
	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records deleted from table tbl_pb_MultiYearTreatments: ' + CONVERT(VARCHAR(10), @RowCount);

	DECLARE @IsolatedBridge BIT = 0;
	EXEC dbo.sp_pb_ProcessMultipleTreatmentsOnSameAsset @ScenId, @IsolatedBridge;

	EXEC dbo.sp_pb_RunTreatmentCancellationLogic @ScenId;

	EXEC dbo.sp_pb_CreateExtendedTimewiseConstrainedProjectsV2 @ScenId;

	EXEC dbo.sp_pb_JoinAdjacentTimewiseProjectsV2 @ScenId;
	EXEC dbo.sp_pb_AddExtendedProjectsForIsolatedTreatments @ScenId;
	EXEC dbo.sp_pb_ApplyScenarioTreatmentShiftToExtendedTreatments @ScenId;
	EXEC dbo.sp_pb_AddMultiYearProjects @ScenId;
	EXEC dbo.sp_pb_SplitCommittedTreatmentsFromProjects @ScenId;
	EXEC dbo.sp_pb_SetAggregatedFieldsOfTimewiseConstrainedProjects @ScenId;

	-- DIG 02/04/2026 V3 functions added to deal with
	-- bridge projects having no super assets that initialy contain large numbers of
	-- treatments lumped together
	-- BEGIN
	EXEC dbo.sp_pb_CreateExtendedUnconstrainedProjectsForBridgesV3 @ScenId;
	EXEC dbo.sp_pb_CreateExtendedSpatiallyConstrainedProjectsV3 @ScenId;
	EXEC dbo.sp_pb_CreateExtendedTimewiseConstrainedProjectsV3 @ScenId;
	EXEC dbo.sp_pb_SplitCommittedTreatmentsFromProjects @ScenId;
	-- Do it again for the newly added projects
	EXEC dbo.sp_pb_SetAggregatedFieldsOfTimewiseConstrainedProjects @ScenId;
	-- END

	PRINT 'sp_pb_CreateExtendedProjectsV2 ScenId=' + CONVERT(VARCHAR(10), @ScenId) + ' ended.';

END
GO
