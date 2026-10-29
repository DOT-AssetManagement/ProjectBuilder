SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
            (Id, PoolId, District, Cnty, [Description], Treatment,
             NumberOfWorkCandidates, TotalCost, TotalScaledBenefit,
             SafetyScore, MobilityAndEconomyScore, EquityAndAccessScore,
             ResilienceAndEnvironmentScore, ConditionAndPerformanceScore,
             TotalScore, YearEarliest, YearLatest)
        SELECT
            Id, PoolId, District, Cnty, [Description], Treatment,
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