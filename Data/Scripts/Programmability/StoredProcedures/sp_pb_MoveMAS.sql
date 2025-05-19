USE [SPP_PBv1]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_MoveMAS]    Script Date: 10/25/2023 1:52:30 PM ******/
DROP PROCEDURE [dbo].[sp_pb_MoveMAS]
GO

/****** Object:  StoredProcedure [dbo].[sp_pb_MoveMAS]    Script Date: 10/25/2023 1:52:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_pb_MoveMAS]
	@ImportSessionId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RowCount INT;

	PRINT 'sp_pb_MoveMAS @ImportSessionId=' + CONVERT(VARCHAR(100), @ImportSessionId) + ' - started...';

	DELETE FROM tbl_pams_MaintainableAssetsSegmentation
	WHERE NetworkId IN (SELECT DISTINCT NetworkId FROM tbl_import_MAS WITH (NOLOCK)
						WHERE ImportSessionId=@ImportSessionId)
	
	SET @RowCount = @@ROWCOUNT;
	PRINT 'Number of records deleted from tbl_pams_MaintainableAssetsSegmentation: ' + CONVERT(VARCHAR(10),@RowCount);

	INSERT INTO tbl_pams_MaintainableAssetsSegmentation
		(NetworkId
		,AssetId
		,District
		,Cnty
		,[Route]
		,Asset
		,Direction
		,FromSection
		,ToSection
		,Area
		,Interstate
		,Lanes
		,Width
		,[Length]
		,SurfaceName
		,Risk
		)
	SELECT NetworkId
      ,AssetId
      ,District
      ,Cnty
      ,[Route]
      ,Asset
      ,Direction
      ,FromSection
      ,ToSection
      ,Area
      ,CONVERT(BIT,CASE Interstate WHEN 'Y' THEN 1 ELSE 0 END) AS Interstate
      ,Lanes
      ,Width
      ,[Length]
      ,SurfaceName
      ,Risk
	 FROM dbo.tbl_import_MAS WITH (NOLOCK)

	SET @RowCount = @@ROWCOUNT;
	PRINT 'Number of recordsinserted into tbl_pams_MaintainableAssetsSegmentation: ' + CONVERT(VARCHAR(10),@RowCount);

	PRINT 'sp_pb_MoveMAS @ImportSessionId=' + CONVERT(VARCHAR(100), @ImportSessionId) + ' - ended.';
END
GO


