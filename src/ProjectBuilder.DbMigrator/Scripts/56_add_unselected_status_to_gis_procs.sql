-- Adds an @IncludeUnselected flag and a Status column to the two stored procedures behind the
-- PB scenario Map page (MapAppService.BuildGisDataAsync -> LoadProjectsAsync/LoadTreatmentsAsync):
-- sp_pb_SelectProjectsForGIS and sp_pb_SelectTreatmentsForGIS.
--
-- Today both procs hardcode "AND IsSelected=1", so a project/treatment that lost out on budget
-- during RunFullOptimizationCycle (SymphonyConductor2.cs) is never deleted -- it just sits at
-- IsSelected=0 -- and is silently excluded from the map. There is no way to see which candidates
-- were NOT funded/selected by a scenario run. See docs/discarded-projects-treatments-map-investigation.md
-- (Part 2, "Unselected treatments/projects") for the traced investigation this implements.
--
-- @IncludeUnselected defaults to 0 so every other caller of these two procs is unaffected --
-- notably ProjectBuilder.Engine/GisJsonHandler/JsonExporter.cs, which calls both directly and
-- must keep returning only selected rows. MapAppService.cs is updated separately to pass 1
-- explicitly, and to read the new Status column ('Selected'/'Unselected') so the map client can
-- color/filter by it instead of the row simply being absent.
--
-- Base body pulled from this database's live OBJECT_DEFINITION (2026-09-14), consistent with
-- this folder's convention for objects that only ever existed in the DB (see steps 44/48/55).
-- Idempotent (ALTER PROCEDURE, matches the procs' current deployed body plus this diff).
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_pb_SelectProjectsForGIS]
	@ScenId INT,
	@District TINYINT = NULL,
	@Cnty TINYINT = NULL,
	@Route INT = NULL,
	@Section VARCHAR(20) = NULL,
	@AppliedTreatment VARCHAR(100) = NULL,
	@Year INT = NULL,
	@IncludeUnselected BIT = 0
AS
BEGIN
	SET NOCOUNT ON;

    SELECT ProjectId, ProjectSchemaId AS SchemaId, ProjectSchemaId / 1000000 AS ProjectType,
	SelectedFirstYear AS [Year],
	UserId, UserNotes,
	NumBridges = (SELECT COUNT(1) FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
					WHERE ScenId=p.ScenId AND AssignedTimewiseConstrainedProjectId = p.ProjectId
						AND (@IncludeUnselected = 1 OR IsSelected=1) AND AssetType='B'),
	NumPaves = (SELECT COUNT(1) FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
					WHERE ScenId=p.ScenId AND AssignedTimewiseConstrainedProjectId = p.ProjectId
						AND (@IncludeUnselected = 1 OR IsSelected=1) AND AssetType='P'),
	ROUND(DirectCost,2) AS TotalCost,
	CASE WHEN p.IsSelected = 1 THEN 'Selected' ELSE 'Unselected' END AS Status
	FROM tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
	WHERE ScenId=@ScenId AND (@IncludeUnselected = 1 OR IsSelected=1)
	  AND (@District IS NULL OR District=@District)
	  AND (@Cnty IS NULL OR Cnty=@Cnty)
	  AND (@Route IS NULL OR [Route] = @Route)
	  AND (@Year IS NULL OR SelectedFirstYear=@Year)
	  AND (@Section IS NULL OR EXISTS(SELECT TOP 1 1 FROM tbl_pb_ExtendedImportedTreatments xt WITH (NOLOCK)
								WHERE xt.ScenId=@ScenId
								  AND xt.AssignedTimewiseConstrainedProjectId = p.ProjectId
								  AND (CONVERT(VARCHAR(10),FromSection) = @Section
										OR CONVERT(VARCHAR(10),FromSection)+'-'+CONVERT(VARCHAR(10),ToSection) = @Section)
									  )
		  )
	   AND (@AppliedTreatment IS NULL OR EXISTS(SELECT TOP 1 1 FROM tbl_pb_ExtendedImportedTreatments xt WITH (NOLOCK)
											WHERE xt.ScenId=@ScenId
												AND xt.AssignedTimewiseConstrainedProjectId=p.ProjectId
												AND xt.Treatment = @AppliedTreatment)
			)
	ORDER BY ProjectId
END
GO

ALTER PROCEDURE [dbo].[sp_pb_SelectTreatmentsForGIS]
	@ScenId INT,
	@District TINYINT = NULL,
	@Cnty TINYINT = NULL,
	@Route INT = NULL,
	@Section VARCHAR(20) = NULL,
	@AppliedTreatment VARCHAR(100) = NULL,
	@Year INT = NULL,
	@TreatmentType CHAR(1) = NULL,
	@IncludeUnselected BIT = 0
AS
BEGIN
	SET NOCOUNT ON;

    SELECT p.ProjectId, p.ProjectSchemaId / 1000000 AS ProjectType
	,xt.ExtendedTreatmentId AS TreatmentId, xt.AssetType AS TreatmentType, xt.Treatment AS AppliedTreatment, xt.ImportTimeGeneratedId
	,xt.District, xt.Cnty, xt.[Route], xt.Direction
    ,xt.FromSection
    ,xt.ToSection
	,xt.ImportTimeGeneratedId
	,xt.UserTreatmentTypeNo
	,xt.PreferredYear, xt.MinYear, xt.MaxYear, xt.PriorityOrder, xt.Risk, xt.IndirectCostDesign, xt.IndirectCostOther, xt.IndirectCostROW, xt.IndirectCostUtilities, xt.IsCommitted
	,c.County
	,Budget = dbo.fn_pb_GetLibCargoValue(xt.AssetType, xt.ImportTimeGeneratedId, 'BUDGET')
	,xt.SelectedYear AS [Year]
	,xt.BRKEY,
	 xt.BRIDGE_ID,
	 OWNER_CODE = dbo.fn_pb_GetLibCargoValue(xt.AssetType, xt.ImportTimeGeneratedId, 'OWNER_CODE')
    ,ROUND(xt.Cost,2) AS Cost
	,ROUND(xt.Benefit,2) AS Benefit
	,MPMSID = dbo.fn_pb_GetLibCargoValue(xt.AssetType, xt.ImportTimeGeneratedId, 'PROJECT ID')
	,Category = dbo.fn_pb_GetLibCargoValue(xt.AssetType, xt.ImportTimeGeneratedId, 'CATEGORY')
	,CASE WHEN xt.IsSelected = 1 THEN 'Selected' ELSE 'Unselected' END AS Status
FROM tbl_pb_ExtendedImportedTreatments xt WITH (NOLOCK)
INNER JOIN tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
	ON p.ScenId=xt.ScenId AND p.ProjectId=xt.AssignedTimewiseConstrainedProjectId
LEFT OUTER JOIN tbl_pb_Counties c WITH (NOLOCK)
	ON c.Cnty=xt.Cnty
WHERE xt.ScenId=@ScenId AND (@IncludeUnselected = 1 OR xt.IsSelected=1)
	  AND (@District IS NULL OR xt.District=@District)
	  AND (@Cnty IS NULL OR xt.Cnty=@Cnty)
	  AND (@Route IS NULL OR xt.[Route] = @Route)
	  AND (@Year IS NULL OR xt.SelectedYear=@Year)
	  AND (@Section IS NULL
			OR CONVERT(VARCHAR(10),xt.FromSection)=@Section
			OR (CONVERT(VARCHAR(10),xt.FromSection) + '-' + CONVERT(VARCHAR(10),xt.ToSection)) = @Section)
	  AND (@AppliedTreatment IS NULL OR xt.Treatment=@AppliedTreatment)
	  AND (@TreatmentType IS NULL OR xt.AssetType = @TreatmentType)
	ORDER BY ProjectId
END
GO
