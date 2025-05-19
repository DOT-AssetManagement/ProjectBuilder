USE [SPP_PBv1]
GO

ALTER TABLE [dbo].[tbl_pb_ImportSessions] DROP CONSTRAINT [DF_tbl_pb_ImportSessions_CreatedAt]
GO

ALTER TABLE [dbo].[tbl_pb_ImportSessions] DROP CONSTRAINT [DF_tbl_pb_ImportSessions_CreatedBy]
GO

ALTER TABLE [dbo].[tbl_pb_ImportSessions] DROP CONSTRAINT [DF_tbl_pb_ImportSessions_Id]
GO

/****** Object:  Table [dbo].[tbl_pb_ImportSessions]    Script Date: 12/18/2023 3:57:07 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbl_pb_ImportSessions]') AND type in (N'U'))
DROP TABLE [dbo].[tbl_pb_ImportSessions]
GO

/****** Object:  Table [dbo].[tbl_pb_ImportSessions]    Script Date: 12/18/2023 3:57:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tbl_pb_ImportSessions](
	[NoId] [int] IDENTITY(1,1) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[ImportSource] [varchar](10) NOT NULL,
	[DataSourceType] [varchar](10) NOT NULL,
	[DataSourceName] [nvarchar](255) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CompletedAt] [datetime] NULL,
	[CompletedStatus] [int] NULL,
	[Notes] [nvarchar](max) NULL,
	[TargetLibraryId] [uniqueidentifier] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[tbl_pb_ImportSessions] ADD  CONSTRAINT [DF_tbl_pb_ImportSessions_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[tbl_pb_ImportSessions] ADD  CONSTRAINT [DF_tbl_pb_ImportSessions_CreatedBy]  DEFAULT (user_name()) FOR [CreatedBy]
GO

ALTER TABLE [dbo].[tbl_pb_ImportSessions] ADD  CONSTRAINT [DF_tbl_pb_ImportSessions_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt]
GO

