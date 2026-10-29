-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 9    : CREATE ABP IDENTITY + PERMISSION TABLES
-- Run in    : PB database
-- Note      : Equivalent to the EF migration 'AddAbpIdentityAndPermissionTables'.
--             Idempotent (each object guarded by IF ... IS NULL). If you apply
--             that EF migration instead, running this step is a harmless no-op.
--             Must run BEFORE Step 11 (which populates these tables).
-- ============================================================

/*
  Creates ONLY the ABP Identity + PermissionManagement tables in the target database.
  Idempotent: each table/index guarded by an existence check.
  Schemas copied verbatim from 20250211061230_Initial migration.
  Does NOT touch any other (business/legacy) tables.
*/
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ===================== Root identity tables (no FK) ===================== */

IF OBJECT_ID(N'dbo.AbpUsers', N'U') IS NULL
CREATE TABLE [dbo].[AbpUsers] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [UserName] nvarchar(256) NOT NULL,
    [NormalizedUserName] nvarchar(256) NOT NULL,
    [Name] nvarchar(64) NULL,
    [Surname] nvarchar(64) NULL,
    [Email] nvarchar(256) NOT NULL,
    [NormalizedEmail] nvarchar(256) NOT NULL,
    [EmailConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit),
    [PasswordHash] nvarchar(256) NULL,
    [SecurityStamp] nvarchar(256) NOT NULL,
    [IsExternal] bit NOT NULL DEFAULT CAST(0 AS bit),
    [PhoneNumber] nvarchar(16) NULL,
    [PhoneNumberConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit),
    [IsActive] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL DEFAULT CAST(0 AS bit),
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL DEFAULT CAST(0 AS bit),
    [AccessFailedCount] int NOT NULL DEFAULT 0,
    [ShouldChangePasswordOnNextLogin] bit NOT NULL,
    [EntityVersion] int NOT NULL,
    [LastPasswordChangeTime] datetimeoffset NULL,
    [ExtraProperties] nvarchar(max) NOT NULL,
    [ConcurrencyStamp] nvarchar(40) NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    CONSTRAINT [PK_AbpUsers] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpRoles', N'U') IS NULL
CREATE TABLE [dbo].[AbpRoles] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [Name] nvarchar(256) NOT NULL,
    [NormalizedName] nvarchar(256) NOT NULL,
    [IsDefault] bit NOT NULL,
    [IsStatic] bit NOT NULL,
    [IsPublic] bit NOT NULL,
    [EntityVersion] int NOT NULL,
    [ExtraProperties] nvarchar(max) NOT NULL,
    [ConcurrencyStamp] nvarchar(40) NOT NULL,
    CONSTRAINT [PK_AbpRoles] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpClaimTypes', N'U') IS NULL
CREATE TABLE [dbo].[AbpClaimTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [Required] bit NOT NULL,
    [IsStatic] bit NOT NULL,
    [Regex] nvarchar(512) NULL,
    [RegexDescription] nvarchar(128) NULL,
    [Description] nvarchar(256) NULL,
    [ValueType] int NOT NULL,
    [ExtraProperties] nvarchar(max) NOT NULL,
    [ConcurrencyStamp] nvarchar(40) NOT NULL,
    CONSTRAINT [PK_AbpClaimTypes] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpLinkUsers', N'U') IS NULL
CREATE TABLE [dbo].[AbpLinkUsers] (
    [Id] uniqueidentifier NOT NULL,
    [SourceUserId] uniqueidentifier NOT NULL,
    [SourceTenantId] uniqueidentifier NULL,
    [TargetUserId] uniqueidentifier NOT NULL,
    [TargetTenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_AbpLinkUsers] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpUserDelegations', N'U') IS NULL
CREATE TABLE [dbo].[AbpUserDelegations] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [SourceUserId] uniqueidentifier NOT NULL,
    [TargetUserId] uniqueidentifier NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    CONSTRAINT [PK_AbpUserDelegations] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpSecurityLogs', N'U') IS NULL
CREATE TABLE [dbo].[AbpSecurityLogs] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [ApplicationName] nvarchar(96) NULL,
    [Identity] nvarchar(96) NULL,
    [Action] nvarchar(96) NULL,
    [UserId] uniqueidentifier NULL,
    [UserName] nvarchar(256) NULL,
    [TenantName] nvarchar(64) NULL,
    [ClientId] nvarchar(64) NULL,
    [CorrelationId] nvarchar(64) NULL,
    [ClientIpAddress] nvarchar(64) NULL,
    [BrowserInfo] nvarchar(512) NULL,
    [CreationTime] datetime2 NOT NULL,
    [ExtraProperties] nvarchar(max) NOT NULL,
    [ConcurrencyStamp] nvarchar(40) NOT NULL,
    CONSTRAINT [PK_AbpSecurityLogs] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpSessions', N'U') IS NULL
CREATE TABLE [dbo].[AbpSessions] (
    [Id] uniqueidentifier NOT NULL,
    [SessionId] nvarchar(128) NOT NULL,
    [Device] nvarchar(64) NOT NULL,
    [DeviceInfo] nvarchar(64) NULL,
    [TenantId] uniqueidentifier NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ClientId] nvarchar(64) NULL,
    [IpAddresses] nvarchar(2048) NULL,
    [SignedIn] datetime2 NOT NULL,
    [LastAccessed] datetime2 NULL,
    [ExtraProperties] nvarchar(max) NULL,
    CONSTRAINT [PK_AbpSessions] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpOrganizationUnits', N'U') IS NULL
CREATE TABLE [dbo].[AbpOrganizationUnits] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [ParentId] uniqueidentifier NULL,
    [Code] nvarchar(95) NOT NULL,
    [DisplayName] nvarchar(128) NOT NULL,
    [EntityVersion] int NOT NULL,
    [ExtraProperties] nvarchar(max) NOT NULL,
    [ConcurrencyStamp] nvarchar(40) NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    [LastModificationTime] datetime2 NULL,
    [LastModifierId] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeleterId] uniqueidentifier NULL,
    [DeletionTime] datetime2 NULL,
    CONSTRAINT [PK_AbpOrganizationUnits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AbpOrganizationUnits_AbpOrganizationUnits_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[AbpOrganizationUnits] ([Id])
);

/* ===================== Junction / dependent identity tables ===================== */

IF OBJECT_ID(N'dbo.AbpOrganizationUnitRoles', N'U') IS NULL
CREATE TABLE [dbo].[AbpOrganizationUnitRoles] (
    [RoleId] uniqueidentifier NOT NULL,
    [OrganizationUnitId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    CONSTRAINT [PK_AbpOrganizationUnitRoles] PRIMARY KEY ([OrganizationUnitId], [RoleId]),
    CONSTRAINT [FK_AbpOrganizationUnitRoles_AbpOrganizationUnits_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [dbo].[AbpOrganizationUnits] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AbpOrganizationUnitRoles_AbpRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AbpRoles] ([Id]) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AbpRoleClaims', N'U') IS NULL
CREATE TABLE [dbo].[AbpRoleClaims] (
    [Id] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [ClaimType] nvarchar(256) NOT NULL,
    [ClaimValue] nvarchar(1024) NULL,
    CONSTRAINT [PK_AbpRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AbpRoleClaims_AbpRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AbpRoles] ([Id]) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AbpUserClaims', N'U') IS NULL
CREATE TABLE [dbo].[AbpUserClaims] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [ClaimType] nvarchar(256) NOT NULL,
    [ClaimValue] nvarchar(1024) NULL,
    CONSTRAINT [PK_AbpUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AbpUserClaims_AbpUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AbpUsers] ([Id]) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AbpUserLogins', N'U') IS NULL
CREATE TABLE [dbo].[AbpUserLogins] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(64) NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [ProviderKey] nvarchar(196) NOT NULL,
    [ProviderDisplayName] nvarchar(128) NULL,
    CONSTRAINT [PK_AbpUserLogins] PRIMARY KEY ([UserId], [LoginProvider]),
    CONSTRAINT [FK_AbpUserLogins_AbpUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AbpUsers] ([Id]) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AbpUserOrganizationUnits', N'U') IS NULL
CREATE TABLE [dbo].[AbpUserOrganizationUnits] (
    [UserId] uniqueidentifier NOT NULL,
    [OrganizationUnitId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorId] uniqueidentifier NULL,
    CONSTRAINT [PK_AbpUserOrganizationUnits] PRIMARY KEY ([OrganizationUnitId], [UserId]),
    CONSTRAINT [FK_AbpUserOrganizationUnits_AbpOrganizationUnits_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [dbo].[AbpOrganizationUnits] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AbpUserOrganizationUnits_AbpUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AbpUsers] ([Id]) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AbpUserRoles', N'U') IS NULL
CREATE TABLE [dbo].[AbpUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    CONSTRAINT [PK_AbpUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AbpUserRoles_AbpRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AbpRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AbpUserRoles_AbpUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AbpUsers] ([Id]) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AbpUserTokens', N'U') IS NULL
CREATE TABLE [dbo].[AbpUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(64) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AbpUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AbpUserTokens_AbpUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AbpUsers] ([Id]) ON DELETE CASCADE
);

/* ===================== Permission management tables ===================== */

IF OBJECT_ID(N'dbo.AbpPermissionGrants', N'U') IS NULL
CREATE TABLE [dbo].[AbpPermissionGrants] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [Name] nvarchar(128) NOT NULL,
    [ProviderName] nvarchar(64) NOT NULL,
    [ProviderKey] nvarchar(64) NOT NULL,
    CONSTRAINT [PK_AbpPermissionGrants] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpPermissionGroups', N'U') IS NULL
CREATE TABLE [dbo].[AbpPermissionGroups] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [DisplayName] nvarchar(256) NOT NULL,
    [ExtraProperties] nvarchar(max) NULL,
    CONSTRAINT [PK_AbpPermissionGroups] PRIMARY KEY ([Id])
);

IF OBJECT_ID(N'dbo.AbpPermissions', N'U') IS NULL
CREATE TABLE [dbo].[AbpPermissions] (
    [Id] uniqueidentifier NOT NULL,
    [GroupName] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [ParentName] nvarchar(128) NULL,
    [DisplayName] nvarchar(256) NOT NULL,
    [IsEnabled] bit NOT NULL,
    [MultiTenancySide] tinyint NOT NULL,
    [Providers] nvarchar(128) NULL,
    [StateCheckers] nvarchar(256) NULL,
    [ExtraProperties] nvarchar(max) NULL,
    CONSTRAINT [PK_AbpPermissions] PRIMARY KEY ([Id])
);

/* ===================== Indexes ===================== */

IF OBJECT_ID(N'dbo.AbpOrganizationUnits', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpOrganizationUnits_Code' AND object_id = OBJECT_ID(N'dbo.AbpOrganizationUnits'))
    CREATE INDEX [IX_AbpOrganizationUnits_Code] ON [dbo].[AbpOrganizationUnits] ([Code]);

IF OBJECT_ID(N'dbo.AbpOrganizationUnits', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpOrganizationUnits_ParentId' AND object_id = OBJECT_ID(N'dbo.AbpOrganizationUnits'))
    CREATE INDEX [IX_AbpOrganizationUnits_ParentId] ON [dbo].[AbpOrganizationUnits] ([ParentId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpOrganizationUnitRoles_RoleId_OrganizationUnitId' AND object_id = OBJECT_ID(N'dbo.AbpOrganizationUnitRoles'))
    CREATE INDEX [IX_AbpOrganizationUnitRoles_RoleId_OrganizationUnitId] ON [dbo].[AbpOrganizationUnitRoles] ([RoleId], [OrganizationUnitId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpPermissionGrants_TenantId_Name_ProviderName_ProviderKey' AND object_id = OBJECT_ID(N'dbo.AbpPermissionGrants'))
    CREATE UNIQUE INDEX [IX_AbpPermissionGrants_TenantId_Name_ProviderName_ProviderKey] ON [dbo].[AbpPermissionGrants] ([TenantId], [Name], [ProviderName], [ProviderKey]) WHERE [TenantId] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpPermissionGroups_Name' AND object_id = OBJECT_ID(N'dbo.AbpPermissionGroups'))
    CREATE UNIQUE INDEX [IX_AbpPermissionGroups_Name] ON [dbo].[AbpPermissionGroups] ([Name]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpPermissions_GroupName' AND object_id = OBJECT_ID(N'dbo.AbpPermissions'))
    CREATE INDEX [IX_AbpPermissions_GroupName] ON [dbo].[AbpPermissions] ([GroupName]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpPermissions_Name' AND object_id = OBJECT_ID(N'dbo.AbpPermissions'))
    CREATE UNIQUE INDEX [IX_AbpPermissions_Name] ON [dbo].[AbpPermissions] ([Name]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpRoleClaims_RoleId' AND object_id = OBJECT_ID(N'dbo.AbpRoleClaims'))
    CREATE INDEX [IX_AbpRoleClaims_RoleId] ON [dbo].[AbpRoleClaims] ([RoleId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpRoles_NormalizedName' AND object_id = OBJECT_ID(N'dbo.AbpRoles'))
    CREATE INDEX [IX_AbpRoles_NormalizedName] ON [dbo].[AbpRoles] ([NormalizedName]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpSecurityLogs_TenantId_Action' AND object_id = OBJECT_ID(N'dbo.AbpSecurityLogs'))
    CREATE INDEX [IX_AbpSecurityLogs_TenantId_Action] ON [dbo].[AbpSecurityLogs] ([TenantId], [Action]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpSecurityLogs_TenantId_ApplicationName' AND object_id = OBJECT_ID(N'dbo.AbpSecurityLogs'))
    CREATE INDEX [IX_AbpSecurityLogs_TenantId_ApplicationName] ON [dbo].[AbpSecurityLogs] ([TenantId], [ApplicationName]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpSecurityLogs_TenantId_Identity' AND object_id = OBJECT_ID(N'dbo.AbpSecurityLogs'))
    CREATE INDEX [IX_AbpSecurityLogs_TenantId_Identity] ON [dbo].[AbpSecurityLogs] ([TenantId], [Identity]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpSecurityLogs_TenantId_UserId' AND object_id = OBJECT_ID(N'dbo.AbpSecurityLogs'))
    CREATE INDEX [IX_AbpSecurityLogs_TenantId_UserId] ON [dbo].[AbpSecurityLogs] ([TenantId], [UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpSessions_Device' AND object_id = OBJECT_ID(N'dbo.AbpSessions'))
    CREATE INDEX [IX_AbpSessions_Device] ON [dbo].[AbpSessions] ([Device]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpSessions_SessionId' AND object_id = OBJECT_ID(N'dbo.AbpSessions'))
    CREATE INDEX [IX_AbpSessions_SessionId] ON [dbo].[AbpSessions] ([SessionId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpSessions_TenantId_UserId' AND object_id = OBJECT_ID(N'dbo.AbpSessions'))
    CREATE INDEX [IX_AbpSessions_TenantId_UserId] ON [dbo].[AbpSessions] ([TenantId], [UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUserClaims_UserId' AND object_id = OBJECT_ID(N'dbo.AbpUserClaims'))
    CREATE INDEX [IX_AbpUserClaims_UserId] ON [dbo].[AbpUserClaims] ([UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUserLogins_LoginProvider_ProviderKey' AND object_id = OBJECT_ID(N'dbo.AbpUserLogins'))
    CREATE INDEX [IX_AbpUserLogins_LoginProvider_ProviderKey] ON [dbo].[AbpUserLogins] ([LoginProvider], [ProviderKey]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUserOrganizationUnits_UserId_OrganizationUnitId' AND object_id = OBJECT_ID(N'dbo.AbpUserOrganizationUnits'))
    CREATE INDEX [IX_AbpUserOrganizationUnits_UserId_OrganizationUnitId] ON [dbo].[AbpUserOrganizationUnits] ([UserId], [OrganizationUnitId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUserRoles_RoleId_UserId' AND object_id = OBJECT_ID(N'dbo.AbpUserRoles'))
    CREATE INDEX [IX_AbpUserRoles_RoleId_UserId] ON [dbo].[AbpUserRoles] ([RoleId], [UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUsers_Email' AND object_id = OBJECT_ID(N'dbo.AbpUsers'))
    CREATE INDEX [IX_AbpUsers_Email] ON [dbo].[AbpUsers] ([Email]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUsers_NormalizedEmail' AND object_id = OBJECT_ID(N'dbo.AbpUsers'))
    CREATE INDEX [IX_AbpUsers_NormalizedEmail] ON [dbo].[AbpUsers] ([NormalizedEmail]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUsers_NormalizedUserName' AND object_id = OBJECT_ID(N'dbo.AbpUsers'))
    CREATE INDEX [IX_AbpUsers_NormalizedUserName] ON [dbo].[AbpUsers] ([NormalizedUserName]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUsers_UserName' AND object_id = OBJECT_ID(N'dbo.AbpUsers'))
    CREATE INDEX [IX_AbpUsers_UserName] ON [dbo].[AbpUsers] ([UserName]);

COMMIT TRANSACTION;
PRINT 'ABP identity + permission tables ensured.';

/* ===================== Version drift: columns the CURRENT ABP model expects =====================
   These columns were added to ABP's identity/permission model AFTER this project's original
   Initial migration (which the CREATE TABLE statements above were copied from). Guarded by
   COL_LENGTH so this is safe whether the tables were just created here or already existed from
   an earlier run of this script. */
IF COL_LENGTH('dbo.AbpUsers', 'LastSignInTime') IS NULL
    ALTER TABLE dbo.AbpUsers ADD LastSignInTime datetimeoffset NULL;

IF COL_LENGTH('dbo.AbpRoles', 'CreationTime') IS NULL
BEGIN
    ALTER TABLE dbo.AbpRoles ADD CreationTime datetime2 NULL;
    EXEC('UPDATE dbo.AbpRoles SET CreationTime = SYSUTCDATETIME() WHERE CreationTime IS NULL');
    EXEC('ALTER TABLE dbo.AbpRoles ALTER COLUMN CreationTime datetime2 NOT NULL');
END

IF COL_LENGTH('dbo.AbpClaimTypes', 'CreationTime') IS NULL
BEGIN
    ALTER TABLE dbo.AbpClaimTypes ADD CreationTime datetime2 NULL;
    EXEC('UPDATE dbo.AbpClaimTypes SET CreationTime = SYSUTCDATETIME() WHERE CreationTime IS NULL');
    EXEC('ALTER TABLE dbo.AbpClaimTypes ALTER COLUMN CreationTime datetime2 NOT NULL');
END

IF COL_LENGTH('dbo.AbpPermissions', 'ResourceName') IS NULL
    ALTER TABLE dbo.AbpPermissions ADD ResourceName nvarchar(256) NULL;
IF COL_LENGTH('dbo.AbpPermissions', 'ManagementPermissionName') IS NULL
    ALTER TABLE dbo.AbpPermissions ADD ManagementPermissionName nvarchar(256) NULL;

-- Tables ABP 10.1 added to the identity/permission model after the original Initial migration
-- (schema taken verbatim from EF's scaffolded output). Without these, external login fails with
-- "Invalid object name 'AbpUserPasswordHistories'" (the user lookup split-includes these navigations).
IF OBJECT_ID(N'dbo.AbpUserPasswordHistories', N'U') IS NULL
CREATE TABLE [dbo].[AbpUserPasswordHistories] (
    [UserId]    uniqueidentifier NOT NULL,
    [Password]  nvarchar(256)    NOT NULL,
    [TenantId]  uniqueidentifier NULL,
    [CreatedAt] datetimeoffset   NOT NULL,
    CONSTRAINT [PK_AbpUserPasswordHistories] PRIMARY KEY ([UserId], [Password]),
    CONSTRAINT [FK_AbpUserPasswordHistories_AbpUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AbpUsers] ([Id]) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AbpUserPasskeys', N'U') IS NULL
CREATE TABLE [dbo].[AbpUserPasskeys] (
    [CredentialId] varbinary(1024)  NOT NULL,
    [TenantId]     uniqueidentifier NULL,
    [UserId]       uniqueidentifier NOT NULL,
    [Data]         nvarchar(max)    NULL,
    CONSTRAINT [PK_AbpUserPasskeys] PRIMARY KEY ([CredentialId]),
    CONSTRAINT [FK_AbpUserPasskeys_AbpUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AbpUsers] ([Id]) ON DELETE CASCADE
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpUserPasskeys_UserId' AND object_id = OBJECT_ID(N'dbo.AbpUserPasskeys'))
    CREATE INDEX [IX_AbpUserPasskeys_UserId] ON [dbo].[AbpUserPasskeys] ([UserId]);

IF OBJECT_ID(N'dbo.AbpResourcePermissionGrants', N'U') IS NULL
CREATE TABLE [dbo].[AbpResourcePermissionGrants] (
    [Id]           uniqueidentifier NOT NULL,
    [TenantId]     uniqueidentifier NULL,
    [Name]         nvarchar(128)    NOT NULL,
    [ProviderName] nvarchar(64)     NOT NULL,
    [ProviderKey]  nvarchar(64)     NOT NULL,
    [ResourceName] nvarchar(256)    NOT NULL,
    [ResourceKey]  nvarchar(256)    NOT NULL,
    CONSTRAINT [PK_AbpResourcePermissionGrants] PRIMARY KEY ([Id])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AbpResourcePermissionGrants_TenantId_Name_ResourceName_ResourceKey_ProviderName_ProviderKey' AND object_id = OBJECT_ID(N'dbo.AbpResourcePermissionGrants'))
    CREATE UNIQUE INDEX [IX_AbpResourcePermissionGrants_TenantId_Name_ResourceName_ResourceKey_ProviderName_ProviderKey]
        ON [dbo].[AbpResourcePermissionGrants] ([TenantId], [Name], [ResourceName], [ResourceKey], [ProviderName], [ProviderKey])
        WHERE [TenantId] IS NOT NULL;

PRINT 'ABP model column drift ensured.';
