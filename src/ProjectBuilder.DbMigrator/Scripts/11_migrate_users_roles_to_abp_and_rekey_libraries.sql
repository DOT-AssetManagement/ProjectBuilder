-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 11   : MIGRATE CUSTOM USERS/ROLES -> ABP  +  RE-KEY LIBRARIES
-- Run in    : PB database
-- Requires  : Steps 9 (ABP tables) and 10 (SP) applied first.
-- Guard     : Aborts if AbpUsers already has rows (prevents a double-run).
-- ============================================================

/*
  One-time migration (data-driven from the existing custom tables):
   1. Copy every custom user (tbl_identity_User) into AbpUsers (long->Guid map kept in dbo._mig_UserMap).
   2. Create the custom roles (tbl_identity_Role, e.g. Guest/Admin/Operator) in AbpRoles (int->Guid map in dbo._mig_RoleMap).
   3. Replicate the role assignments (tbl_identity_UserRoles) into AbpUserRoles.
   4. Re-key tbl_lib_UserLibraries.UserId from bigint -> uniqueidentifier using the user map.
  Touches ONLY AbpUsers/AbpRoles/AbpUserRoles and tbl_lib_UserLibraries. Staging tables dropped on success.
*/
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

-- Guard: do not run twice.
IF (SELECT COUNT(*) FROM dbo.AbpUsers) > 0
BEGIN
    RAISERROR('AbpUsers already has rows - migration appears already applied. Aborting.', 16, 1);
    SET NOEXEC ON;
END
GO

/* ---------- Build long->Guid / int->Guid maps (staging) ---------- */
IF OBJECT_ID('dbo._mig_UserMap') IS NOT NULL DROP TABLE dbo._mig_UserMap;
CREATE TABLE dbo._mig_UserMap (OldId bigint PRIMARY KEY, NewId uniqueidentifier NOT NULL DEFAULT NEWID());
INSERT INTO dbo._mig_UserMap (OldId) SELECT UserId FROM dbo.tbl_identity_User;

IF OBJECT_ID('dbo._mig_RoleMap') IS NOT NULL DROP TABLE dbo._mig_RoleMap;
CREATE TABLE dbo._mig_RoleMap (OldId int PRIMARY KEY, Name nvarchar(256) NOT NULL, NewId uniqueidentifier NOT NULL DEFAULT NEWID());
INSERT INTO dbo._mig_RoleMap (OldId, Name) SELECT RoleId, Name FROM dbo.tbl_identity_Role;
GO

/* ---------- Insert ABP users / roles / user-roles ---------- */
INSERT INTO dbo.AbpUsers
    (Id, TenantId, UserName, NormalizedUserName, Name, Surname, Email, NormalizedEmail, EmailConfirmed,
     PasswordHash, SecurityStamp, IsExternal, PhoneNumber, PhoneNumberConfirmed, IsActive, TwoFactorEnabled,
     LockoutEnabled, AccessFailedCount, ShouldChangePasswordOnNextLogin, EntityVersion, ExtraProperties,
     ConcurrencyStamp, CreationTime, IsDeleted)
SELECT
    m.NewId, NULL, u.Email, UPPER(u.Email), u.Name, NULL, u.Email, UPPER(u.Email), 1,
    NULL, CONVERT(nvarchar(40), NEWID()), 1, NULL, 0, ISNULL(u.IsActive, 1), 0,
    0, 0, 0, 0, N'{}',
    CONVERT(nvarchar(40), NEWID()), SYSUTCDATETIME(), 0
FROM dbo.tbl_identity_User u
JOIN dbo._mig_UserMap m ON u.UserId = m.OldId;

INSERT INTO dbo.AbpRoles
    (Id, TenantId, Name, NormalizedName, IsDefault, IsStatic, IsPublic, EntityVersion, ExtraProperties, ConcurrencyStamp)
SELECT
    r.NewId, NULL, r.Name, UPPER(r.Name), 0, 0, 1, 0, N'{}', CONVERT(nvarchar(40), NEWID())
FROM dbo._mig_RoleMap r;

INSERT INTO dbo.AbpUserRoles (UserId, RoleId, TenantId)
SELECT um.NewId, rm.NewId, NULL
FROM dbo.tbl_identity_UserRoles ur
JOIN dbo._mig_UserMap um ON ur.UserId = um.OldId
JOIN dbo._mig_RoleMap rm ON ur.RoleId = rm.OldId;
GO

/* ---------- Re-key tbl_lib_UserLibraries.UserId bigint -> uniqueidentifier ---------- */
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tbl_lib_UserLibraries_UserId' AND object_id = OBJECT_ID('dbo.tbl_lib_UserLibraries'))
    DROP INDEX IX_tbl_lib_UserLibraries_UserId ON dbo.tbl_lib_UserLibraries;

ALTER TABLE dbo.tbl_lib_UserLibraries ADD UserGuid uniqueidentifier NULL;
GO

UPDATE l SET l.UserGuid = m.NewId
FROM dbo.tbl_lib_UserLibraries l
JOIN dbo._mig_UserMap m ON l.UserId = m.OldId;
GO

-- Safety: every row must be mapped before we drop the old column.
IF EXISTS (SELECT 1 FROM dbo.tbl_lib_UserLibraries WHERE UserGuid IS NULL)
BEGIN
    RAISERROR('Some tbl_lib_UserLibraries rows have unmapped owners (UserGuid IS NULL). Aborting re-key.', 16, 1);
    SET NOEXEC ON;
END
GO

ALTER TABLE dbo.tbl_lib_UserLibraries DROP COLUMN UserId;
GO
EXEC sp_rename 'dbo.tbl_lib_UserLibraries.UserGuid', 'UserId', 'COLUMN';
GO
ALTER TABLE dbo.tbl_lib_UserLibraries ALTER COLUMN UserId uniqueidentifier NOT NULL;
CREATE INDEX IX_tbl_lib_UserLibraries_UserId ON dbo.tbl_lib_UserLibraries (UserId);
GO

/* ---------- Verify ---------- */
SELECT CONCAT('AbpUsers=', (SELECT COUNT(*) FROM dbo.AbpUsers),
              '  AbpRoles=', (SELECT COUNT(*) FROM dbo.AbpRoles),
              '  AbpUserRoles=', (SELECT COUNT(*) FROM dbo.AbpUserRoles),
              '  Libraries=', (SELECT COUNT(*) FROM dbo.tbl_lib_UserLibraries),
              '  LibraryOwnersMatchedToAbpUsers=', (SELECT COUNT(*) FROM dbo.tbl_lib_UserLibraries l JOIN dbo.AbpUsers u ON l.UserId = u.Id)) AS Result;
SELECT 'tbl_lib_UserLibraries.UserId type now: ' + ty.name
FROM sys.columns c JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.tbl_lib_UserLibraries') AND c.name = 'UserId';
GO

/* ---------- Cleanup staging ---------- */
IF OBJECT_ID('dbo._mig_UserMap') IS NOT NULL DROP TABLE dbo._mig_UserMap;
IF OBJECT_ID('dbo._mig_RoleMap') IS NOT NULL DROP TABLE dbo._mig_RoleMap;
SET NOEXEC OFF;
GO
PRINT 'Migration complete.';
