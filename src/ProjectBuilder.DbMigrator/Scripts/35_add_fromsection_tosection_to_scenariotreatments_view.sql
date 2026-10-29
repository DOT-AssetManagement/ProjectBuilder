-- Adds raw FromSection/ToSection int columns to dbo.vw_pb_ui_ScenarioTreatments, the view
-- the ProjectTreatment entity reads (bottom "Project Treatments" bar on the Map page).
--
-- The view already collapses FromSection/ToSection into a single formatted "Section" string:
--   CASE WHEN ToSection=FromSection THEN CONVERT(VARCHAR(10),FromSection)
--        ELSE CONVERT(VARCHAR(10),FromSection) + '-' + CONVERT(VARCHAR(10),ToSection) END
-- so whenever a treatment's FromSection equals its ToSection (the common case for a
-- single-length pavement segment), the grid showed just "10" instead of a from/to range —
-- inconsistent with the map's side "Treatments" panel, which always renders both endpoints
-- (e.g. "30 - 30") because it reads FromSection/ToSection directly from
-- sp_pb_SelectTreatmentsForGIS instead of a pre-formatted string.
--
-- Exposing the raw ints lets the app format the range itself (always "X - Y", matching the
-- side panel) instead of relying on the view's pre-collapsed "Section" string. Appended at
-- the very end of the SELECT list, after the existing "Section" column, which is left in
-- place unchanged for any other consumer.
--
-- Safe to add: every other reader of this view (the legacy sp_pb_ui_SelTreat* lookup procs)
-- selects a single named column, never SELECT *, and the ProjectTreatment EF Core entity
-- (entity.ToView(...)) generates an explicit column list, so unmapped trailing columns are
-- simply ignored until the entity is updated to map them. Idempotent (CREATE OR ALTER).
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[vw_pb_ui_ScenarioTreatments]
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
	SelectedYear,
	t.FromSection,
	t.ToSection
FROM tbl_pb_ExtendedImportedTreatments t
LEFT OUTER JOIN tbl_pb_Counties c
	ON c.Cnty=t.Cnty
LEFT OUTER JOIN tbl_pb_UserTreatments u
	ON u.UserTreatmentTypeNo=t.UserTreatmentTypeNo
WHERE IsSelected=1
GO
