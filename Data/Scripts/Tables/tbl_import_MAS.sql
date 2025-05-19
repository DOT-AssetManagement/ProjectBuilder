USE [SPP_PBv1]
GO

/****** Object:  Table [dbo].[tbl_import_MAS]    Script Date: 10/25/2023 1:50:33 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbl_import_MAS]') AND type in (N'U'))
DROP TABLE [dbo].[tbl_import_MAS]
GO

/****** Object:  Table [dbo].[tbl_import_MAS]    Script Date: 10/25/2023 1:50:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tbl_import_MAS](
	[ImportSessionId] [uniqueidentifier] NOT NULL,
	[NetworkId] [uniqueidentifier] NOT NULL,
	[AssetId] [uniqueidentifier] NOT NULL,
	[District] [tinyint] NULL,
	[Cnty] [tinyint] NULL,
	[Route] [int] NULL,
	[Asset] [nvarchar](50) NULL,
	[Direction] [tinyint] NULL,
	[FromSection] [int] NULL,
	[ToSection] [int] NULL,
	[Area] [float] NULL,
	[Interstate] [varchar](1) NULL,
	[Lanes] [int] NULL,
	[Width] [float] NULL,
	[Length] [float] NULL,
	[SurfaceName] [nvarchar](100) NULL,
	[Risk] [float] NULL,
	[PopulatedBy] [nvarchar](50) NULL,
	[PopulatedAt] [datetime] NULL,
 CONSTRAINT [PK_tbl_import_MAS] PRIMARY KEY CLUSTERED 
(
	[NetworkId] ASC,
	[AssetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


