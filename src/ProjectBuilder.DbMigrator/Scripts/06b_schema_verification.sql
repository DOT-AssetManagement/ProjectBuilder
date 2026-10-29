-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 6b   : SCHEMA VERIFICATION  (corrected mapping)
-- Run in    : PB database (requires linked server access to DSS)
--
-- Mapping corrected against actual DSS table list:
--   - Dss* tables have underscore prefix in DSS  (_DssGoals etc.)
--   - Pool tables are _TblPoolParameters / _TblPoolParametersData
--   - Step 7 renamed tables use their new PB names (tbl_BridgeToPavements etc.)
-- ============================================================


-- ------------------------------------------------------------
-- Section 1 : MISMATCHES
-- Columns that exist in both but differ in type, length, or nullability
-- Expected result: 0 rows
-- ------------------------------------------------------------

SELECT
    pb.TABLE_NAME                                        AS PB_Table,
    pb.COLUMN_NAME                                       AS Column_Name,
    pb.DATA_TYPE                                         AS PB_DataType,
    pb.CHARACTER_MAXIMUM_LENGTH                          AS PB_MaxLength,
    pb.IS_NULLABLE                                       AS PB_Nullable,
    dss.DATA_TYPE                                        AS DSS_DataType,
    dss.CHARACTER_MAXIMUM_LENGTH                         AS DSS_MaxLength,
    dss.IS_NULLABLE                                      AS DSS_Nullable,
    CASE
        WHEN pb.DATA_TYPE <> dss.DATA_TYPE               THEN 'DATA TYPE MISMATCH'
        WHEN ISNULL(CAST(pb.CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)), 'NULL')
          <> ISNULL(CAST(dss.CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)), 'NULL')
                                                         THEN 'LENGTH MISMATCH'
        WHEN pb.IS_NULLABLE <> dss.IS_NULLABLE           THEN 'NULLABILITY MISMATCH'
        ELSE 'OK'
    END                                                  AS Issue

FROM INFORMATION_SCHEMA.COLUMNS pb

INNER JOIN (VALUES
    -- DSS core tables  (underscore-prefixed in DSS)
    ('tbl_dss_Goals',                          '_DssGoals'),
    ('tbl_dss_GoalWeights',                    '_DssGoalWeights'),
    ('tbl_dss_Measures',                       '_DssMeasures'),
    ('tbl_dss_MeasureScalingLinear',            '_DssMeasureScalingLinear'),
    ('tbl_dss_MeasureWeights',                 '_DssMeasureWeights'),
    ('tbl_dss_Projects',                       '_DssProjects'),
    ('tbl_dss_WorkCandidates',                 'TblWorkCandidates'),
    -- Tbl* tables
    ('tbl_dss_AssetTypeParametersData',        'TblAssetTypeParametersData'),
    ('tbl_dss_BenefitParameters',              'TblBenefitParameters'),
    ('tbl_dss_DataSections',                   'TblDataSections'),
    ('tbl_dss_OtherParameters',                'TblOtherParameters'),
    ('tbl_dss_PoolParameters',                 '_TblPoolParameters'),
    ('tbl_dss_PoolParametersData',             '_TblPoolParametersData'),
    ('tbl_dss_ProjectCandidates',              'TblProjectCandidates'),
    ('tbl_dss_RawImportedWorkCandidates',      'TblRawImportedWorkCandidates'),
    ('tbl_dss_ScenarioProjectObjectiveValues', 'TblScenarioProjectObjectiveValues'),
    ('tbl_dss_ScenarioRunsLog',                'TblScenarioRunsLog'),
    ('tbl_dss_UserPools',                      'TblUserPools'),
    -- Standalone tables
    ('tbl_dss_SpoofProjects_NorthwestRPO',     'SpoofProjects_NorthwestRPO'),
    -- OpenIddict tables
    ('tbl_dss_OpenIddictApplications',         'OpenIddictApplications'),
    ('tbl_dss_OpenIddictAuthorizations',       'OpenIddictAuthorizations'),
    ('tbl_dss_OpenIddictScopes',               'OpenIddictScopes'),
    ('tbl_dss_OpenIddictTokens',               'OpenIddictTokens'),
    -- ABP tables
    ('tbl_dss_AbpSettings',                    'AbpSettings'),
    ('tbl_dss_AbpSettingDefinitions',          'AbpSettingDefinitions'),
    -- Step 7 renamed tables — now use their final PB names
    ('tbl_BridgeToPavements',                  'TblBridgeToPavements'),
    ('tbl_Counties',                           'TblCounties'),
    ('tbl_ImportSessions',                     'TblImportSessions'),
    ('tbl_Scenarios',                          'TblScenarios')
) AS Mapping (PB_Table, DSS_Table)
    ON pb.TABLE_NAME = Mapping.PB_Table

INNER JOIN [DSS].INFORMATION_SCHEMA.COLUMNS dss
    ON dss.TABLE_NAME  = Mapping.DSS_Table
   AND dss.COLUMN_NAME = pb.COLUMN_NAME

WHERE pb.TABLE_SCHEMA = 'dbo'
  AND (
        pb.DATA_TYPE <> dss.DATA_TYPE
     OR ISNULL(CAST(pb.CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)), 'NULL')
     <> ISNULL(CAST(dss.CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)), 'NULL')
     OR pb.IS_NULLABLE <> dss.IS_NULLABLE
  )

ORDER BY pb.TABLE_NAME, pb.COLUMN_NAME;


-- ------------------------------------------------------------
-- Section 2 : MISSING COLUMNS IN PB
-- Columns in DSS source that are absent in PB
-- Expected result: 0 rows
-- ------------------------------------------------------------

SELECT
    Mapping.PB_Table,
    dss.COLUMN_NAME                  AS Missing_Column,
    dss.DATA_TYPE,
    dss.CHARACTER_MAXIMUM_LENGTH,
    dss.IS_NULLABLE,
    'EXISTS IN DSS — MISSING IN PB'  AS Issue

FROM (VALUES
    ('tbl_dss_Goals',                          '_DssGoals'),
    ('tbl_dss_GoalWeights',                    '_DssGoalWeights'),
    ('tbl_dss_Measures',                       '_DssMeasures'),
    ('tbl_dss_MeasureScalingLinear',            '_DssMeasureScalingLinear'),
    ('tbl_dss_MeasureWeights',                 '_DssMeasureWeights'),
    ('tbl_dss_Projects',                       '_DssProjects'),
    ('tbl_dss_WorkCandidates',                 'TblWorkCandidates'),
    ('tbl_dss_AssetTypeParametersData',        'TblAssetTypeParametersData'),
    ('tbl_dss_BenefitParameters',              'TblBenefitParameters'),
    ('tbl_dss_DataSections',                   'TblDataSections'),
    ('tbl_dss_OtherParameters',                'TblOtherParameters'),
    ('tbl_dss_PoolParameters',                 '_TblPoolParameters'),
    ('tbl_dss_PoolParametersData',             '_TblPoolParametersData'),
    ('tbl_dss_ProjectCandidates',              'TblProjectCandidates'),
    ('tbl_dss_RawImportedWorkCandidates',      'TblRawImportedWorkCandidates'),
    ('tbl_dss_ScenarioProjectObjectiveValues', 'TblScenarioProjectObjectiveValues'),
    ('tbl_dss_ScenarioRunsLog',                'TblScenarioRunsLog'),
    ('tbl_dss_UserPools',                      'TblUserPools'),
    ('tbl_dss_SpoofProjects_NorthwestRPO',     'SpoofProjects_NorthwestRPO'),
    ('tbl_dss_OpenIddictApplications',         'OpenIddictApplications'),
    ('tbl_dss_OpenIddictAuthorizations',       'OpenIddictAuthorizations'),
    ('tbl_dss_OpenIddictScopes',               'OpenIddictScopes'),
    ('tbl_dss_OpenIddictTokens',               'OpenIddictTokens'),
    ('tbl_dss_AbpSettings',                    'AbpSettings'),
    ('tbl_dss_AbpSettingDefinitions',          'AbpSettingDefinitions'),
    ('tbl_BridgeToPavements',                  'TblBridgeToPavements'),
    ('tbl_Counties',                           'TblCounties'),
    ('tbl_ImportSessions',                     'TblImportSessions'),
    ('tbl_Scenarios',                          'TblScenarios')
) AS Mapping (PB_Table, DSS_Table)

INNER JOIN [DSS].INFORMATION_SCHEMA.COLUMNS dss
    ON dss.TABLE_NAME = Mapping.DSS_Table

LEFT JOIN INFORMATION_SCHEMA.COLUMNS pb
    ON pb.TABLE_NAME   = Mapping.PB_Table
   AND pb.COLUMN_NAME  = dss.COLUMN_NAME
   AND pb.TABLE_SCHEMA = 'dbo'

WHERE pb.COLUMN_NAME IS NULL

ORDER BY Mapping.PB_Table, dss.ORDINAL_POSITION;


-- ------------------------------------------------------------
-- Section 3 : EXTRA COLUMNS IN PB
-- Columns in PB with no matching column in DSS source
-- Expected result: 0 rows (unless intentionally added)
-- Note: PoolId → LibraryId renames will appear here — that is expected.
-- ------------------------------------------------------------

SELECT
    pb.TABLE_NAME                    AS PB_Table,
    pb.COLUMN_NAME                   AS Extra_Column,
    pb.DATA_TYPE,
    pb.CHARACTER_MAXIMUM_LENGTH,
    pb.IS_NULLABLE,
    'EXISTS IN PB — NOT IN DSS'      AS Issue

FROM INFORMATION_SCHEMA.COLUMNS pb

INNER JOIN (VALUES
    ('tbl_dss_Goals',                          '_DssGoals'),
    ('tbl_dss_GoalWeights',                    '_DssGoalWeights'),
    ('tbl_dss_Measures',                       '_DssMeasures'),
    ('tbl_dss_MeasureScalingLinear',            '_DssMeasureScalingLinear'),
    ('tbl_dss_MeasureWeights',                 '_DssMeasureWeights'),
    ('tbl_dss_Projects',                       '_DssProjects'),
    ('tbl_dss_WorkCandidates',                 'TblWorkCandidates'),
    ('tbl_dss_AssetTypeParametersData',        'TblAssetTypeParametersData'),
    ('tbl_dss_BenefitParameters',              'TblBenefitParameters'),
    ('tbl_dss_DataSections',                   'TblDataSections'),
    ('tbl_dss_OtherParameters',                'TblOtherParameters'),
    ('tbl_dss_PoolParameters',                 '_TblPoolParameters'),
    ('tbl_dss_PoolParametersData',             '_TblPoolParametersData'),
    ('tbl_dss_ProjectCandidates',              'TblProjectCandidates'),
    ('tbl_dss_RawImportedWorkCandidates',      'TblRawImportedWorkCandidates'),
    ('tbl_dss_ScenarioProjectObjectiveValues', 'TblScenarioProjectObjectiveValues'),
    ('tbl_dss_ScenarioRunsLog',                'TblScenarioRunsLog'),
    ('tbl_dss_UserPools',                      'TblUserPools'),
    ('tbl_dss_SpoofProjects_NorthwestRPO',     'SpoofProjects_NorthwestRPO'),
    ('tbl_dss_OpenIddictApplications',         'OpenIddictApplications'),
    ('tbl_dss_OpenIddictAuthorizations',       'OpenIddictAuthorizations'),
    ('tbl_dss_OpenIddictScopes',               'OpenIddictScopes'),
    ('tbl_dss_OpenIddictTokens',               'OpenIddictTokens'),
    ('tbl_dss_AbpSettings',                    'AbpSettings'),
    ('tbl_dss_AbpSettingDefinitions',          'AbpSettingDefinitions'),
    ('tbl_BridgeToPavements',                  'TblBridgeToPavements'),
    ('tbl_Counties',                           'TblCounties'),
    ('tbl_ImportSessions',                     'TblImportSessions'),
    ('tbl_Scenarios',                          'TblScenarios')
) AS Mapping (PB_Table, DSS_Table)
    ON pb.TABLE_NAME = Mapping.PB_Table

LEFT JOIN [DSS].INFORMATION_SCHEMA.COLUMNS dss
    ON dss.TABLE_NAME  = Mapping.DSS_Table
   AND dss.COLUMN_NAME = pb.COLUMN_NAME

WHERE pb.TABLE_SCHEMA = 'dbo'
  AND dss.COLUMN_NAME IS NULL

ORDER BY pb.TABLE_NAME, pb.ORDINAL_POSITION;
