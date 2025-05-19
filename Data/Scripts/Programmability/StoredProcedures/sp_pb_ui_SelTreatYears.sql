USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelTreatYears]    Script Date: 11/2/2023 2:44:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ui_SelTreatYears] 
	@ScenId INT,
	@District TINYINT,
	@County NVARCHAR(56),
	@Route INT,
	@Section NVARCHAR(21),
	@TreatmentType NVARCHAR(100)
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT DISTINCT SelectedYear FROM vw_pb_ui_ScenarioTreatments WITH (NOLOCK)
	WHERE ScenId=@ScenId
	  AND SelectedYear IS NOT NULL
	  AND (@District IS NULL OR District=@District)
	  AND (@County IS NULL OR County=@County)
	  AND (@Route IS NULL OR [Route]=@Route)
	  AND (@Section IS NULL OR Section=@Section)
	  AND (@TreatmentType IS NULL OR TreatmentType=@TreatmentType)
	ORDER BY 1

END
GO


