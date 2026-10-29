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
	p.Cnty AS [CountyId],
	CONVERT(VARCHAR(5),c.Cnty) + '-' + COALESCE(c.County,'') AS County,
	[Route],
	Direction,
	PreferredStartingYear,
	TotalCost AS [TotalCostM],
	Benefit AS [BenefitM],
	[NoTreatments] = (SELECT COUNT(1) FROM tbl_pb_ExtendedImportedTreatments t
						WHERE t.ScenId=p.ScenId
						  AND t.AssignedTimewiseConstrainedProjectId = p.ProjectId),
	SectionChain as [Section],
	CASE HasCommitted
		 WHEN 1 THEN 'Y'
		 ELSE 'N'
	END AS [CommitmentStatus],
	CASE IsSelected WHEN 1 THEN 'Y' ELSE 'N' END AS [Selected],
	SelectedFirstYear,
	CASE WHEN EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedImportedTreatments t
						WHERE t.ScenId=p.ScenId
						  AND t.AssignedTimewiseConstrainedProjectId = p.ProjectId AND t.IsUserCreated = 1)
		 THEN 'Y'
		 ELSE 'N'
	END [UserCreated]
FROM tbl_pb_ExtendedTimewiseConstrainedProjects p
LEFT OUTER JOIN tbl_pb_Counties c
	ON c.Cnty=p.Cnty
GO

DROP VIEW IF EXISTS [dbo].[vw_pb_ui_UnfundedScenarioProjects]
GO
