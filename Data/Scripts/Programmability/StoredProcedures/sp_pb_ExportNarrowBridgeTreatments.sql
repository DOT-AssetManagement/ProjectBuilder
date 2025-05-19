USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ExportNarrowBridgeTreatments]    Script Date: 12/8/2023 11:56:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ExportNarrowBridgeTreatments] 
	@ScenId INT
AS
BEGIN
	SET NOCOUNT ON;

    PRINT 'sp_ExportNarrowBridgeTreatments @ScenId=' + CONVERT(VARCHAR(10),@ScenId) + ' - started...';

	/* SQL Used to generate SELECTs
	SELECT ', ' + AttributeName +'=(SELECT COALESCE(TextValue,'''') FROM tbl_pb_CargoData WITH (NOLOCK) ' +
' WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=' + CONVERT(VARCHAR(10),AttributeNo)+')'
FROM tbl_pb_CargoAttributes WITH (NOLOCK) WHERE AssetType='B'
ORDER BY AttributeNo;
	*/

	DECLARE @LibraryId UNIQUEIDENTIFIER;

	SELECT TOP 1 @LibraryId=LibraryId FROM tbl_pb_Scenarios WITH (NOLOCK) WHERE ScenId=@ScenId;
	IF @LibraryId IS NULL BEGIN
		PRINT 'LibraryId is NULL for the scenario.  tbl_pb_ImportedTtreatments is used.';
		SELECT CASE WHEN t.BRKEY IS NULL THEN '' ELSE t.BRKEY END AS BRKEY_
			, CASE WHEN ISNULL(t.BRIDGE_ID,0)=0 THEN '' ELSE CONVERT(NVARCHAR(20),t.BRIDGE_ID) END AS BMSID
			, t.Treatment AS TREATMENT
			, t.SelectedYear AS [YEAR]
			, BUDGET=(SELECT COALESCE(TextValue,'') FROM tbl_pb_CargoData WITH (NOLOCK)  WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=5)
			, t.Cost AS COST
			, CASE t.IsUserTreatment WHEN 1 THEN 'User' ELSE 'ProjectBuilder' END AS [PROJECTSOURCE]
			, '' AS [AREA]
			, CATEGORY=(SELECT COALESCE(TextValue,'') FROM tbl_pb_CargoData WITH (NOLOCK)  WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=2)
			, t.IsCommitted AS [COMMITTED]
		FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
		INNER JOIN tbl_pb_ImportedTreatments a WITH (NOLOCK)
			ON (a.TreatmentID=t.ImportedTreatmentId OR a.ImportTimeGeneratedId=t.ImportTimeGeneratedId)
		 WHERE t.ScenId=@ScenId AND t.AssetType='B' AND t.IsSelected=1
		 ORDER BY a.BRKEY
	END ELSE BEGIN
		PRINT 'LibraryId is set for the scenario.  tbl_lib_LibraryTtreatments is used.';
		SELECT CASE WHEN t.BRKEY IS NULL THEN '' ELSE t.BRKEY END AS BRKEY_
			, CASE WHEN ISNULL(t.BRIDGE_ID,0)=0 THEN '' ELSE CONVERT(NVARCHAR(20),t.BRIDGE_ID) END AS BMSID
			, t.Treatment AS TREATMENT
			, t.SelectedYear AS [YEAR]
			, BUDGET=(SELECT COALESCE(TextValue,'') FROM tbl_pb_CargoData WITH (NOLOCK)  WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=5)
			, t.Cost AS COST
			, CASE t.IsUserTreatment WHEN 1 THEN 'User' ELSE 'ProjectBuilder' END AS [PROJECTSOURCE]
			, '' AS [AREA]
			, CATEGORY=(SELECT COALESCE(TextValue,'') FROM tbl_pb_CargoData WITH (NOLOCK)  WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=2)
			, t.IsCommitted AS [COMMITTED]
		FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
		LEFT OUTER JOIN tbl_lib_LibraryTreatments a WITH (NOLOCK)
			ON a.ImportTimeGeneratedId=t.ImportTimeGeneratedId
		WHERE t.ScenId=@ScenId 
			AND a.LibraryId = @LibraryId
		    AND t.AssetType='B' 
			AND t.IsSelected=1
		  ORDER BY a.BRKEY
	END

	PRINT 'sp_ExportNarrowBridgeTreatments @ScenId=' + CONVERT(VARCHAR(10),@ScenId) + ' - ended.';
END
GO

