-- ============================================================
-- 1. UspUIGetDistricts
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[UspUIGetDistricts]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT District,
           'District-' + CONVERT(VARCHAR(10), District) AS Descr
    FROM   [dbo].[tbl_dss_Counties] WITH (NOLOCK)
    WHERE  District IS NOT NULL
    ORDER BY 1;
END
GO

-- ============================================================
-- 2. UspUIGetCounties
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[UspUIGetCounties]
    @District INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT Cnty, County
    FROM   [dbo].[tbl_dss_Counties] WITH (NOLOCK)
    WHERE  District = @District
    ORDER BY County;
END
GO

-- ============================================================
-- 3. UspUIGetRoutes
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[UspUIGetRoutes]
    @District INT,
    @Cnty     INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT [Route] AS Rte
    FROM   [dbo].[tbl_pams_MaintainableAssetsSegmentation] WITH (NOLOCK)
    WHERE  District = @District
      AND  Cnty     = @Cnty
    ORDER BY 1;
END
GO

-- ============================================================
-- 4. UspUIGetSectionPairs
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[UspUIGetSectionPairs]
    @District  INT,
    @Cnty      INT,
    @Route     INT,
    @Direction BIT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT
           FromSection,
           ToSection,
           CONVERT(VARCHAR(10), FromSection) + '-' + CONVERT(VARCHAR(10), ToSection) AS FromTo
    FROM   [dbo].[tbl_pams_MaintainableAssetsSegmentation] WITH (NOLOCK)
    WHERE  District  = @District
      AND  Cnty      = @Cnty
      AND  [Route]   = @Route
      AND  Direction = CONVERT(TINYINT, @Direction)
    ORDER BY FromSection;
END
GO

-- ============================================================
-- 5. UspUIGetBrkeyAndBMSID
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[UspUIGetBrkeyAndBMSID]
    @District INT,
    @Cnty     INT,
    @Route    INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT
           BRKEY,
           BRIDGE_ID AS BMSID
    FROM   [dbo].[tbl_dss_BridgeToPavements] WITH (NOLOCK)
    WHERE  DISTRICT    = @District
      AND  County_Code = @Cnty
      AND  [Route]     = @Route
    ORDER BY BRKEY;
END
GO
