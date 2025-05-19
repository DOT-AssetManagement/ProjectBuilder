USE [SPP_PBv1]
GO

/****** Object:  View [dbo].[vw_pb_ui_ScenarioTreatments]    Script Date: 11/28/2023 12:52:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE VIEW [dbo].[vw_pb_ui_ScenarioTreatments]
AS
SELECT ScenId, AssetType,
    t.ExtendedTreatmentId,
	t.ImportTimeGeneratedId,
	AssignedTimewiseConstrainedProjectId as ProjectId,
	t.District, 
	CONVERT(VARCHAR(5),c.Cnty) + '-' + COALESCE(c.County,'') AS County,
	t.Cnty as CountyId,
	[Route],
	Direction,
	CASE WHEN ToSection=FromSection
		 THEN CONVERT(VARCHAR(10),FromSection)
		 ELSE CONVERT(VARCHAR(10),FromSection) + '-' + CONVERT(VARCHAR(10),ToSection)
	END AS Section,
	BRKEY, BRIDGE_ID,
	Interstate,
	CASE IsUserTreatment WHEN 1 THEN u.UserTreatmentName ELSE Treatment END AS TreatmentType,
	(Cost+IndirectCostDesign+IndirectCostROW+IndirectCostUtilities+IndirectCostOther) AS TotalCost,
    Cost,
	IndirectCostDesign,
	IndirectCostROW,
	IndirectCostUtilities,
	IndirectCostOther,
	Benefit,
	Risk,
	IsCommitted,
	PriorityOrder,
	IsUserCreated,
	PreferredYear,
	t.UserTreatmentTypeNo,
	MinYear, MaxYear,
	SelectedYear
FROM tbl_pb_ExtendedImportedTreatments t
LEFT OUTER JOIN tbl_pb_Counties c
	ON c.Cnty=t.Cnty
LEFT OUTER JOIN tbl_pb_UserTreatments u
	ON u.UserTreatmentTypeNo=t.UserTreatmentTypeNo
WHERE IsSelected=1
GO


