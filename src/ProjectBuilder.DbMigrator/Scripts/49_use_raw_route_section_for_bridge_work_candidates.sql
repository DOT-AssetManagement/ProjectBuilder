SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[UspMoveRawWorkCandidatesToPoolTables]
	 @SessionId UNIQUEIDENTIFIER,
	 @PoolId UNIQUEIDENTIFIER,
	 @DeleteAll BIT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @RowCount BIGINT;

	PRINT 'UspMoveRawWorkCandidatesToPoolTables - started...';

	IF (@DeleteAll = 1) BEGIN
		DELETE FROM dbo.tbl_dss_WorkCandidates WHERE PoolId = @PoolId;
		SET @RowCount = @@ROWCOUNT;
		PRINT 'DeleteAll requested - existing records deleted from tbl_dss_WorkCandidates: ' + CONVERT(VARCHAR(10), @RowCount);
	END

	CREATE TABLE #PotentialDuplicates (
		ProjectId NVARCHAR(50),
		District TINYINT,
		Cnty TINYINT,
		[Route] INT,
		FromSection INT,
		ToSection INT,
		BRKEY NVARCHAR(50),
		AssetType NVARCHAR(1),
		[Description] NVARCHAR(255),
		Treatment NVARCHAR(50),
		MinYear INT,
		MaxYear INT,
		[Year] INT,
		Cost FLOAT
		);

	INSERT INTO #PotentialDuplicates (ProjectId, District, Cnty, [Route], FromSection, ToSection, BRKEY, AssetType, [Description], Treatment, MinYear, MaxYear, [Year], Cost)
	SELECT ProjectId, District, Cnty = (SELECT TOP 1 Cnty FROM tbl_dss_Counties WITH (NOLOCK) WHERE UPPER(County) = a.County),
		[Route], FromSection, ToSection, BRKEY, AssetType, [Description], Treatment, MinYear, MaxYear, [Year], Cost FROM tbl_dss_RawImportedWorkCandidates a WITH (NOLOCK);

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records inserted into #PotentialDuplicates: ' + CONVERT(VARCHAR(10),@RowCount);

	--SELECT * FROM #PotentialDuplicates;

	CREATE TABLE #DuplicateIDs (
		Id UNIQUEIDENTIFIER
		);

	INSERT INTO #DuplicateIDs (Id)
	SELECT Id FROM tbl_dss_WorkCandidates w WITH (NOLOCK)
	INNER JOIN #PotentialDuplicates d
		ON d.ProjectId=w.ProjectName
	   AND d.District=w.District
	   AND d.Cnty=w.Cnty
	   AND d.AssetType = w.AssetType
	   AND ((d.AssetType='P' AND d.FromSection = w.FromSection AND d.ToSection = w.ToSection) OR (d.AssetType = 'B' AND d.BRKEY = w.BRKEY))
	   AND ISNULL(d.[Description] ,'') = ISNULL(w.[Description],'')
	   AND ISNULL(d.Treatment,'') = ISNULL(w.Treatment,'')
	   AND d.MinYear = w.MinYear
	   AND d.MaxYear = w.MaxYear
	   AND d.[Year] = w.[Year]
	   AND d.Cost = w.Cost
	WHERE w.PoolId = @PoolId;

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Records inserted into #DuplicateIds: ' + CONVERT(VARCHAR(10),@RowCount);

	DROP TABLE #PotentialDuplicates;

	--SELECT * FROM #DuplicateIDs;

	DELETE FROM dbo.tbl_dss_WorkCandidates
	WHERE PoolId=@PoolId
		 AND Id IN (SELECT Id FROM #DuplicateIDs);

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Duplicate records deleted from tbl_dss_WideImportedWorkCandidates: ' + CONVERT(VARCHAR(10), @RowCount);

	DROP TABLE #DuplicateIDs;

    INSERT INTO dbo.tbl_dss_WorkCandidates
           (ImportSessionId
           ,PoolId

           ,ProjectName
           ,District
           ,Cnty
           ,[Route]
           ,AssetType
           ,FromSection
           ,ToSection
           ,BRKEY
           ,BRIDGE_ID
           ,[Description]
           ,Cost

           ,HSIP_Eligible
           ,Excess_Safety_Value
           ,Supported_by_RSA_Safety_Study
           ,Degree_of_Safety_Improvement
           ,Supports_Hazard_Mitigation
           ,Improves_Access_to_Emergency_Services

           ,Travel_Time_Savings
           ,Operating_Cost_Savings
           ,Degree_of_Mobility_Improvement
           ,Freight_Improvement
           ,Facilitates_Transit_Use
           ,Facilitates_Active_Transportation
           ,Bike_Lanes_Constructed
           ,Support_for_Job_Growth
           ,Support_for_Tourism
           ,Support_for_Economic_Goals
           ,Support_for_Recreation
           ,Consistency_with_Existing_Plans

           ,Sidewalks_and_Curb_Ramps_Constructed_or_Improved
           ,Change_in_Accessible_Destinations
           ,Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations
           ,Support_for_Environmental_Justice
           ,Reduced_Flood_Closure_Risk
           ,Wetlands_Improved
           ,Wildlife_Crossings
           ,Reduced_Fuel_Consumption
           ,Consistency_with_Land_Use_Plans

           ,Pavement_Rehabilitated_or_Reconstructed
           ,Bridges_Rehabilitated_or_Replaced
           ,Culverts_Rehabilitated_or_Replaced
           ,Guardrail_Rehabilitated_or_Replaced
           ,Geotechnical_Assets_Rehabilitated_or_Replaced
           ,Facilities_Rehabilitated_or_Reconstructed
           ,Sidewalks_Rehabilitated_or_Reconstructed

           ,ADT
           ,Percent_Trucks
           ,Population_Density
           ,Bike_Commute_Share
           ,BPN
           ,Functional_Classification
           ,National_Highway_System
           ,Pennsylvania_Byway
		   ,Degree_of_Environmental_Improvement
		   ,[Length]
		   ,Speed_Limit
		   ,Detour_Distance
		   ,Treatment
		   ,[Year]
		   ,[MinYear]
		   ,[MaxYear]
          )
	SELECT  @SessionId
		   ,@PoolId
		   ,ProjectID AS ProjectName
           ,a.District
           ,Cnty = (SELECT TOP 1 Cnty FROM tbl_dss_Counties WITH (NOLOCK) WHERE UPPER(County) = a.County)
           ,a.[Route]
           ,AssetType
           ,a.FromSection
           ,a.ToSection
           ,a.BRKEY
           ,a.BRIDGE_ID
           ,[Description]
           ,Cost

           ,HSIP_Eligible
           ,Excess_Safety_Value
           ,Supported_by_RSA_Safety_Study
           ,Degree_of_Safety_Improvement
           ,Supports_Hazard_Mitigation
           ,Improves_Access_to_Emergency_Services

           ,Travel_Time_Savings
           ,Operating_Cost_Savings
           ,Degree_of_Mobility_Improvement

           ,Freight_Improvement
           ,Facilitates_Transit_Use
           ,Facilitates_Active_Transportation
           ,Bike_Lanes_Constructed
           ,Support_for_Job_Growth
           ,Support_for_Tourism
           ,Support_for_Economic_Goals
           ,Support_for_Recreation
           ,Consistency_with_Existing_Plans

           ,Sidewalks_and_Curb_Ramps_Constructed_or_Improved
           ,Change_in_Accessible_Destinations
           ,Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations
           ,Support_for_Environmental_Justice
           ,Reduced_Flood_Closure_Risk
           ,Wetlands_Improved
           ,Wildlife_Crossings
           ,Reduced_Fuel_Consumption
           ,Consistency_with_Land_Use_Plans

           ,Pavement_Rehabilitated_or_Reconstructed
           ,Bridges_Rehabilitated_or_Replaced
           ,Culverts_Rehabilitated_or_Replaced
           ,Guardrail_Rehabilitated_or_Replaced
           ,Geotechnical_Assets_Rehabilitated_or_Replaced
           ,Facilities_Rehabilitated_or_Reconstructed
           ,Sidewalks_Rehabilitated_or_Reconstructed

           ,ADT
           ,Percent_Trucks
           ,Population_Density
           ,Bike_Commute_Share
           ,BPN
           ,Functional_Classification
           ,National_Highway_System
           ,Pennsylvania_Byway
		   ,Degree_of_Environmental_Improvement
		   ,[Length]
		   ,Speed_Limit
		   ,Detour_Distance
		   ,Treatment
		   ,[Year]
		   ,[MinYear]
		   ,[MaxYear]
		FROM tbl_dss_RawImportedWorkCandidates a WITH (NOLOCK);

		SET @RowCount = @@ROWCOUNT;

		PRINT 'New records inserted into tbl_dss_WideImportedWorkCandidates: ' + CONVERT(VARCHAR(10), @RowCount);

		EXEC dbo.UspCalculateBenefits @ImportSessionId=@SessionId;
		EXEC UspCalculateScores @PoolId=@PoolId;
		EXEC dbo.UspUpdateProjectCandidates @PoolId=@PoolId, @CreateOrUpdate='C';

		PRINT 'UspMoveRawWorkCandidatesToPoolTables - ended.';
END
GO
