using ExcelDataReader;
using log4net;
using log4net.Appender;
using log4net.Layout;
using Sylvan.Data.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.IO;
using Veracode.Attributes;

namespace ExcelWrapper
{
    public class ExcelHandler
    {
        static ExcelHandler()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

#if DEBUG
        private static ILog _log = ConfigureLogger();

        private static log4net.ILog ConfigureLogger()
        {
            // Programmatic configuration
            // follows (with some streamlining) the example from Brendan Long and Ron Grabowski
            // org.apache.logging.log4net-user
            // These config statements create a RollingFile Appender.  Rolling File Appenders rollover on each execution of the test harness, 
            // in this case, following the Composite RollingMode.  Alternative log4net appenders may be added  or replace this default appender at the programmer's discretion.

            // PatternLayout layout = new PatternLayout("%d [%t] %-5p %c - %m%n");

            PatternLayout layout = new PatternLayout("%d %-5p %c - %m%n");
            log4net.Appender.RollingFileAppender appender = new RollingFileAppender
            {
                Layout = layout,
                AppendToFile = true,
                MaxFileSize = 10000000,
                RollingStyle = RollingFileAppender.RollingMode.Composite,
                StaticLogFileName = true,

                File = @".\Logs\ProjectBuilder.NET.log" // all logs will be created in the subdirectory logs 
            };

            // Configure filter to accept log messages of any level.
            log4net.Filter.LevelMatchFilter traceFilter = new log4net.Filter.LevelMatchFilter
            {
#if DEBUG
                LevelToMatch = log4net.Core.Level.Debug
#else
                LevelToMatch = log4net.Core.Level.Info
#endif
            };
            appender.ClearFilters();
            appender.AddFilter(traceFilter);

            appender.ImmediateFlush = true;
            appender.ActivateOptions();

            // Attach appender into hierarchy
            log4net.Repository.Hierarchy.Logger root =
                ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root;
            root.AddAppender(appender);
            root.Repository.Configured = true;

            log4net.ILog log = log4net.LogManager.GetLogger("PB-LOGGER");

            log.Debug("PB-LOGGER created.");

            return log;
        }

#else
        private static ILog _log;
#endif

        public static ILog Log
        {
            get { return _log ??= LogManager.GetLogger("PB-LOGGER"); }
            set { _log = value; }
        }

        [CRLFCleanser]
        public static string SafeForLog(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // URL-encode so CR/LF and every other control character is neutralized
            return System.Net.WebUtility.UrlEncode(input);
        }

        public static bool Read(string excelFilePath, out DataSet dataSet, out string errorMessage)
        {
            bool ok = true;
            dataSet = null;
            errorMessage = null;

            Log.Info($"Reading Excel file {excelFilePath} - started...");

            try
            {
                using (var stream = File.OpenRead(excelFilePath))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var conf = new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = a => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true
                            }
                        };

                        dataSet = reader.AsDataSet(conf);
                    }
                }
            }
            catch (Exception ex)
            {
                dataSet = null;
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"Reading Excel file {excelFilePath} - ended");

            return ok;
        }

        // Attention! The reader object must be closed after this function has returned.
        public static bool Write(ExcelDataWriter xwr, DbDataReader reader, string sheetName, out long recordsWritten, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            recordsWritten = 0;

            Log.Info($"Writing to worksheet [{sheetName}] - started...");

            try
            {
                if (reader == null || !reader.HasRows)
                {
                    throw new Exception($"DbDataReader object for work sheet '{sheetName}' was not initialized or have no rows.");
                }

                ExcelDataWriter.WriteResult res = xwr.Write(reader, sheetName);


                if (!res.IsComplete)
                {
                    throw new Exception($"No records were written to work sheet '{sheetName}'");
                }

                recordsWritten = res.RowsWritten;

            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                recordsWritten = 0;
                Log.Error(errorMessage);
            }

            Log.Info($"Writing to worksheet [{sheetName}] - ended. OK={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        public static bool WriteFromStoredProcedure(ExcelDataWriter xwr, SqlConnection connection, string sprocName, int scenId, string sheetName, out long recordsWritten, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            recordsWritten = 0;

            Log.Info($"WriteFromStoredProcedure({sprocName},{scenId},[{sheetName}]) - started...");

            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sprocName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 24000;
                    command.Parameters.Add("ScenId", SqlDbType.Int).Value = scenId;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ExcelDataWriter.WriteResult res = xwr.Write(reader, sheetName);
                        reader.Close();
                        if (!res.IsComplete)
                        {
                            throw new Exception($"No records were written to work sheet '{sheetName}'");
                        }

                        recordsWritten = res.RowsWritten;
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"WriteFromStoredProcedure({sprocName},{scenId},[{sheetName}]) - ended.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        public static bool WriteFromSqlQuery(ExcelDataWriter xwr, SqlConnection connection, string sqlQuery, string sheetName, out long recordsWritten, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            recordsWritten = 0;
            DbDataReader reader = null;

            Log.Info($"WriteFromSqlQuery to worksheet [{sheetName}] - started...");
            Log.Debug($"Query: {sqlQuery}");

            try
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    reader = command.ExecuteReader();
                    ok = Write(xwr, reader, sheetName, out recordsWritten, out errorMessage);
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                recordsWritten = 0;
                Log.Error(errorMessage);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            Log.Info($"Records written: {recordsWritten}");
            Log.Info($"WriteFromSqlQuery to worksheet [{sheetName}] - ended. Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        public static bool WriteToSingleWorkSheetUsingSproc(string dbConnectionString, string xlsxFileName, string sprocName, int scenId, string sheetName, out long recordsWritten, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            recordsWritten = 0;

            Log.Info($"WriteToSingleWorkSheetUsingSProc ([{xlsxFileName}],[{sheetName}],[{sprocName}],{scenId}) - started...");

            try
            {
                using (ExcelDataWriter xwr = ExcelDataWriter.Create(xlsxFileName, new ExcelDataWriterOptions { TruncateStrings = false }))
                {
                    using (SqlConnection connection = new SqlConnection(dbConnectionString))
                    {
                        connection.Open();
                        ok = WriteFromStoredProcedure(xwr, connection, sprocName, scenId, sheetName, out recordsWritten, out errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                recordsWritten = 0;
                Log.Error(errorMessage);
            }

            Log.Info($"WriteToSingleWorkSheetUsingSProc ([{xlsxFileName}],[{sheetName}],[{sprocName}],{scenId}) - ended. OK={ok}, ErrorMessage: '{errorMessage}'");
            return (ok);
        }

        public static bool WriteToSingleWorkSheet(string dbConnectionString, string xlsxFileName, string sqlQuery, string sheetName, out long recordsWritten, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            recordsWritten = 0;

            Log.Info($"WriteToSingleWorkSheet {xlsxFileName}[{sheetName}] - started...");
            Log.Debug($"Query: {sqlQuery}");

            try
            {
                using (ExcelDataWriter xwr = ExcelDataWriter.Create(xlsxFileName, new ExcelDataWriterOptions { TruncateStrings = false }))
                {
                    using (SqlConnection connection = new SqlConnection(dbConnectionString))
                    {
                        connection.Open();
                        ok = WriteFromSqlQuery(xwr, connection, sqlQuery, sheetName, out recordsWritten, out errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                recordsWritten = 0;
                Log.Error(errorMessage);
            }

            Log.Info($"WriteToSingleWorkSheet {xlsxFileName}[{sheetName}] - ended. OK={ok}, ErrorMessage: '{errorMessage}'");
            return (ok);
        }

        public static bool WriteToMultipleWorkSheets(string dbConnectionString, string xlsxFileName, List<string> sqlToSheetNameList, out long recordsWritten, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            recordsWritten = 0;

            Log.Info($"WriteToMultipleWorkSheets {xlsxFileName} - started...");

            try
            {
                if (sqlToSheetNameList == null || sqlToSheetNameList.Count < 1)
                {
                    throw new Exception("The query/worksheet specification List<string> object is null or empty.");
                }

                using (ExcelDataWriter xwr = ExcelDataWriter.Create(xlsxFileName, new ExcelDataWriterOptions { TruncateStrings = false }))
                {
                    using (SqlConnection connection = new SqlConnection(dbConnectionString))
                    {
                        connection.Open();
                        foreach (string entry in sqlToSheetNameList)
                        {
                            string[] pair = entry.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries);
                            if (pair.Length < 2)
                            {
                                throw new Exception($"The '>>' separator was not found in the list entry [{entry}].");
                            }
                            ok = WriteFromSqlQuery(xwr, connection, pair[0], pair[1], out long lRec, out errorMessage);
                            if (!ok)
                            {
                                break;
                            }
                            recordsWritten += lRec;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                recordsWritten = 0;
            }

            Log.Info($"WriteToMultipleWorkSheets {xlsxFileName} - ended. OK={ok}, ErrorMessage='{errorMessage}'.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        #region IMPORT_FROM_EXCEL

        private static void Generic_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Log.Info("T-SQL messages:\n" + e.Message);
        }

        private const string LibraryCargoDataTable = "tbl_lib_CargoData";
        private const string LibraryCargoAttributesTable = "tbl_lib_CargoAttributes";
        private const string ImportCargoDataTable = "tbl_pb_CargoData";
        private const string ImportCargoAttributesTable = "tbl_pb_CargoAttributes";
        private const int BulkInsertTimeoutSeconds = 48000;
        private const int BulkInsertBatchSize = 10000;


        /// <summary>
        /// Imports masintainsable asset segmentation into the staging _import_ table
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="excelFilePath">Full path name of the Excel file.</param>
        /// <param name="worksheetName">Name of the worksheet, like "PAMS_MAS". May be left as null if it is the first worksheet.</param>
        /// <param name="forgiving">True if it is okay for the inputto contain extra columns, false - if not.</param>
        /// <param name="guidSession">(out) Guid of the record created in tbl_pb_ImportSessions</param>
        /// <param name="errorMessage">(out) error message, null if no errors.</param>
        /// <returns>True on success, false on error.</returns>
        private static bool ImportMASFromExcel(string dbConnectionString,
            string excelFilePath,
            string worksheetName,
            bool forgiving,
            out Guid? guidSession,
            out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            List<string> fieldsNotExpectedToBeInExcel = new List<string>()
            {
                "IMPORTSESSIONID",
                "POPULATEDBY",
                "POPULATEDAT"
            };

            Log.Info($"ImportMASFromExcel ([{excelFilePath}]) - started...");

            DataTable dtMAS = null;
            Dictionary<string, string> InputColumnDirection = new Dictionary<string, string>();
            bool bWarnings = false;
            int N = 0;
            guidSession = null;

            try
            {
                ok = Read(excelFilePath, out DataSet ds, out errorMessage);

                if (!ok)
                {
                    throw new Exception(errorMessage);
                }

                if (ds.Tables.Count > 1 && !string.IsNullOrEmpty(worksheetName))
                {
                    if (ds.Tables.IndexOf(worksheetName) < 0)
                    {
                        Log.Warn("PAMS_MAS worksheet not found in the Excel document.  Using the first worksheet instead");
                    }
                    else
                    {
                        dtMAS = ds.Tables[worksheetName];
                    }
                }
                else
                {
                    dtMAS = ds.Tables[0];
                }

                Log.Info($"Worksheet PAMS_MAS contains {dtMAS.Rows.Count} data rows.");

                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_StartNewImportSession";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 48000;
                        command.Parameters.Add("@ImportSource", SqlDbType.VarChar, 10).Value = "MAS";
                        command.Parameters.Add("@DataSourceType", SqlDbType.VarChar, 10).Value = "Excel";
                        command.Parameters.Add("@DataSourceName", SqlDbType.VarChar, 255).Value = excelFilePath;
                        SqlParameter guidParm = command.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier);
                        guidParm.Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        guidSession = Guid.Parse(guidParm.Value.ToString());
                        if (!guidSession.HasValue)
                        {
                            throw new Exception("Import session could not be created.  Please check the log file for more detail.");
                        }
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "TRUNCATE TABLE tbl_import_MAS";
                        command.ExecuteNonQuery();
                        Log.Info("Table tbl_import_MAS truncated. ");
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "TRUNCATE TABLE tbl_pams_MaintainableAssetsSegmentation";
                        command.ExecuteNonQuery();
                        Log.Info("Table tbl_pams_MaintainableAssetsSegmentation truncated. ");
                    }

                    using (BulkInserter biMas = new BulkInserter())
                    {
                        Log.Info("Importing Maintainable Asset Segmentation");

                        ok = biMas.Configure(conn, "tbl_import_MAS", "MAS", null, null, out errorMessage);

                        // Verify integrity of the input
                        for (int i = 0; ok && i < dtMAS.Columns.Count; i++)
                        {
                            string inputColName = dtMAS.Columns[i].ColumnName;
                            if (InputColumnDirection.ContainsKey(inputColName.ToUpper()))
                            {
                                throw new Exception($"Column [{inputColName}] occurs more than once in the Excel worksheet.");
                            }

                            if (biMas.DTable.Columns.Contains(inputColName))
                            {

                                InputColumnDirection.Add(inputColName.ToUpper(), "T");    // goes to the target table
                                continue;
                            }
                            else
                            {
                                if (inputColName.ToUpper().StartsWith("COLUMN"))    // Just a noname column, skip it.
                                {
                                    InputColumnDirection.Add(inputColName.ToUpper(), "S");    // skip
                                    continue;
                                }

                                ok = false;
                                errorMessage = $"Column [{inputColName}] does not belong to the target database table. Neither it is being listed among the cargo attributes.";
                                Log.Error(errorMessage);

                                using (SqlCommand cmd = new SqlCommand())
                                {
                                    cmd.Connection = conn;

                                    if (!forgiving)
                                    {
                                        cmd.CommandText = @"UPDATE tbl_pb_ImportSessions 
SET CompletedAt=CURRENT_TIMESTAMP, CompletedStatus=-1, Notes=@Notes WHERE Id=@Guid";
                                        cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = errorMessage;
                                        cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        cmd.CommandText = @"UPDATE tbl_pb_ImportSessions 
SET CompletedAt=CURRENT_TIMESTAMP, CompletedStatus=2, Notes=ISNULL(Notes,'') + '| ' + @Notes WHERE Id=@Guid";
                                        cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = errorMessage;
                                        cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                                        cmd.ExecuteNonQuery();
                                        bWarnings = true;
                                    }
                                }

                                if (forgiving)
                                {
                                    ok = true;
                                    errorMessage = null;
                                    InputColumnDirection.Add(inputColName.ToUpper(), "S");    // skip
                                }
                            }
                        }


                        if (ok)
                        {
                            for (int i = 0; ok && i < dtMAS.Rows.Count; i++)
                            {
                                DataRow dr = dtMAS.Rows[i];
                                DataRow r = biMas.NewRow();

                                Guid importTimeGeneratedGuid = Guid.NewGuid();

                                r["ImportSessionId"] = guidSession;

                                for (int j = 0; ok && j < dtMAS.Columns.Count; j++)
                                {
                                    string colName = dtMAS.Columns[j].ColumnName;

                                    if (InputColumnDirection.ContainsKey(colName.ToUpper()) && InputColumnDirection[colName.ToUpper()] == "T")
                                    {
                                        string typeName = biMas.DTable.Columns[colName].DataType.Name.ToLower().Substring(0, 3);

                                        if (biMas.DTable.Columns[colName.ToUpper()].DataType.Name.ToLower() == "int64")
                                        {
                                            typeName = "big";
                                        }

                                        ok = AssignColumnValue(ref r, colName, typeName, dr[j], dr[j].ToString(), out errorMessage);
                                        if (!ok)
                                        {
                                            errorMessage += $"\nwhen processing column {j} of row {i} (both indexes are zero-based.";
                                            throw new Exception(errorMessage);
                                        }
                                    }
                                    else
                                    {
                                        continue;   // do nothing
                                    }
                                }

                                if (ok)
                                {
                                    ok = biMas.AddRow(r, out errorMessage);
                                    N++;
                                }
                                if (!ok)
                                {
                                    throw new Exception(errorMessage);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }
            finally
            {
                if (guidSession.HasValue)
                {
                    using (SqlConnection conn = new SqlConnection(dbConnectionString))
                    {
                        conn.Open();
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                        if (!ok)
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = $"DELETE FROM tbl_import_MAS WHERE ImportSessionId=@SessionId";
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandTimeout = 48000;
                                cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                                cmd.ExecuteNonQuery();
                            }

                        }

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = conn;

                            if (ok && bWarnings)
                            {
                                cmd.CommandText = @"UPDATE tbl_pb_ImportSessions 
SET CompletedAt=CURRENT_TIMESTAMP,  Notes='Excel import completed with warnings: ' + ISNULL(Notes,'') WHERE Id=@Guid";
                            }
                            else
                            {
                                cmd.CommandText = @"UPDATE tbl_pb_ImportSessions 
SET CompletedAt=CURRENT_TIMESTAMP,  CompletedStatus=@Status, Notes=@Notes WHERE Id=@Guid";
                                cmd.Parameters.Add("@Status", SqlDbType.Int).Value = ok ? 1 : -1;
                                cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = ok ? "Excel import completed successfully." : errorMessage;
                            }
                            cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            Log.Info($"Number of imported maintainable asset segmentation records: {N}");
            Log.Info($"ImportMASFromExcel ([{excelFilePath}]) -finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        /// <summary>
        /// Imports masintainsable asset segmentation into the staging _import_ table
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="excelFilePath">Full path name of the Excel file.</param>
        /// <param name="worksheetName">Name of the worksheet, like "PAMS_MAS". May be left as null if it is the first worksheet.</param>
        /// <param name="forgiving">True if it is okay for the inputto contain extra columns, false - if not.</param>
        /// <param name="errorMessage">(out) error message, null if no errors.</param>
        /// <returns>True on success, false on error.</returns>
        public static bool ImportMaintainableAssetSegmentation(string dbConnectionString,
                        string excelFilePath, string worksheetName, bool forgiving,
                        out string errorMessage)
        {
            errorMessage = null;

            bool ok = ImportMASFromExcel(dbConnectionString, excelFilePath, worksheetName,
                            forgiving, out Guid? guidSession, out errorMessage);

            if (!ok)
            {
                return ok;
            }

            Log.Info($"ImportMaintainableAssetSegmentation ([{excelFilePath}]) - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.sp_pb_MoveMAS";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 12000;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"ImportMaintainableAssetSegmentation ([{excelFilePath}]) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        // Skips the rename when the target name already exists so the duplicate-column check is not tripped.
        private static void RenameTreatmentColumnIfFree(DataTable dt, string from, string to)
        {
            if (dt.Columns.Contains(from) && !dt.Columns.Contains(to))
            {
                dt.Columns[from].ColumnName = to;
            }
        }

        private static bool ImportTreatmentsFromExcelWithCargo(string dbConnectionString, string src,
                string assetType,
                string excelFilePath,
                string excelTabName,
                bool forgiving,
                out Guid? guidSession,
                out string errorMessage,
                Guid? targetLibraryId,
                out List<string> superAssets)
        {
            bool ok = true;
            errorMessage = null;
            bool bWarnings = false;
            guidSession = null;
            int N = 0;
            superAssets = null;

            if (targetLibraryId != null)
            {
                forgiving = true;
            }

            List<string> fieldsNotExpectedToBeInExcel = new List<string>()
            {
                "IMPORTSESSIONID",
                "IMPORTTIMEGENERATEDID",
                "ASSETTYPE",
                "POPULATEDBY",
                "POPULATEDAT"
            };

            Dictionary<string, int> CargoAttributes = new Dictionary<string, int>();
            Dictionary<string, string> InputColumnDirection = new Dictionary<string, string>();

            Log.InfoFormat("ImportTreatmentsFromExcelWithCargo ([{0}],{1},{2}]) - started...", SafeForLog(excelFilePath), SafeForLog(src), SafeForLog(assetType));

            string targetTable = assetType == "B" ? "tbl_import_BAMS_Treatments" : "tbl_import_PAMS_Treatments";

            try
            {
                DataTable dtTreatments = null;

                ok = Read(excelFilePath, out DataSet ds, out errorMessage);

                if (!ok)
                {
                    throw new Exception(errorMessage);
                }

                if (string.IsNullOrEmpty(excelTabName))
                {
                    dtTreatments = ds.Tables[0];
                }
                else if (ds.Tables.Contains(excelTabName))
                {
                    dtTreatments = ds.Tables[excelTabName];
                }
                else
                {
                    throw new Exception($"Tab [{excelTabName}] not found in the file {excelFilePath}");
                }

                // Export-report headers AssetName/County map to staging columns Asset/Cnty.
                RenameTreatmentColumnIfFree(dtTreatments, "AssetName", "Asset");
                RenameTreatmentColumnIfFree(dtTreatments, "County", "Cnty");

                if (assetType == "P" && dtTreatments.Columns.Contains("CRS"))
                {
                    superAssets = new List<string>();
                }

                Log.Info($"Treatments worksheet contains {dtTreatments.Rows.Count} data rows.");

                string yearColumnName = null;
                foreach (DataColumn col in dtTreatments.Columns)
                {
                    if (string.Equals(col.ColumnName, "YEAR", StringComparison.OrdinalIgnoreCase))
                    {
                        yearColumnName = col.ColumnName;
                        break;
                    }
                }

                if (yearColumnName == null)
                {
                    throw new Exception("Import failed: the worksheet does not contain a 'Year' column. Please check the Excel file and try again.");
                }

                for (int i = 0; i < dtTreatments.Rows.Count; i++)
                {
                    var yearValue = dtTreatments.Rows[i][yearColumnName];
                    if (yearValue == null || yearValue == DBNull.Value || string.IsNullOrWhiteSpace(yearValue.ToString()))
                    {
                        int excelRowNumber = i + 2;
                        string sheetName = dtTreatments.TableName ?? excelTabName ?? "Sheet1";
                        throw new Exception($"Import failed: 'Year' is required but is missing in row {excelRowNumber} of sheet '{sheetName}'. Please fill in all Year values and re-import.");
                    }
                }

                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_StartNewImportSession";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 48000;
                        command.Parameters.Add("@ImportSource", SqlDbType.VarChar, 10).Value = src;
                        command.Parameters.Add("@DataSourceType", SqlDbType.VarChar, 10).Value = "Excel";
                        command.Parameters.Add("@DataSourceName", SqlDbType.VarChar, 255).Value = excelFilePath;
                        SqlParameter guidParm = command.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier);
                        guidParm.Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        guidSession = Guid.Parse(guidParm.Value.ToString());
                        if (!guidSession.HasValue)
                        {
                            throw new Exception("Import session could not be created.  Please check the log file for more detail.");
                        }
                    }

                    // Cargo (Category/Treatment Type) cleanup for rows the import actually replaces is now
                    // handled by sp_pb_Move{BAMS|PAMS}TreatmentsToLibraryTreatments, scoped to the same
                    // treatment rows it deletes -- wiping the whole library/asset-type here blanked
                    // Category on pre-existing rows that "Keep All"/"Keep User Created" leave untouched.

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = $@"
SELECT AttributeName, AttributeNo
FROM {ImportCargoAttributesTable} WITH (NOLOCK)
WHERE AssetType = @AssetType
ORDER BY AttributeNo";
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@AssetType", assetType);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CargoAttributes.Add(reader[0].ToString().ToUpper(), Convert.ToInt32(reader[1]));
                            }
                            reader.Close();
                        }
                    }

                    using (BulkInserter biTreat = new BulkInserter())
                    {
                        Log.Info("Importing treatments");

                        ok = biTreat.Configure(conn, targetTable, "Treatments", BulkInsertBatchSize, BulkInsertTimeoutSeconds, out errorMessage);
                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }

                        for (int i = 0; ok && i < dtTreatments.Columns.Count; i++)
                        {
                            string inputColName = dtTreatments.Columns[i].ColumnName;
                            if (InputColumnDirection.ContainsKey(inputColName.ToUpper()))
                            {
                                throw new Exception($"Column [{inputColName}] occurs more than once in the Excel worksheet.");
                            }

                            if (biTreat.DTable.Columns.Contains(inputColName))
                            {
                                InputColumnDirection.Add(inputColName.ToUpper(), "T");
                                continue;
                            }

                            if (CargoAttributes.ContainsKey(inputColName.ToUpper()))
                            {
                                InputColumnDirection.Add(inputColName.ToUpper(), "C");
                                continue;
                            }

                            if (inputColName.ToUpper().StartsWith("COLUMN"))
                            {
                                InputColumnDirection.Add(inputColName.ToUpper(), "S");
                                continue;
                            }

                            ok = false;
                            errorMessage = $"Column [{inputColName}] does not belong to the target database table. Neither it is being listed among the cargo attributes.";
                            Log.Error(errorMessage);

                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conn;

                                if (!forgiving)
                                {
                                    cmd.CommandText = @"UPDATE tbl_pb_ImportSessions 
SET CompletedAt=CURRENT_TIMESTAMP, CompletedStatus=-1, Notes=@Notes WHERE Id=@Guid";
                                    cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = errorMessage;
                                    cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                                    cmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    cmd.CommandText = @"UPDATE tbl_pb_ImportSessions 
SET CompletedAt=CURRENT_TIMESTAMP, CompletedStatus=2, Notes=ISNULL(Notes,'') + '| ' + @Notes WHERE Id=@Guid";
                                    cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = errorMessage;
                                    cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                                    cmd.ExecuteNonQuery();
                                    bWarnings = true;
                                }
                            }

                            if (forgiving)
                            {
                                ok = true;
                                errorMessage = null;
                                InputColumnDirection.Add(inputColName.ToUpper(), "S");
                            }
                        }

                        for (int i = 0; ok && i < biTreat.DTable.Columns.Count; i++)
                        {
                            string colName = biTreat.DTable.Columns[i].ColumnName;

                            if (fieldsNotExpectedToBeInExcel.Contains(colName.ToUpper()))
                            {
                                continue;
                            }

                            if (!InputColumnDirection.ContainsKey(colName.ToUpper()))
                            {
                                throw new Exception($"The target database table {targetTable} expects column but the file is empty.");
                            }
                        }

                        if (ok)
                        {
                            using (BulkInserter biLibCargo = new BulkInserter())
                            using (BulkInserter biCargo = new BulkInserter())
                            {
                                bool importToLibrary = targetLibraryId != null;

                                if (importToLibrary)
                                {
                                    ok = biLibCargo.Configure(conn, LibraryCargoDataTable, "dtCargoData", BulkInsertBatchSize, BulkInsertTimeoutSeconds, out errorMessage);
                                    if (!ok)
                                    {
                                        throw new Exception(errorMessage);
                                    }
                                }
                                else
                                {
                                    ok = biCargo.Configure(conn, ImportCargoDataTable, "Cargo", BulkInsertBatchSize, BulkInsertTimeoutSeconds, out errorMessage);
                                    if (!ok)
                                    {
                                        throw new Exception(errorMessage);
                                    }
                                }

                                Dictionary<string, int> cargoColumns = new Dictionary<string, int>();

                                if (importToLibrary)
                                {
                                    // Attribute numbers must stay stable across imports: existing Cargo rows for
                                    // treatments that "Keep All"/"Keep User Created" leave untouched still point at
                                    // these numbers, so re-numbering from scratch on every import would silently
                                    // repoint them at the wrong attribute name.
                                    using (SqlCommand existingAttrCommand = conn.CreateCommand())
                                    {
                                        existingAttrCommand.CommandText = $@"
SELECT AttributeName, AttributeNo
FROM {LibraryCargoAttributesTable}
WHERE LibraryId = @LibraryId AND AssetType = @AssetType";
                                        existingAttrCommand.Parameters.Add("@LibraryId", SqlDbType.UniqueIdentifier).Value = targetLibraryId.Value;
                                        existingAttrCommand.Parameters.Add("@AssetType", SqlDbType.VarChar, 50).Value = assetType.Substring(0, 1).ToUpper();
                                        using (SqlDataReader reader = existingAttrCommand.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                cargoColumns[reader.GetString(0)] = Convert.ToInt32(reader[1]);
                                            }
                                        }
                                    }

                                    using (BulkInserter biLibCargoAttr = new BulkInserter())
                                    {
                                        ok = biLibCargoAttr.Configure(conn, LibraryCargoAttributesTable, "dtLibCargoAttr", null, BulkInsertTimeoutSeconds, out errorMessage);
                                        if (!ok)
                                        {
                                            throw new Exception(errorMessage);
                                        }

                                        int j = cargoColumns.Count == 0 ? 0 : cargoColumns.Values.Max();
                                        foreach (DataColumn column in dtTreatments.Columns)
                                        {
                                            if (column.ColumnName.ToUpper().EndsWith("ID"))
                                            {
                                                continue;
                                            }

                                            string attributeName = column.ColumnName.ToUpper();
                                            if (cargoColumns.ContainsKey(attributeName))
                                            {
                                                continue;
                                            }

                                            cargoColumns.Add(attributeName, ++j);

                                            DataRow attrRow = biLibCargoAttr.NewRow();
                                            attrRow["LibraryId"] = targetLibraryId.Value;
                                            attrRow["AssetType"] = assetType.Substring(0, 1).ToUpper();
                                            attrRow["AttributeNo"] = j;
                                            attrRow["AttributeName"] = attributeName;
                                            attrRow["AttributeType"] = column.DataType.Name.Length <= 10
                                                ? column.DataType.Name
                                                : column.DataType.Name.Substring(0, 10);
                                            ok = biLibCargoAttr.AddRow(attrRow, out errorMessage);
                                            if (!ok)
                                            {
                                                throw new Exception(errorMessage);
                                            }
                                        }
                                    }
                                }

                                for (int i = 0; ok && i < dtTreatments.Rows.Count; i++)
                                {
                                    DataRow dr = dtTreatments.Rows[i];
                                    DataRow r = biTreat.NewRow();

                                    Guid importTimeGeneratedGuid = Guid.NewGuid();

                                    r["ImportSessionId"] = guidSession;
                                    r["ImportTimeGeneratedId"] = importTimeGeneratedGuid;
                                    r["AssetType"] = assetType;

                                    for (int j = 0; j < dtTreatments.Columns.Count; j++)
                                    {
                                        string colName = dtTreatments.Columns[j].ColumnName;

                                        if (superAssets != null && colName.ToUpper() == "CRS")
                                        {
                                            string crs = dr[j].ToString();
                                            if (!superAssets.Contains(crs))
                                            {
                                                superAssets.Add(crs);
                                            }
                                        }

                                        if (importToLibrary && cargoColumns.ContainsKey(colName.ToUpper()))
                                        {
                                            int attrNo = cargoColumns[colName.ToUpper()];
                                            string cargoVal = dr[j].ToString();
                                            DataRow libcr = biLibCargo.NewRow();
                                            libcr["ImportSessionId"] = guidSession.Value;
                                            libcr["LibraryId"] = targetLibraryId;
                                            libcr["AssetType"] = assetType.ToUpper()[0];
                                            libcr["ImportTimeGeneratedGuid"] = importTimeGeneratedGuid;
                                            libcr["AttributeNo"] = attrNo;
                                            libcr["TextValue"] = cargoVal.Length <= 255 ? cargoVal : cargoVal.Substring(0, 255);
                                            if (double.TryParse(cargoVal, out double val))
                                            {
                                                libcr["NumericValue"] = val;
                                            }
                                            ok = biLibCargo.AddRow(libcr, out errorMessage);
                                            if (!ok)
                                            {
                                                throw new Exception(errorMessage);
                                            }
                                        }
                                        else if (!importToLibrary
                                            && InputColumnDirection.ContainsKey(colName.ToUpper())
                                            && InputColumnDirection[colName.ToUpper()] == "C"
                                            && CargoAttributes.ContainsKey(colName.ToUpper()))
                                        {
                                            DataRow cargoRow = biCargo.NewRow();
                                            cargoRow["ImportSessionId"] = guidSession.Value;
                                            cargoRow["ImportTimeGeneratedGuid"] = importTimeGeneratedGuid;
                                            cargoRow["AttributeNo"] = CargoAttributes[colName.ToUpper()];
                                            string val = dr[colName].ToString();
                                            cargoRow["TextValue"] = val;
                                            if (double.TryParse(val, out double d))
                                            {
                                                cargoRow["NumericValue"] = d;
                                            }
                                            ok = biCargo.AddRow(cargoRow, out errorMessage);
                                            if (!ok)
                                            {
                                                throw new Exception(errorMessage);
                                            }
                                        }

                                        if (InputColumnDirection.ContainsKey(colName.ToUpper()) && InputColumnDirection[colName.ToUpper()] == "T")
                                        {
                                            string typeName = biTreat.DTable.Columns[colName].DataType.Name.ToLower().Substring(0, 3);

                                            if (biTreat.DTable.Columns[colName.ToUpper()].DataType.Name.ToLower() == "int64")
                                            {
                                                typeName = "big";
                                            }

                                            ok = AssignColumnValue(ref r, colName, typeName, dr[j], dr[j].ToString(), out errorMessage);
                                            if (!ok)
                                            {
                                                errorMessage += $"\nwhen processing row {i} column {j} (both indexes are zero-based).";
                                                throw new Exception(errorMessage);
                                            }
                                        }
                                    }

                                    if (ok)
                                    {
                                        ok = biTreat.AddRow(r, out errorMessage);
                                        N++;
                                    }
                                    if (!ok)
                                    {
                                        throw new Exception(errorMessage);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }
            finally
            {
                if (guidSession.HasValue)
                {
                    using (SqlConnection conn = new SqlConnection(dbConnectionString))
                    {
                        conn.Open();
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                        if (!ok)
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = $"DELETE FROM {targetTable} WHERE ImportSessionId=@SessionId";
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandTimeout = 48000;
                                cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = $"DELETE FROM {ImportCargoDataTable} WHERE ImportSessionId=@SessionId";
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandTimeout = 48000;
                                cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = conn;

                            if (ok && bWarnings)
                            {
                                cmd.CommandText = @"UPDATE tbl_pb_ImportSessions 
SET CompletedAt=CURRENT_TIMESTAMP,  Notes='Excel import completed with warnings: ' + ISNULL(Notes,'') WHERE Id=@Guid";
                            }
                            else
                            {
                                cmd.CommandText = @"UPDATE tbl_pb_ImportSessions 
SET CompletedAt=CURRENT_TIMESTAMP,  CompletedStatus=@Status, Notes=@Notes WHERE Id=@Guid";
                                cmd.Parameters.Add("@Status", SqlDbType.Int).Value = ok ? 1 : -1;
                                cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = ok ? "Excel import completed successfully." : errorMessage;
                            }
                            cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            Log.InfoFormat("Records loaded from {0}: {1}", SafeForLog(excelFilePath), N);
            Log.InfoFormat("ImportTreatmentsFromExcelWithCargo ([{0}],{1},{2}) - finished.  Ok={3}, ErrorMessage: '{4}'", SafeForLog(excelFilePath), SafeForLog(src), SafeForLog(assetType), ok, SafeForLog(errorMessage));

            return ok;
        }

        /// <summary>
        /// Validates an uploaded treatment Excel file before import: checks that the expected source-specific
        /// columns are present and that a 'Year' column exists with a non-empty value in every data row.
        /// No database operations are performed.
        /// </summary>
        public static bool ValidateTreatmentImportFile(string excelFilePath, string excelTabName, string importType, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                if (!TryGetTreatmentWorksheet(excelFilePath, excelTabName, out DataTable dt, out string sheetName, out errorMessage))
                    return false;

                if (!ValidateTreatmentSourceSignature(dt, importType, out errorMessage))
                    return false;

                return ValidateTreatmentYearColumn(dt, sheetName, out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = $"Could not validate the uploaded file: {ex.Message}";
                return false;
            }
        }

        private static bool TryGetTreatmentWorksheet(string excelFilePath, string excelTabName, out DataTable dt, out string sheetName, out string errorMessage)
        {
            dt = null;
            sheetName = excelTabName ?? "Sheet1";
            errorMessage = null;

            bool ok = Read(excelFilePath, out DataSet ds, out errorMessage);
            if (!ok)
                return false;

            if (ds == null || ds.Tables.Count == 0)
            {
                errorMessage = "The uploaded file could not be read as a valid Excel file, or it contains no worksheets. Please check the file and try again.";
                return false;
            }

            if (string.IsNullOrEmpty(excelTabName))
            {
                dt = ds.Tables[0];
            }
            else if (ds.Tables.Contains(excelTabName))
            {
                dt = ds.Tables[excelTabName];
            }
            else
            {
                errorMessage = $"Tab '{excelTabName}' was not found in the uploaded file. Please check the Tab Name field and try again.";
                return false;
            }

            sheetName = dt.TableName ?? excelTabName ?? "Sheet1";
            return true;
        }

        private static bool ValidateTreatmentSourceSignature(DataTable dt, string importType, out string errorMessage)
        {
            errorMessage = null;

            bool hasBridgeSignature = HasAnyColumn(dt, "BRKEY", "BRKEY_", "BMSID");
            bool hasPavementSignature = HasAnyColumn(dt, "CRS");

            switch ((importType ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "B":
                    if (!hasBridgeSignature)
                    {
                        errorMessage = hasPavementSignature
                            ? "The uploaded file looks like a PAMS treatment file because it contains a 'CRS' column but no bridge identifier columns. Select PAMS or upload a BAMS treatment file with 'BRKEY', 'BRKEY_', or 'BMSID'."
                            : "The uploaded file does not look like a BAMS treatment file. BAMS imports require a bridge identifier column such as 'BRKEY', 'BRKEY_', or 'BMSID'.";
                        return false;
                    }
                    break;
                case "P":
                    if (!hasPavementSignature)
                    {
                        errorMessage = hasBridgeSignature
                            ? "The uploaded file looks like a BAMS treatment file because it contains bridge identifier columns but no 'CRS' column. Select BAMS or upload a PAMS treatment file with 'CRS'."
                            : "The uploaded file does not look like a PAMS treatment file. PAMS imports require a 'CRS' column.";
                        return false;
                    }
                    break;
            }

            return true;
        }

        private static bool ValidateTreatmentYearColumn(DataTable dt, string sheetName, out string errorMessage)
        {
            errorMessage = null;

            string yearColumnName = null;
            foreach (DataColumn col in dt.Columns)
            {
                if (string.Equals(col.ColumnName, "YEAR", StringComparison.OrdinalIgnoreCase))
                {
                    yearColumnName = col.ColumnName;
                    break;
                }
            }

            if (yearColumnName == null)
            {
                errorMessage = "The uploaded file does not contain a 'Year' column. Please check the file and try again.";
                return false;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var val = dt.Rows[i][yearColumnName];
                if (val == null || val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString()))
                {
                    errorMessage = $"'Year' is required in every row. Row {i + 2} of sheet '{sheetName}' is missing a Year value. Please fill it in and re-import.";
                    return false;
                }
            }

            return true;
        }

        private static bool HasAnyColumn(DataTable dt, params string[] columnNames)
        {
            foreach (DataColumn column in dt.Columns)
            {
                foreach (string columnName in columnNames)
                {
                    if (string.Equals(column.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Primary procedure for treatments' import.  It first loads data from Excel to the staging table
        /// and then moves it to tbl_pb_ImportedTreatments. 
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="src">'BAMS' or 'PAMS', eberything else results in error</param>
        /// <param name="assetType">'B' or 'P', everything else results in error</param>
        /// <param name="excelFilePath">Pathname of the source Excel file.  File may not be open in any other application at the time of the call.</param>
        /// <param name="excelTabName">Name of the Excel tab (worksheet) where treatment data is expected to be found, e.g. "PAMS_TREATMENTS". Leave it null if Excel contains just one sheet.</param>
        /// <param name="forgiving">Set to True if source may contain extra (unexpected) columns. Normally should be False;</param>
        /// <param name="targetLibraryId">GUID of the target library, leave it as null if import is to be done into tbl_pb_ImportedTreatments</param>
        /// <param name="errorMessage">Error message, null if no errors</param>
        /// <returns>True on success, False on failure</returns>
        public static bool ImportTreatments(string dbConnectionString, string src, string assetType, string excelFilePath, string excelTabName, bool forgiving,
                Guid? targetLibraryId,
                bool fromScratch,
                bool keepUserTreatments,
                out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            Guid? guidSession = null;

            Log.InfoFormat("ImportTreatments ({0},{1},[{2}],[{3},{4}]) - started...", SafeForLog(src), SafeForLog(assetType), SafeForLog(excelFilePath), SafeForLog(excelTabName), forgiving);

            ok = ImportTreatmentsFromExcelWithCargo(dbConnectionString,
                        src, assetType, excelFilePath, excelTabName, forgiving,
                        out guidSession, out errorMessage, targetLibraryId,
                        out List<string> superAssets);

            if (ok)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbConnectionString))
                    {
                        conn.Open();
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                        using (SqlCommand cmd = conn.CreateCommand())
                        {

                            if (targetLibraryId.HasValue)
                            {
                                cmd.CommandText = $"dbo.sp_pb_Move{src}TreatmentsToLibraryTreatments";
                            }
                            else
                            {
                                cmd.CommandText = $"dbo.sp_pb_Move{src}TreatmentsToImportedTreatments";
                            }

                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 48000;
                            cmd.Parameters.Add("@ImportSessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;

                            if (targetLibraryId.HasValue)
                            {
                                cmd.Parameters.Add("@TargetLibraryId", SqlDbType.UniqueIdentifier).Value = targetLibraryId.Value;
                                cmd.Parameters.Add("@FromScratch", SqlDbType.Bit).Value = fromScratch ? 1 : 0;
                                cmd.Parameters.Add("@KeepUserTreatments", SqlDbType.Bit).Value = keepUserTreatments ? 1 : 0;
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ok = false;
                    errorMessage = ex.Message;
                    Log.Error(errorMessage);
                }
            }

            Log.InfoFormat("ImportTreatments ({0},{1},[{2}],[{3},{4}]) - finished. Ok={5},  ErrorMessage: '{6}", SafeForLog(src), SafeForLog(assetType), SafeForLog(excelFilePath), SafeForLog(excelTabName), forgiving, ok, SafeForLog(errorMessage));

            return ok;
        }

        private static readonly string[] B2PCountyCodeAliases = new[] { "County ID", "County_Code", "CountyCode", "CNTY" };
        private static readonly string[] B2PRouteAliases = new[] { "Route", "ROUTE_TXT", "SR" };
        private static readonly string[] B2PSegmentAliases = new[] { "Segment", "SEGMENT_TXT", "sSeg" };
        private static readonly string[] B2POffsetAliases = new[] { "Offset", "OFFSET" };

        private static readonly Dictionary<string, string[]> B2PColumnAliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["BRKEY"] = new[] { "BRKEY", "BRKEY_", "BMSID" },
            ["DISTRICT"] = new[] { "DISTRICT" },
            ["COUNTY"] = new[] { "COUNTY", "County" },
            ["BRIDGE_ID"] = new[] { "BRIDGE_ID", "BridgeId", "Bridge_ID" },
            ["County ID"] = B2PCountyCodeAliases,
            ["County_Code"] = B2PCountyCodeAliases,
            ["Route"] = B2PRouteAliases,
            ["Segment"] = B2PSegmentAliases,
            ["Offset"] = B2POffsetAliases,
        };

        private static string? FindExpectedColumn(IList<string> expectedColumns, string[] aliases)
        {
            foreach (string alias in aliases)
            {
                string? match = expectedColumns.FirstOrDefault(c =>
                    string.Equals(c, alias, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static string[] GetB2PColumnAliases(string destCol)
        {
            if (B2PColumnAliases.TryGetValue(destCol, out string[] mappedAliases))
            {
                return mappedAliases;
            }

            return new[] { destCol };
        }

        private static DataTable? FindWorksheetByName(DataSet ds, string worksheetName)
        {
            foreach (DataTable table in ds.Tables.Cast<DataTable>())
            {
                if (string.Equals(table.TableName, worksheetName, StringComparison.OrdinalIgnoreCase))
                {
                    return table;
                }
            }

            return null;
        }

        private static DataTable SelectB2PWorksheet(DataSet ds, string? worksheetName)
        {
            if (!string.IsNullOrWhiteSpace(worksheetName))
            {
                DataTable? named = FindWorksheetByName(ds, worksheetName);
                if (named == null)
                {
                    var allSheets = string.Join(", ", ds.Tables.Cast<DataTable>().Select(t => t.TableName));
                    throw new Exception($"Tab '{worksheetName}' was not found in the uploaded file. Available worksheets: {allSheets}.");
                }

                Log.Info($"B2P import: using worksheet '{named.TableName}' from Tab Name.");
                return named;
            }

            DataTable? bridgeSheet = FindWorksheetByName(ds, "Bridge");
            if (bridgeSheet != null)
            {
                Log.Info($"B2P import: using worksheet '{bridgeSheet.TableName}' because no Tab Name was provided.");
                return bridgeSheet;
            }

            if (HasAnyColumn(ds.Tables[0], "BRKEY", "BRKEY_", "BMSID"))
            {
                return ds.Tables[0];
            }

            foreach (DataTable table in ds.Tables.Cast<DataTable>())
            {
                if (HasAnyColumn(table, "BRKEY", "BRKEY_", "BMSID"))
                {
                    Log.Info($"B2P import: using worksheet '{table.TableName}' (contains BRKEY) instead of first worksheet '{ds.Tables[0].TableName}'.");
                    return table;
                }
            }

            return ds.Tables[0];
        }

        private static object GetB2PSourceValue(DataRow row, DataTable source, params string[] columnNames)
        {
            foreach (string name in columnNames)
            {
                foreach (DataColumn col in source.Columns)
                {
                    if (!string.Equals(col.ColumnName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    object val = row[col];
                    if (val != null && val != DBNull.Value && !string.IsNullOrWhiteSpace(val.ToString()))
                    {
                        return val;
                    }
                }
            }

            return DBNull.Value;
        }

        private static bool TryParseBridgeIdFields(string bridgeId, out int countyCode, out int route, out int segment, out int offset)
        {
            countyCode = route = segment = offset = 0;
            if (string.IsNullOrWhiteSpace(bridgeId))
            {
                return false;
            }

            string digits = new string(bridgeId.Where(char.IsDigit).ToArray());
            if (digits.Length < 14)
            {
                return false;
            }

            if (digits.Length > 14)
            {
                digits = digits.Substring(0, 14);
            }

            countyCode = int.Parse(digits.Substring(0, 2));
            route = int.Parse(digits.Substring(2, 4));
            segment = int.Parse(digits.Substring(6, 4));
            offset = int.Parse(digits.Substring(10, 4));
            return true;
        }

        private static bool IsMissingValue(object value)
        {
            return value == null
                || value == DBNull.Value
                || string.IsNullOrWhiteSpace(value.ToString());
        }

        private static DataTable NormalizeB2PWorksheet(DataTable source, IList<string> expectedColumns)
        {
            var sourceNames = source.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            bool exactMatch = sourceNames.Count == expectedColumns.Count
                && !expectedColumns.Except(sourceNames, StringComparer.OrdinalIgnoreCase).Any()
                && !sourceNames.Except(expectedColumns, StringComparer.OrdinalIgnoreCase).Any();

            if (exactMatch)
            {
                return source;
            }

            var normalized = new DataTable(source.TableName);
            foreach (string col in expectedColumns)
            {
                normalized.Columns.Add(col);
            }

            foreach (DataRow srcRow in source.Rows)
            {
                DataRow destRow = normalized.NewRow();
                string bridgeId = null;

                foreach (string destCol in expectedColumns)
                {
                    object val = GetB2PSourceValue(srcRow, source, GetB2PColumnAliases(destCol));
                    if (!IsMissingValue(val))
                    {
                        destRow[destCol] = val;
                        if (destCol.Equals("BRIDGE_ID", StringComparison.OrdinalIgnoreCase))
                        {
                            bridgeId = val.ToString();
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(bridgeId)
                    && TryParseBridgeIdFields(bridgeId, out int county, out int route, out int segment, out int offset))
                {
                    string? countyCol = FindExpectedColumn(expectedColumns, B2PCountyCodeAliases);
                    string? routeCol = FindExpectedColumn(expectedColumns, B2PRouteAliases);
                    string? segmentCol = FindExpectedColumn(expectedColumns, B2PSegmentAliases);
                    string? offsetCol = FindExpectedColumn(expectedColumns, B2POffsetAliases);

                    if (countyCol != null && IsMissingValue(destRow[countyCol]))
                    {
                        destRow[countyCol] = county;
                    }

                    if (routeCol != null && IsMissingValue(destRow[routeCol]))
                    {
                        destRow[routeCol] = route;
                    }

                    if (segmentCol != null && IsMissingValue(destRow[segmentCol]))
                    {
                        destRow[segmentCol] = segment;
                    }

                    if (offsetCol != null && IsMissingValue(destRow[offsetCol]))
                    {
                        destRow[offsetCol] = offset;
                    }
                }

                normalized.Rows.Add(destRow);
            }

            Log.Info($"B2P import: normalized worksheet '{source.TableName}' to expected destination columns.");
            return normalized;
        }

        private static bool ImportB2PCrosswalkFromExcel(string dbConnectionString, string excelFilePath, string? worksheetName, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.InfoFormat("ImportB2PCrosswalkFromExcel ExcelFile: [{0}].[{1}] - started...", SafeForLog(excelFilePath), SafeForLog(worksheetName));

            try
            {
                ok = Read(excelFilePath, out DataSet ds, out errorMessage);

                if (!ok)
                {
                    throw new Exception(errorMessage);
                }

                if (ds.Tables.Count < 1)
                {
                    throw new Exception("First worksheet in the Excel document was not found.");
                }

                DataTable dtB2P = SelectB2PWorksheet(ds, worksheetName);
                Log.Info($"Worksheet '{dtB2P.TableName}' contains {dtB2P.Rows.Count} data rows.");

                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "TRUNCATE TABLE tbl_import_B2P";
                        command.ExecuteNonQuery();
                        Log.Info("TABLE tbl_import_B2P truncated");
                    }

                    using (BulkInserter biB2P = new BulkInserter())
                    {
                        Log.Info("Importing B2P crosswalk...");

                        ok = biB2P.Configure(conn, "tbl_import_B2P", "B2P", null, null, out errorMessage);

                        if (ok)
                        {
                            var expectedColumns = biB2P.DTable.Columns.Cast<DataColumn>()
                                .Select(c => c.ColumnName)
                                .Where(c => !c.Equals("PopulatedBy", StringComparison.OrdinalIgnoreCase)
                                         && !c.Equals("PopulatedAt", StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            dtB2P = NormalizeB2PWorksheet(dtB2P, expectedColumns);

                            ok = ValidateWorksheetColumns(ds, dtB2P, biB2P, out errorMessage);
                            if (!ok)
                            {
                                throw new Exception(errorMessage);
                            }
                        }

                        for (int i = 0; ok && i < dtB2P.Rows.Count; i++)
                        {
                            DataRow dr = dtB2P.Rows[i];
                            DataRow r = biB2P.NewRow();

                            ok = AssignRowValues(dtB2P, biB2P, dr, ref r, out errorMessage);
                            if (!ok)
                            {
                                errorMessage += $"\nwhen processing row {i} (zero-based)";
                                break;
                            }

                            ok = biB2P.AddRow(r, out errorMessage);
                            if (!ok)
                            {
                                throw new Exception(errorMessage);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.InfoFormat("ImportB2PCrosswalkFromExcel ExcelFile: [{0}].[{1}] - ended.", SafeForLog(excelFilePath), SafeForLog(worksheetName));

            return ok;
        }


        public static bool ImportBridgeToPavement(string dbConnectionString, string excelFilePath, string? worksheetName, out string errorMessage)
        {
            errorMessage = null;

            Log.InfoFormat("ImportBridgeToPavement ([{0}].[{1}]) - started...", SafeForLog(excelFilePath), SafeForLog(worksheetName));

            bool ok = ImportB2PCrosswalkFromExcel(dbConnectionString, excelFilePath, worksheetName, out errorMessage);
            if (ok)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbConnectionString))
                    {
                        conn.Open();
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = "dbo.sp_pb_MoveB2P";
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandTimeout = 12000;
                            SqlParameter retVal = command.Parameters.Add("@RetVal", SqlDbType.Int);
                            retVal.Direction = ParameterDirection.ReturnValue;
                            command.ExecuteNonQuery();
                            int N = Convert.ToInt32(retVal.Value);
                            Log.Info($"Bridge-to-Pavement records inserted: {N}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ok = false;
                    errorMessage = ex.Message;
                    Log.Error(errorMessage);
                }
            }

            Log.InfoFormat("ImportBridgeToPavement ([{0}].[{1}]) - finished.  Ok={2}, ErrorMessage: '{3}'", SafeForLog(excelFilePath), SafeForLog(worksheetName), ok, SafeForLog(errorMessage));

            return (ok);
        }

        public static bool ImportBridgeToPavementToDss(string dbConnectionString, string excelFilePath, string? worksheetName, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"ImportBridgeToPavementToDss ([{excelFilePath}].[{worksheetName}]) - started...");

            try
            {
                ok = Read(excelFilePath, out DataSet ds, out errorMessage);
                if (!ok)
                {
                    throw new Exception(errorMessage);
                }

                if (ds.Tables.Count < 1)
                {
                    throw new Exception("First worksheet in the Excel document was not found.");
                }

                DataTable dtB2P = SelectB2PWorksheet(ds, worksheetName);

                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = "TRUNCATE TABLE tbl_dss_BridgeToPavements";
                        command.ExecuteNonQuery();
                        Log.Info("TABLE tbl_dss_BridgeToPavements truncated");
                    }

                    using (BulkInserter biB2P = new BulkInserter())
                    {
                        ok = biB2P.Configure(conn, "tbl_dss_BridgeToPavements", "B2P_DSS", null, null, out errorMessage);
                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }

                        var expectedColumns = biB2P.DTable.Columns.Cast<DataColumn>()
                            .Select(c => c.ColumnName)
                            .Where(c => !c.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase)
                                     && !c.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        dtB2P = NormalizeB2PWorksheet(dtB2P, expectedColumns);
                        ok = ValidateWorksheetColumns(ds, dtB2P, biB2P, out errorMessage);
                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }

                        for (int i = 0; ok && i < dtB2P.Rows.Count; i++)
                        {
                            DataRow dr = dtB2P.Rows[i];
                            DataRow r = biB2P.NewRow();
                            ok = AssignRowValues(dtB2P, biB2P, dr, ref r, out errorMessage);
                            if (!ok)
                            {
                                errorMessage += $"\nwhen processing row {i} (zero-based)";
                                break;
                            }

                            if (biB2P.DTable.Columns.Contains("CreatedBy"))
                            {
                                r["CreatedBy"] = "Admin";
                            }
                            if (biB2P.DTable.Columns.Contains("CreatedAt"))
                            {
                                r["CreatedAt"] = DateTime.Now;
                            }

                            ok = biB2P.AddRow(r, out errorMessage);
                            if (!ok)
                            {
                                throw new Exception(errorMessage);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"ImportBridgeToPavementToDss ([{excelFilePath}].[{worksheetName}]) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }

        public static bool ImportPAMSSuperAssets(string dbConnectionString, string excelFilePath, string? worksheetName, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            Guid? guidSession = null;

            Log.Info($"ImportPAMSSuperAssets ExcelFile: [{excelFilePath}] - started...");

            try
            {
                ok = Read(excelFilePath, out DataSet ds, out errorMessage);
                if (!ok) throw new Exception(errorMessage);
                if (ds.Tables.Count < 1) throw new Exception("First worksheet in the Excel document was not found.");

                DataTable dt;
                if (ds.Tables.Count > 1 && !string.IsNullOrEmpty(worksheetName))
                {
                    if (ds.Tables.IndexOf(worksheetName) < 0)
                    {
                        dt = ds.Tables[0];
                        Log.Warn($"{worksheetName} worksheet not found in the Excel document.  Using the first worksheet instead");
                    }
                    else
                    {
                        dt = ds.Tables[worksheetName];
                    }
                }
                else
                {
                    dt = ds.Tables[0];
                }
                Log.Info($"Worksheet contains {dt.Rows.Count} data rows.");

                var sourceColumns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                if (!sourceColumns.Contains("Asset", StringComparer.OrdinalIgnoreCase))
                {
                    throw new Exception("Worksheet does not match the expected PAMS Super Assets segmentation format - "
                        + "missing required column: Asset.");
                }

                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.sp_pb_StartNewImportSession";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 12000;
                        cmd.Parameters.Add("@ImportSource", SqlDbType.VarChar, 50).Value = "PAMS";
                        cmd.Parameters.Add("@DataSourceType", SqlDbType.VarChar, 20).Value = "Excel";
                        cmd.Parameters.Add("@DataSourceName", SqlDbType.NVarChar, 255).Value = excelFilePath;
                        SqlParameter guidParm = cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier);
                        guidParm.Direction = ParameterDirection.InputOutput;
                        guidParm.Value = Guid.NewGuid();
                        cmd.ExecuteNonQuery();
                        guidSession = (Guid)guidParm.Value;
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "TRUNCATE TABLE tbl_import_PAMS_SuperAssets";
                        cmd.ExecuteNonQuery();
                        Log.Info("TABLE tbl_import_PAMS_SuperAssets truncated");
                    }

                    for (int i = 0; ok && i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO tbl_import_PAMS_SuperAssets
                                (ImportSessionId, Asset, Cnty, Route, Direction, FromSegment, ToSegment)
                                VALUES (@ImportSessionId, @Asset, @Cnty, @Route, @Direction, @FromSegment, @ToSegment)";

                            object ColVal(string col) {
                                if (!dt.Columns.Contains(col)) return DBNull.Value;
                                object v = dr[col];
                                return (v == null || v == DBNull.Value || string.IsNullOrWhiteSpace(v?.ToString())) ? DBNull.Value : v;
                            }

                            cmd.Parameters.Add("@ImportSessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                            cmd.Parameters.Add("@Asset",       SqlDbType.NVarChar, 200).Value = ColVal("Asset");
                            cmd.Parameters.Add("@Cnty",        SqlDbType.TinyInt  ).Value = ColVal("Cnty");
                            cmd.Parameters.Add("@Route",       SqlDbType.SmallInt ).Value = ColVal("Route");
                            cmd.Parameters.Add("@Direction",   SqlDbType.Bit      ).Value = ColVal("Direction");
                            cmd.Parameters.Add("@FromSegment", SqlDbType.SmallInt ).Value = ColVal("FromSegment");
                            cmd.Parameters.Add("@ToSegment",   SqlDbType.SmallInt ).Value = ColVal("ToSegment");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    Log.Info($"{dt.Rows.Count} rows inserted into tbl_import_PAMS_SuperAssets");

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.sp_pb_PopulateSuperAssetSegmentation";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 48000;
                        cmd.Parameters.Add("@ImportSessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                        cmd.Parameters.Add("@IsMaster", SqlDbType.Int).Value = 1;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"ImportPAMSSuperAssets ExcelFile: [{excelFilePath}] - ended.  Ok={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }

        /// <summary>
        /// Imports work candidates
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="poolId">Guid of the target user pool (library) Id</param>
        /// <param name="excelFilePath">Full path name of the Excel file.</param>
        /// <param name="worksheetName">Name of the worksheet, like "WorkCandidates". May be left as null if it is the first worksheet.</param>
        /// <param name="errorMessage">(out) error message, null if no errors.</param>
        public static bool ImportScoreParameters(string dbConnectionString, Guid poolId, string excelFilePath, string? worksheetName, out string? errorMessage, bool fromScratch, bool keepUserCreated)
        {
            bool ok = true;
            errorMessage = null;
            Guid? guidSession = null;

            //Log.Info($"ImportScoreParameters [{excelFilePath}] - started...");

            try
            {
                ok = Read(excelFilePath, out DataSet ds, out errorMessage);
                if (!ok)
                {
                    throw new Exception(errorMessage);
                }

                DataTable dtSP = null;

                if (ds.Tables.Count > 1 && !string.IsNullOrEmpty(worksheetName))
                {
                    if (ds.Tables.IndexOf(worksheetName) < 0)
                    {
                        dtSP = ds.Tables[0];
                        //Log.Warn($"{worksheetName} worksheet not found in the Excel document.  Using the first worksheet instead");
                    }
                    else
                    {
                        dtSP = ds.Tables[worksheetName];
                    }
                }
                else
                {
                    dtSP = ds.Tables[0];
                }

                //Log.Info($"Worksheet [{worksheetName}] contains {dtSP.Rows.Count} data rows.");


                using (SqlConnection connection = new SqlConnection(dbConnectionString))
                {
                    connection.Open();
                    connection.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);


                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "TRUNCATE TABLE tbl_dss_RawImportedScoreParameters";
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "dbo.UspStartImportSession";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 48000;
                        command.Parameters.Add("@ImportSourceType", SqlDbType.VarChar, 10).Value = "ScoreParameters";
                        command.Parameters.Add("@DataSourceType", SqlDbType.VarChar, 10).Value = "Excel";
                        command.Parameters.Add("@DataSourceName", SqlDbType.VarChar, 255).Value = excelFilePath;
                        command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        SqlParameter guidParm = command.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier);
                        guidParm.Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        guidSession = Guid.Parse(guidParm.Value.ToString());
                        if (!guidSession.HasValue)
                        {
                            throw new Exception("Import session could not be created.  Please check the log file for more detail.");
                        }
                    }

                    using (BulkInserter bi = new BulkInserter())
                    {
                        ok = bi.Configure(connection, "tbl_dss_RawImportedScoreParameters", "dtSP", null, null, out errorMessage);
                        if (ok)
                        {
                            // tbl_dss_RawImportedScoreParameters has an "Intercept" column that isn't part of
                            // PoolScoreParameterDto/Entity and isn't written by ExportAsync's round-trip file
                            // (only the blank template includes it), so it's always absent from a re-imported
                            // export. Without filtering, Configure's column mapping (built from every
                            // destination column) fails Flush() with "ColumnName 'Intercept' does not match
                            // up with any column in data source."
                            bi.FilterMappingsToSource(dtSP);
                            bi.DTable = dtSP;
                            bi.Flush();
                        }
                    }

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.UspMoveScoreParametersToPool";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        command.Parameters.Add("@fromScratch", SqlDbType.Bit).Value = fromScratch ? 1 : 0;
                        command.Parameters.Add("@keepUserCreated", SqlDbType.Bit).Value = keepUserCreated ? 1 : 0;
                        command.ExecuteNonQuery();
                    }



                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.UspCreateScoreSQL";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                //Log.Error(errorMessage);
            }
            finally
            {
                if (guidSession.HasValue)
                {
                    using (SqlConnection conn = new SqlConnection(dbConnectionString))
                    {
                        conn.Open();
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = conn;

                            cmd.CommandText = @"UPDATE tbl_dss_ImportSessions
SET CompletedAt=CURRENT_TIMESTAMP,  CompletedStatus=@Status, Notes=@Notes WHERE Id=@Guid";
                            cmd.Parameters.Add("@Status", SqlDbType.Int).Value = ok ? 1 : -1;
                            cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = ok ? "Excel import completed successfully." : errorMessage;
                            cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            //Log.Info($"ImportScoreParameters [{excelFilePath}] - ended.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }


        /// <summary>
        /// Imports work candidates
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="excelFilePath">Full path name of the Excel file.</param>
        /// <param name="importSourceName">Optional name of the source/partner, e.g., "Northwest RPO". May be null. </param>
        /// <param name="worksheetName">Name of the worksheet, like "WorkCandidates". May be left as null if it is the first worksheet.</param>
        /// <param name="poolId">Guid of the target user pool (library) Id</param>
        /// <param name="forgiving">True if it is okay for the inputto contain extra columns, false - if not.</param>
        /// <param name="deleteAll">True to delete every existing tbl_dss_WorkCandidates row for the pool before importing, false to merge (only exact duplicates of the new rows are replaced).</param>
        /// <param name="guidSession">(out) Guid of the record created in TblImportSessions</param>
        /// <param name="errorMessage">(out) error message, null if no errors.</param>
        public static bool ImportDSSWorkCandidatesFromExcel(string dbConnectionString,
            string excelFilePath,
            string? importSourceName,
            string? worksheetName,
            Guid poolId,
            bool forgiving,
            bool deleteAll,
            out Guid? guidSession,
            out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            guidSession = null;

            //Log.Info($"ImportDSSWorkCandidatesFromExcel [{excelFilePath}.{worksheetName}] - started...");
            try
            {
                ok = Read(excelFilePath, out DataSet ds, out errorMessage);

                DataTable dtWC = null;

                if (!ok)
                {
                    throw new Exception(errorMessage);
                }

                if (ds.Tables.Count > 1 && !string.IsNullOrEmpty(worksheetName))
                {
                    if (ds.Tables.IndexOf(worksheetName) < 0)
                    {
                        dtWC = ds.Tables[0];
                        //Log.Warn($"{worksheetName} worksheet not found in the Excel document.  Using the first worksheet instead");
                    }
                    else
                    {
                        dtWC = ds.Tables[worksheetName];
                    }
                }
                else
                {
                    dtWC = ds.Tables[0];
                }

                //Log.Info($"Worksheet [{worksheetName}] contains {dtWC.Rows.Count} data rows.");

                using (SqlConnection connection = new SqlConnection(dbConnectionString))
                {
                    connection.Open();
                    connection.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "TRUNCATE TABLE tbl_dss_RawImportedWorkCandidates";
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "dbo.UspStartImportSession";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 48000;
                        command.Parameters.Add("@ImportSourceType", SqlDbType.VarChar, 10).Value = "WorkCand";
                        if (importSourceName != null)
                        {
                            command.Parameters.Add("@ImportSourceName", SqlDbType.NVarChar, 50).Value = importSourceName;
                        }
                        command.Parameters.Add("@DataSourceType", SqlDbType.VarChar, 10).Value = "Excel";
                        command.Parameters.Add("@DataSourceName", SqlDbType.VarChar, 255).Value = excelFilePath;
                        command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        SqlParameter guidParm = command.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier);
                        guidParm.Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        guidSession = Guid.Parse(guidParm.Value.ToString());
                        if (!guidSession.HasValue)
                        {
                            throw new Exception("Import session could not be created.  Please check the log file for more detail.");
                        }
                    }

                    // The Work Candidates export writes the project column as "ProjectName" (the
                    // entity's real property name), but the raw staging table's column is
                    // "ProjectID" -- re-importing an export otherwise silently drops this mapping
                    // below (FilterMappingsToSource only keeps columns present in the sheet), so
                    // every reimported row loses its project grouping.
                    if (dtWC.Columns.Contains("ProjectName") && !dtWC.Columns.Contains("ProjectID"))
                    {
                        dtWC.Columns["ProjectName"].ColumnName = "ProjectID";
                    }

                    using (BulkInserter bi = new BulkInserter())
                    {
                        ok = bi.Configure(connection, "tbl_dss_RawImportedWorkCandidates", "dtWC", null, null, out errorMessage);
                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }
                        if (forgiving)
                            bi.FilterMappingsToSource(dtWC);
                        bi.DTable = dtWC;
                        bi.Flush();
                    }

                    try
                    {
                        // Your existing code

                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "dbo.UspMoveRawWorkCandidatesToPoolTables";
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                            command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                            command.Parameters.Add("@DeleteAll", SqlDbType.Bit).Value = deleteAll ? 1 : 0;
                            command.CommandTimeout = 12000;
                            command.ExecuteNonQuery();
                        }

                        // Your remaining code

                    }
                    catch (SqlException sqlEx)
                    {
                        ok = false;
                        errorMessage = $"SQL Error: {sqlEx.Message}, Error Number: {sqlEx.Number}";
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        errorMessage = $"Error: {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                //Log.Error(errorMessage);
            }
            finally
            {
                if (guidSession.HasValue)
                {
                    using (SqlConnection conn = new SqlConnection(dbConnectionString))
                    {
                        conn.Open();
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = conn;

                            cmd.CommandText = @"UPDATE tbl_dss_ImportSessions
SET CompletedAt=CURRENT_TIMESTAMP,  CompletedStatus=@Status, Notes=@Notes WHERE Id=@Guid";
                            cmd.Parameters.Add("@Status", SqlDbType.Int).Value = ok ? 1 : -1;
                            cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = ok ? "Excel import completed successfully." : errorMessage;
                            cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            //Log.Info($"ImportDSSWorkCandidatesFromExcel [{excelFilePath}.{worksheetName}] - finished.  Ok={ok}, ErrorMessage: {errorMessage}");

            return (ok);
        }

        public static bool WriteDSSOutputUsingSingleSProc(string dbConnectionString, Guid? poolId, double? threshold, string xlsxFileName, List<string> workSheetNames, string storedProc, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"WriteDSSOutputUsingSingleSProc {xlsxFileName} - started...");

            try
            {

                using (ExcelDataWriter xwr = ExcelDataWriter.Create(xlsxFileName, new ExcelDataWriterOptions { TruncateStrings = false }))
                {
                    using (SqlConnection connection = new SqlConnection(dbConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = connection.CreateCommand())
                        {

                            command.CommandText = storedProc;
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandTimeout = 12000;
                            if (poolId != null)
                            {
                                command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId.Value;
                            }
                            if (threshold != null)
                            {
                                command.Parameters.Add("@RelativeEfficiencyThreshold", SqlDbType.Float).Value = threshold.Value;
                            }


                            SqlDataAdapter a = new SqlDataAdapter(command);
                            DataSet dataSet = new DataSet();
                            a.Fill(dataSet);
                            for (int i = 0; ok && i < workSheetNames.Count; i++)
                            {
                                string name = workSheetNames[i];
                                DbDataReader reader = dataSet.Tables[i].CreateDataReader();
                                ExcelDataWriter.WriteResult res = xwr.Write(reader, name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"WriteDSSOutputUsingSingleSProc {xlsxFileName} - ended. OK={ok}, ErrorMessage='{errorMessage}'.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        /// <summary>
        /// Imports Maintainable Asset Segmentation Excel file
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="excelFilePath">Pathname of the input Excel file</param>
        /// <param name="worksheetName">Name of the worksheet.  May be null if Excel documentc ontains only one sheet.</param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on success, False on failure</returns>
        public static bool ImportMASFromExcelToDSS(string dbConnectionString, string excelFilePath, string? worksheetName, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            Guid? guidSession = null;

            Log.Info($"ImportMASFromExcelToDSS [{excelFilePath}].[{worksheetName}] - started...");

            try
            {
                ok = Read(excelFilePath, out DataSet ds, out errorMessage);
                if (!ok)
                {
                    throw new Exception(errorMessage);
                }

                DataTable dtMAS = null;

                if (ds.Tables.Count > 1 && !string.IsNullOrEmpty(worksheetName))
                {
                    if (ds.Tables.IndexOf(worksheetName) < 0)
                    {
                        dtMAS = ds.Tables[0];
                        Log.Warn($"{worksheetName} worksheet not found in the Excel document.  Using the first worksheet instead");
                    }
                    else
                    {
                        dtMAS = ds.Tables[worksheetName];
                    }
                }
                else
                {
                    dtMAS = ds.Tables[0];
                }
                Log.Info($"Worksheet contains {dtMAS.Rows.Count} data rows.");

                using (SqlConnection connection = new SqlConnection(dbConnectionString))
                {
                    connection.Open();
                    connection.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "TRUNCATE TABLE tbl_dss_RawImportedMAS";
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "dbo.UspStartImportSession";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 48000;
                        command.Parameters.Add("@ImportSourceType", SqlDbType.VarChar, 10).Value = "MAS";
                        command.Parameters.Add("@ImportSourceName", SqlDbType.VarChar, 50).Value = "Administrator";
                        command.Parameters.Add("@DataSourceType", SqlDbType.VarChar, 10).Value = "Excel";
                        command.Parameters.Add("@DataSourceName", SqlDbType.VarChar, 255).Value = excelFilePath;
                        command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = DBNull.Value;
                        SqlParameter guidParm = command.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier);
                        guidParm.Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        guidSession = Guid.Parse(guidParm.Value.ToString());
                        if (!guidSession.HasValue)
                        {
                            throw new Exception("Import session could not be created.  Please check the log file for more detail.");
                        }
                    }

                    using (BulkInserter bi = new BulkInserter())
                    {
                        ok = bi.Configure(connection, "tbl_dss_RawImportedMAS", "dtMAS", null, null, out errorMessage);
                        if (ok)
                        {

                            bi.DTable = dtMAS;
                            bi.Flush();
                        }
                    }

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.UspMoveRawMaintainableAssetSegmentation";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 48000;
                        command.Parameters.Add("@ImportSessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }
            finally
            {
                if (guidSession.HasValue)
                {
                    using (SqlConnection conn = new SqlConnection(dbConnectionString))
                    {
                        conn.Open();
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = conn;

                            cmd.CommandText = @"UPDATE tbl_dss_ImportSessions
SET CompletedAt=CURRENT_TIMESTAMP,  CompletedStatus=@Status, Notes=@Notes WHERE Id=@Guid";
                            cmd.Parameters.Add("@Status", SqlDbType.Int).Value = ok ? 1 : -1;
                            cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = ok ? "Excel import completed successfully." : errorMessage;
                            cmd.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier).Value = guidSession.Value;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            Log.Info($"ImportMASFromExcelToDSS [{excelFilePath}].[{worksheetName}] - ended.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }


        #endregion

        #region EXPORT

        public static bool ExportTreatmentsToBAMS(string dbConnectionString, string xlsxFilePath, int scenId, out long recordsWritten, out string errorMessage)
        {
            recordsWritten = 0;
            errorMessage = null;

            bool ok = ExcelWrapper.ExcelHandler.WriteToSingleWorkSheetUsingSproc(dbConnectionString, xlsxFilePath, "sp_pb_ExportNarrowBridgeTreatments", scenId, "Committed Projects", out recordsWritten, out errorMessage);

            return (ok);
        }

        public static bool ExportProjectsToBAMS(string dbConnectionString, string xlsxFilePath, int scenId, out long recordsWritten, out string errorMessage)
        {
            recordsWritten = 0;
            errorMessage = null;

            bool ok = ExcelWrapper.ExcelHandler.WriteToSingleWorkSheetUsingSproc(dbConnectionString, xlsxFilePath, "sp_pb_ExportBridgeProjects", scenId, "Bundled Treatment Projects", out recordsWritten, out errorMessage);

            return (ok);
        }

        public static bool ExportTreatmentsToPAMS(string dbConnectionString, string xlsxFilePath, int scenId, out long recordsWritten, out string errorMessage)
        {
            recordsWritten = 0;
            errorMessage = null;

            bool ok = ExcelWrapper.ExcelHandler.WriteToSingleWorkSheetUsingSproc(dbConnectionString, xlsxFilePath, "sp_pb_ExportNarrowPavementTreatments", scenId, "Committed Projects", out recordsWritten, out errorMessage);

            return (ok);
        }

        public static bool ExportProjectsToPAMS(string dbConnectionString, string xlsxFilePath, int scenId, out long recordsWritten, out string errorMessage)
        {
            recordsWritten = 0;
            errorMessage = null;

            bool ok = ExcelWrapper.ExcelHandler.WriteToSingleWorkSheetUsingSproc(dbConnectionString, xlsxFilePath, "sp_pb_ExportPavementProjects", scenId, "Bundled Treatment Projects", out recordsWritten, out errorMessage);

            return (ok);
        }
        #endregion

        // Reports every column mismatch at once so a bad file doesn't have to be fixed one column per upload.
        private static bool ValidateWorksheetColumns(DataSet ds, DataTable dtSource, BulkInserter biDestination, out string errorMessage)
        {
            errorMessage = null;

            var destColumns = biDestination.DTable.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName).ToList();

            // PopulatedBy/PopulatedAt carry database defaults and are never supplied by the spreadsheet.
            var expected = destColumns
                .Where(c => !c.Equals("PopulatedBy", StringComparison.OrdinalIgnoreCase)
                         && !c.Equals("PopulatedAt", StringComparison.OrdinalIgnoreCase)
                         && !c.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase)
                         && !c.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var sourceColumns = dtSource.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName).ToList();

            var unexpected = sourceColumns
                .Where(s => !destColumns.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList();
            var missing = expected
                .Where(e => !sourceColumns.Contains(e, StringComparer.OrdinalIgnoreCase)).ToList();

            if (unexpected.Count == 0 && missing.Count == 0)
            {
                return true;
            }

            var problems = new List<string>();
            if (unexpected.Count > 0)
            {
                problems.Add($"unrecognized column(s): {string.Join(", ", unexpected)}");
            }
            if (missing.Count > 0)
            {
                problems.Add($"missing required column(s): {string.Join(", ", missing)}");
            }

            errorMessage = $"Worksheet '{dtSource.TableName}' does not match the expected import format - {string.Join("; ", problems)}."
                         + $" Expected exactly these columns: {string.Join(", ", expected)}.";

            if (ds.Tables.Count > 1)
            {
                var allSheets = ds.Tables.Cast<DataTable>().Select(t => t.TableName);
                errorMessage += $" Note: this workbook contains {ds.Tables.Count} worksheets ({string.Join(", ", allSheets)}).";
            }

            Log.Error(errorMessage);
            return false;
        }

        private static bool AssignRowValues(DataTable dtSource, BulkInserter biDestination, DataRow drInput, ref DataRow rOutput, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            for (int j = 0; ok && j < dtSource.Columns.Count; j++)
            {
                string colName = dtSource.Columns[j].ColumnName;

                if (!biDestination.DTable.Columns.Contains(colName))
                {
                    ok = false;
                    errorMessage = $"Column '{colName}' in the Excel file does not match any expected destination column.";
                    Log.Error(errorMessage);
                    break;
                }

                string typeName = biDestination.DTable.Columns[colName].DataType.Name.ToLower().Substring(0, 3);

                if (biDestination.DTable.Columns[colName].DataType.Name.ToLower() == "int64")
                {
                    typeName = "big";
                }

                object o = drInput[j];

                if (o == null || o == DBNull.Value)
                {
                    continue;
                }
                string colValue = o.ToString().ToUpper();
                if (string.IsNullOrWhiteSpace(colValue) || colValue == "NULL")
                {
                    continue;
                }

                ok = AssignColumnValue(ref rOutput, colName, typeName, o, colValue, out errorMessage);
                if (!ok)
                {
                    errorMessage += $"\nwhen processing row {j} (0-based) of DataTable {dtSource.TableName}";
                    Log.Error(errorMessage);
                }
            }

            return ok;
        }

        private static bool AssignColumnValue(ref DataRow r, string colName, string typeName, object o, string colValue, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            try
            {
                if (o == DBNull.Value)
                {
                    r[colName] = DBNull.Value;
                    return (ok);
                }

                // Blank cells (e.g. empty MinYear/MaxYear) become NULL for non-text columns.
                if (typeName != "str" && string.IsNullOrWhiteSpace(colValue))
                {
                    r[colName] = DBNull.Value;
                    return (ok);
                }

                switch (typeName)
                {
                    case "gui":
                        r[colName] = Guid.Parse(colValue);
                        break;
                    case "int":
                        r[colName] = Convert.ToInt32(o);
                        break;
                    case "big":
                        r[colName] = Convert.ToInt64(o);
                        break;
                    case "dou":
                        r[colName] = Convert.ToDouble(o);
                        break;
                    case "byt":
                    case "tin":
                        r[colName] = Convert.ToByte(o);
                        break;
                    case "boo":
                        r[colName] = (colValue == "0" || colValue == "N") ? 0 : 1;
                        break;
                    default:
                        r[colName] = o.ToString();
                        break;
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                errorMessage += $"\nwhen trying to assign the value of {colValue} or object {0} of implied type {typeName} to column {colName}.";
                Log.Error(errorMessage);
            }

            return ok;
        }


    }
}