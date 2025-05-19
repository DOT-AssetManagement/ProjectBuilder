using ExcelDataReader;
using log4net;
using log4net.Appender;
using log4net.Layout;
using PBLogic;
using Sylvan.Data.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;

namespace ExcelWrapper
{
    public class ExcelHandler
    {
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
            get { return _log; }
            set { _log = value; }
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
                using(SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.sp_pb_MoveMAS";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 12000;
                        cmd.Parameters.Add("@ImportSessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"ImportMaintainableAssetSegmentation ([{excelFilePath}]) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        private static bool ImportTreatmentsFromExcelWithCargo(string dbConnectionString, string src,
                string assetType,
                string excelFilePath,
                string excelTabName,
                bool forgiving,
                out Guid? guidSession,
                out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            bool bWarnings = false;
            guidSession = null;
            int N = 0;

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

            Log.Info($"ImportTreatmentsFromExcelWithCargo ([{excelFilePath}],{src},{assetType}]) - started...");

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
                    dtTreatments = ds.Tables[0];    // We assume that treatments come in the first tab of the Excel document;
                }
                else if (ds.Tables.Contains(excelTabName))
                {
                    dtTreatments = ds.Tables[excelTabName];
                }
                else
                {
                    throw new Exception($"Tab [{excelTabName}] not found in the file {excelFilePath}");
                }

                Log.Info($"Treatments worksheet contains {dtTreatments.Rows.Count} data rows.");

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

                    // Populate CargoAttributes dictionary
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = $@"SELECT AttributeName, AttributeNo 
FROM tbl_pb_CargoAttributes WITH (NOLOCK) WHERE AssetType='{assetType}' ORDER BY AttributeNo";
                        command.CommandType = CommandType.Text;
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

                        ok = biTreat.Configure(conn, targetTable, "Treatments", null, null, out errorMessage);
                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }

                        // Verify integrity of the input
                        for (int i = 0; ok && i < dtTreatments.Columns.Count; i++)
                        {
                            string inputColName = dtTreatments.Columns[i].ColumnName;
                            if (InputColumnDirection.ContainsKey(inputColName.ToUpper()))
                            {
                                throw new Exception($"Column [{inputColName}] occurs more than once in the Excel worksheet.");
                            }

                            if (biTreat.DTable.Columns.Contains(inputColName))
                            {

                                InputColumnDirection.Add(inputColName.ToUpper(), "T");    // goes to the target table
                                continue;
                            }
                            else
                            {
                                if (CargoAttributes.ContainsKey(inputColName.ToUpper()))
                                {
                                    InputColumnDirection.Add(inputColName.ToUpper(), "C");    // goes to cargo
                                    continue;
                                }

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
                            using (BulkInserter biCargo = new BulkInserter())
                            {
                                ok = biCargo.Configure(conn, "tbl_pb_CargoData", "Cargo", null, null, out errorMessage);
                                if (!ok)
                                {
                                    throw new Exception(errorMessage);
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
                                        else if (InputColumnDirection.ContainsKey(colName.ToUpper()) && InputColumnDirection[colName.ToUpper()] == "C")
                                        {
                                            if (CargoAttributes.ContainsKey(colName.ToUpper()))
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
                                            }
                                        }
                                        else
                                        {
                                            continue;   // do nothing
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
                                cmd.CommandText = $"TRUNCATE TABLE {targetTable}";
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandTimeout = 48000;
                                cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = guidSession.Value;
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = $"DELETE FROM tbl_pb_CargoData WHERE ImportSessionId=@SessionId";
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

            Log.Info($"Records loaded from {excelFilePath}: {N}");
            Log.Info($"ImportTreatmentsFromExcelWithCargo ([{excelFilePath}],{src},{assetType}) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
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

            Log.Info($"ImportTreatments ({src},{assetType},[{excelFilePath}],[{excelTabName},{forgiving}]) - started...");

            ok = ImportTreatmentsFromExcelWithCargo(dbConnectionString,
                        src, assetType, excelFilePath, excelTabName, forgiving,
                        out guidSession, out errorMessage);

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

            Log.Info($"ImportTreatments ({src},{assetType},[{excelFilePath}],[{excelTabName},{forgiving}]) - finished. Ok={ok},  ErrorMessage: '{errorMessage}");

            return ok;
        }

        private static bool ImportB2PCrosswalkFromExcel(string dbConnectionString, string excelFilePath, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"ImportB2PCrosswalkFromExcel ExcelFile: [{excelFilePath}] - started...");

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

                DataTable dtB2P = ds.Tables[0];
                Log.Info($"Worksheet contains {dtB2P.Rows.Count} data rows.");

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

                        for (int i = 0; ok && i < dtB2P.Rows.Count; i++)
                        {
                            DataRow dr = dtB2P.Rows[i];
                            DataRow r = biB2P.NewRow();

                            ok = AssignRowValues(dtB2P, biB2P, dr, ref r, out errorMessage);
                            if (!ok)
                            {
                                errorMessage += $"\nwhen processing row {i} (zero-based)";
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

            Log.Info($"ImportB2PCrosswalkFromExcel ExcelFile: [{excelFilePath}] - ended.");

            return ok;
        }


        public static bool ImportBridgeToPavement(string dbConnectionString, string excelFilePath, out string errorMessage)
        {
            errorMessage = null;

            Log.Info($"ImportBridgeToPavement ([{excelFilePath}]) - started...");

            bool ok = ImportB2PCrosswalkFromExcel(dbConnectionString, excelFilePath, out errorMessage);
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

            Log.Info($"ImportBridgeToPavement ([{excelFilePath}]) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
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

        private static bool AssignRowValues(DataTable dtSource, BulkInserter biDestination, DataRow drInput, ref DataRow rOutput, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            for (int j = 0; ok && j < dtSource.Columns.Count; j++)
            {
                string colName = dtSource.Columns[j].ColumnName;
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
            bool ok =true;
            errorMessage = null;

            try
            {
                if (o == DBNull.Value)
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
            catch(Exception ex)
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