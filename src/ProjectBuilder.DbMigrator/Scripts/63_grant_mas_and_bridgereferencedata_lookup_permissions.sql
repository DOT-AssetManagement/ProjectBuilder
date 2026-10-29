SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @RoleName nvarchar(256);
DECLARE @Perms TABLE (Name nvarchar(128) PRIMARY KEY);

INSERT INTO @Perms (Name) VALUES
    (N'dss.SetUp.Lookups.MaintainableAssetsSegmentation'),
    (N'dss.SetUp.Lookups.BridgeReferenceData');

-- ── Admin ──
SELECT TOP (1) @RoleName = Name FROM dbo.AbpRoles
WHERE NormalizedName = N'ADMIN' AND TenantId IS NULL;

IF @RoleName IS NOT NULL
BEGIN
    INSERT INTO dbo.AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey)
    SELECT NEWID(), NULL, p.Name, N'R', @RoleName
    FROM @Perms p
    WHERE EXISTS (SELECT 1 FROM dbo.AbpPermissions ap WHERE ap.Name = p.Name)
      AND NOT EXISTS (
        SELECT 1 FROM dbo.AbpPermissionGrants g
        WHERE g.Name = p.Name AND g.ProviderName = N'R' AND g.ProviderKey = @RoleName AND g.TenantId IS NULL
      );
    PRINT CONCAT('Granted Admin role (', @RoleName, ') ', @@ROWCOUNT, ' new permission(s).');
END
ELSE
    PRINT 'Admin role not found — skipped.';

-- ── Operator ──
SELECT TOP (1) @RoleName = Name FROM dbo.AbpRoles
WHERE NormalizedName = N'OPERATOR' AND TenantId IS NULL;

IF @RoleName IS NOT NULL
BEGIN
    INSERT INTO dbo.AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey)
    SELECT NEWID(), NULL, p.Name, N'R', @RoleName
    FROM @Perms p
    WHERE EXISTS (SELECT 1 FROM dbo.AbpPermissions ap WHERE ap.Name = p.Name)
      AND NOT EXISTS (
        SELECT 1 FROM dbo.AbpPermissionGrants g
        WHERE g.Name = p.Name AND g.ProviderName = N'R' AND g.ProviderKey = @RoleName AND g.TenantId IS NULL
      );
    PRINT CONCAT('Granted operator role (', @RoleName, ') ', @@ROWCOUNT, ' new permission(s).');
END
ELSE
    PRINT 'operator role not found — skipped.';

COMMIT TRANSACTION;
GO
