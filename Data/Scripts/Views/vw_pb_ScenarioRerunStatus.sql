USE [SPP_PBv1]
GO

/****** Object:  View [dbo].[vw_pb_ScenarioRerunStatus]    Script Date: 12/8/2023 1:57:17 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER VIEW [dbo].[vw_pb_ScenarioRerunStatus] AS
SELECT ScenId, ScenarioName, LibraryId, u.[Name] AS LibraryName, LastScenarioRun, LastLibraryUpdated,
	CONVERT(BIT,CASE WHEN LastScenarioRun < LastLibraryUpdated THEN 1 ELSE 0 END) AS ScenarioNeedsRerun
FROM
(
	SELECT ScenId, ScenarioName, s.LibraryId, COALESCE(s.LastFinished, s.LastRunAt, s.LastStarted, s.CreatedAt) AS LastScenarioRun, lib.LastUpdatedAt AS LastLibraryUpdated
	FROM tbl_pb_Scenarios s
	INNER JOIN (
		SELECT LibraryId, MAX(PopulatedAt) AS LastUpdatedAt 
		FROM tbl_lib_LibraryTreatments t
		GROUP BY LibraryId
		) lib
	ON lib.LibraryId=s.LibraryId
) q
LEFT OUTER JOIN tbl_lib_UserLibraries u
ON u.ID = q.LibraryId

GO

