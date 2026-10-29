SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH('dbo.tbl_dss_ProjectCandidates', 'ProjectName') IS NULL
BEGIN
    PRINT 'Adding tbl_dss_ProjectCandidates.ProjectName...';
    ALTER TABLE [dbo].[tbl_dss_ProjectCandidates]
        ADD [ProjectName] NVARCHAR(50) NULL;
END
ELSE
BEGIN
    PRINT 'tbl_dss_ProjectCandidates.ProjectName already exists - nothing to add.';
END
GO

-- Backfill existing rows from their work candidates' ProjectName (all candidates for a given
-- ProjectCandidateId share the same ProjectName -- see UspUpdateProjectCandidates' GROUP BY).
UPDATE pc
    SET pc.ProjectName = w.ProjectName
FROM tbl_dss_ProjectCandidates pc
CROSS APPLY (
    SELECT TOP 1 ProjectName
    FROM tbl_dss_WorkCandidates w
    WHERE w.ProjectCandidateId = pc.Id AND w.ProjectName IS NOT NULL
) w
WHERE pc.ProjectName IS NULL;
GO

ALTER PROCEDURE [dbo].[UspUpdateProjectCandidates]
    @PoolId           UNIQUEIDENTIFIER = NULL,
    @CreateOrUpdate   CHAR(1),
    @ProjectId        UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Rowcount BIGINT;
    PRINT 'dbo.UspUpdateProjectCandidates [' + @CreateOrUpdate + '] - started...';

    IF @CreateOrUpdate = 'C' OR @ProjectId IS NOT NULL BEGIN
        IF @PoolID IS NULL BEGIN
            TRUNCATE TABLE tbl_dss_ProjectCandidates;
            PRINT 'Table tbl_dss_ProjectCandidates truncated';
        END ELSE BEGIN
            DELETE FROM tbl_dss_ProjectCandidates
            WHERE PoolId = @PoolId AND (@ProjectId IS NULL OR Id = @ProjectId);
            SET @Rowcount = @@ROWCOUNT;
            PRINT 'Number of old records deleted: ' + CONVERT(VARCHAR(10), @Rowcount);
        END
    END

    DECLARE @NewProjId UNIQUEIDENTIFIER = NEWID();

    CREATE TABLE #Projects (
        Id                          UNIQUEIDENTIFIER,
        PoolId                      UNIQUEIDENTIFIER,
        District                    TINYINT,
        Cnty                        TINYINT,
        ProjectName                 NVARCHAR(50),
        [Description]               NVARCHAR(255),
        Treatment                   NVARCHAR(MAX),
        YearEarliest                INT,
        YearLatest                  INT,
        NumberOfWorkCandidates      INT,
        TotalCost                   FLOAT,
        TotalScaledBenefit          FLOAT,
        SafetyScore                 FLOAT,
        MobilityAndEconomyScore     FLOAT,
        EquityAndAccessScore        FLOAT,
        ResilienceAndEnvironmentScore FLOAT,
        ConditionAndPerformanceScore  FLOAT
    );

    INSERT INTO #Projects
        (Id, PoolId, District, Cnty, ProjectName, [Description], Treatment,
         YearEarliest, YearLatest, NumberOfWorkCandidates, TotalCost, TotalScaledBenefit,
         SafetyScore, MobilityAndEconomyScore, EquityAndAccessScore,
         ResilienceAndEnvironmentScore, ConditionAndPerformanceScore)
    SELECT
        MAX(ProjectCandidateId)     AS ProjectId,
        PoolId,
        MAX(District)               AS District,
        MAX(Cnty)                   AS Cnty,
        ProjectName,
            ProjectName + ' - ' + LEFT(ISNULL((
            SELECT TOP 1 Description
            FROM tbl_dss_WorkCandidates f WITH (NOLOCK)
            WHERE (@PoolId IS NULL OR f.PoolId = @PoolId)
              AND (@ProjectId IS NULL OR f.ProjectCandidateId = @ProjectId OR @CreateOrUpdate = 'C')
              AND f.ProjectName = w.ProjectName
            ORDER BY f.CreatedAt ASC
        ), ''), 25)                AS [Description],
        Treatment = REPLACE(REPLACE(REPLACE(
            (SELECT DISTINCT Treatment AS Q
             FROM tbl_dss_WorkCandidates v WITH (NOLOCK)
             WHERE (@PoolId IS NULL OR PoolId = @PoolId)
               AND (@ProjectId IS NULL OR ProjectCandidateId = @ProjectId OR @CreateOrUpdate = 'C')
               AND v.ProjectName = w.ProjectName
             GROUP BY Treatment, PoolId, ProjectCandidateId
             ORDER BY 1 DESC FOR XML PATH ('')), '</Q><Q>', '+'), '<Q>', ''), '</Q>', ''),
        MIN(ISNULL(MinYear, 9999))  AS YearEarliest,
        MAX(ISNULL(MaxYear, 0))     AS YearLatest,
        COUNT(1)                    AS NumberOfCandidates,
        SUM(Cost)                   AS TotalCost,
        SUM(Predicted_Scaled_Benefit) AS TotalScaledBenefit,
        SUM(Safety_Score)                        AS SafetyScore,
        SUM(Mobility_and_Access_Score)            AS MobilityAndEconomyScore,
        SUM(Equity_and_Access_Score)              AS EquityAndAccessScore,
        SUM(Resilience_and_Environment_Score)     AS ResilienceAndEnvironmentScore,
        SUM(Condition_and_Performance_Score)      AS ConditionAndPerformanceScore
    FROM tbl_dss_WorkCandidates w WITH (NOLOCK)
    WHERE (@PoolId IS NULL OR PoolId = @PoolId)
      AND (@ProjectId IS NULL OR ProjectCandidateId = @ProjectId OR @CreateOrUpdate = 'C')
    GROUP BY PoolId, ProjectCandidateId, ProjectName
    ORDER BY CONVERT(INT, ProjectName);

    SET @Rowcount = @@ROWCOUNT;
    PRINT 'Number of records inserted into #Projects: ' + CONVERT(VARCHAR(10), @Rowcount);

    UPDATE #Projects SET Id = COALESCE(Id, NEWID());

    IF @CreateOrUpdate = 'C' OR @ProjectId IS NOT NULL BEGIN
        INSERT INTO tbl_dss_ProjectCandidates
            (Id, PoolId, District, Cnty, ProjectName, [Description], Treatment,
             NumberOfWorkCandidates, TotalCost, TotalScaledBenefit,
             SafetyScore, MobilityAndEconomyScore, EquityAndAccessScore,
             ResilienceAndEnvironmentScore, ConditionAndPerformanceScore,
             TotalScore, YearEarliest, YearLatest)
        SELECT
            Id, PoolId, District, Cnty, ProjectName, [Description], Treatment,
            NumberOfWorkCandidates, TotalCost, TotalScaledBenefit,
            SafetyScore, MobilityAndEconomyScore, EquityAndAccessScore,
            ResilienceAndEnvironmentScore, ConditionAndPerformanceScore,
            SafetyScore + MobilityAndEconomyScore + EquityAndAccessScore
                + ResilienceAndEnvironmentScore + ConditionAndPerformanceScore AS TotalScore,
            YearEarliest, YearLatest
        FROM #Projects;

        SET @Rowcount = @@ROWCOUNT;
        PRINT 'Number of records inserted into tbl_dss_ProjectCandidates: ' + CONVERT(VARCHAR(10), @Rowcount);

        UPDATE tbl_dss_WorkCandidates
        SET ProjectCandidateId = (SELECT Id FROM #Projects p WHERE p.PoolId = w.PoolId AND p.ProjectName = w.ProjectName)
        FROM tbl_dss_WorkCandidates w WITH (ROWLOCK)
        WHERE w.ProjectCandidateId IS NULL
          AND (@PoolId IS NULL OR w.PoolId = @PoolId);

        SET @Rowcount = @@ROWCOUNT;
        PRINT 'Number of records in tbl_dss_WorkCandidates updated: ' + CONVERT(VARCHAR(10), @Rowcount);
    END ELSE BEGIN
        UPDATE tbl_dss_ProjectCandidates
            SET District                    = p.District,
                Cnty                        = p.Cnty,
                ProjectName                = p.ProjectName,
                [Description]              = p.[Description],
                Treatment                  = p.Treatment,
                NumberOfWorkCandidates     = p.NumberOfWorkCandidates,
                TotalCost                  = p.TotalCost,
                TotalScaledBenefit         = p.TotalScaledBenefit,
                SafetyScore                = p.SafetyScore,
                MobilityAndEconomyScore    = p.MobilityAndEconomyScore,
                EquityAndAccessScore       = p.EquityAndAccessScore,
                ResilienceAndEnvironmentScore = p.ResilienceAndEnvironmentScore,
                ConditionAndPerformanceScore  = p.ConditionAndPerformanceScore,
                TotalScore                 = p.SafetyScore + p.MobilityAndEconomyScore
                                             + p.EquityAndAccessScore
                                             + p.ResilienceAndEnvironmentScore
                                             + p.ConditionAndPerformanceScore,
                YearEarliest               = p.YearEarliest,
                YearLatest                 = p.YearLatest
        FROM tbl_dss_ProjectCandidates c WITH (ROWLOCK)
        INNER JOIN #Projects p ON p.Id = c.Id
        WHERE (@ProjectId IS NULL OR p.Id = @ProjectId);

        SET @Rowcount = @@ROWCOUNT;
        PRINT 'Number of records updated tbl_dss_ProjectCandidates: ' + CONVERT(VARCHAR(10), @Rowcount);
    END

    DROP TABLE #Projects;
    PRINT 'dbo.UspUpdateProjectCandidates [' + @CreateOrUpdate + '] - ended.';
END
GO

ALTER PROCEDURE [dbo].[UspGetScenarioResultsForGIS]
	@ScenId UNIQUEIDENTIFIER,
	@EffThreshold FLOAT = 0
AS
BEGIN
	SET NOCOUNT ON;
	PRINT 'UspGetScenarioResultsForGIS - started...';
	DECLARE @PoolId UNIQUEIDENTIFIER = (SELECT PoolId FROM tbl_dss_Scenarios WITH (NOLOCK) WHERE Id=@ScenId);

	SELECT TOP 1 ScenNo, ScenName, [Description], PoolName, [User], LastRun
	FROM (
		SELECT s.Id AS ScenId, ROW_NUMBER() OVER(ORDER BY s.Id) AS ScenNo,
		s.ScenName, s.[Description], p.[Name] AS PoolName, u.[Name] AS [User],
		LastRun = (SELECT TOP 1 EndedAt FROM tbl_dss_ScenarioRunsLog r WITH (NOLOCK) WHERE r.ScenId=s.Id AND [Status]='Success' AND EndedAt IS NOT NULL ORDER BY RunNo DESC)
		FROM tbl_dss_Scenarios s WITH (NOLOCK)
		INNER JOIN tbl_dss_UserPools p WITH (NOLOCK) ON p.Id=s.PoolId
		LEFT OUTER JOIN AbpUsers u WITH (NOLOCK) ON u.Id=p.UserId
	) q
	WHERE q.ScenId=@ScenId;

	SELECT
		p.Id AS [ProjectId],
		p.ProjectName AS [ProjectNo],
		COALESCE(t.[Name],'DSS') AS [PlanningPartner],
		p.District, p.Cnty, c.County, p.[Description], p.Treatment,
		p.YearEarliest AS [MinYear], p.YearLatest AS [MaxYear],
		NumPave = (SELECT COUNT(1) FROM tbl_dss_WorkCandidates w WHERE w.ProjectCandidateId = p.Id AND w.AssetType='P'),
		NumBridge = (SELECT COUNT(1) FROM tbl_dss_WorkCandidates w WHERE w.ProjectCandidateId = p.Id AND w.AssetType='B'),
		TotalCost AS [Cost], TotalScaledBenefit AS [Benefit], RelativeEfficiency AS [Efficiency]
	FROM tbl_dss_ProjectCandidates p WITH (NOLOCK)
	INNER JOIN tbl_dss_UserPools up WITH (NOLOCK) ON up.Id = p.PoolId
	LEFT OUTER JOIN tbl_dss_Counties c WITH (NOLOCK) ON c.Cnty=p.Cnty
	LEFT OUTER JOIN AbpUsers u WITH (NOLOCK) ON u.Id=up.UserId
	LEFT OUTER JOIN AbpTenants t WITH (NOLOCK) ON t.Id = u.TenantId
	WHERE (@PoolId IS NULL OR p.PoolId=@PoolId)
	  AND (@EffThreshold IS NULL OR ISNULL(p.RelativeEfficiency,0) >= @EffThreshold)
	ORDER BY RelativeEfficiency DESC, TotalCost DESC, District, Cnty, p.Id;

	SELECT
		w.ProjectCandidateId AS [ProjId], w.Id AS [TreatId],
		ROW_NUMBER() OVER (ORDER BY w.Id) AS TreatNo,
		t.[Name] AS [Partner], w.AssetType, w.Treatment AS TreatmentType,
		w.[Description] AS TreatmentName, w.District, w.Cnty, c.County, w.[Route],
		w.FromSection, w.ToSection, w.BRKEY, w.BRIDGE_ID AS [BMSID],
		COALESCE(m.Asset, CONVERT(VARCHAR(10),w.Cnty) + '_' + CONVERT(VARCHAR(10),w.[Route]) + '_'
			+ CONVERT(VARCHAR(10), w.[FromSection]%2) + '_' + CONVERT(VARCHAR(10), w.FromSection) + '-' + CONVERT(VARCHAR(10), w.ToSection)) AS [CRS],
		COALESCE(m.Direction, w.FromSection%2) AS [Direction],
		w.[Year], w.Cost, w.Predicted_Scaled_Benefit AS [Benefit],
		CASE WHEN w.Cost <= 0 THEN 0 ELSE w.Predicted_Scaled_Benefit/w.Cost END AS [BCRatio]
	FROM tbl_dss_WorkCandidates w WITH (NOLOCK)
	INNER JOIN tbl_dss_ProjectCandidates p WITH (NOLOCK) ON p.PoolId=w.PoolId AND p.Id = w.ProjectCandidateId
	LEFT OUTER JOIN tbl_dss_Counties c WITH (NOLOCK) ON c.Cnty = w.Cnty
	LEFT OUTER JOIN tbl_pams_MaintainableAssetsSegmentation m WITH (NOLOCK)
		ON m.District=w.District AND m.Cnty=w.Cnty AND m.[Route]=w.[Route]
		AND m.FromSection=w.FromSection AND m.ToSection=w.ToSection
	INNER JOIN tbl_dss_UserPools up WITH (NOLOCK) ON up.Id = p.PoolId
	LEFT OUTER JOIN AbpUsers u WITH (NOLOCK) ON u.Id=up.UserId
	LEFT OUTER JOIN AbpTenants t WITH (NOLOCK) ON t.Id = u.TenantId
	WHERE (@PoolId IS NULL OR w.PoolId = @PoolId)
	  AND (@EffThreshold IS NULL OR ISNULL(p.RelativeEfficiency,0) >= @EffThreshold)
	ORDER BY w.ProjectCandidateId, w.Id;

	PRINT 'UspGetScenarioResultsForGIS - ended.';
END
GO
