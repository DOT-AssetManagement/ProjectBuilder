USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_SelScenProjDistricts]    Script Date: 11/2/2023 2:37:47 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_pb_SelScenProjDistricts] 
	@ScenId INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT DISTINCT District FROM vw_pb_ui_ScenarioProjects WITH (NOLOCK)
	WHERE ScenId=@ScenId

END
GO


