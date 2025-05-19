USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelTreatSections]    Script Date: 11/2/2023 2:43:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





ALTER PROCEDURE [dbo].[sp_pb_ui_SelTreatSections] 
	@ScenId INT,
	@District TINYINT,
	@County NVARCHAR(56),
	@Route INT
AS
BEGIN
	
	SET NOCOUNT ON;

	IF @County IS NULL OR @Route IS NULL BEGIN
		SELECT Section FROM vw_pb_ui_ScenarioTreatments WITH (NOLOCK)
		WHERE ScenId < 0		-- Return no rows of county o route are not specified
	END ELSE BEGIN
		SELECT DISTINCT Section FROM vw_pb_ui_ScenarioTreatments WITH (NOLOCK)
		WHERE ScenId=@ScenId
		  AND (@District IS NULL OR District=@District)
		  AND County=@County	-- Section numeration is specific to county and route 
		  AND [Route]=@Route
		ORDER BY 1
	END

END
GO


