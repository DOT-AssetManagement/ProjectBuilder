using DEALib;
using DSS.Utilities;
using ExcelWrapper;
using log4net;
using log4net.Appender;
using log4net.Layout;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using ProjectBuilder.Entities;
using System;
using System.Data;
using System.IO;
using System.Text;
using Veracode.Attributes;

#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace PBLogic
{
    public class DataManager
    {
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

        private static ILog _log = ConfigureLogger();

        public static ILog Log
        {
            get { return _log; }
            set { _log = value; }
        }

        public string ConnectionString { get; set; }
        public int ScenId { get; set; }
        public string HomeDirectory { get; set; }

        [CRLFCleanser]
        public static string SafeForLog(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // URL-encode so CR/LF and every other control character is neutralized
            return System.Net.WebUtility.UrlEncode(input);
        }


        /// <summary>
        /// Deletes scenario
        /// </summary>
        // <param name="errorMessage">(out) error message on error; null on success</param>
        /// <returns>True on success, False on error</returns>
        public bool DeleteScenario(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"DeleteScenario for ScenID={ScenId} started...");

            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand(
                        $@"SELECT  DISTINCT table_name
  FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WITH(NOLOCK)
  WHERE COLUMN_NAME = 'ScenId'", conn))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 480000;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dt.Load(reader);
                            reader.Close();
                        }
                    }

                    foreach (DataRow r in dt.Rows)
                    {
                        string rawTableName = r[0]?.ToString();
                        string sql = $"DELETE FROM [{rawTableName}] WHERE ScenId = @ScenId";

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandTimeout = 480000;
                            command.Parameters.AddWithValue("@ScenId", ScenId);
                            long N = command.ExecuteNonQuery();
                            if (!string.IsNullOrEmpty(sql))
                            {
                                sql = sql.Replace("\r", "\\r")
                                         .Replace("\n", "\\n")
                                         .Replace("\t", "\\t")
                                         .Replace("\"", "\\\"")
                                         .Replace("'", "\\'");
                            }

                            Log.Info($"{sql} - {N} records deleted.");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"DeleteScenario for ScenID={ScenId} finished.\tOK={ok}\tError message: {errorMessage}");
            return ok;
        }

        /// <summary>
        /// Creates a new scenario copying its inouts from an existing scenario
        /// </summary>
        /// <param name="newScenId">(out) ScenID of the newly created scenario</param>
        /// <param name="errorMessage">(our) error message onfailure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool CopyScenario(out int newScenId, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;
            newScenId = -1;

            Log.Info($"CopyScenario ScenId={ScenId} started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand($"dbo.sp_pb_CopyScenario", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120000;
                        command.Parameters.AddWithValue("@ScenIdFrom", ScenId);
                        SqlParameter outParm = command.Parameters.Add("@RetVal", SqlDbType.Int);
                        outParm.Direction = ParameterDirection.ReturnValue;
                        command.ExecuteNonQuery();
                        newScenId = (int)(command.Parameters["@RetVal"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"CopyScenario ScenId={ScenId} finished. New ScenId={newScenId}\tOK={ok}\tError message: {errorMessage}");

            return (ok);
        }

        /// <summary>
        /// Save scenario inputs to an XML file (used for testing only)
        /// </summary>
        /// <param name="xmlFileName">Name of the XML file to be created.  Gets created in the DATA subdirectory of home directory</param>
        /// <param name="errorMessage">(out) Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool SaveScenarioInputsToXml(string xmlFileName, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"SaveScenarioInputsToXml ScenId={ScenId}, XML file: {xmlFileName} - started...");

            string xmlFilePath = Path.Combine(HomeDirectory + "\\DATA", xmlFileName);

            try
            {
                using (DataSet ds = new DataSet("PBScenario"))
                {
                    DataTable dtScenarios = ds.Tables.Add("Scenarios");
                    DataTable dtScenBudget = ds.Tables.Add("ScenBudget");
                    DataTable dtScenParm = ds.Tables.Add("ScenParm");

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand command = new SqlCommand(
                            $"SELECT ScenarioName, ISNULL(RecommendedUtilityProxy,'') AS RecommendedUtilityProxy FROM tbl_pb_Scenarios WITH (NOLOCK) WHERE ScenId={ScenId}",
                                conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                dtScenarios.Load(reader);
                                reader.Close();
                            }
                        }

                        using (SqlCommand command = new SqlCommand(
                          $"SELECT YearWork, BridgeBudget, PavementBudget, OpenBudget" +
                          $" FROM tbl_pb_ScenBudget WITH (NOLOCK) WHERE ScenId={ScenId} ORDER BY YearWork",
                              conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                dtScenBudget.Load(reader);
                                reader.Close();
                            }
                        }

                        using (SqlCommand command = new SqlCommand(
                         $"SELECT ParmID, ParmValue" +
                         $" FROM tbl_pb_ScenParm WITH (NOLOCK) WHERE ScenId={ScenId} ORDER BY ParmValue",
                             conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                dtScenParm.Load(reader);
                                reader.Close();
                            }
                        }


                        ds.WriteXml(xmlFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"SaveScenarioInputsToXml ScenId={ScenId}, XML file path: {xmlFilePath} - finsihed." +
                $"\tOK={ok}\tErrorMessage: {errorMessage}");

            return (ok);
        }

        #region Obsolete
        /// <summary>
        /// Save sorted scenario statistics to XMLfile
        /// </summary>
        /// <param name="xmlFileName">Name of the XML file to be created in the \DATA subdirectory</param>
        /// <param name="errorMessage">Error message on failure, null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool SaveScenarioStatisticsToXmlFile(string xmlFileName, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            string xmlFilePath = Path.Combine(HomeDirectory + "\\DATA", xmlFileName);

            Log.Info($"SaveScenarioStatisticsToXmlFile ScenId={ScenId}, FileName: {xmlFileName} - started...");

            try
            {
                using (DataSet ds = new DataSet("Statistics"))
                {
                    DataTable dtSummary = ds.Tables.Add("Summary");
                    DataTable dtByDistrict = ds.Tables.Add("By_District");
                    DataTable dtByDistrictYear = ds.Tables.Add("By_District_and_Year");


                    using (SqlConnection conn = new SqlConnection(ConnectionString, null))
                    {
                        conn.Open();

                        using (SqlCommand command = new SqlCommand(
                            $"SELECT * FROM tbl_pb_ScenarioStatistics WITH (NOLOCK) WHERE ScenID={ScenId}",
                            conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                dtSummary.Load(reader);
                            }
                        }

                        using (SqlCommand command = new SqlCommand(
                         $"SELECT * FROM vw_pb_ScenarioStatisticsByDistrictTotals WITH (NOLOCK) WHERE ScenID={ScenId} " +
                         "ORDER BY District",
                         conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                dtByDistrict.Load(reader);
                            }
                        }

                        using (SqlCommand command = new SqlCommand(
                        $"SELECT * FROM tbl_pb_ScenarioStatisticsByDistrict WITH (NOLOCK) WHERE ScenID={ScenId} " +
                        "ORDER BY District, YearWork",
                        conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                dtByDistrictYear.Load(reader);
                            }
                        }
                    }

                    ds.WriteXml(xmlFilePath);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"SaveScenarioStatisticsToXmlFile ScenId={ScenId}, FileName: {xmlFilePath} - Finished. OK={ok}\tError message: {errorMessage}");

            return (ok);
        }

#if OBSOLETE
        /// <summary>
        /// Creates a new scenario populating its input tables from XML file
        /// </summary>
        /// <param name="xmlFilePath">Full path to the XML file to be read</param>
        /// <param name="newScenId">(out) ScenID of the newly created scenario</param>
        /// <param name="errorMessage">Error message on failure, null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool CreateNewScenario(string xmlFilePath, out int newScenId, out string errorMessage)
        {
            bool ok = true;
            newScenId = -1;
            errorMessage = null;

            Log.Info($"CreateNewScenario from the file {xmlFilePath} - started...");

            try
            {
                using (DataSet ds = new DataSet("PBScenario"))
                {
                    ds.ReadXml(xmlFilePath);

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        string scenName = ds.Tables["Scenarios"].Rows[0]["ScenarioName"].ToString();
                        string code = ds.Tables["Scenarios"].Rows[0]["RecommendedUtilityProxy"].ToString();

                        if (string.IsNullOrEmpty(code))
                        {
                            code = "HPB";
                        }

                        using (SqlCommand command = new SqlCommand("dbo.sp_pb_CreateNewScenario", conn))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandTimeout = 120000;
                            command.Parameters.AddWithValue("@NewScenarioName", scenName);
                            command.Parameters.AddWithValue("@Code", code);
                            SqlParameter retVal = command.Parameters.Add("@RetVal", SqlDbType.Int);
                            retVal.Direction = ParameterDirection.ReturnValue;
                            command.ExecuteNonQuery();
                            newScenId = (int)(retVal.Value);
                        }

                        StringBuilder sb = new StringBuilder();
                        sb.Append("INSERT INTO tbl_pb_ScenBudget (ScenID, YearWork, BridgeBudget, PavementBudget, OpenBudget) " +
                            "VALUES ");
                        sb.AppendLine();
                        int N = ds.Tables["ScenBudget"].Rows.Count;
                        for (int i = 0; i < N; i++)
                        {
                            int yearWork = Convert.ToInt32(ds.Tables["ScenBudget"].Rows[i]["YearWork"]);
                            double bridgeBudget = Convert.ToDouble(ds.Tables["ScenBudget"].Rows[i]["BridgeBudget"]);
                            double pavementBudget = Convert.ToDouble(ds.Tables["ScenBudget"].Rows[i]["PavementBudget"]);
                            double openBudget = Convert.ToDouble(ds.Tables["ScenBudget"].Rows[i]["OpenBudget"]);
                            sb.AppendLine($"({newScenId}, {yearWork}, {bridgeBudget}, {pavementBudget}, {openBudget})");
                            if (i < N - 1)
                            {
                                sb.Append(",");
                            }
                        }
                        sb.Append(";");
                        using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                        {
                            N = command.ExecuteNonQuery();
                        }
                        sb.Clear();

                        N = ds.Tables["ScenParm"].Rows.Count;
                        sb.AppendLine("INSERT INTO tbl_pb_ScenParm (ScenID, ParmID, ParmValue) VALUES ");
                        for (int i = 0; i < N; i++)
                        {
                            string parmId = ds.Tables["ScenParm"].Rows[i]["ParmID"].ToString();
                            double parmValue = Convert.ToDouble(ds.Tables["ScenParm"].Rows[i]["ParmValue"]);
                            sb.AppendLine($"({newScenId}, '{parmId}', {parmValue})");
                            if (i < N - 1)
                            {
                                sb.Append(",");
                            }
                        }
                        sb.Append(";");
                        using (SqlCommand command = new SqlCommand(sb.ToString(), conn))
                        {
                            N = command.ExecuteNonQuery();
                        }
                        sb.Clear();

                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"CreateNewScenario from the file {xmlFilePath} - finished.  NewScenId={newScenId}  OK={ok}  Error Message: {errorMessage}");

            return (ok);
        }

         /// <summary>
        /// Wraps object's CreateNewScenario method
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="xmlFilePath">Fulle pathname of the XML file from which a new scenario is to be created</param>
        /// <param name="newScenId">(out) ScenID of the newlyc reated scenario</param>
        /// <param name="errorMessage">(out) Error message on failure, null on success</param>
        /// <returns>True on success, False on failure</returns>
        public static bool CreateNewScenario(string connectionString, string xmlFilePath, out int newScenId, out string errorMessage)
        {
            bool ok = true;
            newScenId = -1;
            errorMessage = null;

            Log.InfoFormat("CreateNewScenario (static) from the file {0} - started...", SafeForLog(xmlFilePath));

            DataManager dataManager = new DataManager()
            {
                ConnectionString = connectionString
            };

            ok = dataManager.CreateNewScenario(xmlFilePath, out newScenId, out errorMessage);

            Log.InfoFormat("CreateNewScenario (static) from the file {0} - finished.  NewScenId={1}  OK={2}  Error Message: {3}", SafeForLog(xmlFilePath), newScenId, ok, SafeForLog(errorMessage));

            return (ok);
        }
#endif
        #endregion




        public bool CreateNewScenario(string scenarioName, string libraryId, int firstYear, int lastYear, int setDefault, out int newScenId, out string errorMessage, string CreatedBy, bool isShared = false)
        {
            bool ok = true;
            newScenId = -1;
            errorMessage = null;

            Log.InfoFormat("CreateNewScenario '{0}' for {1}{2} - started...", SafeForLog(scenarioName), firstYear, lastYear);

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand("dbo.sp_pb_CreateNewScenario", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120000;
                        command.Parameters.Add("@NewScenarioName", SqlDbType.VarChar, 100).Value = scenarioName;
                        if (!string.IsNullOrEmpty(libraryId))
                        {
                            command.Parameters.Add("@LibraryId", SqlDbType.VarChar, 100).Value = libraryId;
                        }
                        else
                        {
                            command.Parameters.Add("@LibraryId", SqlDbType.VarChar, 100).Value = DBNull.Value;
                        }
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 50).Value = CreatedBy;
                        command.Parameters.Add("@Code", SqlDbType.VarChar, 4).Value = "HPB";
                        command.Parameters.Add("@SetDefaults", SqlDbType.Bit).Value = setDefault;
                        command.Parameters.Add("@FirstYear", SqlDbType.Int).Value = firstYear;
                        command.Parameters.Add("@LastYear", SqlDbType.Int).Value = lastYear;
                        command.Parameters.Add("@IsShared", SqlDbType.Bit).Value = isShared;
                        SqlParameter retVal = command.Parameters.Add("@RetVal", SqlDbType.Int);
                        retVal.Direction = ParameterDirection.ReturnValue;
                        command.ExecuteNonQuery();
                        newScenId = (int)(retVal.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.InfoFormat("CreateNewScenario '{0}' for {1}{2} - ended. OK={3}, ErrorMessage='{4}'", SafeForLog(scenarioName), firstYear, lastYear, ok, SafeForLog(errorMessage));

            return (ok);
        }


        private bool UnlockScenario(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(
                        $"UPDATE tbl_pb_Scenarios SET Locked=0 WHERE ScenID={ScenId}", conn))
                    {
                        command.CommandTimeout = 120000;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }
            return (ok);
        }

        #region StaticMethods

        /// <summary>
        /// Wraps object's DeleteScenario method
        /// </summary>
        /// <param name="configurationString">Database connection string</param>
        /// <param name="scenId">ScenID of the scenario to be deleted</param>
        /// <param name="withInputs">True if scenario input tables are also to be cleared</param>
        /// <param name="errorMessage">(out) Error message in failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public static bool DeleteScenario(string configurationString, int scenId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            _log.Info($"DeleteScenario (static) for ScenID={scenId} started...");

            DataManager dataManager = new DataManager()
            {
                ScenId = scenId,
                ConnectionString = configurationString
            };

            ok = dataManager.DeleteScenario(out errorMessage);

            Log.Info($"DeleteScenario (static) for ScenID={scenId} finished.\tOK={ok}\tError message: {errorMessage}");
            return ok;
        }


        /// <summary>
        /// Wraps object's CopyScenario method
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="fromScenId">ScenID of the source scenario</param>
        /// <param name="newScenId">(out)ScenID of the newlyc reated scenario</param>
        /// <param name="errorMessage">(out) Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public static bool CopyScenario(string connectionString, int fromScenId, out int newScenId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            newScenId = -1;

            _log.Info($"CopyScenario (static) for from ScenID={fromScenId} started...");

            DataManager dataManager = new DataManager()
            {
                ConnectionString = connectionString,
                ScenId = fromScenId
            };

            ok = dataManager.CopyScenario(out newScenId, out errorMessage);
            _log.Info($"CopyScenario (static) for from ScenID={fromScenId} tp ScenId={newScenId} finished. Ok={ok}\tError message: {errorMessage}");

            return ok;
        }

        /// <summary>
        /// Wraps object's SaveScenarioInputsToXml method
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="homeDirectory">Home directory.  It must have \Data subdirectory</param>
        /// <param name="xmlFileName">Bane of the XML file to be created</param>
        /// <param name="ScenId">ScenID of the scenario to be saved to XML</param>
        /// <param name="(out) errorMessage">Error message on failure, null on success</param>
        /// <returns>True on success, False on failure</returns>
        public static bool SaveScenarioInputsToXml(string connectionString, string homeDirectory,
                                                    string xmlFileName, int ScenId, out string errorMessage)
        {
            Log.Info("SaveScenarioInputsToXml (static) - started...");
            DataManager dataManager = new DataManager()
            {
                ConnectionString = connectionString,
                HomeDirectory = homeDirectory,
                ScenId = ScenId
            };

            bool ok = dataManager.SaveScenarioInputsToXml(xmlFileName, out errorMessage);
            Log.Info($"SaveScenarioInputsToXml  (static) - finished.  OK={ok}\tError message: {errorMessage}");
            return (ok);
        }


        /// <summary>
        /// Wraps object's SaveScenarioStatisticsToXmlFile
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="homeDirectory">Home directory which must have \DATA subdirectory</param>
        /// <param name="scenId">ScenID of the scenario for which statistics is to be collected</param>
        /// <param name="xmlFileName">Name of the scenario file to be created in the \DATA subdirectory</param>
        /// <param name="errorMessage">(Out) Error message on error, null on success</param>
        /// <returns>True on success, False on error</returns>
        public static bool SaveScenarioStatisticsToXmlFile(string connectionString, string homeDirectory,
            int scenId, string xmlFileName, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.InfoFormat("SaveScenarioStatisticsToXmlFile (static) ScenId={0}, FileName: {1} - started...", scenId, SafeForLog(xmlFileName));

            DataManager dataManager = new DataManager()
            {
                ConnectionString = connectionString,
                HomeDirectory = homeDirectory,
                ScenId = scenId
            };

            ok = dataManager.SaveScenarioStatisticsToXmlFile(xmlFileName, out errorMessage);

            Log.InfoFormat("SaveScenarioStatisticsToXmlFile (static) ScenId={0}, FileName: {1} - Finished. OK={2}\tError message: {3}", scenId, SafeForLog(xmlFileName), ok, SafeForLog(errorMessage));


            return ok;
        }




        public static bool CreateNewScenario(string connectionString, string libraryId, string scenarioName, int firstYear, int lastYear, int setDefault, out int newScenId, out string errorMessage, string CreatedBy = "", bool isShared = false)
        {
            bool ok = true;
            newScenId = -1;
            errorMessage = null;

            Log.InfoFormat("CreateNewScenario (static) '{0}' for {1}{2} - started...", SafeForLog(scenarioName), firstYear, lastYear);


            DataManager manager = new DataManager()
            {
                ConnectionString = connectionString
            };

            ok = manager.CreateNewScenario(scenarioName, libraryId, firstYear, lastYear, setDefault, out newScenId, out errorMessage, CreatedBy, isShared);

            Log.InfoFormat("CreateNewScenario (static) '{0}' for {1}{2} - ended. OK={3}, ErrorMessage='{4}'", SafeForLog(scenarioName), firstYear, lastYear, ok, SafeForLog(errorMessage));

            return (ok);
        }

        public static bool UnlockScenario(string connectionString, int scenId, out string errorMessage)
        {
            Log.Info($"UnlockScenario for ScenID={scenId} - started...");

            DataManager dataManager = new DataManager()
            {
                ConnectionString = connectionString,
                ScenId = scenId
            };

            bool ok = dataManager.UnlockScenario(out errorMessage);

            Log.Info($"UnlockScenario for ScenID={scenId} - ended.");

            return ok;
        }

        public static bool CreateExtendedScenarioProjects(string connectionString, int scenId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"CreateExtendedScenarioProjects scenId={scenId} - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_CreateExtendedProjectsV2";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 24000;
                        command.Parameters.Add("@ScenId", SqlDbType.Int).Value = scenId;
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

            Log.Info($"CreateExtendedScenarioProjects ended.  Ok={ok}, ErrorMessage={errorMessage}");

            return (ok);
        }




        public static bool CreateScenarioProjects(string connectionString, int scenId, bool cleanUp, bool x1, bool l1, bool l2, bool b1, bool b2, bool n1, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"CreateScenarioProjects scenId={scenId}, cleanup={cleanUp}, x1={x1}, l1={l1}, l2={l2}, b1={b1}, b2={b2}, n1={n1} - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                        command.CommandText = "dbo.sp_pb_CreateScenarioProjects";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 24000;
                        command.Parameters.Add("@ScenId", SqlDbType.Int).Value = scenId;
                        command.Parameters.Add("@CleanUp", SqlDbType.Bit).Value = cleanUp ? 1 : 0;
                        command.Parameters.Add("@X1", SqlDbType.Bit).Value = x1 ? 1 : 0;
                        command.Parameters.Add("@L1", SqlDbType.Bit).Value = l1 ? 1 : 0;
                        command.Parameters.Add("@L2", SqlDbType.Bit).Value = l2 ? 1 : 0;
                        command.Parameters.Add("@B1", SqlDbType.Bit).Value = b1 ? 1 : 0;
                        command.Parameters.Add("@B2", SqlDbType.Bit).Value = b2 ? 1 : 0;
                        command.Parameters.Add("@N1", SqlDbType.Bit).Value = n1 ? 1 : 0;
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

            Log.Info($"CreateScenarioProjects ended.  Ok={ok}, ErrorMessage={errorMessage}");

            return (ok);
        }

        private static void Generic_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Log.Info("T-SQL messages:\n" + e.Message);
        }

        private static void Conn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Log.Info("dbo.sp_pb_CreateScenarioProjects messages:\n" + e.Message);
        }

        private static void ImportFromPamsAndBams_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Log.Info("dbo.sp_pb_ImportDataFromPAMSandBAMS:\n" + e.Message);
        }

#if OBSOLETE
        public static bool ImportDataFromPAMSandBAMS(string connectionString, bool doTruncate ,bool doKeepUserTreatments,  out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"PopulateImportedTreatmentsTable doTruncate={doTruncate} started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(ImportFromPamsAndBams_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_ImportDataFromPAMSandBAMS";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 24000;
                        command.Parameters.Add("@DoTruncate", SqlDbType.Bit).Value = doTruncate ? 1 : 0;
                        command.Parameters.Add("@PreserveUserTreatments", SqlDbType.Bit).Value = doKeepUserTreatments ? 1 : 0;
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

            Log.Info($"PopulateImportedTreatmentsTable doTruncate={doTruncate} ended.  OK={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

#endif

        public static bool PopulateScenarioTreatmentsTable(string connectionString, int scenId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"PopulateScenarioTreatmentsTable scenId={scenId} started...");

            try
            {
                ScenarioFilter.Log = Log;
                ok = ScenarioFilter.PopulateFilteredTreatmentsForScenario(connectionString, scenId, out errorMessage);
                if (!ok)
                {
                    throw new Exception(errorMessage);
                }
                

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_PopulateExtendedTreatmentsV2";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 24000;
                        command.Parameters.Add("@ScenId", SqlDbType.Int).Value = scenId;
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

            Log.Info($"PopulateScenarioTreatmentsTable scenId={scenId} ended.  OK={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }


        public static bool GetScenarioStatistics(string connectionString, int scenId, out DataSet ds, string xmlFilePath, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            ds = new DataSet($"Scenario_{scenId}");
            ds.Tables.Add("Projects");
            ds.Tables.Add("Treatments");
            ds.Tables.Add("NeedsAndWork");
            ds.Tables.Add("Benefits");

            Log.Info($"GetScenarioStatistics ScenId={scenId}, xmlFilePath='{xmlFilePath}' - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_CollectScenarioStatistics";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ScenId", SqlDbType.Int).Value = scenId;
                        command.CommandTimeout = 60000;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.TableMappings.Add("Table", "Projects");
                            adapter.TableMappings.Add("Table1", "Treatments");
                            adapter.TableMappings.Add("Table2", "NeedsAndWork");
                            adapter.TableMappings.Add("Table3", "Benefits");
                            adapter.Fill(ds);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(xmlFilePath))
                {
                    ds.WriteXml(xmlFilePath);
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"GetScenarioStatistics ScenId={scenId}, xmlFilePath='{xmlFilePath}' - ended. OK={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }

        public static bool GetScenarioStatisticsByDistrict(string connectionString, int scenId, int? district, out DataSet ds, string xmlFilePath, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            ds = new DataSet
            {
                DataSetName = district.HasValue ? $"Scenario_{scenId}_District_{district.Value}" :
                                                 $"Scenario_{scenId}_All_Districts"
            };
            ds.Tables.Add("Projects");
            ds.Tables.Add("Treatments");
            ds.Tables.Add("NeedsAndWork");
            ds.Tables.Add("Benefits");

            Log.Info($"GetScenarioStatisticsByDistrict ScenId={scenId}, District={district}, xmlFilePath='{xmlFilePath}' - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_CollectScenarioStatisticsByDistrict";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ScenId", SqlDbType.Int).Value = scenId;
                        if (district.HasValue)
                        {
                            command.Parameters.Add("@District", SqlDbType.Int).Value = district.Value;
                        }
                        else
                        {
                            command.Parameters.Add("@District", SqlDbType.Int).Value = DBNull.Value;
                        }
                        command.CommandTimeout = 60000;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.TableMappings.Add("Table", "Projects");
                            adapter.TableMappings.Add("Table1", "Treatments");
                            adapter.TableMappings.Add("Table2", "NeedsAndWork");
                            adapter.TableMappings.Add("Table3", "Benefits");
                            adapter.Fill(ds);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(xmlFilePath))
                {
                    ds.WriteXml(xmlFilePath);
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"GetScenarioStatistics ScenId={scenId}, xmlFilePath='{xmlFilePath}' - ended. OK={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }

        // The calling procedure should make sure that libraryName does not exist in the database table
        public static bool CreateNewUserLibrary(string dbConnectionString, Guid userId, string libraryName, string libraryDescription, bool shared, out string libraryId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            libraryId = null;

            Log.InfoFormat("CreateNewUserLibrary ({0},'{1}','{2}') - started...", userId, SafeForLog(libraryName), SafeForLog(libraryDescription));

            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_CreateNewUserLibrary";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120000;
                        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                        command.Parameters.Add("@LibraryName", SqlDbType.NVarChar, 50).Value = libraryName;
                        command.Parameters.Add("@Description", SqlDbType.NVarChar).Value = libraryDescription;
                        command.Parameters.Add("@Shared", SqlDbType.Bit).Value = shared ? 1 : 0;
                        SqlParameter retVal = command.Parameters.Add("@RetVal", SqlDbType.Int);
                        retVal.Direction = ParameterDirection.ReturnValue;
                        SqlParameter paramLibraryId = command.Parameters.Add("@LibraryId", SqlDbType.VarChar, 100);
                        paramLibraryId.Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        if (Convert.ToInt32(retVal.Value) < 1)
                        {
                            throw new Exception("Library record could not be added.  Please check the log for more information.:");
                        }
                        libraryId = paramLibraryId.Value.ToString();
                        Log.Info($"New library ID: {libraryId}");
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.InfoFormat("CreateNewUserLibrary ({0},'{1}','{2}') - finished. Ok={3}, ErrorMessage: '{4}'", userId, SafeForLog(libraryName), SafeForLog(libraryDescription), ok, SafeForLog(errorMessage));


            return (ok);
        }

        public static bool UnAssignProjectTreatments(string dbConnectionString, int projectId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"UnAssignProjectTreatments ([{projectId}]) - started...");
            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    string sql = $"Update tbl_pb_ExtendedImportedTreatments " +
                        $"SET AssignedTimewiseConstrainedProjectId = NULL," +
                        $"IsSelected = NULL, SelectedYear = NULL WHERE AssignedTimewiseConstrainedProjectId={projectId}";
                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 480000;
                        long N = command.ExecuteNonQuery();
                        Log.Info($"{sql} - {N} records deleted.");
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"UnAssignProjectTreatments ([{projectId}]) - finished. Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        public static bool DeactivateUserLibrary(string dbConnectionString, string libraryId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"DeactivateUserLibrary ([{libraryId}]) - started...");
            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_DeactivateUserLibrary";
                        SqlParameter retVal = command.Parameters.Add("@RetVal", SqlDbType.Int);
                        retVal.Direction = ParameterDirection.ReturnValue;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 12000;
                        command.Parameters.Add("@LibraryId", SqlDbType.VarChar, 100).Value = libraryId;
                        command.ExecuteNonQuery();
                        if (Convert.ToInt32(retVal.Value) < 1)
                        {
                            throw new Exception($"Library [{libraryId}] was not found.");
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

            Log.Info($"DeactivateUserLibrary ([{libraryId}]) - finished. Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        public static bool ReactivateUserLibrary(string dbConnectionString, string libraryId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"ReactivateUserLibrary ([{libraryId}]) - started...");
            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_ReactivateUserLibrary";
                        SqlParameter retVal = command.Parameters.Add("@RetVal", SqlDbType.Int);
                        retVal.Direction = ParameterDirection.ReturnValue;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 12000;
                        command.Parameters.Add("@LibraryId", SqlDbType.VarChar, 100).Value = libraryId;
                        command.ExecuteNonQuery();
                        if (Convert.ToInt32(retVal.Value) < 1)
                        {
                            throw new Exception($"Library [{libraryId}] was not found.");
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

            Log.Info($"ReactivateUserLibrary ([{libraryId}]) - finished. Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        public static bool PopulateUserLibrary(string dbConnectionString, string libraryId,
            out string errorMessage,
            bool fromScratch = false,
            string? sourceLibraryId = null,
            string? assetType = null,
            int? district = null, short? cnty = null, int? route = null,
            string? simulationId = null,
            string? networkId = null, int? minYear = null, int? maxYear = null
           )
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"PopulateUserLibrary ({libraryId}, {fromScratch}) - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "sp_pb_PopulateLibraryTreatments";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 12000;
                        command.Parameters.Add("@LibraryId", SqlDbType.VarChar, 100).Value = libraryId;
                        if (!string.IsNullOrEmpty(sourceLibraryId))
                        {
                            command.Parameters.Add("SourceLibraryId", SqlDbType.VarChar, 100).Value = sourceLibraryId;
                        }
                        command.Parameters.Add("@FromScratch", SqlDbType.Bit).Value = fromScratch ? 1 : 0;
                        if (!string.IsNullOrEmpty(assetType))
                        {
                            command.Parameters.Add("@AssetType", SqlDbType.Char, 1).Value = assetType.ToUpper()[0];
                        }
                        if (district.HasValue)
                        {
                            command.Parameters.Add("@District", SqlDbType.TinyInt).Value = district.Value;
                        }
                        if (cnty.HasValue)
                        {
                            command.Parameters.Add("@Cnty", SqlDbType.TinyInt).Value = cnty.Value;
                        }
                        if (route.HasValue)
                        {
                            command.Parameters.Add("@Route", SqlDbType.Int).Value = route.Value;
                        }
                        if (!string.IsNullOrEmpty(simulationId))
                        {
                            command.Parameters.Add("@SimulationId", SqlDbType.VarChar, 100).Value = simulationId;
                        }
                        if (!string.IsNullOrEmpty(networkId))
                        {
                            command.Parameters.Add("@NetworkId", SqlDbType.VarChar, 100).Value = networkId;
                        }
                        if (minYear.HasValue)
                        {
                            command.Parameters.Add("@MinYear", SqlDbType.Int).Value = minYear.Value;
                        }
                        if (maxYear.HasValue)
                        {
                            command.Parameters.Add("@MaxYear", SqlDbType.Int).Value = maxYear.Value;
                        }

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

            Log.Info($"PopulateUserLibrary ({libraryId}, {fromScratch}) - ended.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        public static bool CopyUserLibrary(string dbConnectionString, string sourceLibraryId, string targetLibraryId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"CopyUserLibrary ({sourceLibraryId},{targetLibraryId}) - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "sp_pb_CopyUserLibrary";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 48000;
                        command.Parameters.Add("@SourceLibraryId", SqlDbType.VarChar, 100).Value = sourceLibraryId;
                        command.Parameters.Add("@TargetLibraryId", SqlDbType.VarChar, 100).Value = targetLibraryId;
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

            Log.Info($"CopyUserLibrary ({sourceLibraryId},{targetLibraryId}) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        /*
         * 
         *  public enum TreatmentStatus
            {
                /// <summary>
                ///     Indicates the existence of incomplete logic in the analysis engine.
                /// </summary>
                Undefined,

                /// <summary>
                ///     Indicates a treatment that has been fully applied.
                /// </summary>
                Applied,

                /// <summary>
                ///     Indicates a treatment that has been partially applied (during the leading years of a
                ///     multi-year treatment period).
                /// </summary>
                Progressed,
            }

        public enum TreatmentCause
        {
            /// <summary>
            ///     Indicates the existence of incomplete logic in the analysis engine.
            /// </summary>
            Undefined,

            /// <summary>
            ///     Indicates a treatment was not selected by the analysis engine.
            /// </summary>
            NoSelection,

            /// <summary>
            ///     Indicates a treatment was selected by the analysis engine.
            /// </summary>
            SelectedTreatment,

            /// <summary>
            ///     Indicates a treatment was scheduled by a previous treatment selection.
            /// </summary>
            ScheduledTreatment,

            /// <summary>
            ///     Indicates a treatment explicitly pre-selected in the input to the analysis engine.
            /// </summary>
            CommittedProject,

            /// <summary>
            ///     Indicates a non-initial year of a multi-year treatment. (Initial year uses <see cref="SelectedTreatment"/>.)
            /// </summary>
            CashFlowProject,
        }
        */


        /// <summary>
        /// Creates a new treatment in tbl_lib_LibraryTreatments and returns its unique ImportTimeGeneratedId as the out parameters.
        /// </summary>
        /// <param name="connectionString">Connection string for the database</param>
        /// <param name="libraryId">Library ID as a string.  Library must exist.</param>
        /// <param name="userTreatmentTypeNo">Type of treatment to be created. Must be selected as one of UserTreatmentTypeNo from tbl_pb_UserTreatments.</param>
        /// <param name="district">District, must be a valid district code.</param>
        /// <param name="cnty">County code, must be a valid code.</param>
        /// <param name="route">Route number,must be a valid route number.</param>
        /// <param name="fromSection">Must be a valid section number for the route.</param>
        /// <param name="preferredYear">Preferred year for the treatment.</param>
        /// <param name="directCost">Direct cost of the treatment.</param>
        /// <param name="benefit">Benefitof the treatment.</param>
        /// <param name="treatmentTimeGeneratedId">(out) unique ImportTimeGeneratedId of the created treatment.</param>
        /// <param name="errorMessage">(out) error message, null if no errors.</param>
        /// Parameters below are optional. They either have defaults or are resolved inside the method.
        /// <param name="treatmentName">Name of the treatment. Not more than 100 characters defaults to 'User treatment'.</param>
        /// <param name="assetType">Single character type of asset.  May NOT be 'P' or 'B'.  Defaults to 'U'.</param>
        /// <param name="treatmentStatus">To be selected from enum TreatmentStatus, see above. Defaults to Undefined (0).</param>
        /// <param name="treatmentCause">To be selected from enum TreatmentCause, see above. Defaults to CommittedProject (4).</param>
        /// <param name="minYear">Earliest year for the treatment.  Defaults to preferred year.</param>
        /// <param name="maxYear">Latest year for the treatment. Defaults to preferred year.</param>
        /// <param name="toSection">To-section of the treatment extent.  Defaults to fromSection.</param>
        /// <param name="direction">Direction: 0 or 1.  Incurred from the fromSection oddity if ommitted.</param>
        /// <param name="offset">Offset, defaults to NULL.</param>
        /// <param name="brkey">BRKEY if asset is a known bridge. Defaults to NULL.</param>
        /// <param name="assetId">AssetId as a string if known.  Defaults to NULL.</param>
        /// <param name="assetName">Name of the asset if known. Not more than 50 characters. Defaults to NULL.</param>
        /// <param name="isInterstate">Interstate: true or false.  Defaults to false.</param>
        /// <param name="isIsolatedBridge">True if it is a bridge whose BKEY is not in tbl_pb_BridgeToPavement.  Defaults to false.</param>
        /// <param name="priorityOrder">Number between 1 and 4. 1 being  the highest priority. Defaults to 1.</param>
        /// <param name="riskScore">Risk score if known.  Defaults to NULL.</param>
        /// <param name="remianingLife">Remaining life if known. Defaults to NULL.</param>
        /// <param name="isCommitted">True or false. Defaults to true./</param>
        /// <param name="ignoresSpendingLimit">True or false. Defaults to true.</param>
        /// <param name="indirectDesignCost">Indirect design cost if known. Defaults to NULL.</param>
        /// <param name="indirectROWCost">Indirect right-of-way cost if known. Defaults to NULL.</param>
        /// <param name="indirectUtilitiesCost">Indirect utilities cost if known. Defaults to NULL.</param>
        /// <param name="indirectOtherCost">Other indirect cost if known. Defaults to NULL.</param>
        /// <returns>True on success. False on error.</returns>
        public static bool CreateUserTreatment(string connectionString,
            string libraryId,
            int userTreatmentTypeNo,
            int district,
            int cnty,
            int route,
            int fromSection,
            int preferredYear,
            double directCost,
            double benefit,
            out Guid? treatmentTimeGeneratedId,
            out string errorMessage,
            string treatmentName = "User treatment",
            char? assetType = 'U',
            int treatmentStatus = 0,
            int treatmentCause = 4,
            int? minYear = null,
            int? maxYear = null,
            int? toSection = null,
            int? direction = null,
            int? offset = null,
            string? brkey = null,
            string? assetId = null,
            string? assetName = null,
            bool isInterstate = false,
            bool isIsolatedBridge = false,
            int priorityOrder = 1,
            double? riskScore = null,
            double? remainingLife = null,
            bool isCommitted = true,
            bool ignoresSpendingLimit = false,
            double? indirectDesignCost = null,
            double? indirectROWCost = null,
            double? indirectUtilitiesCost = null,
            double? indirectOtherCost = null
           )
        {
            bool ok = true;
            errorMessage = null;
            treatmentTimeGeneratedId = Guid.NewGuid();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
INSERT INTO dbo.tbl_lib_LibraryTreatments
    (LibraryId,ImportedTreatmentId,ImportTimeGeneratedId,SimulationId,NetworkId,AssetId
    ,AssetType,Asset,District,Cnty,[Route],Direction,FromSection,ToSection,Offset
    ,Interstate,Treatment,Benefit,Cost,Risk,PriorityOrder,PreferredYear,MinYear,MaxYear
    ,BRKEY,RemainingLife,TreatmentFundingIgnoresSpendingLimit
    ,TreatmentStatus,TreatmentCause,IsCommitted,IsIsolatedBridge
	,IsUserTreatment,UserTreatmentTypeNo
    ,IndirectCostDesign,IndirectCostROW,IndirectCostUtilities,IndirectCostOther)
VALUES
	(@LibraryId,@ImportedTreatmentId,@ImportTimeGeneratedId,@SimulationId,@NetworkId,@AssetId
    ,@AssetType,@Asset,@District,@Cnty,@Route,@Direction,@FromSection,@ToSection,@Offset
    ,@Interstate,@Treatment,@Benefit,@Cost,@Risk,@PriorityOrder,@PreferredYear,@MinYear,@MaxYear
    ,@BRKEY,@RemainingLife,@TreatmentFundingIgnoresSpendingLimit
    ,@TreatmentStatus,@TreatmentCause,@IsCommitted,@IsIsolatedBridge
	,@IsUserTreatment,@UserTreatmentTypeNo
    ,@IndirectCostDesign,@IndirectCostROW,@IndirectCostUtilities,@IndirectCostOther)";
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Add("@LibraryId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(libraryId);
                        cmd.Parameters.Add("@ImportedTreatmentId", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@ImportTimeGeneratedId", SqlDbType.UniqueIdentifier).Value = treatmentTimeGeneratedId;
                        cmd.Parameters.Add("@SimulationId", SqlDbType.UniqueIdentifier).Value = new Guid();
                        cmd.Parameters.Add("@NetworkId", SqlDbType.UniqueIdentifier).Value = new Guid();

                        if (string.IsNullOrEmpty(assetId))
                        {
                            cmd.Parameters.Add("@AssetId", SqlDbType.UniqueIdentifier).Value = new Guid();
                        }
                        else
                        {
                            cmd.Parameters.Add("@AssetId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(assetId);
                        }

                        cmd.Parameters.Add("@AssetType", SqlDbType.VarChar, 1).Value = assetType.ToString();

                        if (string.IsNullOrEmpty(assetName))
                        {
                            cmd.Parameters.Add("@Asset", SqlDbType.NVarChar, 50).Value = DBNull.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@Asset", SqlDbType.NVarChar, 50).Value = assetName;
                        }

                        cmd.Parameters.Add("@District", SqlDbType.TinyInt).Value = district;
                        cmd.Parameters.Add("@Cnty", SqlDbType.TinyInt).Value = cnty;
                        cmd.Parameters.Add("@Route", SqlDbType.Int).Value = route;

                        if (direction.HasValue)
                        {
                            cmd.Parameters.Add("@Direction", SqlDbType.TinyInt).Value = direction.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@Direction", SqlDbType.TinyInt).Value = fromSection % 2;
                        }

                        cmd.Parameters.Add("@FromSection", SqlDbType.Int).Value = fromSection;

                        if (toSection.HasValue)
                        {
                            cmd.Parameters.Add("@ToSection", SqlDbType.Int).Value = toSection;
                        }
                        else
                        {
                            cmd.Parameters.Add("@ToSection", SqlDbType.Int).Value = fromSection;
                        }

                        if (offset.HasValue)
                        {
                            cmd.Parameters.Add("@Offset", SqlDbType.Int).Value = offset.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@Offset", SqlDbType.Int).Value = DBNull.Value;
                        }

                        cmd.Parameters.Add("@Interstate", SqlDbType.Bit).Value = isInterstate ? 1 : 0;

                        if (string.IsNullOrEmpty(treatmentName))
                        {
                            cmd.Parameters.Add("@Treatment", SqlDbType.NVarChar, 100).Value = "User treatment";
                        }
                        else
                        {
                            cmd.Parameters.Add("@Treatment", SqlDbType.NVarChar, 100).Value = treatmentName;
                        }

                        cmd.Parameters.Add("@Benefit", SqlDbType.Float).Value = benefit;
                        cmd.Parameters.Add("@Cost", SqlDbType.Float).Value = directCost;

                        if (riskScore.HasValue)
                        {
                            cmd.Parameters.Add("@Risk", SqlDbType.Float).Value = riskScore.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@Risk", SqlDbType.Float).Value = DBNull.Value;
                        }

                        cmd.Parameters.Add("@PriorityOrder", SqlDbType.TinyInt).Value = priorityOrder;
                        cmd.Parameters.Add("@PreferredYear", SqlDbType.Int).Value = preferredYear;

                        if (minYear.HasValue)
                        {
                            cmd.Parameters.Add("@MinYear", SqlDbType.Int).Value = minYear.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@MinYear", SqlDbType.Int).Value = preferredYear;
                        }

                        if (maxYear.HasValue)
                        {
                            cmd.Parameters.Add("@MaxYear", SqlDbType.Int).Value = maxYear.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@MaxYear", SqlDbType.Float).Value = preferredYear;
                        }

                        if (string.IsNullOrEmpty(brkey))
                        {
                            cmd.Parameters.Add("@brkey", SqlDbType.NVarChar, 15).Value = DBNull.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@brkey", SqlDbType.NVarChar, 15).Value = brkey;
                        }

                        if (remainingLife.HasValue)
                        {
                            cmd.Parameters.Add("@RemainingLife", SqlDbType.Float).Value = remainingLife.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@RemainingLife", SqlDbType.Float).Value = DBNull.Value;
                        }

                        cmd.Parameters.Add("@TreatmentFundingIgnoresSpendingLimit", SqlDbType.Bit).Value = ignoresSpendingLimit ? 1 : 0;
                        cmd.Parameters.Add("@TreatmentStatus", SqlDbType.TinyInt).Value = treatmentStatus;
                        cmd.Parameters.Add("@TreatmentCause", SqlDbType.TinyInt).Value = treatmentCause;
                        cmd.Parameters.Add("@IsCommitted", SqlDbType.Bit).Value = isCommitted ? 1 : 0;
                        cmd.Parameters.Add("@IsIsolatedBridge", SqlDbType.Bit).Value = isIsolatedBridge ? 1 : 0;
                        cmd.Parameters.Add("@IsUserTreatment", SqlDbType.Bit).Value = 1;
                        cmd.Parameters.Add("@UserTreatmentTypeNo", SqlDbType.Int).Value = userTreatmentTypeNo;

                        if (indirectDesignCost.HasValue)
                        {
                            cmd.Parameters.Add("@IndirectCostDesign", SqlDbType.Float).Value = indirectDesignCost.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@IndirectCostDesign", SqlDbType.Float).Value = DBNull.Value;
                        }

                        if (indirectROWCost.HasValue)
                        {
                            cmd.Parameters.Add("@IndirectCostROW", SqlDbType.Float).Value = indirectROWCost.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@IndirectCostROW", SqlDbType.Float).Value = DBNull.Value;
                        }

                        if (indirectUtilitiesCost.HasValue)
                        {
                            cmd.Parameters.Add("@IndirectCostUtilities", SqlDbType.Float).Value = indirectUtilitiesCost.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@IndirectCostUtilities", SqlDbType.Float).Value = DBNull.Value;
                        }

                        if (indirectOtherCost.HasValue)
                        {
                            cmd.Parameters.Add("@IndirectCostOther", SqlDbType.Float).Value = indirectOtherCost.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@IndirectCostOther", SqlDbType.Float).Value = DBNull.Value;
                        }

                        int N = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                treatmentTimeGeneratedId = null;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            return (ok);
        }

#endregion

        #region DssMethods

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

            ExcelHandler.Log = Log;

            ok = ExcelHandler.ImportScoreParameters(dbConnectionString, poolId, excelFilePath, worksheetName, out errorMessage, fromScratch, keepUserCreated);

            return ok;
        }

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

            ExcelHandler.Log = Log;

            ok = ExcelHandler.ImportDSSWorkCandidatesFromExcel(dbConnectionString, excelFilePath,
                importSourceName, worksheetName, poolId, forgiving, deleteAll, out guidSession, out errorMessage);

            return ok;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dbConnectionString">Connection string</param>
        /// <param name="workCandidateId">Guid Id of the recored in TblWorkCandidates table which must exist.</param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on success, False on failure</returns>
        public static bool UpdateWorkCandidate(string dbConnectionString, Guid workCandidateId, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("UpdateWorkCandidate - started...");
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnectionString))
                {
                    connection.Open();
                    connection.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.UspUpdateWorkCandidate";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 12000;
                        command.Parameters.Add("@WorkCandidateId", SqlDbType.UniqueIdentifier).Value = workCandidateId;
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

            Log.Info("UpdateWorkCandidate - finished.");

            return ok;
        }

        public static bool RunScenario(string dbConnectionString, Guid scenarioId, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"Run scenario [{scenarioId}] started...");
            Guid? runId = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnectionString))
                {
                    connection.Open();
                    connection.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.UspStartScenarioRun";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ScenId", SqlDbType.UniqueIdentifier).Value = scenarioId;
                        SqlParameter parmId = command.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier);
                        parmId.Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        runId = parmId.Value as Guid?;
                    }

                    DEAContext.Log = Log;
                    DEAContext.ConnectionString = connection.ConnectionString;
                    DEAContext? dea = null;

                    ok = DEAContext.LoadFromDatabase(scenarioId, out dea, out errorMessage);

                    if (!ok)
                    {
                        throw new Exception(errorMessage);
                    }

                    ok = dea.RunDEA(out errorMessage);

                    if (!ok)
                    {
                        throw new Exception(errorMessage);
                    }

                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $@"DELETE FROM tbl_dss_ScenarioProjectObjectiveValues
WHERE ScenId=CONVERT(UNIQUEIDENTIFIER,'{scenarioId}')";
                        cmd.CommandType = CommandType.Text;
                        int N = cmd.ExecuteNonQuery();
                        Log.Info($"Number of old records deleted from tbl_dss_ScenarioProjectObjectiveValues: {N}");
                    }

                    using (BulkInserter bulkInserter = new BulkInserter())
                    {
                        bulkInserter.Configure(connection, "tbl_dss_ScenarioProjectObjectiveValues", "DT", null, null, out errorMessage);
                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }
                        foreach (DEALib.Project p in dea.ProjectList)
                        {
                            DataRow r = bulkInserter.NewRow();
                            r["ScenId"] = scenarioId;
                            r["ProjectId"] = Guid.Parse(p.Name);
                            r["RelativeEfficiency"] = p.R;
                            ok = bulkInserter.AddRow(r, out errorMessage);
                            if (!ok)
                            {
                                throw new Exception();
                            }
                        }
                    }

                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "dbo.UspUpdateRelativeEfficiencies";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 24000;
                        cmd.Parameters.Add("@ScenId", SqlDbType.UniqueIdentifier).Value = scenarioId;
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
            finally
            {
                if (runId != null)
                {
                    using (SqlConnection connection = new SqlConnection(dbConnectionString))
                    {
                        connection.Open();
                        connection.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                        using (SqlCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "dbo.UspCompleteScenarioRun";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@RunId", SqlDbType.UniqueIdentifier).Value = runId;
                            if (errorMessage != null)
                            {
                                cmd.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, 4000).Value = errorMessage;
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            Log.Info($"Run scenario [{scenarioId}] finished. Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }


        /// <summary>
        /// Exports to Excel projects and work candidates for pavements and bridges
        /// associated with a scenario.
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="scenId">ScenId; may be null, in which case data for all scenarios is exported</param>
        /// <param name="effThreshold">Relative efficiency threshold.  May be null, in which casde no efficiency threshold is applided</param>
        /// <param name="xlsxFilePath">Path name of the Excel file to be created.</param>
        /// <param name="errorMessage">(out) Error message.  Null if no errors.</param>
        /// <returns>True on success, false on failure</returns>
        public static bool ExportScenarioData(string dbConnectionString, Guid? scenId, double? effThreshold, string xlsxFilePath, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;
            ExcelHandler.Log = _log;

            Log.Info($"ExportScenarioData (scenId=[{scenId}] , XLSX=[{xlsxFilePath}], effThreshold=({effThreshold})]) - started...");
            try
            {
                Guid? poolId = null;
                string sprocName = "dbo.UspGetScenarioOutput";

                if (scenId != null)
                {
                    using (SqlConnection connection = new SqlConnection(dbConnectionString))
                    {
                        connection.Open();
                        connection.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                        using (SqlCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = $"SELECT PoolId FROM TblScenarios WITH (NOLOCK) WHERE Id=CONVERT(UNIQUEIDENTIFIER, '{scenId}')";
                            cmd.CommandType = CommandType.Text;
                            object o = cmd.ExecuteScalar();
                            if (o != null && o != DBNull.Value)
                            {
                                poolId = Guid.Parse(o.ToString());
                            }
                        }

                        using (SqlCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = $"SELECT DeaOutputSproc FROM TblScenarios WITH (NOLOCK) WHERE Id=CONVERT(UNIQUEIDENTIFIER, '{scenId}')";
                            cmd.CommandType = CommandType.Text;
                            object o = cmd.ExecuteScalar();
                            if (o != null)
                            {
                                sprocName = o.ToString();
                            }
                        }
                    }
                }

                ok = ExcelHandler.WriteDSSOutputUsingSingleSProc(dbConnectionString, poolId
                    , effThreshold
                    , xlsxFilePath
                    , new List<string> { "Projects", "Pavements", "Bridges" }
                    , sprocName
                    , out errorMessage);
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"ExportScenarioData (scenId=[{scenId}] , XLSX=[{xlsxFilePath}], effThreshold=({effThreshold})]) - finished.  OK={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }

        /// <summary>
        /// Exports to Excel projects and work candidates for pavements and bridges
        /// associated with a scenario.
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="scenId">ScenId; may be null, in which case data for all scenarios is exported</param>
        /// <param name="effThreshold">Relative efficiency threshold.  May be null, in which casde no efficiency threshold is applided</param>
        /// <param name="xlsxFilePath">Path name of the Excel file to be created.</param>
        /// <param name="errorMessage">(out) Error message.  Null if no errors.</param>
        /// <returns>True on success, false on failure</returns>
        public static bool ExportPoolData(string dbConnectionString, Guid? poolId, double? effThreshold, string xlsxFilePath, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;
            ExcelHandler.Log = _log;

            Log.Info($"ExportPoolData (poolId=[{poolId}] , XLSX=[{xlsxFilePath}], effThreshold=({effThreshold})]) - started...");
            string sprocName = "dbo.UspGetScenarioOutput";  // Default sproc

            ok = ExcelHandler.WriteDSSOutputUsingSingleSProc(dbConnectionString, poolId
                 , effThreshold
                 , xlsxFilePath
                 , new List<string> { "Projects", "Pavements", "Bridges" }
                 , sprocName
                 , out errorMessage);

            Log.Info($"ExportPoolData (poolId=[{poolId}] , XLSX=[{xlsxFilePath}], effThreshold=({effThreshold})]) - finished.  OK={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }

        /// <summary>
        /// This function must be called by the UI each time users save their changes in work candidate data
        /// (table TblWorkCandidates) or TblPoolScoreParameters
        /// </summary>
        /// <param name="dbConnectionString">Database connection string</param>
        /// <param name="poolId">PoolId</param>
        /// <param name="scoreParametersUpdated">True if function is being called after changes in TblPoolScoreParameters, otherwise false.</param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <param name="bDoCreate">By default is true, can be passed as false if method is caalled if called after a change of parameters or if no deletes or additions happened among the work candidates.  Leave it as True if uncertain.</param>
        /// <returns>True on success, False on failure</returns>
        public static bool UpdatePool(string dbConnectionString, Guid poolId, bool scoreParametersUpdated, out string? errorMessage, bool bDoCreate = true)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"UpdatePool [{poolId}] - started....");

            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    if (scoreParametersUpdated)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "dbo.UspCreateScoreSQL";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                            cmd.CommandTimeout = 12000;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.UspCalculateBenefits";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        cmd.CommandTimeout = 12000;
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.UspCalculateScores";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        cmd.CommandTimeout = 12000;
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.UspUpdateProjectCandidates";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        cmd.Parameters.Add("@CreateOrUpdate", SqlDbType.Char, 1).Value = "C";
                        cmd.CommandTimeout = 12000;
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

            Log.Info($"UpdatePool [{poolId}] - finished.");

            return (ok);
        }


        /// <summary>
        /// Creates a new scenario records in TblScenarios
        /// </summary>
        /// <param name="connectionString">database connection string</param>
        /// <param name="scenarioName">Scenario name</param>
        /// <param name="description">(optional) description, may be left null</param>
        /// <param name="poolId">Pool Id to be associated with (must exist)</param>
        /// <param name="deaInputSproc">DEA input sproc. May be left null to use default</param>
        /// <param name="deaOutputSproc">DEA output sproc. May be left null to use default</param>
        /// <param name="scenId">(out) generated Id of newly created scenario</param>
        /// <param name="errorMessage">(out) error message, null if no errors.</param>
        /// <returns>True on success, False on failure</returns>
        public static bool CreateScenario(string connectionString, string scenarioName, string? description, Guid poolId,
            string? deaInputSproc, string? deaOutputSproc,
            out Guid? scenId, out string? errorMessage)
        {
            bool ok = true;
            scenId = null;
            errorMessage = null;

            Log.InfoFormat("Create scenario [{0}] - started...", SafeForLog(scenarioName));
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TOP 1 ScenName FROM TblScenarios WITH (NOLOCK) WHERE UPPER(ScenName)=@ScenName";
                        cmd.Parameters.Add("@ScenName", SqlDbType.NVarChar, 50).Value = scenarioName.ToUpper();
                        object o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value)
                        {
                            throw new Exception($"Scenario [{o}] already exists.");
                        }
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TOP 1 UserId FROM TblUserPools WITH (NOLOCK) WHERE Id=@PoolId";
                        cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        object o = cmd.ExecuteScalar();
                        if (o == null || o == DBNull.Value)
                        {
                            throw new Exception($"PoolId [{poolId}] is not associated with any user in the TblUserPools table.");
                        }
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TOP 1 Measure FROM TblPoolScoreParameters WITH (NOLOCK) WHERE PoolId=@PoolId";
                        cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        object o = cmd.ExecuteScalar();
                        if (o == null || o == DBNull.Value)
                        {
                            throw new Exception($"There score calculation parameters (weights and scaling) for PoolId [{poolId}].");
                        }
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TOP 1 ScoreName FROM TblPoolScoreSQL WITH (NOLOCK) WHERE PoolId=@PoolId";
                        cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        object o = cmd.ExecuteScalar();
                        if (o == null || o == DBNull.Value)
                        {
                            throw new Exception($"Score calculation SQL has not ben generated for PoolId [{poolId}].");
                        }
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TOP 1 Id FROM TblWorkCandidates WITH (NOLOCK) WHERE PoolId=@PoolId";
                        cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                        object o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value)
                        {
                            throw new Exception($"Work candidate data already exists for PoolId [{poolId}].");
                        }
                    }

                    int N = 0;

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"INSERT INTO TblScenarios (ScenName, Description, DeaInputSproc, DeaOutputSproc, PoolId)
 VALUES (@ScenName, @Description, @DeaInputSproc, @DeaOutputSproc, @PoolId)";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@ScenName", SqlDbType.NVarChar, 50).Value = scenarioName;

                        if (description != null)
                        {
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = description;
                        }
                        else
                        {
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = DBNull.Value;
                        }

                        if (deaInputSproc != null)
                        {
                            cmd.Parameters.Add("@DeaInputSproc", SqlDbType.NVarChar, 255).Value = deaInputSproc;
                        }
                        else
                        {
                            cmd.Parameters.Add("@DeaInputSproc", SqlDbType.NVarChar, 255).Value = "dbo.UspGetProjectCandidates";
                        }

                        if (deaOutputSproc != null)
                        {
                            cmd.Parameters.Add("@DeaOutputSproc", SqlDbType.NVarChar, 255).Value = deaOutputSproc;
                        }
                        else
                        {
                            cmd.Parameters.Add("@DeaOutputSproc", SqlDbType.NVarChar, 255).Value = "dbo.UspGetScenarioOutput";
                        }

                        cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;

                        N = cmd.ExecuteNonQuery();
                    }

                    if (N > 0)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT TOP 1 Id FROM TblScenarios WITH (NOLOCK) WHERE PoolId=@PoolId ORDER BY ScenNo DESC";
                            cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                            object o = cmd.ExecuteScalar();
                            if (o == null || o == DBNull.Value)
                            {
                                throw new Exception("Scenario Id could nt be retrieved from the TblScenarios table.");
                            }

                            scenId = Guid.Parse(o.ToString());

                            Log.Info($"The Id of the newly created scenario is [{scenId}]");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                // Log.Error(errorMessage);
            }

            // Log.Info($"Create scenario [{scenarioName}] - finished.");

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
        public static bool ImportMaintainableAssetSegmentation(string dbConnectionString, string excelFilePath, string? worksheetName, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            ExcelHandler.Log = Log;
            ok = ExcelHandler.ImportMASFromExcelToDSS(dbConnectionString, excelFilePath, worksheetName, out errorMessage);

            return (ok);
        }

        public static bool ImportPAMSSuperAssets(string dbConnectionString, string excelFilePath, string? worksheetName, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            ExcelHandler.Log = Log;
            ok = ExcelHandler.ImportPAMSSuperAssets(dbConnectionString, excelFilePath, worksheetName, out errorMessage);
            return ok;
        }

        public static bool ClearPoolWorkCandidateData(string dbConnectionString, Guid poolId, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;


            try
            {
                Log.Info($"ClearPoolWorkCandidateData [{poolId}]  - started...");
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "UspDeletePoolWorkData";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 12000;
                        cmd.Parameters.Add("PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
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
            Log.Info($"ClearPoolWorkCandidateData [{poolId}]  - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }

        /// <summary>
        /// Creates new user pool and populates with default parameters
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="poolName">Name of the pool to be created. 50 characters maximum/</param>
        /// <param name="userId">UserId of the user for the pool to be associated with. User must exist/</param>
        /// <param name="poolId">(out) Id of the created pool.</param>
        /// <param name="errorMessage">(out) error message, null if no errors.</param>
        /// <returns>True on success, false on failure.</returns>
        public static bool CreateUserPool(string connectionString, string poolName, long userId, out Guid? poolId, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            poolId = null;

            Log.Info($"CreateUserPool [{poolName}] - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"SELECT TOP 1 Id FROM tbl_lib_UserLibraries WITH (NOLOCK)
 WHERE UPPER([Name]) = @PoolName AND UserId=@UserId";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@PoolName", SqlDbType.NVarChar, 50).Value = poolName;
                        cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                        object o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value)
                        {
                            throw new Exception($"Pool [{poolName}] of the user [{userId}] already exists.");
                        }
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.UspCreateUserPool";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@PoolName", SqlDbType.NVarChar, 50).Value = poolName;
                        cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                        SqlParameter parmPoolId = cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier);
                        parmPoolId.Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        poolId = Guid.Parse(parmPoolId.Value.ToString());
                        Log.Info($"Newly created poolId=[{poolId}]");
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"CreateUserPool [{poolName}] - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        /// <summary>
        /// Returns DssGisOutput object populated from scenario data
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="scenId">Scenario Id</param>
        /// <param name="RelativeEfficiencyThreshold">Relative efficiency lower threshold</param>
        /// <param name="gis">(out) populated DssGisOutput object</param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on syccess, False on failure</returns>
        public static bool GetGisJsonOutput(string connectionString, Guid scenId, double RelativeEfficiencyThreshold, out DssGisOutput? gis, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            gis = null;

             Log.Info($"GetGisJsonOutput scenId=[{scenId}] - started...");

            try
            {
                Dictionary<Guid,short> map = new Dictionary<Guid,short>();
                Dictionary<Guid,int> projMap = new Dictionary<Guid,int>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    DataSet ds = new DataSet();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "dbo.UspGetScenarioResultsForGIS";
                        cmd.CommandTimeout = 12000;
                        cmd.Parameters.Add("@ScenId", SqlDbType.UniqueIdentifier).Value = scenId;
                        cmd.Parameters.Add("@EffThreshold", SqlDbType.Float).Value = RelativeEfficiencyThreshold;
                        SqlDataAdapter a = new SqlDataAdapter(cmd);
                        a.Fill(ds);
                    }

                    // DIG 7/5/2024
                    // Attention!  Commented out lines are expected to be restored once the receieving GIS API has been adjusted.

                    gis = new DssGisOutput();

                    gis.Scenario.EfficiencyThreshold = RelativeEfficiencyThreshold;

                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        AssignInt(r, "ScenNo", ref gis.Scenario.ScenId);
                        AssignString(r, "ScenName", ref gis.Scenario.Name);
                        // AssignString(r, "Description", ref gis.Scenario.Description);
                        AssignString(r, "PoolName", ref gis.Scenario.PoolName);
                        AssignString(r, "User", ref gis.Scenario.User);
                        AssignDateTime(r, "LastRun", ref gis.Scenario.LastRun);
                        break;  // There should be only one record
                    }

                    foreach (DataRow r in ds.Tables[1].Rows)
                    {
                        DssProject p = new DssProject();

                        Guid? projectId = Guid.Parse(r["ProjectId"].ToString());
                        AssignInt(r, "ProjectNo", ref p.Id);

                        /*
                        AssignString(r, "PlanningPartner", ref p.Partner);
                        AssignInt(r, "District", ref p.District);
                        AssignInt(r, "Cnty", ref p.Cnty);
                        AssignString(r, "County", ref p.County);
                        AssignString(r, "Description", ref p.Description);

                        AssignInt(r, "MinYear", ref p.MinYear);
                        AssignInt(r, "MaxYear", ref p.MaxYear);
                        */
                        AssignInt(r, "MinYear", ref p.Year);        // DIG 7/5/2024 For now
                        AssignInt(r, "NumPave", ref p.NumPave);
                        AssignInt(r, "NumBridge", ref p.NumBridge);
                        AssignDouble(r, "Cost", ref p.Cost);
                        /*
                        AssignDouble(r, "Benefit", ref p.Benefit);
                        AssignDouble(r, "Efficiency", ref p.Efficiency);
                        */


                        if (p.NumBridge >0)
                        {
                            if (p.NumPave > 0)
                            {
                                p.ProjectType = 3;
                            }
                            else
                            {
                                p.ProjectType = 1;
                            }
                        }
                        else
                        {
                            p.ProjectType = 2;
                        }

                        if (projectId != null && p.Id != null)
                        {
                            map.Add(projectId.Value, p.ProjectType);
                            projMap.Add(projectId.Value, p.Id.Value);
                        }

                        gis.Projects.Add(p);
                    }

                    foreach (DataRow r in ds.Tables[2].Rows)
                    {
                        DssWorkCandidate w = new DssWorkCandidate();

                        Guid? projectId = Guid.Parse(r["ProjId"].ToString());

                        if (projectId != null)
                        {
                            w.ProjectType = map[projectId.Value];
                            w.ProjectId = projMap[projectId.Value];
                        }

                        AssignInt(r, "TreatNo", ref w.TreatmentId);
                        AssignString(r, "AssetType", ref w.AssetType);
                        AssignString(r, "TreatmentType", ref w.TreatmentType);
                        AssignString(r, "TreatmentName", ref w.TreatmentName);

                        AssignString(r, "Partner", ref w.Partner);
                        AssignInt(r, "District", ref w.District);
                        AssignInt(r, "Cnty", ref w.Cnty);
                        AssignString(r, "County", ref w.County);
                        AssignInt(r, "Route", ref w.Route);
                        AssignInt(r, "Direction", ref w.Direction);
                        AssignInt(r, "FromSection", ref w.FromSection);
                        AssignInt(r, "ToSection", ref w.ToSection);
                        if (w.AssetType == "B")
                        {
                            AssignString(r, "BRKEY", ref w.BRKEY);
                            AssignLong(r, "BMSID", ref w.BRIDGE_ID);
                        }
                        /*
                        if (w.AssetType == "P")
                        {
                            AssignString(r, "CRS", ref w.CRS);
                        }
                        */
                        AssignInt(r, "Year", ref w.Year);
                        AssignDouble(r, "Cost", ref w.Cost);
                        AssignDouble(r, "Benefit", ref w.Benefit);
                        AssignDouble(r, "BCRatio", ref w.BCRatio);
                        gis.WorkCandidates.Add(w);
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
                gis = null;
            }

            Log.Info($"GetGisJsonOutput scenId=[{scenId}] - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }


        /// <summary>
        /// Returns DssGisOutput object populated from scenario data as a JSON string
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="scenId">Scenario Id</param>
        /// <param name="RelativeEfficiencyThreshold">Relative efficiency lower threshold</param>
        /// <param name="gisJson">(out) JSON string /param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on syccess, False on failure</returns>
        public static bool GetGisJsonStringOutput(string connectionString, Guid scenId, double RelativeEfficiencyThreshold, bool Indented, out string? gisJson, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            gisJson = null;

            ok = GetGisJsonOutput(connectionString, scenId, RelativeEfficiencyThreshold,
                out DssGisOutput? gisOutput, out errorMessage);

            if (ok && gisOutput != null)
            {
                try
                {
                    gisJson = JsonConvert.SerializeObject(gisOutput, Indented ? Formatting.Indented : Formatting.None);
                }
                catch (Exception ex)
                {
                    ok = false;
                    errorMessage = ex.Message;
                    // Log.Error(errorMessage);
                    gisJson = null;
                }
            }

            return ok;
        }

        private static void AssignString(DataRow r, string colName, ref string? target)
        {
            if (r[colName] != null && r[colName] != DBNull.Value)
            {
                target = r[colName].ToString();
            }
        }

        private static void AssignDateTime(DataRow r, string colName, ref DateTime? target)
        {
            if (r[colName] != null && r[colName] != DBNull.Value)
            {
                target = Convert.ToDateTime(r[colName]);
            }
        }

        private static void AssignGuid(DataRow r, string colName, ref Guid? target)
        {
            if (r[colName] != null && r[colName] != DBNull.Value)
            {
                target = Guid.Parse(r[colName].ToString());
            }
        }

        private static void AssignInt(DataRow r, string colName, ref int? target)
        {
            if (r[colName] != null && r[colName] != DBNull.Value)
            {
                target = Convert.ToInt32(r[colName]);
            }
        }

        private static void AssignLong(DataRow r, string colName, ref long? target)
        {
            if (r[colName] != null && r[colName] != DBNull.Value)
            {
                target = Convert.ToInt64(r[colName]);
            }
        }

        private static void AssignDouble(DataRow r, string colName, ref double? target)
        {
            if (r[colName] != null && r[colName] != DBNull.Value)
            {
                target = Convert.ToDouble(r[colName]);
            }
        }

        /// <summary>
        /// Saves newky created work candidate and updates projects appropriately
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="wc">WorkCandidate object to be saved</param>
        /// <param name="workCandidateId">(out) Id of the newly created work candidate</param>
        /// <param name="projectCandidateId">(out) Id of the project to which work candidate belongs</param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on success, false on failure</returns>
        public static bool CreateNewWorkCandidate(string connectionString, WorkCandidateEntity wc, out Guid? workCandidateId, out Guid? projectCandidateId, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;
            workCandidateId = null;
            projectCandidateId = null;

            Log.Info("CreateNewWorkCandidate - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);

                    if (wc.ProjectCandidateId == null)
                    {
                        if (wc.ProjectName == null)
                        {
                            throw new Exception("Provided WorkCandidate object has no ProjectName specified. (ProjectName memeber is null).");
                        }

                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            // Match on ProjectName directly -- Description is "<ProjectName> - <first
                            // candidate desc>" (see UspUpdateProjectCandidates), never just the plain
                            // name, so comparing against it here always missed and gave every new
                            // candidate its own project even when its name matched an existing one.
                            cmd.CommandText = @"SELECT Id FROM tbl_dss_ProjectCandidates WITH (NOLOCK) WHERE
PoolId=@PoolId AND ProjectName=@Name";
                            cmd.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = wc.UserLibraryId;
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 250).Value = wc.ProjectName;
                            object o = cmd.ExecuteScalar();
                            if (o == null || o == DBNull.Value)
                            {
                                wc.ProjectCandidateId = Guid.NewGuid();
                                Log.InfoFormat("Project '{0}' does not exist. It will have to be created.  Its Id will be '{1}'.", SafeForLog(wc.ProjectName), wc.ProjectCandidateId);
                            }
                            else
                            {
                                wc.ProjectCandidateId = Guid.Parse(o.ToString());
                            }
                        }

                        using (BulkInserter bi = new BulkInserter())
                        {
                            ok = bi.Configure(conn, "tbl_dss_WorkCandidates", "TWC", null, null, out errorMessage);
                            if (!ok)
                            {
                                throw new Exception(errorMessage);
                            }

                            DataRow r = bi.NewRow();

                            workCandidateId = wc.Id;
                            if (workCandidateId == Guid.Empty)
                            {
                                workCandidateId = Guid.NewGuid();
                            }

                            projectCandidateId = wc.ProjectCandidateId;

                            r["Id"] = workCandidateId;
                            r["ImportSessionId"] = Guid.Empty;
                            r["PoolId"] = wc.UserLibraryId;
                            r["ImportTimeGeneratedId"] = Guid.Empty;

                            r["ProjectCandidateId"] = wc.ProjectCandidateId;
                            r["ProjectName"] = wc.ProjectName;

                            if (wc.District != null) { r["District"] = wc.District; }
                            if (wc.Cnty != null) { r["Cnty"] = wc.Cnty; }
                            if (wc.Route != null) { r["Route"] = wc.Route; }
                            if (wc.AssetType != null) { r["AssetType"] = wc.AssetType; }
                            if (wc.FromSection != null) { r["FromSection"] = wc.FromSection; }
                            if (wc.ToSection != null) { r["ToSection"] = wc.ToSection; }
                            if (wc.BRKEY != null) { r["BRKEY"] = wc.BRKEY; }
                            if (wc.BRIDGE_ID != null) { r["BRIDGE_ID"] = wc.BRIDGE_ID; }
                            if (wc.Description != null) { r["Description"] = wc.Description; }
                            if (wc.Cost != null) { r["Cost"] = wc.Cost; }
                            if (wc.HSIP_Eligible != null) { r["HSIP_Eligible"] = wc.HSIP_Eligible; }
                            if (wc.Excess_Safety_Value != null) { r["Excess_Safety_Value"] = wc.Excess_Safety_Value; }
                            if (wc.Supported_by_RSA_Safety_Study != null) { r["Supported_by_RSA_Safety_Study"] = wc.Supported_by_RSA_Safety_Study; }
                            if (wc.Degree_of_Safety_Improvement != null) { r["Degree_of_Safety_Improvement"] = wc.Degree_of_Safety_Improvement; }
                            if (wc.Supports_Hazard_Mitigation != null) { r["Supports_Hazard_Mitigation"] = wc.Supports_Hazard_Mitigation; }
                            if (wc.Improves_Access_to_Emergency_Services != null) { r["Improves_Access_to_Emergency_Services"] = wc.Improves_Access_to_Emergency_Services; }
                            if (wc.Travel_Time_Savings != null) { r["Travel_Time_Savings"] = wc.Travel_Time_Savings; }
                            if (wc.Operating_Cost_Savings != null) { r["Operating_Cost_Savings"] = wc.Operating_Cost_Savings; }
                            if (wc.Degree_of_Mobility_Improvement != null) { r["Degree_of_Mobility_Improvement"] = wc.Degree_of_Mobility_Improvement; }
                            if (wc.Freight_Improvement != null) { r["Freight_Improvement"] = wc.Freight_Improvement; }
                            if (wc.Facilitates_Transit_Use != null) { r["Facilitates_Transit_Use"] = wc.Facilitates_Transit_Use; }
                            if (wc.Facilitates_Active_Transportation != null) { r["Facilitates_Active_Transportation"] = wc.Facilitates_Active_Transportation; }
                            if (wc.Bike_Lanes_Constructed != null) { r["Bike_Lanes_Constructed"] = wc.Bike_Lanes_Constructed; }
                            if (wc.New_Cyclists != null) { r["New_Cyclists"] = wc.New_Cyclists; }
                            if (wc.Support_for_Job_Growth != null) { r["Support_for_Job_Growth"] = wc.Support_for_Job_Growth; }
                            if (wc.Support_for_Tourism != null) { r["Support_for_Tourism"] = wc.Support_for_Tourism; }
                            if (wc.Support_for_Economic_Goals != null) { r["Support_for_Economic_Goals"] = wc.Support_for_Economic_Goals; }
                            if (wc.Support_for_Recreation != null) { r["Support_for_Recreation"] = wc.Support_for_Recreation; }
                            if (wc.Consistency_with_Existing_Plans != null) { r["Consistency_with_Existing_Plans"] = wc.Consistency_with_Existing_Plans; }
                            if (wc.Sidewalks_and_Curb_Ramps_Constructed_or_Improved != null) { r["Sidewalks_and_Curb_Ramps_Constructed_or_Improved"] = wc.Sidewalks_and_Curb_Ramps_Constructed_or_Improved; }
                            if (wc.Change_in_Accessible_Destinations != null) { r["Change_in_Accessible_Destinations"] = wc.Change_in_Accessible_Destinations; }
                            if (wc.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations != null) { r["Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations"] = wc.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations; }
                            if (wc.Support_for_Environmental_Justice != null) { r["Support_for_Environmental_Justice"] = wc.Support_for_Environmental_Justice; }
                            if (wc.Predicted_Equity_and_Access_Benefit != null) { r["Predicted_Equity_and_Access_Benefit"] = wc.Predicted_Equity_and_Access_Benefit; }
                            if (wc.Equity_and_Access_Score != null) { r["Equity_and_Access_Score"] = wc.Equity_and_Access_Score; }
                            if (wc.Reduced_Flood_Closure_Risk != null) { r["Reduced_Flood_Closure_Risk"] = wc.Reduced_Flood_Closure_Risk; }
                            if (wc.Wetlands_Improved != null) { r["Wetlands_Improved"] = wc.Wetlands_Improved; }
                            if (wc.Wildlife_Crossings != null) { r["Wildlife_Crossings"] = wc.Wildlife_Crossings; }
                            if (wc.Reduced_Fuel_Consumption != null) { r["Reduced_Fuel_Consumption"] = wc.Reduced_Fuel_Consumption; }
                            if (wc.Degree_of_Environmental_Improvement != null) { r["Degree_of_Environmental_Improvement"] = wc.Degree_of_Environmental_Improvement; }
                            if (wc.Consistency_with_Land_Use_Plans != null) { r["Consistency_with_Land_Use_Plans"] = wc.Consistency_with_Land_Use_Plans; }
                            if (wc.Pavement_Rehabilitated_or_Reconstructed != null) { r["Pavement_Rehabilitated_or_Reconstructed"] = wc.Pavement_Rehabilitated_or_Reconstructed; }
                            if (wc.Bridges_Rehabilitated_or_Replaced != null) { r["Bridges_Rehabilitated_or_Replaced"] = wc.Bridges_Rehabilitated_or_Replaced; }
                            if (wc.Culverts_Rehabilitated_or_Replaced != null) { r["Culverts_Rehabilitated_or_Replaced"] = wc.Culverts_Rehabilitated_or_Replaced; }
                            if (wc.Guardrail_Rehabilitated_or_Replaced != null) { r["Guardrail_Rehabilitated_or_Replaced"] = wc.Guardrail_Rehabilitated_or_Replaced; }
                            if (wc.Geotechnical_Assets_Rehabilitated_or_Replaced != null) { r["Geotechnical_Assets_Rehabilitated_or_Replaced"] = wc.Geotechnical_Assets_Rehabilitated_or_Replaced; }
                            if (wc.Facilities_Rehabilitated_or_Reconstructed != null) { r["Facilities_Rehabilitated_or_Reconstructed"] = wc.Facilities_Rehabilitated_or_Reconstructed; }
                            if (wc.Sidewalks_Rehabilitated_or_Reconstructed != null) { r["Sidewalks_Rehabilitated_or_Reconstructed"] = wc.Sidewalks_Rehabilitated_or_Reconstructed; }
                            if (wc.ADT != null) { r["ADT"] = wc.ADT; }
                            if (wc.Percent_Trucks != null) { r["Percent_Trucks"] = wc.Percent_Trucks; }
                            if (wc.Population_Density != null) { r["Population_Density"] = wc.Population_Density; }
                            if (wc.Bike_Commute_Share != null) { r["Bike_Commute_Share"] = wc.Bike_Commute_Share; }
                            if (wc.BPN != null) { r["BPN"] = wc.BPN; }
                            if (wc.Length != null) { r["Length"] = wc.Length; }
                            if (wc.Speed_Limit != null) { r["Speed_Limit"] = wc.Speed_Limit; }
                            if (wc.Detour_Distance != null) { r["Detour_Distance"] = wc.Detour_Distance; }
                            if (wc.Functional_Classification != null) { r["Functional_Classification"] = wc.Functional_Classification; }
                            if (wc.National_Highway_System != null) { r["National_Highway_System"] = wc.National_Highway_System; }
                            if (wc.Pennsylvania_Byway != null) { r["Pennsylvania_Byway"] = wc.Pennsylvania_Byway; }
                            if (wc.Bicycle_PA_Route != null) { r["Bicycle_PA_Route"] = wc.Bicycle_PA_Route; }
                            if (wc.Interstate_Emergency_Detour != null) { r["Interstate_Emergency_Detour"] = wc.Interstate_Emergency_Detour; }
                            if (wc.LFAR_BOF_Eligible != null) { r["LFAR_BOF_Eligible"] = wc.LFAR_BOF_Eligible; }
                            if (wc.Treatment != null) { r["Treatment"] = wc.Treatment; }
                            if (wc.Year != null) { r["Year"] = wc.Year; }
                            if (wc.MinYear != null) { r["MinYear"] = wc.MinYear; }
                            if (wc.MaxYear != null) { r["MaxYear"] = wc.MaxYear; }
                            if (wc.CreatedBy != null) { r["CreatedBy"] = wc.CreatedBy; }
                            if (wc.CreatedAt != null) { r["CreatedAt"] = wc.CreatedAt; }

                            ok = bi.AddRow(r, out errorMessage);
                            if (!ok)
                            {
                                throw new Exception(errorMessage);
                            }
                        }

                        using(SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "dbo.UspUpdateWorkCandidate";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@WorkCandidateId", SqlDbType.UniqueIdentifier).Value = workCandidateId;
                            cmd.CommandTimeout = 12000;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"CreateNewWorkCandidate - ended.  Ok={ok}, ErrorMessage: '{errorMessage}'");
            return ok;
        }

        #endregion

    }

}
