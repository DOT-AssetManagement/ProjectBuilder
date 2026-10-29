-- Creates dbo.UsfCalcScore, the scalar function every generated Pool Score Parameters
-- formula (tbl_dss_PoolScoreSql, produced by UspCreateScoreSQL) calls to turn a raw
-- work-candidate measure into a weighted 0-100(ish) contribution.
--
-- This function was never migrated from the original DSS_DB: it isn't a table (so the
-- SELECT INTO migration steps in this folder never copied it), and its CREATE FUNCTION
-- text does not exist in the DSS or pbweb source repos or their git history — it only
-- ever lived directly on the original DSS SQL Server instance. Without it, every call in
-- every generated scoring formula fails with "Cannot find ... UsfCalcScore", so
-- UspCalculateScores silently leaves every score column NULL and charts/reports that
-- depend on scores (e.g. Outputs > Charts > Projects And Goals Flow Diagram) show
-- "Scores have not been computed" even after Pool Score Parameters are imported.
--
-- Reconstructed from the calling convention (verified against every generated formula
-- in tbl_dss_PoolScoreSql): UsfCalcScore(@Value, @Weight, @Min, @ScoreAtMin, @Max,
-- @ScoreAtMax, @Slope) linearly interpolates @Value between (@Min, @ScoreAtMin) and
-- (@Max, @ScoreAtMax) using the already-precomputed @Slope = (@ScoreAtMax - @ScoreAtMin)
-- / (@Max - @Min) -- confirmed to hold exactly for every parameter row currently in
-- tbl_dss_PoolScoreParameters -- clamps to the [@ScoreAtMin, @ScoreAtMax] range (in
-- whichever order they're given, since slope can be negative for "lower is better"
-- measures), then scales by @Weight. Weights within a pool sum to ~1.0 per score
-- category, consistent with a weighted-average composite in the 0-100 range.
--
-- IMPORTANT: this is a reconstruction, not the recovered original. Verify against any
-- known-good sample scores you may have before relying on it for real prioritization
-- decisions.
IF OBJECT_ID('dbo.UsfCalcScore', 'FN') IS NOT NULL
    DROP FUNCTION dbo.UsfCalcScore;
GO

CREATE FUNCTION dbo.UsfCalcScore
(
    @Value      FLOAT,
    @Weight     FLOAT,
    @Min        FLOAT,
    @ScoreAtMin FLOAT,
    @Max        FLOAT,
    @ScoreAtMax FLOAT,
    @Slope      FLOAT
)
RETURNS FLOAT
AS
BEGIN
    DECLARE @Score FLOAT;

    IF @Value IS NULL OR @Weight IS NULL
        RETURN 0;

    SET @Score = @ScoreAtMin + (@Value - @Min) * @Slope;

    IF @ScoreAtMin <= @ScoreAtMax
    BEGIN
        IF @Score < @ScoreAtMin SET @Score = @ScoreAtMin;
        IF @Score > @ScoreAtMax SET @Score = @ScoreAtMax;
    END
    ELSE
    BEGIN
        IF @Score > @ScoreAtMin SET @Score = @ScoreAtMin;
        IF @Score < @ScoreAtMax SET @Score = @ScoreAtMax;
    END

    RETURN @Score * @Weight;
END
GO
