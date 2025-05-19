USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_SelectTreatmentsForGIS]    Script Date: 12/18/2023 5:49:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER PROCEDURE [dbo].[sp_pb_SelectTreatmentsForGIS]
	@ScenId INT,
	@District TINYINT = NULL,
	@Cnty TINYINT = NULL,
	@Route INT = NULL,
	@Section VARCHAR(20) = NULL,
	@AppliedTreatment VARCHAR(100) = NULL,
	@Year INT = NULL,
	@TreatmentType CHAR(1) = NULL
AS
BEGIN
	SET NOCOUNT ON;

    SELECT p.ProjectId, p.ProjectSchemaId / 1000000 AS ProjectType
	,xt.ExtendedTreatmentId AS TreatmentId, xt.AssetType AS TreatmentType, xt.Treatment AS AppliedTreatment, lib.ImportTimeGeneratedId
	,xt.District, xt.Cnty, xt.[Route], xt.Direction 
    ,xt.FromSection
    ,xt.ToSection 
	,xt.PreferredYear, xt.MinYear, xt.MaxYear, xt.PriorityOrder, xt.Risk, xt.IndirectCostDesign, xt.IndirectCostOther, xt.IndirectCostROW, xt.IndirectCostUtilities, xt.IsCommitted
	,c.County
	,Budget = (SELECT crg.TextValue FROM tbl_pb_CargoData crg WITH (NOLOCK)
			   INNER JOIN tbl_pb_CargoAttributes ca WITH (NOLOCK)
					ON ca.AttributeNo = crg.AttributeNo
				WHERE UPPER(ca.AttributeName) = 'BUDGET'
				  AND crg.ImportTimeGeneratedGuid = xt.ImportTimeGeneratedId)
	,xt.SelectedYear AS [Year]
	,xt.BRKEY, 
	 xt.BRIDGE_ID, 
	 OWNER_CODE = (SELECT crg.TextValue FROM tbl_pb_CargoData crg WITH (NOLOCK)
				   INNER JOIN tbl_pb_CargoAttributes ca WITH (NOLOCK)
						ON ca.AttributeNo = crg.AttributeNo
					WHERE UPPER(ca.AttributeName) = 'OWNER_CODE'
				  AND crg.ImportTimeGeneratedGuid = xt.ImportTimeGeneratedId)
    ,ROUND(xt.Cost,2) AS Cost
	,ROUND(xt.Benefit,2) AS Benefit
	,MPMSID = (SELECT crg.TextValue FROM tbl_pb_CargoData crg WITH (NOLOCK)
			   INNER JOIN tbl_pb_CargoAttributes ca WITH (NOLOCK)
					ON ca.AttributeNo = crg.AttributeNo
				WHERE UPPER(ca.AttributeName) = 'PROJECTSOURCEID'
				  AND crg.ImportTimeGeneratedGuid = xt.ImportTimeGeneratedId)
FROM tbl_pb_ExtendedImportedTreatments xt WITH (NOLOCK)
INNER JOIN tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
	ON p.ScenId=xt.ScenId AND p.ProjectId=xt.AssignedTimewiseConstrainedProjectId
LEFT OUTER JOIN tbl_pb_Counties c WITH (NOLOCK)
	ON c.Cnty=xt.Cnty
WHERE xt.ScenId=@ScenId AND xt.IsSelected=1
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

