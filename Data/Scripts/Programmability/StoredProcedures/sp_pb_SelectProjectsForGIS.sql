USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_SelectProjectsForGIS]    Script Date: 12/18/2023 5:48:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_pb_SelectProjectsForGIS]
	@ScenId INT,
	@District TINYINT = NULL,
	@Cnty TINYINT = NULL,
	@Route INT = NULL,
	@Section VARCHAR(20) = NULL,
	@AppliedTreatment VARCHAR(100) = NULL,
	@Year INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

    SELECT ProjectId, ProjectSchemaId AS SchemaId, ProjectSchemaId / 1000000 AS ProjectType,
	SelectedFirstYear AS [Year],
	UserId, UserNotes,
	NumBridges = (SELECT COUNT(1) FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
					WHERE ScenId=p.ScenId AND AssignedTimewiseConstrainedProjectId = p.ProjectId 
						AND IsSelected=1 AND AssetType='B'),
	NumPaves = (SELECT COUNT(1) FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
					WHERE ScenId=p.ScenId AND AssignedTimewiseConstrainedProjectId = p.ProjectId 
						AND IsSelected=1 AND AssetType='P'),
	ROUND(DirectCost,2) AS TotalCost
	FROM tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
	WHERE ScenId=@ScenId AND IsSelected=1
	  AND (@District IS NULL OR District=@District)
	  AND (@Cnty IS NULL OR Cnty=@Cnty)
	  AND (@Route IS NULL OR [Route] = @Route)
	  AND (@Year IS NULL OR SelectedFirstYear=@Year)
	  AND (@Section IS NULL OR EXISTS(SELECT TOP 1 1 FROM tbl_pb_ExtendedImportedTreatments xt WITH (NOLOCK)
								WHERE xt.ScenId=@ScenId
								  AND xt.AssignedTimewiseConstrainedProjectId = p.ProjectId
								  AND (CONVERT(VARCHAR(10),FromSection) = @Section
										OR CONVERT(VARCHAR(10),FromSection)+'-'+CONVERT(VARCHAR(10),ToSection) = @Section)
									  )			
		  )
	   AND (@AppliedTreatment IS NULL OR EXISTS(SELECT TOP 1 1 FROM tbl_pb_ExtendedImportedTreatments xt WITH (NOLOCK)
											WHERE xt.ScenId=@ScenId
												AND xt.AssignedTimewiseConstrainedProjectId=p.ProjectId
												AND xt.Treatment = @AppliedTreatment)
			)
	ORDER BY ProjectId
END
GO

