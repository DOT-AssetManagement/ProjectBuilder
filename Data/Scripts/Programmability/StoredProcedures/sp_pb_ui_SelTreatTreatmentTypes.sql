USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelTreatTreatmentTypes]    Script Date: 11/2/2023 2:44:09 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ui_SelTreatTreatmentTypes] 
	@ScenId INT,
	@District TINYINT,
	@County NVARCHAR(56),
	@Route INT,
	@Section NVARCHAR(21)
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT DISTINCT TreatmentType FROM vw_pb_ui_ScenarioTreatments WITH (NOLOCK)
	WHERE ScenId=@ScenId
	  AND (@District IS NULL OR District=@District)
	  AND (@County IS NULL OR County=@County)
	  AND (@Route IS NULL OR [Route]=@Route)
	  AND (@Section IS NULL OR Section=@Section)
	ORDER BY 1

END
GO


