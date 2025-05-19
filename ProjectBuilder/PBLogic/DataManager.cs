using log4net;
using log4net.Appender;
using log4net.Layout;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

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
                        string sql = $"DELETE FROM {r[0]} WHERE ScenId={@ScenId}";
                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandTimeout = 480000;
                            long N = command.ExecuteNonQuery();
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
        public bool CopyScenario(out int newScenId, out string errorMessage)
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
        public bool SaveScenarioInputsToXml(string xmlFileName, out string errorMessage)
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

            Log.Info($"CreateNewScenario (static) from the file {xmlFilePath} - started...");

            DataManager dataManager = new DataManager()
            {
                ConnectionString = connectionString
            };

            ok = dataManager.CreateNewScenario(xmlFilePath, out newScenId, out errorMessage);

            Log.Info($"CreateNewScenario (static) from the file {xmlFilePath} - finished.  NewScenId={newScenId}  OK={ok}  Error Message: {errorMessage}");

            return (ok);
        }
#endif
        #endregion




        public bool CreateNewScenario(string scenarioName, string libraryId, int firstYear, int lastYear, int setDefault, out int newScenId, out string errorMessage, string CreatedBy)
        {
            bool ok = true;
            newScenId = -1;
            errorMessage = null;

            Log.Info($"CreateNewScenario '{scenarioName}' for {firstYear}{lastYear} - started...");

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

            Log.Info($"CreateNewScenario '{scenarioName}' for {firstYear}{lastYear} - ended. OK={ok}, ErrorMessage='{errorMessage}'");

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

            Log.Info($"SaveScenarioStatisticsToXmlFile (static) ScenId={scenId}, FileName: {xmlFileName} - started...");

            DataManager dataManager = new DataManager()
            {
                ConnectionString = connectionString,
                HomeDirectory = homeDirectory,
                ScenId = scenId
            };

            ok = dataManager.SaveScenarioStatisticsToXmlFile(xmlFileName, out errorMessage);

            Log.Info($"SaveScenarioStatisticsToXmlFile (static) ScenId={scenId}, FileName: {xmlFileName} - Finished. OK={ok}\tError message: {errorMessage}");


            return ok;
        }

       


        public static bool CreateNewScenario(string connectionString, string libraryId, string scenarioName, int firstYear, int lastYear, int setDefault, out int newScenId, out string errorMessage, string CreatedBy = "")
        {
            bool ok = true;
            newScenId = -1;
            errorMessage = null;

            Log.Info($"CreateNewScenario (static) '{scenarioName}' for {firstYear}{lastYear} - started...");

            DataManager manager = new DataManager()
            {
                ConnectionString = connectionString
            };

            ok = manager.CreateNewScenario(scenarioName, libraryId, firstYear, lastYear, setDefault, out newScenId, out errorMessage, CreatedBy);

            Log.Info($"CreateNewScenario (static) '{scenarioName}' for {firstYear}{lastYear} - ended. OK={ok}, ErrorMessage='{errorMessage}'");

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
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                        command.CommandText = "dbo.sp_pb_CreateExtendedProjects";
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_PopulateExtendedTreatments";
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
        public static bool CreateNewUserLibrary(string dbConnectionString, long userId, string libraryName, string libraryDescription, bool shared, out string libraryId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            libraryId = null;

            Log.Info($"CreateNewUserLibrary ({userId},'{libraryName}','{libraryDescription}') - started...");

            try
            {
                using(SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Generic_InfoMessage);
                    using(SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_CreateNewUserLibrary";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout=120000;
                        command.Parameters.Add("@UserId", SqlDbType.BigInt).Value = userId;
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
            catch(Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"CreateNewUserLibrary ({userId},'{libraryName}','{libraryDescription}') - finished. Ok={ok}, ErrorMessage: '{errorMessage}'");

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
            catch(Exception ex)
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
            string sourceLibraryId = null,
            string assetType = null, 
            int? district=null, short? cnty=null, int? route=null, 
            string simulationId=null,
            string networkId=null, int? minYear=null, int? maxYear=null
           )
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"PopulateUserLibrary ({libraryId}, {fromScratch}) - started...");

            try
            {
                using(SqlConnection conn = new SqlConnection(dbConnectionString))
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
            catch(Exception ex)
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
            string brkey = null,
            string assetId = null,
            string assetName = null, 
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
                using(SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using(SqlCommand cmd = conn.CreateCommand())
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

                        if (string.IsNullOrEmpty(assetName)) {
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
            catch(Exception ex)
            {
                ok = false;
                treatmentTimeGeneratedId = null;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            return (ok);
        }
       
#endregion

    }

}
