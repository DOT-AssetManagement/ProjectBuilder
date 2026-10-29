-- User-created library treatments (IsUserTreatment = 1) have NULL ImportSessionId, but
-- sp_pb_PopulateExtendedTreatmentsV2's #Treatments temp table requires it NOT NULL.
-- The proc previously resolved @ImportSessionId only AFTER the INSERT, so any filtered
-- user treatment caused "Cannot insert the value NULL into column 'ImportSessionId'".
-- Fix: resolve the library's ImportSessionId first, then COALESCE it onto each row.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_pb_PopulateExtendedTreatmentsV2]
	@ScenId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @LibraryId UNIQUEIDENTIFIER;
	DECLARE @RowCount BIGINT;

	DECLARE @YearFirst INT;
	DECLARE @YearLast INT;

	PRINT 'sp_pb_PopulateExtendedTreatmentsV2 @ScenId=' + CONVERT(VARCHAR(10),@ScenId) + ' - started...';

	DELETE FROM tbl_pb_ExtendedImportedTreatments WHERE ScenId=@ScenId;
	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records deleted from table tbl_pb_ExtendedImportedTreatments: ' + CONVERT(VARCHAR(10), @RowCount);

	-- This table MUST have the exact same structure as tbl_pb_ImportedTreatments,
	-- and its structure must be updated whenever any changes occcur to tbl_pb_ImportedTreatments
	CREATE TABLE #Treatments (
		ImportSessionId uniqueidentifier NOT NULL,
		TreatmentId INT NULL,
		ImportTimeGeneratedId uniqueidentifier NOT NULL PRIMARY KEY,
		SimulationId uniqueidentifier NOT NULL,
		NetworkId uniqueidentifier NOT NULL,
		AssetId uniqueidentifier NOT NULL,
		AssetType varchar(1) NOT NULL,
		Asset nvarchar(50) NULL,
		SuperAssetId uniqueidentifier NULL,
		District tinyint NULL,
		Cnty tinyint NULL,
		[Route] int NULL,
		Direction tinyint NULL,
		FromSegment int NULL,
		ToSegment int NULL,
		Offset int NULL,
		Interstate bit NULL,
		Treatment nvarchar(max) NULL,
		Benefit float NULL,
		Cost float NULL,
		Risk float NULL,
		PriorityOrder tinyint NULL,
		PreferredYear int NULL,
		MinYear int NULL,
		MaxYear int NULL,
		BRKEY nvarchar(15) NULL,
		BRIDGE_ID bigint NULL,
		RemainingLife float NULL,
		TreatmentFundingIgnoresSpendingLimit bit NULL,
		TreatmentStatus tinyint NULL,
		TreatmentCause tinyint NULL,
		IsCommitted bit NULL,
		IsIsolatedBridge bit NULL,
		IsUserTreatment bit DEFAULT(0) NULL,
		UserTreatmentTypeNo int DEFAULT(0) NULL,
		IndirectCostDesign float NULL,
		IndirectCostROW float NULL,
		IndirectCostUtilities float,
		IndirectCostOther float,
		CRS varchar(100) NULL
	)

	SELECT TOP 1 @LibraryId = LibraryId FROM tbl_pb_Scenarios WITH (NOLOCK) WHERE ScenId=@ScenId;

	PRINT '@LibraryId is ' + CONVERT(VARCHAR(100),@LibraryId) + '.  Treatment data will be pulled from tbl_lib_LibraryTreatments';

	SELECT @RowCount = COUNT(1)
	FROM tbl_pb_ScenarioFilteredTreatments WITH (NOLOCK)
	WHERE ScenId = @ScenId;

	DECLARE @NumFilters BIGINT = @RowCount;
	PRINT 'Number of filtering records for scenario '
		+ CONVERT(VARCHAR(10), @ScenId) + ': ' + CONVERT(VARCHAR(10),@NumFilters);

	DECLARE @ImportSessionId UNIQUEIDENTIFIER;

	SELECT TOP 1 @ImportSessionId = ImportSessionId
	FROM tbl_lib_LibraryTreatments WITH (NOLOCK)
	WHERE LibraryId = @LibraryId AND AssetType='P' AND ImportSessionId IS NOT NULL;

	IF @ImportSessionId IS NULL BEGIN
		PRINT 'There were no P<avement> records in the library';
		PRINT 'Falling back onto just any existing record for ImportSessionId'
		SELECT TOP 1 @ImportSessionId = ImportSessionId
		FROM tbl_lib_LibraryTreatments WITH (NOLOCK)
		WHERE LibraryId = @LibraryId AND ImportSessionId IS NOT NULL
	END

	PRINT 'ImportSessionId=' + CONVERT(VARCHAR(100), @ImportSessionId);

	IF @ImportSessionId IS NULL
	BEGIN
		RAISERROR('Could not resolve ImportSessionId for the scenario treatment library.', 16, 0);
		RETURN;
	END

	INSERT INTO #Treatments (
		 ImportSessionId
        ,TreatmentId
		,ImportTimeGeneratedId
        ,SimulationId
        ,NetworkId
        ,AssetId
        ,AssetType
        ,Asset
        ,District
        ,Cnty
        ,[Route]
        ,Direction
        ,FromSegment
        ,ToSegment
        ,Offset
        ,Interstate
        ,Treatment
        ,Benefit
        ,Cost
        ,Risk
        ,PriorityOrder
        ,PreferredYear
        ,MinYear
        ,MaxYear
        ,BRKEY
        ,BRIDGE_ID
        ,RemainingLife
        ,TreatmentFundingIgnoresSpendingLimit
        ,TreatmentStatus
        ,TreatmentCause
        ,IsCommitted
        ,IsIsolatedBridge
		,IsUserTreatment
		,UserTreatmentTypeNo
		,IndirectCostDesign
		,IndirectCostROW
		,IndirectCostUtilities
		,IndirectCostOther
		,CRS)
	SELECT  COALESCE(t.ImportSessionId, @ImportSessionId)
		,ImportedTreatmentId
		,ImportTimeGeneratedId
        ,SimulationId
        ,NetworkId
        ,AssetId
        ,AssetType
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
        ,PriorityOrder
        ,PreferredYear
        ,MinYear
        ,MaxYear
        ,BRKEY
        ,BRIDGE_ID
        ,RemainingLife
        ,TreatmentFundingIgnoresSpendingLimit
        ,TreatmentStatus
        ,TreatmentCause
        ,IsCommitted
        ,IsIsolatedBridge
		,IsUserTreatment
		,UserTreatmentTypeNo
		,IndirectCostDesign
		,IndirectCostROW
		,IndirectCostUtilities
		,IndirectCostOther
		,CRS =	CASE AssetType
					WHEN 'B' THEN NULL
					ELSE dbo.fn_pb_GetLibCargoValue('P', ImportTimeGeneratedId, 'CRS')
				END
	FROM tbl_lib_LibraryTreatments t WITH (NOLOCK)
	WHERE LibraryId=@LibraryId
	AND (@NumFilters = 0
			 OR
			 ImportTimeGeneratedId IN (SELECT ImportTimeGeneratedId -- DIG 8/19/2025 Added filtering
									FROM tbl_pb_ScenarioFilteredTreatments WITH (NOLOCK)
									WHERE ScenId = @ScenId)
		)

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Filtered records pulled from tbl_lib_LibraryTreatments into #Treatments: ' + CONVERT(VARCHAR(10),@RowCount);

	CREATE INDEX IX_#Treatments_AssetType ON #Treatments (AssetType);
	CREATE INDEX IX_#Treatments_District ON #Treatments (District);
	CREATE INDEX IX_#Treatments_Cnty ON #Treatments (Cnty);
	CREATE INDEX IX_#Treatments_Route ON #Treatments ([Route]);
	CREATE INDEX IX_#Treatments_FromSegment ON #Treatments (FromSegment);
	CREATE INDEX IX_#Treatments_ToSegment ON #Treatments (ToSegment);
	CREATE INDEX IX_#Treatments_Direction ON #Treatments (Direction);
	CREATE INDEX IX_#Treatments_MinYear ON #Treatments(MinYear);
	CREATE INDEX IX_#Treatments_MaxYear ON #Treatments(MaxYear);
	CREATE INDEX IX_#Treatments_SuperAssetId ON #Treatments(SuperAssetId);

	CREATE TABLE #SuperMAS (
		AssetId UNIQUEIDENTIFIER,
		Asset NVARCHAR(50),
		District TINYINT,
		Cnty TINYINT,
		[Route] INT,
		Direction TINYINT,
		FromSegment INT,
		ToSegment INT
		);

	-- Super Assets
	IF @ImportSessionId IS NOT NULL AND EXISTS(SELECT TOP 1 1 FROM tbl_pams_MaintainableSuperAssetsSegmentation WITH (NOLOCK)
												WHERE ImportSessionId=@ImportSessionId)
	BEGIN
		PRINT 'Associated MaintainableSuperAssetsSegmentation set was found';

		INSERT INTO #SuperMAS (AssetId, District, Cnty, [Route], Direction, FromSegment, ToSegment, Asset)
		SELECT AssetId, District, Cnty, [Route], Direction, FromSegment, ToSegment, Asset
		FROM tbl_pams_MaintainableSuperAssetsSegmentation WITH (NOLOCK)
		WHERE ImportSessionId = @ImportSessionId;

		SET @RowCount = @@ROWCOUNT;
		PRINT 'Records inserted into #SuperMAS: ' + CONVERT(VARCHAR(10),@RowCount);
	END ELSE BEGIN

		PRINT 'Associated MaintainableSuperAssetsSegmentation set was NOT found';

		INSERT INTO #SuperMAS (AssetId, District, Cnty, [Route], Direction, FromSegment, ToSegment, Asset)
		SELECT AssetId, District, Cnty, [Route], Direction, FromSegment, ToSegment, Asset
		FROM tbl_pams_MaintainableSuperAssetsSegmentation WITH (NOLOCK)
		WHERE IsMaster=1;

		SET @RowCount = @@ROWCOUNT;
		PRINT 'MASTER records inserted into #SuperMAS: ' + CONVERT(VARCHAR(10),@RowCount);

	END

	DECLARE @SingleExtentSuperAssetsOk BIT = 1;  -- Hard coded
	PRINT '@SingleExtentSuperAssetsOk=' + CONVERT(VARCHAR(1), @SingleExtentSuperAssetsOk);

	UPDATE #Treatments
	SET SuperAssetId = (SELECT TOP 1 AssetId FROM #SuperMAS WHERE Asset=t.CRS)
	FROM #Treatments t
	WHERE SuperAssetId IS NULL
	  AND (@SingleExtentSuperAssetsOk = 1
	       OR t.Asset IS NULL
		   OR t.Asset <> t.CRS
		  );

	DECLARE @N INT;
	SELECT @N = COUNT(1) FROM #Treatments WHERE SuperAssetId IS NOT NULL;
	PRINT 'Number of records where SuperAssetId was set using UPDATE on Asset=CRS: ' + CONVERT(VARCHAR(10),@N);

	-- Associate treatments with super assets where they were not supplied in the CRS column
	-- if the incoming Excel worksheet

	UPDATE #Treatments
	SET SuperAssetId = (SELECT TOP 1 AssetId FROM #SuperMAS s WITH (NOLOCK)
						WHERE s.Cnty=t.Cnty
						  AND s.[Route]=t.[Route]
						  AND s.Direction=t.Direction
						  AND s.FromSegment <= t.FromSegment
						  AND s.ToSegment >= t.ToSegment
						 ORDER BY s.FromSegment)
	FROM #Treatments t
	WHERE SuperAssetId IS NULL
	 AND (@SingleExtentSuperAssetsOk = 1
	       OR t.Asset IS NULL
		   OR t.Asset <> t.CRS
		  );

	DECLARE @M INT;
	SELECT @M = COUNT(1) FROM #Treatments WHERE SuperAssetId IS NOT NULL;
	PRINT 'Number of records where SuperAssetId was set using UPDATE on extents: ' + CONVERT(VARCHAR(10),@M-@N);
	PRINT 'Total number of records where SuperAssetId was set: ' + CONVERT(VARCHAR(10),@M);


	--SELECT * FROM #SuperMAS ORDER BY Cnty, Route, Direction, FromSegment
	--SELECT * FROM #Treatments ORDER BY Cnty, Route, Direction, FromSegment;

	DROP TABLE #SuperMAS;
	--RETURN;

	SELECT @YearFirst = COALESCE(CONVERT(INT,ParmValue), 0) FROM vw_pb_ScenarioParameters WITH (NOLOCK)
	WHERE ScenId=@ScenId AND ParmId='YFST';

	SELECT @YearLast = COALESCE(CONVERT(INT,ParmValue), 0) FROM vw_pb_ScenarioParameters WITH (NOLOCK)
	WHERE ScenId=@ScenId AND ParmId='YLST';

	PRINT '@YearFirst=' + CONVERT(VARCHAR(10), @YearFirst) + ', @YearLast=' + CONVERT(VARCHAR(10), @YearLast);

	INSERT INTO tbl_pb_ExtendedImportedTreatments (ScenId, ImportedTreatmentId, AssetType,
		District, Cnty, [Route], Direction,
		FromSection, ToSection,
		Interstate,
		Treatment, Benefit, Cost, Risk, IsCommitted, PriorityOrder, PreferredYear, MinYear, MaxYear,
		BRKEY, BRIDGE_ID,
		IndirectCostDesign,
		IndirectCostROW,
		IndirectCostUtilities,
		IndirectCostOther,
		ImportTimeGeneratedId,
		SuperAssetId,
		IsGap,
		ImportSessionId,
		AssetId,
		IsolatedBridge)
	SELECT @ScenId, TreatmentId AS ImportedTreatmentId, AssetType, District, Cnty, [Route], Direction,
		FromSegment, ToSegment, Interstate,
		Treatment, Benefit, Cost, Risk, IsCommitted, PriorityOrder, PreferredYear,
		CASE WHEN MinYear > @YearFirst THEN MinYear ELSE @YearFirst END AS MinYear,
		CASE WHEN MaxYear < @YearLast THEN MaxYear ELSE @YearLast END AS MaxYear,
		BRKEY, BRIDGE_ID,
		IndirectCostDesign,
		IndirectCostROW,
		IndirectCostUtilities,
		IndirectCostOther,
		ImportTimeGeneratedId,
		SuperAssetId,
		0 AS IsGap,
		ImportSessionId,
		AssetId,
		CONVERT(BIT,IsIsolatedBridge)
	FROM #Treatments WITH (NOLOCK)
	WHERE MaxYear >= @YearFirst
		  AND MinYear <= @YearLast
		  AND PreferredYear BETWEEN @YearFirst AND @YearLast
		  AND IsIsolatedBridge = 0
	ORDER BY TreatmentID;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Library treatment records inserted into tbl_pb_ExtendedImportedTreatments: ' + CONVERT(VARCHAR(10), @RowCount);

	DROP TABLE #Treatments;


    PRINT 'sp_pb_PopulateExtendedTreatmentsV2 @ScenId=' + CONVERT(VARCHAR(10),@ScenId) + ' - ended';
END
GO
