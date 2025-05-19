USE [SPP_PBv1]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_tbl_pams_MaintainableAssetsSegmentation_Asset]    Script Date: 10/31/2023 3:54:50 PM ******/
CREATE NONCLUSTERED INDEX [IX_tbl_pams_MaintainableAssetsSegmentation_Asset] ON [dbo].[tbl_pams_MaintainableAssetsSegmentation]
(
	[Asset] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

