-- Speeds up sp_pb_SelectTreatmentsForGIS, the treatments query behind the PB scenario Map page
-- (MapAppService.LoadTreatmentsAsync) and ProjectBuilder.Engine/GisJsonHandler/JsonExporter.cs.
--
-- The proc called the scalar UDF fn_pb_GetLibCargoValue four times per row (BUDGET, OWNER_CODE,
-- PROJECT ID, CATEGORY), each a lookup into tbl_lib_CargoData (~10.8M rows). For a 15,760-row
-- scenario that measured ~10.8s, against ~0.36s for the same SELECT without the UDF calls.
--
-- Changes:
--   * BUDGET and OWNER_CODE are resolved once per (AssetType, ImportTimeGeneratedId) into a temp
--     table with one set-based join, then joined back. Same semantics as the UDF: no match or a
--     NULL TextValue returns '' and the value is NVARCHAR(255).
--   * MPMSID and Category are dropped -- neither caller reads them.
--   * The duplicate ImportTimeGeneratedId column is dropped.
-- Every other column, name, type and the row set/order are unchanged. Both callers read columns
-- by name, and no other database object calls this proc.
--
-- Idempotent (ALTER PROCEDURE).
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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

	-- One cargo lookup per distinct import key in the scenario, instead of one UDF call per row per attribute.
	SELECT d.AssetType, d.ImportTimeGeneratedGuid,
		Budget     = CONVERT(NVARCHAR(255), MAX(CASE WHEN g.AttributeName = 'BUDGET'     THEN d.TextValue END)),
		OWNER_CODE = CONVERT(NVARCHAR(255), MAX(CASE WHEN g.AttributeName = 'OWNER_CODE' THEN d.TextValue END))
	INTO #Cargo
	FROM (SELECT DISTINCT AssetType, ImportTimeGeneratedId
		  FROM tbl_pb_ExtendedImportedTreatments WITH (NOLOCK)
		  WHERE ScenId = @ScenId) k
	INNER JOIN tbl_lib_CargoData d WITH (NOLOCK)
		ON d.AssetType = k.AssetType AND d.ImportTimeGeneratedGuid = k.ImportTimeGeneratedId
	INNER JOIN tbl_lib_CargoAttributes g WITH (NOLOCK)
		ON g.LibraryId = d.LibraryId
		AND g.AssetType = d.AssetType
		AND g.AttributeNo = d.AttributeNo
		AND g.AttributeName IN ('BUDGET', 'OWNER_CODE')
	GROUP BY d.AssetType, d.ImportTimeGeneratedGuid;

	SELECT p.ProjectId, p.ProjectSchemaId / 1000000 AS ProjectType
	,xt.ExtendedTreatmentId AS TreatmentId, xt.AssetType AS TreatmentType, xt.Treatment AS AppliedTreatment, xt.ImportTimeGeneratedId
	,xt.District, xt.Cnty, xt.[Route], xt.Direction
	,xt.FromSection
	,xt.ToSection
	,xt.UserTreatmentTypeNo
	,xt.PreferredYear, xt.MinYear, xt.MaxYear, xt.PriorityOrder, xt.Risk, xt.IndirectCostDesign, xt.IndirectCostOther, xt.IndirectCostROW, xt.IndirectCostUtilities, xt.IsCommitted
	,c.County
	,Budget = ISNULL(cg.Budget, N'')
	,xt.SelectedYear AS [Year]
	,xt.BRKEY,
	 xt.BRIDGE_ID,
	 OWNER_CODE = ISNULL(cg.OWNER_CODE, N'')
	,ROUND(xt.Cost,2) AS Cost
	,ROUND(xt.Benefit,2) AS Benefit
	,CASE WHEN xt.IsSelected = 1 THEN 'Selected' ELSE 'Unselected' END AS Status
FROM tbl_pb_ExtendedImportedTreatments xt WITH (NOLOCK)
INNER JOIN tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
	ON p.ScenId=xt.ScenId AND p.ProjectId=xt.AssignedTimewiseConstrainedProjectId
LEFT OUTER JOIN tbl_pb_Counties c WITH (NOLOCK)
	ON c.Cnty=xt.Cnty
LEFT OUTER JOIN #Cargo cg
	ON cg.AssetType = xt.AssetType AND cg.ImportTimeGeneratedGuid = xt.ImportTimeGeneratedId
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
