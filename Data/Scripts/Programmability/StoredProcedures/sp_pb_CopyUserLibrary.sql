USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_CopyUserLibrary]    Script Date: 10/9/2023 5:31:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER PROCEDURE [dbo].[sp_pb_CopyUserLibrary]		
	@SourceLibraryId VARCHAR(100),
	@TargetLibraryId VARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RowCount INT;

    PRINT 'sp_pb_CopyUserLibrary (' + @SourceLibraryId + ',' + @TargetLibraryId + ') - started...';

	DELETE FROM tbl_lib_LibraryTreatments WHERE LibraryId=CONVERT(UNIQUEIDENTIFIER,@TargetLibraryId);
	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records deleted from the target library: ' + CONVERT(VARCHAR(10),@RowCount);

	INSERT INTO tbl_lib_LibraryTreatments
           (LibraryId
           ,ImportedTreatmentID
		   ,ImportTimeGeneratedId
           ,AssetType
           ,AssetId
           ,Asset
           ,District
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
           ,IsCommitted
           ,PriorityOrder
           ,PreferredYear
           ,MinYear
           ,MaxYear
           ,BRKEY
           ,BRIDGE_ID
           ,IsIsolatedBridge
           ,PopulatedBy
           ,PopulatedAt
           ,SimulationId
           ,NetworkId
           ,TreatmentFundingIgnoresSpendingLimit
           ,TreatmentStatus
           ,TreatmentCause
           ,RemainingLife)
	SELECT CONVERT(UNIQUEIDENTIFIER, @TargetLibraryId) AS LibraryId
		   ,ImportedTreatmentID
		   ,ImportTimeGeneratedId
           ,AssetType
           ,AssetId
           ,Asset
           ,District
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
           ,IsCommitted
           ,PriorityOrder
           ,PreferredYear
           ,MinYear
           ,MaxYear
           ,BRKEY
           ,BRIDGE_ID
           ,IsIsolatedBridge
           ,PopulatedBy
           ,PopulatedAt
           ,SimulationId
           ,NetworkId
           ,TreatmentFundingIgnoresSpendingLimit
           ,TreatmentStatus
           ,TreatmentCause
           ,RemainingLife
		FROM tbl_lib_LibraryTreatments WITH (NOLOCK)
		WHERE LibraryId = CONVERT(UNIQUEIDENTIFIER, @SourceLibraryId);
   
		SET @RowCount = @@ROWCOUNT;
		PRINT 'Records transferred to the target library: ' + CONVERT(VARCHAR(10),@RowCount);

		UPDATE tbl_lib_UserLibraries SET SourceLibraryId=CONVERT(UNIQUEIDENTIFIER, @SourceLibraryId)
			, UpdatedBy=USER, UpdatedAt=CURRENT_TIMESTAMP
		WHERE Id=CONVERT(UNIQUEIDENTIFIER,@TargetLibraryId);

		SET @RowCount = @@ROWCOUNT;
		PRINT 'Records updated in tbl_lib_UserLibraries: ' + CONVERT(VARCHAR(10),@RowCount);

		PRINT 'sp_pb_CopyUserLibrary (' + @SourceLibraryId + ',' + @TargetLibraryId + ') - ended.';
END
GO


