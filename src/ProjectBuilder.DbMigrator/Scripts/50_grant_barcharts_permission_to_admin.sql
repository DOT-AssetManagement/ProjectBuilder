SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @RoleName nvarchar(256);

SELECT TOP (1) @RoleName = Name FROM dbo.AbpRoles
WHERE NormalizedName = N'ADMIN' AND TenantId IS NULL;

IF @RoleName IS NOT NULL
BEGIN
    INSERT INTO dbo.AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey)
    SELECT NEWID(), NULL, p.Name, N'R', @RoleName
    FROM dbo.AbpPermissions p
    WHERE p.Name = N'dss.Charts.BarCharts'
      AND NOT EXISTS (
        SELECT 1 FROM dbo.AbpPermissionGrants g
        WHERE g.Name = p.Name AND g.ProviderName = N'R' AND g.ProviderKey = @RoleName AND g.TenantId IS NULL
      );
    PRINT CONCAT('Granted Admin role (', @RoleName, ') ', @@ROWCOUNT, ' new BarCharts permission(s).');
END
ELSE
    PRINT 'Admin role not found — skipped.';

COMMIT TRANSACTION;
GO
