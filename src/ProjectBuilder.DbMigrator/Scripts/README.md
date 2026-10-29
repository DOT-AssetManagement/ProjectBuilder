# PB + DSS Database Merge Migration

## Two Approaches — Pick One

### Approach A : Manual CREATE TABLE
Files `02a` and `03a`. Schema is hand-written based on the original DSS definitions.
- Works without a linked server connection to DSS_DB
- Requires running `08_schema_mismatch_fixes.sql` after to correct any type/length/nullability gaps
- Slower to maintain if DSS schema changes

### Approach B : SELECT INTO (Recommended)
Files `02b` and `03b`. Schema is copied directly from the live DSS.
- Requires linked server `[DSS]` to be active at migration time
- Types, lengths, and nullability are **guaranteed to match** — no fix script needed
- Skip Step 8 entirely when using this approach
- ⚠️ `IDENTITY` columns are **not** preserved. Columns like `SessionNo`, `RunNo`, `LibNo` become plain `int`. Add IDENTITY back manually if the application requires it.

---

## Execution Order

Run all scripts in the **PB database** in the order shown. Steps 2 and 3 have two versions — run only one.

| File | Step | Approach | Description |
|------|------|----------|-------------|
| `01_alter_pb_scenarios.sql` | 1 | Both | Adds 4 columns to `tbl_pb_Scenarios` |
| `02a_create_dss_settings_tables_manual.sql` | 2 | A only | Creates ABP settings tables via CREATE TABLE |
| `02b_create_dss_settings_tables_select_into.sql` | 2 | B only | Creates ABP settings tables via SELECT INTO from DSS_DB |
| `03a_create_dss_tables_manual.sql` | 3 | A only | Creates all DSS tables via CREATE TABLE |
| `03b_create_dss_tables_select_into.sql` | 3 | B only | Creates all DSS tables via SELECT INTO from DSS_DB |
| `04_create_indexes.sql` | 4 | Both | Indexes on ABP settings tables |
| `05_column_renames.sql` | 5 | Both | Renames `PoolId` → `LibraryId` in 2 tables |
| `06_verify.sql` | 6 | Both | Read-only check — lists shared table names |
| `06b_schema_verification.sql` | 6b | Both | Full column-level schema comparison against DSS_DB |
| `07_shared_table_cleanup.sql` | 7 | Both | Drops DSS duplicates; renames PB tables to shared names |
| `08_schema_mismatch_fixes.sql` | 8 | A only | Fixes type/length/nullability gaps found by 06b |
| `09_create_abp_identity_permission_tables.sql` | 9 | Both | Creates the ABP identity + permission tables (`AbpUsers`, `AbpRoles`, `AbpUserRoles`, `AbpPermissionGrants`, …). Idempotent. |
| `10_alter_sp_createnewuserlibrary_userid_guid.sql` | 10 | Both | Alters `sp_pb_CreateNewUserLibrary` so `@UserId` is `uniqueidentifier`. |
| `11_migrate_users_roles_to_abp_and_rekey_libraries.sql` | 11 | Both | Migrates custom users/roles → ABP and re-keys `tbl_lib_UserLibraries.UserId` (bigint → Guid). |

---

## ABP Identity Migration (Steps 9–11)

Steps 9–11 switch authentication/authorization from the legacy custom user tables
(`tbl_identity_User` / `tbl_identity_Role` / `tbl_identity_UserRoles`) to ABP's
built-in identity (`AbpUsers` / `AbpRoles` / `AbpUserRoles`) and permission tables.

**Run them in order, after steps 1–8, and only once:**

1. **Step 9** creates the empty ABP identity + permission tables. This is the SQL
   equivalent of the EF migration `AddAbpIdentityAndPermissionTables`; if you apply
   that EF migration instead, Step 9 is a harmless no-op (it is idempotent), so it is
   safe either way. It also ends with a **self-heal block** that adds columns the
   current ABP 10.1 model expects but the repo's older `Initial` schema omitted
   (`AbpUsers.LastSignInTime`, `AbpRoles.CreationTime`, `AbpClaimTypes.CreationTime`,
   `AbpPermissions.ResourceName` / `ManagementPermissionName`). If you ever see an
   `Invalid column name` / `Invalid object name` error from ABP at login/startup,
   **re-run Step 9** — it adds any missing columns without recreating tables. Step 9 also
   creates tables ABP 10.1 added (`AbpUserPasswordHistories`, `AbpUserPasskeys`,
   `AbpResourcePermissionGrants`) — e.g. external login fails with
   `Invalid object name 'AbpUserPasswordHistories'` without them.
2. **Step 10** updates `sp_pb_CreateNewUserLibrary` to take a `uniqueidentifier` owner
   id (libraries are now owned by the ABP user Guid).
3. **Step 11** copies the existing custom users, roles and role-assignments into the
   ABP tables (matched by email), then converts `tbl_lib_UserLibraries.UserId` from
   `bigint` to `uniqueidentifier`, remapping every library to its owner's new ABP Guid.
   It **aborts automatically if `AbpUsers` already contains rows**, so it cannot be
   run twice by accident.

The legacy `tbl_identity_*` tables are **left in place** (not dropped) but are no
longer used by the application.

---

## Source Table Name Map (DSS_DB → PB)

| DSS_DB Table | PB Table | Note |
|---|---|---|
| `_DssGoals` | `tbl_dss_Goals` | Underscore-prefixed in DSS |
| `_DssGoalWeights` | `tbl_dss_GoalWeights` | Underscore-prefixed in DSS |
| `_DssMeasures` | `tbl_dss_Measures` | Underscore-prefixed in DSS |
| `_DssMeasureScalingLinear` | `tbl_dss_MeasureScalingLinear` | Underscore-prefixed in DSS |
| `_DssMeasureWeights` | `tbl_dss_MeasureWeights` | Underscore-prefixed in DSS |
| `_DssProjects` | `tbl_dss_Projects` | Underscore-prefixed in DSS |
| `TblWorkCandidates` | `tbl_dss_WorkCandidates` | Not _DssWorkCandidates — confirmed |
| `TblAssetTypeParametersData` | `tbl_dss_AssetTypeParametersData` | |
| `TblBenefitParameters` | `tbl_dss_BenefitParameters` | |
| `TblBridgeToPavements` | `tbl_dss_BridgeToPavements` | Dropped in Step 7; PB version renamed to `tbl_BridgeToPavements` |
| `TblCounties` | `tbl_dss_Counties` | Dropped in Step 7; PB version renamed to `tbl_Counties` |
| `TblDataSections` | `tbl_dss_DataSections` | |
| `TblImportSessions` | `tbl_dss_ImportSessions` | Dropped in Step 7; PB version renamed to `tbl_ImportSessions` |
| `TblOtherParameters` | `tbl_dss_OtherParameters` | |
| `_TblPoolParameters` | `tbl_dss_PoolParameters` | Underscore-prefixed in DSS |
| `_TblPoolParametersData` | `tbl_dss_PoolParametersData` | Underscore-prefixed in DSS |
| `TblProjectCandidates` | `tbl_dss_ProjectCandidates` | |
| `TblRawImportedWorkCandidates` | `tbl_dss_RawImportedWorkCandidates` | |
| `TblScenarioProjectObjectiveValues` | `tbl_dss_ScenarioProjectObjectiveValues` | |
| `TblScenarioRunsLog` | `tbl_dss_ScenarioRunsLog` | |
| `TblScenarios` | `tbl_dss_Scenarios` | Dropped in Step 7; PB version renamed to `tbl_Scenarios` |
| `TblUserPools` | `tbl_dss_UserPools` | |
| `SpoofProjects_NorthwestRPO` | `tbl_dss_SpoofProjects_NorthwestRPO` | |
| `OpenIddictApplications` | `tbl_dss_OpenIddictApplications` | |
| `OpenIddictAuthorizations` | `tbl_dss_OpenIddictAuthorizations` | |
| `OpenIddictScopes` | `tbl_dss_OpenIddictScopes` | |
| `OpenIddictTokens` | `tbl_dss_OpenIddictTokens` | |
| `AbpSettings` | `tbl_dss_AbpSettings` | |
| `AbpSettingDefinitions` | `tbl_dss_AbpSettingDefinitions` | |

---

## Step 5 Note — Intentional Column Renames

`PoolId` was renamed to `LibraryId` in two tables as part of Step 5. The schema verification script (06b) will report these as "EXISTS IN PB — NOT IN DSS" — this is expected and should not be actioned.

| Table | DSS Column | PB Column |
|-------|-----------|-----------|
| `tbl_dss_ProjectCandidates` | `PoolId` | `LibraryId` |
| `tbl_dss_PoolParametersData` | `PoolId` | `LibraryId` |

---

## Running via sqlcmd (Approach B example)

```bash
sqlcmd -S <server> -d <PB_database> -i 01_alter_pb_scenarios.sql
sqlcmd -S <server> -d <PB_database> -i 02b_create_dss_settings_tables_select_into.sql
sqlcmd -S <server> -d <PB_database> -i 03b_create_dss_tables_select_into.sql
sqlcmd -S <server> -d <PB_database> -i 04_create_indexes.sql
sqlcmd -S <server> -d <PB_database> -i 05_column_renames.sql
sqlcmd -S <server> -d <PB_database> -i 06_verify.sql
sqlcmd -S <server> -d <PB_database> -i 06b_schema_verification.sql
sqlcmd -S <server> -d <PB_database> -i 07_shared_table_cleanup.sql
sqlcmd -S <server> -d <PB_database> -I -i 09_create_abp_identity_permission_tables.sql
sqlcmd -S <server> -d <PB_database> -I -i 10_alter_sp_createnewuserlibrary_userid_guid.sql
sqlcmd -S <server> -d <PB_database> -I -i 11_migrate_users_roles_to_abp_and_rekey_libraries.sql
```

> The `-I` flag (QUOTED_IDENTIFIER ON) is required for steps 9–11 because of the
> filtered index on `AbpPermissionGrants` and the stored-procedure ALTER.