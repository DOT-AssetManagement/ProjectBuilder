-- Fixes Treatment Pool imports with "Keep All" (and "Keep User Created") silently blanking the
-- "Treatment Type" / Category column for pre-existing treatments that the import is not supposed
-- to touch.
--
-- Root cause: Category/Treatment Type is not a real column on tbl_lib_LibraryTreatments -- it is a
-- dynamic attribute stored in tbl_lib_CargoData/tbl_lib_CargoAttributes, keyed by the treatment
-- row's ImportTimeGeneratedId/ImportTimeGeneratedGuid. ExcelHandler.ImportTreatmentsFromExcelWithCargo
-- unconditionally wiped ALL tbl_lib_CargoAttributes/tbl_lib_CargoData rows for the target
-- LibraryId+AssetType on every import, then only re-populated Cargo rows for the treatments present
-- in the current Excel file. sp_pb_Move{BAMS|PAMS}TreatmentsToLibraryTreatments, however, only
-- deletes/replaces tbl_lib_LibraryTreatments rows that exactly match a row in the current import
-- (by AssetId/Direction/Year/Treatment) when @FromScratch=0 ("Keep All"), or only the non-user rows
-- when @FromScratch=1 AND @KeepUserTreatments=1 ("Keep User Created") -- every other existing row is
-- left untouched on purpose. Because the wholesale Cargo wipe was not scoped to the same set of rows
-- the stored procedure actually replaces, those untouched rows permanently lost their Category value.
--
-- Fix (paired with an ExcelHandler.cs change that stops the unconditional Cargo wipe and makes
-- Cargo attribute numbering stable/reusable across imports instead of rebuilding it from scratch):
-- both stored procedures now delete tbl_lib_CargoData rows for exactly the ImportTimeGeneratedId set
-- they are about to delete from tbl_lib_LibraryTreatments, in every branch (full reset, "keep user
-- created" reset, and the "keep all" duplicate-match branch) -- Cargo data lifecycle now mirrors the
-- treatment row's lifecycle instead of being wiped independently. Base bodies pulled from this
-- database's live OBJECT_DEFINITION. Idempotent (ALTER PROCEDURE).
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_pb_MoveBAMSTreatmentsToLibraryTreatments]
	@ImportSessionId UNIQUEIDENTIFIER,
	@TargetLibraryId UNIQUEIDENTIFIER,
	@FromScratch BIT = 1,
	@KeepUserTreatments BIT = 1
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RowCount INT;
	DECLARE @DeletedIds TABLE (ImportTimeGeneratedId UNIQUEIDENTIFIER PRIMARY KEY);

	PRINT 'sp_pb_MoveBAMSTreatmentsToLibraryTreatments @ImportSessionId=[' +
		CONVERT(VARCHAR(100), @ImportSessionId) + ']' +
		', @TargetLibraryId=[' + CONVERT(VARCHAR(100), @TargetLibraryId) + ']' +
		', @FromScratch=' + CONVERT(VARCHAR(10), @FromScratch) +
		', @KeepUserTreatments=' + CONVERT(VARCHAR(10), @KeepUserTreatments) +
		' - started...';

	IF @FromScratch = 1 BEGIN
		IF @KeepUserTreatments = 0 BEGIN
			INSERT INTO @DeletedIds (ImportTimeGeneratedId)
			SELECT ImportTimeGeneratedId
			FROM tbl_lib_LibraryTreatments
			WHERE LibraryId = @TargetLibraryId
			  AND AssetType = 'B';
		END ELSE BEGIN
			INSERT INTO @DeletedIds (ImportTimeGeneratedId)
			SELECT ImportTimeGeneratedId
			FROM tbl_lib_LibraryTreatments
			WHERE LibraryId = @TargetLibraryId
			  AND AssetType = 'B'
			  AND IsUserTreatment = 0;
		END

		DELETE cd
		FROM tbl_lib_CargoData cd
		INNER JOIN @DeletedIds d ON d.ImportTimeGeneratedId = cd.ImportTimeGeneratedGuid
		WHERE cd.LibraryId = @TargetLibraryId
		  AND cd.AssetType = 'B';

		DELETE FROM tbl_lib_LibraryTreatments
		WHERE LibraryId = @TargetLibraryId
		  AND ImportTimeGeneratedId IN (SELECT ImportTimeGeneratedId FROM @DeletedIds);

		SET @RowCount = @@ROWCOUNT;
		PRINT 'Old treatment records deleted: ' + CONVERT(VARCHAR(10), @RowCount);
	END ELSE BEGIN
		INSERT INTO @DeletedIds (ImportTimeGeneratedId)
		SELECT a.ImportTimeGeneratedId
		FROM tbl_lib_LibraryTreatments a WITH (NOLOCK)
		INNER JOIN tbl_import_BAMS_Treatments b WITH (NOLOCK)
			ON b.ImportSessionId = @ImportSessionId
			AND b.AssetId = a.AssetId
			AND a.Direction = b.Segment % 2
			AND b.[Year] = a.[PreferredYear]
			AND b.Treatment = a.Treatment
			AND b.AssetType = a.AssetType
		WHERE a.LibraryId = @TargetLibraryId
		  AND a.AssetType = 'B';

		DELETE cd
		FROM tbl_lib_CargoData cd
		INNER JOIN @DeletedIds d ON d.ImportTimeGeneratedId = cd.ImportTimeGeneratedGuid
		WHERE cd.LibraryId = @TargetLibraryId
		  AND cd.AssetType = 'B';

		DELETE FROM tbl_lib_LibraryTreatments
		WHERE LibraryId = @TargetLibraryId
		  AND ImportTimeGeneratedId IN (SELECT ImportTimeGeneratedId FROM @DeletedIds);

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
	  ,ImportSessionId
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
	  ,COALESCE(btp.[Route], ibt.[Route]) AS [Route]
	  ,ibt.Segment % 2 AS Direction
	  ,ibt.Segment AS FromSection
	  ,ibt.Segment AS ToSection
	  ,ibt.Offset
	  ,CONVERT(BIT, CASE ibt.Interstate WHEN 'Y' THEN 1 ELSE 0 END) AS Interstate
	  ,Treatment
	  ,COALESCE(Benefit, 0) AS Benefit
	  ,COALESCE(Cost, 0) AS Cost
	  ,COALESCE(Risk, 0) AS Risk
	  ,COALESCE(PriorityOrder, 4) AS PriorityOrder
	  ,IsCommitted = CONVERT(BIT, CASE COALESCE(enumCause.EnumInt, 0) WHEN 4 THEN 1 ELSE 0 END)
	  ,[Year] AS PreferredYear
	  ,COALESCE(MinYear, [Year]) AS MinYear
	  ,COALESCE(MaxYear, [Year]) AS MaxYear
	  ,ibt.BRKEY
	  ,CONVERT(BIGINT, btp.BRIDGE_ID) AS BRIDGE_ID
	  ,COALESCE(RemainingLife, 0) AS RemainingLife
	  ,COALESCE(TreatmentFundingIgnoresSpendingLimit, 0) AS TreatmentIgnoresSpendingLimit
	  ,COALESCE(enumStatus.EnumInt, 0) AS TreatmentStatus
	  ,COALESCE(enumCause.EnumInt, 0) AS TreatmentCause
	  ,IsIsolatedBridge = CONVERT(BIT, (CASE WHEN btp.BRIDGE_ID IS NULL THEN 1 ELSE 0 END))
	  ,@ImportSessionId
	FROM dbo.tbl_import_BAMS_Treatments ibt WITH (NOLOCK)
	LEFT OUTER JOIN tbl_pb_BridgeToPavements btp WITH (NOLOCK)
		ON btp.BRKEY = ibt.BRKEY
	LEFT OUTER JOIN tbl_ara_Enumerations enumCause WITH (NOLOCK)
		ON enumCause.EnumFamily = 'TreatmentCause' AND enumCause.EnumName = ibt.TreatmentCause
	LEFT OUTER JOIN tbl_ara_Enumerations enumStatus WITH (NOLOCK)
		ON enumStatus.EnumFamily = 'TreatmentStatus' AND enumStatus.EnumName = ibt.TreatmentStatus
	WHERE ibt.ImportSessionId = @ImportSessionId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Treatment records inserted: ' + CONVERT(VARCHAR(10), @RowCount);

	UPDATE tbl_pb_ImportSessions SET CompletedStatus = CompletedStatus + 10,
		CompletedAt = CURRENT_TIMESTAMP,
		Notes = ISNULL(Notes, '') + ' ' + CONVERT(VARCHAR(10), @RowCount) +
			' treatment records successfully moved to tbl_lib_LibraryTreatments.',
		TargetLibraryId = @TargetLibraryId
	WHERE Id = @ImportSessionId;

	PRINT 'sp_pb_MoveBAMSTreatmentsToLibraryTreatments @ImportSessionId=[' +
		CONVERT(VARCHAR(100), @ImportSessionId) + ']' +
		', @TargetLibraryId=[' + CONVERT(VARCHAR(100), @TargetLibraryId) + ']' +
		', @FromScratch=' + CONVERT(VARCHAR(10), @FromScratch) +
		', @KeepUserTreatments=' + CONVERT(VARCHAR(10), @KeepUserTreatments) +
		' - ended.';
END
GO

ALTER PROCEDURE [dbo].[sp_pb_MovePAMSTreatmentsToLibraryTreatments]
	@ImportSessionId UNIQUEIDENTIFIER,
	@TargetLibraryId UNIQUEIDENTIFIER,
	@FromScratch BIT = 1,
	@KeepUserTreatments BIT = 1
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RowCount INT;
	DECLARE @DeletedIds TABLE (ImportTimeGeneratedId UNIQUEIDENTIFIER PRIMARY KEY);

	PRINT 'sp_pb_MovePAMSTreatmentsToLibraryTreatments @ImportSessionId=[' +
		CONVERT(VARCHAR(100), @ImportSessionId) + ']' +
		', @TargetLibraryId=[' + CONVERT(VARCHAR(100), @TargetLibraryId) + ']' +
		', @FromScratch=' + CONVERT(VARCHAR(10), @FromScratch) +
		', @KeepUserTreatments=' + CONVERT(VARCHAR(10), @KeepUserTreatments) +
		' - started...';

	IF @FromScratch = 1 BEGIN
		IF @KeepUserTreatments = 0 BEGIN
			INSERT INTO @DeletedIds (ImportTimeGeneratedId)
			SELECT ImportTimeGeneratedId
			FROM tbl_lib_LibraryTreatments
			WHERE LibraryId = @TargetLibraryId
			  AND AssetType = 'P';
		END ELSE BEGIN
			INSERT INTO @DeletedIds (ImportTimeGeneratedId)
			SELECT ImportTimeGeneratedId
			FROM tbl_lib_LibraryTreatments
			WHERE LibraryId = @TargetLibraryId
			  AND AssetType = 'P'
			  AND IsUserTreatment = 0;
		END

		DELETE cd
		FROM tbl_lib_CargoData cd
		INNER JOIN @DeletedIds d ON d.ImportTimeGeneratedId = cd.ImportTimeGeneratedGuid
		WHERE cd.LibraryId = @TargetLibraryId
		  AND cd.AssetType = 'P';

		DELETE FROM tbl_lib_LibraryTreatments
		WHERE LibraryId = @TargetLibraryId
		  AND ImportTimeGeneratedId IN (SELECT ImportTimeGeneratedId FROM @DeletedIds);

		SET @RowCount = @@ROWCOUNT;
		PRINT 'Old treatment records deleted: ' + CONVERT(VARCHAR(10), @RowCount);
	END ELSE BEGIN
		INSERT INTO @DeletedIds (ImportTimeGeneratedId)
		SELECT a.ImportTimeGeneratedId
		FROM tbl_lib_LibraryTreatments a WITH (NOLOCK)
		INNER JOIN tbl_import_PAMS_Treatments b WITH (NOLOCK)
			ON b.ImportSessionId = @ImportSessionId
			AND b.AssetId = a.AssetId
			AND b.Direction = a.Direction
			AND b.[Year] = a.[PreferredYear]
			AND b.Treatment = a.Treatment
			AND b.AssetType = a.AssetType
		WHERE a.LibraryId = @TargetLibraryId
		  AND a.AssetType = 'P';

		DELETE cd
		FROM tbl_lib_CargoData cd
		INNER JOIN @DeletedIds d ON d.ImportTimeGeneratedId = cd.ImportTimeGeneratedGuid
		WHERE cd.LibraryId = @TargetLibraryId
		  AND cd.AssetType = 'P';

		DELETE FROM tbl_lib_LibraryTreatments
		WHERE LibraryId = @TargetLibraryId
		  AND ImportTimeGeneratedId IN (SELECT ImportTimeGeneratedId FROM @DeletedIds);

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
	  ,ImportSessionId
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
	  ,ibt.[Route]
	  ,Direction
	  ,ibt.FromSection
	  ,ibt.ToSection
	  ,CONVERT(BIT, CASE ibt.Interstate WHEN 'Y' THEN 1 ELSE 0 END) AS Interstate
	  ,Treatment
	  ,COALESCE(Benefit, 0) AS Benefit
	  ,COALESCE(Cost, 0) AS Cost
	  ,COALESCE(Risk, 0) AS Risk
	  ,COALESCE(PriorityOrder, 4) AS PriorityOrder
	  ,IsCommitted = CONVERT(BIT, CASE COALESCE(enumCause.EnumInt, 0) WHEN 4 THEN 1 ELSE 0 END)
	  ,[Year] AS PreferredYear
	  ,COALESCE(MinYear, [Year]) AS MinYear
	  ,COALESCE(MaxYear, [Year]) AS MaxYear
	  ,COALESCE(RemainingLife, 0) AS RemainingLife
	  ,COALESCE(TreatmentFundingIgnoresSpendingLimit, 0) AS TreatmentIgnoresSpendingLimit
	  ,COALESCE(enumStatus.EnumInt, 0) AS TreatmentStatus
	  ,COALESCE(enumCause.EnumInt, 0) AS TreatmentCause
	  ,ImportSessionId
	FROM dbo.tbl_import_PAMS_Treatments ibt WITH (NOLOCK)
	LEFT OUTER JOIN tbl_ara_Enumerations enumCause WITH (NOLOCK)
		ON enumCause.EnumFamily = 'TreatmentCause' AND enumCause.EnumName = ibt.TreatmentCause
	LEFT OUTER JOIN tbl_ara_Enumerations enumStatus WITH (NOLOCK)
		ON enumStatus.EnumFamily = 'TreatmentStatus' AND enumStatus.EnumName = ibt.TreatmentStatus
	WHERE ibt.ImportSessionId = @ImportSessionId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Treatment records inserted: ' + CONVERT(VARCHAR(10), @RowCount);

	UPDATE tbl_pb_ImportSessions SET CompletedStatus = CompletedStatus + 10,
		CompletedAt = CURRENT_TIMESTAMP,
		Notes = ISNULL(Notes, '') + ' ' + CONVERT(VARCHAR(10), @RowCount) +
			' treatment records successfully moved to tbl_lib_LibraryTreatments.',
		TargetLibraryId = @TargetLibraryId
	WHERE Id = @ImportSessionId;

	PRINT 'sp_pb_MovePAMSTreatmentsToLibraryTreatments @ImportSessionId=[' +
		CONVERT(VARCHAR(100), @ImportSessionId) + ']' +
		', @TargetLibraryId=[' + CONVERT(VARCHAR(100), @TargetLibraryId) + ']' +
		', @FromScratch=' + CONVERT(VARCHAR(10), @FromScratch) +
		', @KeepUserTreatments=' + CONVERT(VARCHAR(10), @KeepUserTreatments) +
		' - ended.';
END
GO
