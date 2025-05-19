USE [SPP_PBv1]
GO

/****** Object:  View [dbo].[vw_pb_ui_ScenarioProjects]    Script Date: 12/9/2023 10:12:49 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER VIEW [dbo].[vw_pb_ui_ScenarioProjects] AS
SELECT ScenId,
	ProjectId,
	UserId,
	UserNotes,
	CASE ProjectSchemaId / 1000000
		WHEN 1 THEN 'Bridge'
		WHEN 2 THEN 'Pavement'
		ELSE 'Combined'
	END AS AssetType,
	p.District,
	CONVERT(VARCHAR(5),c.Cnty) + '-' + COALESCE(c.County,'') AS County,
	[Route],
	Direction,
	PreferredStartingYear AS [Preferred Starting Year],
	TotalCost / 1000000 AS [Total Cost ($M)],
	Benefit / 1000000 AS [Benefit ($M)],
	[No. Treatments] = (SELECT COUNT(1) FROM tbl_pb_ExtendedImportedTreatments t 
						WHERE t.ScenId=p.ScenId 
						  AND t.AssignedTimewiseConstrainedProjectId = p.ProjectId),
	CASE HasCommitted
		 WHEN 1 THEN 'Y'
		 ELSE 'N'
	END AS [Commitment Status],
	CASE IsSelected WHEN 1 THEN 'Y' ELSE 'N' END AS [Selected],
	SelectedFirstYear AS [Selected First Year],
	CASE WHEN EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedImportedTreatments t 
						WHERE t.ScenId=p.ScenId 
						  AND t.AssignedTimewiseConstrainedProjectId = p.ProjectId AND t.IsUserCreated = 1)
		 THEN 'Y'
		 ELSE 'N'
	END [User Created]
FROM tbl_pb_ExtendedTimewiseConstrainedProjects p
LEFT OUTER JOIN tbl_pb_Counties c
	ON c.Cnty=p.Cnty
WHERE IsSelected=1
GO

