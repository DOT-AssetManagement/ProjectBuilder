USE [SPP_PBv1]
GO
/****** Object:  StoredProcedure [dbo].[sp_pb_CreateNewScenario]    Script Date: 10/16/2023 9:05:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_pb_CreateNewScenario]
	@NewScenarioName VARCHAR(100),
	@LibraryId NVARCHAR(100) = NULL,
	@Code VARCHAR(4) = NULL,
	@SetDefaults BIT = 1,
	@FirstYear INT = NULL,
	@LastYear INT = NULL,
	@CreatedBy nvarchar(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RowCount BIGINT;
    INSERT INTO tbl_pb_Scenarios (ScenarioName, LibraryId, RecommendedUtilityProxy, CreatedBy)
	VALUES (LEFT(@NewScenarioName, 100),
		CASE WHEN @LibraryId IS NULL THEN NULL ELSE CONVERT(UNIQUEIDENTIFIER, @LibraryId) END,
		CASE WHEN ISNULL(@Code,'') = '' THEN NULL ELSE @Code END,@CreatedBy);
	PRINT 'Records inserted into tbl_pb_Scenarios: ' + CONVERT(VARCHAR(4),@@ROWCOUNT);
	DECLARE @NewScenId INT;
	SET @NewScenId = CONVERT(INT,@@IDENTITY);
	PRINT 'New scenario ID :' + CONVERT(VARCHAR(20), @NewScenId);
	SET @RowCount = 0;
	IF @SetDefaults = 1 BEGIN
		INSERT INTO tbl_pb_ScenParm (ScenId, ParmID, ParmValue)
		SELECT @NewScenId, ParmId, DefaultValue
		FROM tbl_pb_Parameters WITH (NOLOCK)
		WHERE ParmFamily='SCEN' AND DefaultValue IS NOT NULL
		SET @RowCount = @@ROWCOUNT;
	END
	IF @FirstYear IS NOT NULL AND @SetDefaults = 0 BEGIN
		INSERT INTO tbl_pb_ScenParm (ScenId, ParmID, ParmValue)
		VALUES (@NewScenId, 'YFST', CONVERT(FLOAT,@FirstYear))
		SET @RowCount = @RowCount + 1
	END
	IF @LastYear IS NOT NULL AND @SetDefaults = 0 BEGIN
		INSERT INTO tbl_pb_ScenParm (ScenId, ParmID, ParmValue)
		VALUES (@NewScenId, 'YLST', CONVERT(FLOAT,@LastYear))
		SET @RowCount = @RowCount + 1
	END
	PRINT 'Number of default value records insterted into tbl_pb_ScenParm: ' + CONVERT(VARCHAR(10), @RowCount);
	
	RETURN @NewScenId;
END