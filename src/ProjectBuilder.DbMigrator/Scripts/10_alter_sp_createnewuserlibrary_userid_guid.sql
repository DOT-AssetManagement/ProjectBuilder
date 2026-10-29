-- ============================================================
-- MIGRATION : PB + DSS DATABASE MERGE
-- STEP 10   : ALTER SP sp_pb_CreateNewUserLibrary  (@UserId bigint -> uniqueidentifier)
-- Run in    : PB database
-- Why       : Library ownership is being re-keyed to the ABP user Guid (Step 11),
--             so the create-library stored procedure must accept a uniqueidentifier.
-- ============================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

ALTER PROCEDURE [dbo].[sp_pb_CreateNewUserLibrary]
    @UserId uniqueidentifier,
    @LibraryName nvarchar(50),
    @Description nvarchar(max),
    @Shared BIT = 0,
    @LibraryId varchar(100) OUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RowCount INT;

    PRINT 'sp_pb_CreateNewUserLibrary - started...';

    INSERT INTO tbl_lib_UserLibraries(UserId, [Name], [Description], IsSharedLibrary)
        VALUES (@UserId, @LibraryName, @Description, @Shared);

    SET @RowCount = @@ROWCOUNT;

    SET @LibraryId = NULL;
    IF @RowCount > 0 BEGIN
        SELECT TOP 1 @LibraryId = CONVERT(VARCHAR(100), Id)
        FROM tbl_lib_UserLibraries WITH (NOLOCK)
        WHERE UserId = @UserId
        ORDER BY LibNo DESC
    END

    PRINT 'Rows inserted into tbl_pb_TreatmentLibraries: ' + CONVERT(VARCHAR(10), @RowCount);
    PRINT 'New library ID is: ' + @LibraryId;
    PRINT 'sp_pb_CreateNewUserLibrary - ended...';

    RETURN @RowCount;
END
GO
