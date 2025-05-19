USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelTreatAssetTypes]    Script Date: 11/2/2023 2:41:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ui_SelTreatAssetTypes] 
	@ScenId INT
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT DISTINCT AssetType FROM vw_pb_ui_ScenarioTreatments WITH (NOLOCK)
	WHERE ScenId=@ScenId
	ORDER BY 1

END
GO


