USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelTreatCounties]    Script Date: 11/2/2023 2:42:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ui_SelTreatCounties] 
	@ScenId INT,
	@District TINYINT
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT DISTINCT County FROM vw_pb_ui_ScenarioTreatments WITH (NOLOCK)
	WHERE ScenId=@ScenId
	  AND (@District IS NULL OR District=@District)
	ORDER BY 1

END
GO


