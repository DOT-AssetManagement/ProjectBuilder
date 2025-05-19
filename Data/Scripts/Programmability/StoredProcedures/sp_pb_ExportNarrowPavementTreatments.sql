USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ExportNarrowPavementTreatments]    Script Date: 12/8/2023 11:57:14 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ExportNarrowPavementTreatments] 
	@ScenId INT
AS
BEGIN
	
	SET NOCOUNT ON;

	/* SQL used to generate SELECT
	SELECT ', ' + AttributeName +'=(SELECT COALESCE(TextValue,'''') FROM tbl_pb_CargoData WITH (NOLOCK) ' +
' WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=' + CONVERT(VARCHAR(10),AttributeNo)+')'
FROM tbl_pb_CargoAttributes WITH (NOLOCK) WHERE AssetType='P'
ORDER BY AttributeNo;
	*/
	 PRINT 'sp_ExportNarrowPavementTreatments @ScenId=' + CONVERT(VARCHAR(10),@ScenId) + ' - started...';

	DECLARE @LibraryId UNIQUEIDENTIFIER;

	SELECT TOP 1 @LibraryId=LibraryId FROM tbl_pb_Scenarios WITH (NOLOCK) WHERE ScenId=@ScenId;
	IF @LibraryId IS NULL BEGIN
		PRINT 'LibraryId is NULL for the scenario.  tbl_pb_ImportedTreatments is used.';
		SELECT a.Asset AS CRS
			, t.Treatment AS TREATMENT
			, t.SelectedYear AS [YEAR]
			, BUDGET=(SELECT COALESCE(TextValue,'') FROM tbl_pb_CargoData WITH (NOLOCK)  WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=19)
			, t.Cost AS [COST]
			, CASE t.IsUserCreated WHEN 0 THEN 'ProjectBuider' ELSE 'User' END AS [PROJECTSOURCE]
			, [AREA] = (SELECT TOP 1 Area FROM tbl_pams_MaintainableAssetsSegmentation s WITH (NOLOCK)
				WHERE s.Asset=a.Asset)
			, CATEGORY=(SELECT COALESCE(TextValue,'') FROM tbl_pb_CargoData WITH (NOLOCK)  WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=20)
			, t.IsCommitted AS [COMMITTED]
		  FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
		  INNER JOIN tbl_pb_ImportedTreatments a WITH (NOLOCK)
			ON (a.TreatmentID=t.ImportedTreatmentId OR a.ImportTimeGeneratedId=t.ImportTimeGeneratedId)
		  WHERE t.ScenId=@ScenId 
			AND t.AssetType='P' 
			AND t.IsSelected=1
		  ORDER BY t.ExtendedTreatmentID
	END ELSE BEGIN
		PRINT 'LibraryId is SET for the scenario.  tbl_lib_LibraryTreatments is used.';
		SELECT a.Asset AS CRS
			, t.Treatment AS TREATMENT
			, t.SelectedYear AS [YEAR]
			, BUDGET=(SELECT COALESCE(TextValue,'') FROM tbl_pb_CargoData WITH (NOLOCK)  WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=19)
			, t.Cost AS [COST]
			, CASE t.IsUserCreated WHEN 0 THEN 'ProjectBuider' ELSE 'User' END AS [PROJECTSOURCE]
			, [AREA] = (SELECT TOP 1 Area FROM tbl_pams_MaintainableAssetsSegmentation s WITH (NOLOCK)
				WHERE s.Asset=a.Asset)
			, CATEGORY=(SELECT COALESCE(TextValue,'') FROM tbl_pb_CargoData WITH (NOLOCK)  WHERE ImportTimeGeneratedGuid=a.ImportTimeGeneratedId AND AttributeNo=20)
			, t.IsCommitted AS [COMMITTED]
		  FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
		  INNER JOIN tbl_lib_LibraryTreatments a WITH (NOLOCK)
			ON a.ImportTimeGeneratedId = t.ImportTimeGeneratedId
		  WHERE t.ScenId=@ScenId 
			AND t.AssetType='P' 
			AND t.IsSelected=1
			AND a.LibraryId = @LibraryId
		  ORDER BY t.ExtendedTreatmentID
	END

	PRINT 'sp_ExportNarrowPavementTreatments @ScenId=' + CONVERT(VARCHAR(10),@ScenId) + ' - started...';

   
END
GO

