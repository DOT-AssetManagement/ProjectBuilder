USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_ui_SelProjCounties]    Script Date: 11/2/2023 2:38:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_pb_ui_SelProjCounties] 
	@ScenId INT,
	@District TINYINT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT DISTINCT County FROM vw_pb_ui_ScenarioProjects WITH (NOLOCK)
	WHERE ScenId=@ScenId AND (@District IS NULL OR District=@District)
	ORDER BY 1;

END
GO


