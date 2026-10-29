-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 2    : CREATE DSS SETTINGS TABLES
-- Run in    : PB database
-- ============================================================

CREATE TABLE [dbo].[tbl_dss_AbpSettings]
(
    [Id]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Name]         NVARCHAR(128)    NOT NULL,
    [Value]        NVARCHAR(MAX)    NULL,
    [ProviderName] NVARCHAR(64)     NULL,
    [ProviderKey]  NVARCHAR(64)     NULL
);
GO

CREATE TABLE [dbo].[tbl_dss_AbpSettingDefinitions]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Name]               NVARCHAR(128)    NOT NULL,
    [DisplayName]        NVARCHAR(256)    NULL,
    [Description]        NVARCHAR(MAX)    NULL,
    [DefaultValue]       NVARCHAR(MAX)    NULL,
    [IsVisibleToClients] BIT              NOT NULL DEFAULT(0),
    [Providers]          NVARCHAR(MAX)    NULL
);
GO
