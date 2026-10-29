-- ============================================================
-- STEP 62 : CREATE tbl_pb_BridgeReferenceDataFromPennshare
--
-- New master reference table for the District/County/Route/Section
-- location dropdowns when Asset Type = Bridge, replacing the current
-- behavior of deriving those options from a scenario's own imported
-- ProjectTreatments. Populated (and fully replaced on every run) by the
-- new Admin > Import > "Fetch Bridge Reference Data" page, which pulls
-- from PennDOT's public PennShare "Pennsylvania Bridges" ArcGIS layer:
--   https://gis.penndot.pa.gov/gis/rest/services/opendata/pennsylvaniabridges/MapServer/0
--
-- Only the columns needed for the location cascade are kept (not every
-- BMS2 column the source exposes), matching the shape of the existing
-- tbl_pb_BridgeToPavements crosswalk table:
--   BRKEY (source field, esriFieldTypeDouble)   -> Id (varchar, PK)
--   DISTRICT   (string "05")                    -> District (int)
--   CTY_CODE   (string "21")                     -> CountyCode (int)
--   BRIDGE_ID  (string, BMS id)                  -> BridgeId (varchar)
--   ST_RT_NO   (string "0022")                    -> Route (int)
--   SEG_NO     (string "0090")                    -> Segment (int)
--   OFFSET     (smallint)                         -> OffSet (int)
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'tbl_pb_BridgeReferenceDataFromPennshare'
)
BEGIN
    CREATE TABLE [dbo].[tbl_pb_BridgeReferenceDataFromPennshare] (
        [BRKEY]      NVARCHAR(30) NOT NULL,
        [DISTRICT]   INT          NULL,
        [County_Code] INT         NULL,
        [BRIDGE_ID]  NVARCHAR(30) NULL,
        [Route]      INT          NULL,
        [Segment]    INT          NULL,
        [Offset]     INT          NULL,
        [CreatedBy]  NVARCHAR(256) NULL,
        [CreatedAt]  DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_tbl_pb_BridgeReferenceDataFromPennshare] PRIMARY KEY CLUSTERED ([BRKEY])
    );

    CREATE NONCLUSTERED INDEX [IX_tbl_pb_BridgeReferenceDataFromPennshare_DCRS]
        ON [dbo].[tbl_pb_BridgeReferenceDataFromPennshare] ([DISTRICT], [County_Code], [Route], [Segment]);

    PRINT 'Created table tbl_pb_BridgeReferenceDataFromPennshare';
END
ELSE
BEGIN
    PRINT 'Table tbl_pb_BridgeReferenceDataFromPennshare already exists — skipped';
END
GO
