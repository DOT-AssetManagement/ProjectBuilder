USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelTreatRoutes]    Script Date: 11/2/2023 2:43:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ui_SelTreatRoutes] 
	@ScenId INT,
	@District TINYINT,
	@County NVARCHAR(56)
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT DISTINCT [Route] FROM vw_pb_ui_ScenarioTreatments WITH (NOLOCK)
	WHERE ScenId=@ScenId
	  AND (@District IS NULL OR District=@District)
	  AND (@County IS NULL OR County=@County)
	ORDER BY 1

END
GO


