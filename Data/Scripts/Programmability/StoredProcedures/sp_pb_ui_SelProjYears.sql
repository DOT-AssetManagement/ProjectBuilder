USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelProjYears]    Script Date: 11/2/2023 2:40:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ui_SelProjYears] 
	@ScenId INT,
	@District TINYINT,
	@County NVARCHAR(56),
	@Route INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT DISTINCT [Selected First Year] FROM vw_pb_ui_ScenarioProjects WITH (NOLOCK)
	WHERE ScenId=@ScenId 
	  AND [Selected First Year] IS NOT NULL
	  AND (@District IS NULL OR District=@District)
	  AND (@County IS NULL OR County=@County)
	  AND (@Route IS NULL OR [Route] = @Route)
	ORDER BY 1;

END
GO


