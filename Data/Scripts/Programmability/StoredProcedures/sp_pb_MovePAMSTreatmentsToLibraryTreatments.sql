USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_MovePAMSTreatmentsToImportedTreatments]    Script Date: 12/18/2023 3:55:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER PROCEDURE [dbo].[sp_pb_MovePAMSTreatmentsToImportedTreatments] 
	@ImportSessionId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RowCount INT;

	PRINT 'sp_pb_MovePAMSTreatmentsToImportedTreatments @ImportSessionId=[' +
		CONVERT(VARCHAR(100), @ImportSessionId) + '] - started...';

	DELETE FROM tbl_pb_ImportedTreatments 
	WHERE ImportTimeGeneratedId IN
		(
		SELECT t.ImportTimeGeneratedId FROM tbl_pb_ImportedTreatments t WITH (NOLOCK)
		INNER JOIN  (SELECT DISTINCT SimulationId, NetworkId 
					 FROM tbl_import_PAMS_Treatments WITH (NOLOCK)
					 WHERE ImportSessionId = @ImportSessionId) a
			ON a.SimulationId=t.SimulationId AND a.NetworkId=t.NetworkId
		)
	 AND AssetType='P';
	 SET @RowCount = @@ROWCOUNT;
	 PRINT 'Old treatment records deleted: ' + CONVERT(VARCHAR(10), @RowCount);
	   
	 INSERT INTO tbl_pb_ImportedTreatments(
		ImportTimeGeneratedId
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
      ,RemainingLife
      ,TreatmentFundingIgnoresSpendingLimit
      ,TreatmentStatus
      ,TreatmentCause
	  )
    SELECT ImportTimeGeneratedId
      ,SimulationId
      ,NetworkId
      ,AssetId
      ,AssetType
      ,Asset
      ,ibt.District
      ,Cnty
      ,ibt.[Route]
      ,Direction
      ,ibt.FromSection
      ,ibt.ToSection
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
      ,COALESCE(RemainingLife,0) AS RemainingLife
      ,COALESCE(TreatmentFundingIgnoresSpendingLimit,0) AS TreatmentIgnoresSpendingLimit
      ,COALESCE(enumStatus.EnumInt, 0) AS TreatmentStatus
      ,COALESCE(enumCause.EnumInt,0) AS TreatmentCause
  FROM dbo.tbl_import_PAMS_Treatments ibt WITH (NOLOCK)
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
		' treatment records successfully moved to tbl_pb_ImportedTreatments.'
	WHERE Id = @ImportSessionId;

  PRINT 'sp_pb_MovePAMSTreatmentsToImportedTreatments @ImportSessionId=[' +
		CONVERT(VARCHAR(100), @ImportSessionId) + '] - ended.';

END
GO

