USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelTreatDistricts]    Script Date: 11/2/2023 2:42:41 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ui_SelTreatDistricts] 
	@ScenId INT,
	@AssetType CHAR(1)
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT DISTINCT District FROM vw_pb_ui_ScenarioTreatments WITH (NOLOCK)
	WHERE ScenId=@ScenId
	  AND (@AssetType IS NULL OR AssetType=@AssetType)
	ORDER BY 1

END
GO


