USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_MoveBAMSTreatmentsToLibraryTreatments]    Script Date: 12/18/2023 3:55:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_pb_MoveBAMSTreatmentsToLibraryTreatments] 
	@ImportSessionId UNIQUEIDENTIFIER,
	@TargetLibraryId UNIQUEIDENTIFIER,
	@FromScratch BIT = 1,
	@KeepUserTreatments BIT =1
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RowCount INT;

	PRINT 'sp_pb_MoveBAMSTreatmentsToLibraryTreatments @ImportSessionId=[' +
		CONVERT(VARCHAR(100), @ImportSessionId) + ']' + 
		', @TargetLibraryId=[' + CONVERT(VARCHAR(100), @TargetLibraryId) + ']' +
		', @FromScratch=' + CONVERT(VARCHAR(10),@FromScratch) +
		', @KeepUserTreatments=' + CONVERT(VARCHAR(10),@KeepUserTreatments) +
		' - started...';
	
	IF @FromScratch = 1 BEGIN
		IF @KeepUserTreatments = 0 BEGIN
			DELETE FROM tbl_lib_LibraryTreatments 
			WHERE ImportTimeGeneratedId IN
				(
				SELECT ImportTimeGeneratedId FROM tbl_lib_LibraryTreatments t WITH (NOLOCK)
				INNER JOIN  (SELECT DISTINCT SimulationId, NetworkId 
							 FROM tbl_import_BAMS_Treatments WITH (NOLOCK)
							 WHERE ImportSessionId = @ImportSessionId) a
					ON a.SimulationId=t.SimulationId AND a.NetworkId=t.NetworkId
				WHERE t.LibraryId = @TargetLibraryId
				)
			 AND AssetType='B' AND LibraryId=@TargetLibraryId;
			 SET @RowCount = @@ROWCOUNT;
		 END ELSE BEGIN
		 	DELETE FROM tbl_lib_LibraryTreatments 
			WHERE ImportTimeGeneratedId IN
				(
				SELECT ImportTimeGeneratedId FROM tbl_lib_LibraryTreatments t WITH (NOLOCK)
				INNER JOIN  (SELECT DISTINCT SimulationId, NetworkId 
							 FROM tbl_import_BAMS_Treatments WITH (NOLOCK)
							 WHERE ImportSessionId = @ImportSessionId) a
					ON a.SimulationId=t.SimulationId AND a.NetworkId=t.NetworkId
				WHERE t.LibraryId = @TargetLibraryId AND IsUserTreatment=0
				)
			 AND AssetType='B' AND LibraryId=@TargetLibraryId;
			 SET @RowCount = @@ROWCOUNT;
		 END
		 PRINT 'Old treatment records deleted: ' + CONVERT(VARCHAR(10), @RowCount);
	END ELSE BEGIN
		DELETE FROM tbl_lib_LibraryTreatments
		WHERE LibraryId = @TargetLibraryId
		  AND ImportTimeGeneratedId IN 
		  (
			SELECT a.ImportTimeGeneratedId
			FROM tbl_lib_LibraryTreatments a WITH (NOLOCK)
			INNER JOIN tbl_import_BAMS_Treatments b WITH (NOLOCK)
				ON b.ImportSessionId= @ImportSessionId
				AND b.AssetId=a.AssetId 
				AND a.Direction = b.Segment % 2 
				AND b.[Year]=a.[PreferredYear]
				AND b.Treatment=a.Treatment
				AND b.AssetType=a.AssetType
			WHERE a.LibraryId = @TargetLibraryId
			  AND a.AssetType='B'
		  )
		  SET @RowCount = @@ROWCOUNT;
		  PRINT 'Old duplicated treatment records deleted: ' + CONVERT(VARCHAR(10), @RowCount);
	END
	   
	 INSERT INTO tbl_lib_LibraryTreatments(
	   LibraryId
	  ,ImportTimeGeneratedId
	  ,ImportedTreatmentId
      ,SimulationId
      ,NetworkId
      ,AssetId
      ,AssetType
      ,Asset
      ,ibt.District
      ,Cnty
      ,[Route]
      ,Direction
      ,FromSection
      ,ToSection
	  ,Offset
      ,Interstate
      ,Treatment
      ,Benefit
      ,Cost
      ,Risk
      ,PriorityOrder
	  ,IsCommitted
      ,PreferredYear
      ,MinYear
      ,MaxYear
      ,ibt.BRKEY
	  ,BRIDGE_ID
      ,RemainingLife
      ,TreatmentFundingIgnoresSpendingLimit
      ,TreatmentStatus
      ,TreatmentCause
	  ,IsIsolatedBridge
	  )
    SELECT @TargetLibraryId
	  ,ImportTimeGeneratedId
	  ,0 AS ImportedTreatmentId
	  ,SimulationId
      ,NetworkId
      ,AssetId
      ,AssetType
      ,Asset
      ,ibt.District
      ,Cnty
      ,COALESCE(btp.[Route],ibt.[Route]) AS [Route] -- bridge-to-pavement data is more authoritative
      ,ibt.Segment % 2 AS Direction   -- We cannot trust Direction designation that comes from Excel for BAMS
      ,ibt.Segment AS FromSection
      ,ibt.Segment AS ToSection
	  ,ibt.Offset
      ,CONVERT(BIT,CASE ibt.Interstate WHEN 'Y' THEN 1 ELSE 0 END) AS Interstate
      ,Treatment
      ,COALESCE(Benefit,0) AS Benefit
      ,COALESCE(Cost,0) AS Cost
      ,COALESCE(Risk,0) AS Risk
      ,COALESCE(PriorityOrder,4) AS PriorityOrder
	  ,IsCommitted = CONVERT(BIT,CASE COALESCE(enumCause.EnumInt,0) WHEN 4 THEN 1 ELSE 0 END)
      ,[Year] AS PreferredYear
      ,COALESCE(MinYear,[Year]) AS MinYear
      ,COALESCE(MaxYear,[Year]) AS MaxYear
      ,ibt.BRKEY
	  ,CONVERT(BIGINT,btp.BRIDGE_ID) AS BRIDGE_ID
      ,COALESCE(RemainingLife,0) AS RemainingLife
      ,COALESCE(TreatmentFundingIgnoresSpendingLimit,0) AS TreatmentIgnoresSpendingLimit
      ,COALESCE(enumStatus.EnumInt, 0) AS TreatmentStatus
      ,COALESCE(enumCause.EnumInt,0) AS TreatmentCause
	  ,IsIsolatedBridge = CONVERT(BIT,(CASE WHEN btp.BRIDGE_ID IS NULL THEN 1 ELSE 0 END))
  FROM dbo.tbl_import_BAMS_Treatments ibt WITH (NOLOCK)
  LEFT OUTER JOIN tbl_pb_BridgeToPavements btp WITH (NOLOCK)
	ON btp.BRKEY=ibt.BRKEY
  LEFT OUTER JOIN tbl_ara_Enumerations enumCause WITH (NOLOCK)
	ON enumCause.EnumFamily='TreatmentCause' AND enumCause.EnumName=ibt.TreatmentCause
  LEFT OUTER JOIN tbl_ara_Enumerations enumStatus WITH (NOLOCK)
	ON enumStatus.EnumFamily='TreatmentStatus' AND enumStatus.EnumName=ibt.TreatmentStatus
  WHERE ibt.ImportSessionId = @ImportSessionId;

  SET @RowCount = @@ROWCOUNT;
  PRINT 'Treatment records inserted: ' + CONVERT(VARCHAR(10), @RowCount);

  UPDATE tbl_pb_ImportSessions SET CompletedStatus=CompletedStatus + 10,
		CompletedAt = CURRENT_TIMESTAMP,
		Notes = ISNULL(Notes,'') + ' ' + CONVERT(VARCHAR(10), @RowCount) +
			' treatment records successfully moved to tbl_lib_LibraryTreatments.',
		TargetLibraryId=@TargetLibraryId
	WHERE Id = @ImportSessionId;

  PRINT 'sp_pb_MoveBAMSTreatmentsToLibraryTreatments @ImportSessionId=[' +
		CONVERT(VARCHAR(100), @ImportSessionId) + ']' + 
		', @TargetLibraryId=[' + CONVERT(VARCHAR(100), @TargetLibraryId) + ']' +
		', @FromScratch=' + CONVERT(VARCHAR(10),@FromScratch) +
		', @KeepUserTreatments=' + CONVERT(VARCHAR(10),@KeepUserTreatments) +
		' - ended.';

END
GO

