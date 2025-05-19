using log4net;
using log4net.Appender;
using log4net.Layout;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace PBLogic
{
    public partial class SymphonyConductor
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

        public string ConnectionString { get; set; }
        public int ScenId { get; set; }

        public string Code = "NONE";

        public string HomeDirectory { get; set; }

        public SymphonyConductor()
        {

        }

        /// <summary>
        /// Logs parameters.  Does not log connection string in Release mode
        /// </summary>
        private void LogParameters()
        {
#if DEBUG
            Log.Info($"Connection string:\t{ConnectionString}");
#endif
            Log.Info($"Home directory:\t{HomeDirectory}");
            Log.Info($"ScenID:\t{ScenId}");
        }





        /// <summary>
        /// Populates tbl_pb_LPAlternatives table
        /// </summary>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on success, False on falure</returns>
        public bool CreateAlternativesTable(out string errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"CreateAlternativesTable (ScenID={ScenId} - Started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                    using (SqlCommand command = new SqlCommand("dbo.sp_pb_CreateExtendedAlternatives", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 240000;
                        command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
                         int N = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"CreateAlternativesTable - Finished.\tOK={ok}\tError message: {errorMessage}");
            return ok;
        }

        /// <summary>
        /// Populates tbl_pb_AlternativesMatrix table
        /// </summary>
        /// <param name="errorMessage">Error message, null if no errors</param>
        /// <returns>True on success, False on failure</returns>
        public bool CreateAlternativesMatrix(bool doTreatments, int District, out string errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"CreateAlternativesMatrix (ScenID={ScenId}, Code='{Code}' - Started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        if (doTreatments)
                        {
                            command.CommandText = "dbo.sp_pb_CreateTreatmentAlternativesMatrix";
                        }
                        else
                        {
                            command.CommandText = "dbo.sp_pb_CreateAlternativesMatrix";
                        }
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 240000;
                        command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
                        command.Parameters.Add("@Code", SqlDbType.VarChar, 4).Value = Code;
                        if (doTreatments)
                        {
                            command.Parameters.Add("@District", SqlDbType.Int).Value = District;
                        }
                        int N = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"CreateAlternativesMatrix - Finished.\tOK={ok}\tError message: {errorMessage}");
            return ok;
        }

        /// <summary>
        /// Populates tbl_pb_AlternativesMatrix table
        /// </summary>
        /// <param name="errorMessage">Error message, null if no errors</param>
        /// <returns>True on success, False on failure</returns>
        public bool CreateAlternativesMatrix(out string errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"CreateExtendedAlternativesMatrix (ScenID={ScenId}, Code='{Code}' - Started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_CreateExtendedAlternativesMatrix";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 240000;
                        command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
                        int N = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"CreateAlternativesMatrix - Finished.\tOK={ok}\tError message: {errorMessage}");
            return ok;
        }

        /// <summary>
        /// Sets alternative selection flags in tbl_pb_Alternatives
        /// </summary>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on success, False on failure</returns>
        public bool ProcessLPSolution(bool doBinary, int iteration, out string errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"ProcessLPSolution doBinary={doBinary} - Started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = doBinary ? "dbo.sp_pb_ProcessLPSolutionStrict"
                                                       : "dbo.sp_pb_ProcessLPSolutionRelaxed";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 240000;
                        command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
                        command.Parameters.Add("@Iter", SqlDbType.Int).Value = iteration;
                        int N = command.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_ComputeExtendedBudgetSpent";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 240000;
                        command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
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

            Log.Info($"ProcessLPSolution ProcessLPSolution doBinary={doBinary} - Finished.\tOK={ok}\tError message: {errorMessage}");
            return ok;
        }


        /// <summary>
        /// Collects statistics for all utility combinations
        /// </summary>
        /// <param name="doTraining">When set to True statistics by district is not colected</param>
        /// <param name="errorMessage">Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool CollectScenarioStatistics(bool doTraining, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("CollectScenarioStatistics - Started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                    using (SqlCommand command = new SqlCommand("dbo.sp_pb_CollectScenarioStatistics", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120000;
                        command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
                        int N = command.ExecuteNonQuery();
                    }

                    if (!doTraining)
                    {
                        using (SqlCommand command = new SqlCommand("dbo.sp_pb_CollectScenarioStatisticsByDistrict", conn))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandTimeout = 120000;
                            command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
                            int N = command.ExecuteNonQuery();
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

            Log.Info($"CollectScenarioStatistics - Finished.\tOK={ok}\tError message: {errorMessage}");
            return ok;
        }



      
        public bool GenerateMpsFileForMultiYearProjects(ScenarioCommunique filter, bool AddBinaryBounds, out string mpsFilePath,
            out string errorMessage, out bool degenerateCase)
        {
            bool ok = true;
            errorMessage = null;
            mpsFilePath = null;
            degenerateCase = false;
            string sql;

            Log.Info("GenerateMpsFile - started");
            LogParameters();

            mpsFilePath = Path.Combine(HomeDirectory, $"LP\\SC_{ScenId}.mps");
            Log.Info($"Generating file {mpsFilePath}");
            Log.Info($"Filter: UserOrCommitment={filter.Commitment}, ProjectsOnly={filter.ProjectsOnly}, District={filter.District}, MaxPriority={filter.MaxPriority}");

            try
            {
                using (StreamWriter sw = File.CreateText(mpsFilePath))
                {
                    sw.WriteLine("NAME SC_{0}_{1}", ScenId, Code);
                    Log.InfoFormat("MPS file path:\t{0}", mpsFilePath);

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();

                        sw.WriteLine("ROWS");
                        sw.WriteLine(" N UTILITY");
                        /* Budget constraints */

                        sql = $@"SELECT ConstrName 
FROM vw_pb_ExtendedScenarioBudgetConstraints v WITH (NOLOCK)
WHERE ScenID={ScenId} ";

                        if (filter.MixAssetBudgets)
                        {
                            sql += @"
AND CGroup=3";
                        }
                        else
                        {
                            sql += @"
AND CGroup IN (1,2)";
                        }

                        sql += $@"
ORDER BY ConstrName";
                        Log.Debug(sql);

                        int j = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" L {reader["ConstrName"]}");
                                    j++;
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of constraint rows written: {j}");


                        /* Projects */

                        sql = $@"SELECT DISTINCT 'X'+CONVERT(VARCHAR(20),AltReferenceId) AS RefId
FROM tbl_pb_ExtendedAlternatives a WITH (NOLOCK)
WHERE ScenId={ScenId}
AND NOT EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedAlternatives b WITH (NOLOCK)
				  WHERE b.ScenId=a.ScenId 
                    AND b.AltReferenceId=a.AltReferenceId
					AND Selected=1)";

                        if (filter.Commitment)
                        {
                            sql += @"
AND (AltReferenceOrigin LIKE 'U%' OR HasCommitted=1) ";
                        }
                        if (filter.MaxPriority.HasValue)
                        {
                            sql += $@"
AND (PriorityOrder <= {filter.MaxPriority}) ";
                        }
                        if (filter.District.HasValue)
                        {
                            sql += $@"
AND (District = {filter.District}) ";
                        }

                        sql += @"
ORDER BY 1";
                        Log.Debug(sql);

                        int k = 0;
                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" E {reader[0]}");
                                    k++;
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of project rows written: {k}");

                        if (k <= 0)
                        {
                            degenerateCase = true;
                            throw new Exception("No treatment alternatives left for selection.");
                        }

                        sw.WriteLine("COLUMNS");

                        sql = $@"SELECT AltNo, ConstrName, ConstrNameR
    , 'X' + CONVERT(VARCHAR(20), ProjectId) AS RefId, Cost, Utility
FROM  vw_pb_MPS_Image v WITH (NOLOCK)
WHERE ScenId={ScenId}
  AND NOT EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedAlternatives b WITH (NOLOCK)
				  WHERE b.ScenId=v.ScenId
				    AND b.AltReferenceId=v.ProjectId
					AND Selected=1) ";

                      
                        if (filter.Commitment)
                        {
                            sql += @"
    AND (AltReferenceOrigin LIKE 'U%' OR HasCommitted=1) ";
                        }
                        if (filter.MaxPriority.HasValue)
                        {
                            sql += $@"
    AND (PriorityOrder <= {filter.MaxPriority}) ";
                        }
                        if (filter.District.HasValue)
                        {
                            sql += $@"
    AND (District = {filter.District}) ";
                        }

                        sql += @"
ORDER BY 1";

                        Log.Debug(sql);

                        j = 0;
                        k = 0;
                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                long prevAltNo = 0;
                                while (reader.Read())
                                {
                                    long altNo = Convert.ToInt64(reader["AltNo"]);
                                    string constrName = reader["ConstrName"].ToString();
                                    string constrNameR = reader["ConstrNameR"].ToString();
                                    string refId = reader["RefId"].ToString();
                                    double cost = Convert.ToDouble(reader["Cost"]);
                                    double util = Convert.ToDouble(reader["Utility"]);

                                    if (prevAltNo < altNo)
                                    {
                                        sw.WriteLine($" {altNo} {refId} 1 UTILITY {-util}");
                                        prevAltNo = altNo;
                                        j++;
                                    }

                                    if (altNo % 100 > 0)
                                    {
                                        if (cost >= 0.01)
                                        {
                                            if (filter.MixAssetBudgets)
                                            {
                                                sw.WriteLine($" {altNo} {constrNameR} {cost}");
                                            }
                                            else
                                            {
                                                sw.WriteLine($" {altNo} {constrName} {cost}");
                                            }
                                            k++;
                                        }
                                    }
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of utility columns written: {j}");
                        Log.Debug($"# of cost columns written: {k}");

                        sw.WriteLine("RHS");
                        sql = sql = $@"SELECT ConstrName, RemainingBudget
FROM vw_pb_ExtendedScenarioBudgetConstraints WITH (NOLOCK)
WHERE ScenId={ScenId} ";

                        if (filter.MixAssetBudgets)
                        {
                            sql += @"
AND CGroup=3";
                        }
                        else
                        {
                            sql += @"
AND CGroup IN (1,2)";
                        }
                        sql += @"
ORDER BY ConstrName";

                        Log.Debug(sql);
                        j = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    double cost = Convert.ToDouble(reader[1]);
                                    if (cost >= 0.1)
                                    {
                                        sw.WriteLine($" RHSLE {reader[0]} {cost}");
                                        j++;
                                    }
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of cost RHSLE written: {j}");

                        sql = $@"SELECT DISTINCT 'X'+CONVERT(VARCHAR(20),AltReferenceId) AS RefId
FROM tbl_pb_ExtendedAlternatives a WITH (NOLOCK)
WHERE ScenId={ScenId} 
AND NOT EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedAlternatives b WITH (NOLOCK)
				  WHERE b.ScenId=a.ScenId
              	    AND b.AltReferenceId=a.AltReferenceId
					AND Selected=1)";


                        if (filter.Commitment)
                        {
                            sql += @"
AND (AltReferenceOrigin LIKE 'U%' OR HasCommitted=1) ";
                        }
                        if (filter.MaxPriority.HasValue)
                        {
                            sql += $@"
AND (PriorityOrder <= {filter.MaxPriority}) ";
                        }
                        if (filter.District.HasValue)
                        {
                            sql += $@"
AND (District = {filter.District}) ";
                        }

                        sql += @"
ORDER BY 1";

                        Log.Debug(sql);

                        j = 0;
                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" RHSLE {reader[0]} 1");
                                    j++;
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of project RHSLE written: {j}");

                        if (AddBinaryBounds)
                        {
                            sw.WriteLine("BOUNDS");

                            sql = $@"SELECT DISTINCT AltNo
FROM tbl_pb_ExtendedAlternatives a WITH (NOLOCK)
WHERE ScenId={ScenId}
AND NOT EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedAlternatives b WITH (NOLOCK)
				  WHERE b.ScenId=a.ScenId
				    AND b.AltReferenceId=a.AltReferenceId
					AND Selected=1)";
                          
                            if (filter.Commitment)
                            {
                                sql += @"
AND (AltReferenceOrigin LIKE 'U%' OR HasCommitted=1) ";
                            }
                            if (filter.MaxPriority.HasValue)
                            {
                                sql += $@"
AND (PriorityOrder <= {filter.MaxPriority}) ";
                            }
                            if (filter.District.HasValue)
                            {
                                sql += $@"
AND (District = {filter.District}) ";
                            }

                            sql += @"
ORDER BY 1";

                            Log.Debug(sql);
                            j = 0;

                            using (SqlCommand command = new SqlCommand(sql, conn))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        sw.WriteLine($" UI BOUND {reader[0]} 1");
                                        j++;
                                    }
                                    reader.Close();
                                }
                            }

                            Log.Debug($"# of UI BOUND written: {j}"); ;
                        }
                    }

                  

                    sw.WriteLine("ENDATA");
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                if (!degenerateCase)
                {
                    ok = false;
                }
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.InfoFormat("GenerateMpsFile - ended\tOK={0};\tError message: {1}", ok, errorMessage);
            return ok;
        }

        #region Obsolete

        public bool GenerateMpsFile(ScenarioCommunique filter, bool AddBinaryBounds, out string mpsFilePath,
            out string errorMessage, out bool degenerateCase)
        {
            bool ok = true;
            errorMessage = null;
            mpsFilePath = null;
            degenerateCase = false;
            string sql;

            Log.Info("GenerateMpsFile - started");
            LogParameters();

            mpsFilePath = Path.Combine(HomeDirectory, $"LP\\SC_{ScenId}.mps");
            Log.Info($"Generating file {mpsFilePath}");
            Log.Info($"Filter: UserOrCommitment={filter.Commitment}, ProjectsOnly={filter.ProjectsOnly}, District={filter.District}, MaxPriority={filter.MaxPriority}");

            try
            {
                using (StreamWriter sw = File.CreateText(mpsFilePath))
                {
                    sw.WriteLine("NAME SC_{0}_{1}", ScenId, Code);
                    Log.InfoFormat("MPS file path:\t{0}", mpsFilePath);

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();

                        sw.WriteLine("ROWS");
                        sw.WriteLine(" N UTILITY");
                        /* Budget constraints */

                        sql = $@"SELECT ConstrName 
FROM vw_pb_Extended_LP_RHS v WITH (NOLOCK)
WHERE ScenID={ScenId} ";

                        if (filter.MixAssetBudgets)
                        {
                            sql += @"
AND CGroup=3";
                        }
                        else
                        {
                            sql += @"
AND CGroup IN (1,2)";
                        }

                        sql += $@"
ORDER BY ConstrName";
                        Log.Debug(sql);

                        int j = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" L {reader["ConstrName"]}");
                                    j++;
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of constraint rows written: {j}");


                        /* Projects */

                        sql = $@"SELECT DISTINCT 'X'+CONVERT(VARCHAR(20),AltReferenceId) AS RefId
FROM tbl_pb_ExtendedAlternatives a WITH (NOLOCK)
WHERE ScenId={ScenId}
AND NOT EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedAlternatives b WITH (NOLOCK)
				  WHERE b.ScenId=a.ScenId 
                    AND b.AltReferenceId=a.AltReferenceId
					AND Selected=1)";

                        if (filter.Commitment)
                        {
                            sql += @"
AND (AltReferenceOrigin LIKE 'U%' OR HasCommitted=1) ";
                        }
                        if (filter.MaxPriority.HasValue)
                        {
                            sql += $@"
AND (PriorityOrder <= {filter.MaxPriority}) ";
                        }
                        if (filter.District.HasValue)
                        {
                            sql += $@"
AND (District = {filter.District}) ";
                        }

                        sql += @"
ORDER BY 1";
                        Log.Debug(sql);

                        int k = 0;
                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" E {reader[0]}");
                                    k++;
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of project rows written: {k}");

                        if (k <= 0)
                        {
                            degenerateCase = true;
                            throw new Exception("No treatment alternatives left for selection.");
                        }


                        sw.WriteLine("COLUMNS");
                        sql = $@"SELECT AltNo, ConstrName, 
	'X'+CONVERT(VARCHAR(20),AltReferenceId) AS RefId, 
	Cost, Utility
FROM (
    SELECT a.AltNo, a.District, a.AltReferenceId, a.HasCommitted, a.PriorityOrder, 
	    ISNULL(Cost,0) AS Cost, a.Utility, a.ConstrName, a.CGroup
    FROM tbl_pb_ExtendedAlternativeMatrix a WITH(NOLOCK)
    WHERE a.ScenID={ScenId}
    UNION ALL
    SELECT a.AltNo, a.District, a.AltReferenceId, a.HasCommitted, a.PriorityOrder,
	    0 AS Cost, 0 AS Utility, NULL AS ConstrName,  0 AS CGroup
    FROM tbl_pb_ExtendedAlternatives a WITH (NOLOCK)
    WHERE  a.ScenID={ScenId} AND YearWork=-1
) q
WHERE NOT EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedAlternatives b WITH (NOLOCK)
				  WHERE b.ScenId={ScenId}
				    AND b.AltReferenceId=q.AltReferenceId
					AND Selected=1) ";

                        if (filter.MixAssetBudgets)
                        {
                            sql += @"
AND CGroup IN (0,3)";
                        }
                        else
                        {
                            sql += @"
AND CGroup IN (0,1,2)";
                        }

                        if (filter.Commitment)
                        {
                            sql += @"
AND (AltReferenceOrigin LIKE 'U%' OR HasCommitted=1) ";
                        }
                        if (filter.MaxPriority.HasValue)
                        {
                            sql += $@"
AND (PriorityOrder <= {filter.MaxPriority}) ";
                        }
                        if (filter.District.HasValue)
                        {
                            sql += $@"
AND (District = {filter.District}) ";
                        }

                        sql += @"
ORDER BY 1";

                        Log.Debug(sql);

                        j = 0;
                        k = 0;
                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                long prevAltNo = 0;
                                while (reader.Read())
                                {
                                    long altNo = Convert.ToInt64(reader[0]);
                                    string constrName = reader[1].ToString();
                                    string refId = reader[2].ToString();
                                    double cost = Convert.ToDouble(reader[3]);
                                    double util = Convert.ToDouble(reader[4]);

                                    if (prevAltNo < altNo)
                                    {
                                        sw.WriteLine($" {altNo} {refId} 1 UTILITY {-util}");
                                        prevAltNo = altNo;
                                        j++;
                                    }
                                    if (altNo % 100 > 0)
                                    {
                                        if (cost >= 0.01)
                                        {
                                            sw.WriteLine($" {altNo} {constrName} {cost}");
                                            k++;
                                        }
                                    }
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of utility columns written: {j}");
                        Log.Debug($"# of cost columns written: {k}");

                        sw.WriteLine("RHS");
                        sql = sql = $@"SELECT ConstrName, RemainingBudget
FROM vw_pb_Extended_LP_RHS WITH (NOLOCK)
WHERE ScenId={ScenId} ";

                        if (filter.MixAssetBudgets)
                        {
                            sql += @"
AND CGroup=3";
                        }
                        else
                        {
                            sql += @"
AND CGroup IN (1,2)";
                        }
                        sql += @"
ORDER BY ConstrName";

                        Log.Debug(sql);
                        j = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    double cost = Convert.ToDouble(reader[1]);
                                    if (cost >= 0.1)
                                    {
                                        sw.WriteLine($" RHSLE {reader[0]} {cost}");
                                        j++;
                                    }
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of cost RHSLE written: {j}");

                        sql = $@"SELECT DISTINCT 'X'+CONVERT(VARCHAR(20),AltReferenceId) AS RefId
FROM tbl_pb_ExtendedAlternatives a WITH (NOLOCK)
WHERE ScenId={ScenId} 
AND NOT EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedAlternatives b WITH (NOLOCK)
				  WHERE b.ScenId=a.ScenId
              	    AND b.AltReferenceId=a.AltReferenceId
					AND Selected=1)";


                        if (filter.Commitment)
                        {
                            sql += @"
AND (AltReferenceOrigin LIKE 'U%' OR HasCommitted=1) ";
                        }
                        if (filter.MaxPriority.HasValue)
                        {
                            sql += $@"
AND (PriorityOrder <= {filter.MaxPriority}) ";
                        }
                        if (filter.District.HasValue)
                        {
                            sql += $@"
AND (District = {filter.District}) ";
                        }

                        sql += @"
ORDER BY 1";

                        Log.Debug(sql);

                        j = 0;
                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" RHSLE {reader[0]} 1");
                                    j++;
                                }
                                reader.Close();
                            }
                        }

                        Log.Debug($"# of project RHSLE written: {j}");

                        if (AddBinaryBounds)
                        {
                            sw.WriteLine("BOUNDS");

                            sql = $@"SELECT DISTINCT AltNo
FROM tbl_pb_ExtendedAlternatives a WITH (NOLOCK)
WHERE ScenId={ScenId}
AND NOT EXISTS (SELECT TOP 1 1 FROM tbl_pb_ExtendedAlternatives b WITH (NOLOCK)
				  WHERE b.ScenId=a.ScenId
				    AND b.AltReferenceId=a.AltReferenceId
					AND Selected=1)";

                            if (filter.Commitment)
                            {
                                sql += @"
AND (AltReferenceOrigin LIKE 'U%' OR HasCommitted=1) ";
                            }
                            if (filter.MaxPriority.HasValue)
                            {
                                sql += $@"
AND (PriorityOrder <= {filter.MaxPriority}) ";
                            }
                            if (filter.District.HasValue)
                            {
                                sql += $@"
AND (District = {filter.District}) ";
                            }

                            sql += @"
ORDER BY 1";

                            Log.Debug(sql);
                            j = 0;

                            using (SqlCommand command = new SqlCommand(sql, conn))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        sw.WriteLine($" UI BOUND {reader[0]} 1");
                                        j++;
                                    }
                                    reader.Close();
                                }
                            }

                            Log.Debug($"# of UI BOUND written: {j}"); ;
                        }
                    }



                    sw.WriteLine("ENDATA");
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                if (!degenerateCase)
                {
                    ok = false;
                }
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.InfoFormat("GenerateMpsFile - ended\tOK={0};\tError message: {1}", ok, errorMessage);
            return ok;
        }
        /// <summary>
        /// Generates MPS file for Symphony
        /// </summary>
        /// <param name="District">If it has value then MPS formulation gets created for just this district</param>
        /// <param name="mpsFilePath">(out) full pathname of the generated MPS file</param>
        /// <param name="AddBinaryBounds">If true then a strict binary formulation is produced,  otherwise - relaxed</param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns></returns>

        public bool GenerateMpsFile(bool bIgnoreBPConstraints, bool AddBinaryBounds, out string mpsFilePath, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            mpsFilePath = null;
            string sql;
            double dMillion = 1000000;

            Log.Info("GenerateMpsFile - started");
            LogParameters();

            mpsFilePath = Path.Combine(HomeDirectory, $"LP\\SC_{ScenId}_{Code}.mps");
            Log.Info($"Generating file {mpsFilePath}");

            if (bIgnoreBPConstraints)
            {
                Log.Info("Ignoring separate constraints for bridge and pavement budgets.");
            }

            try
            {
                using (StreamWriter sw = File.CreateText(mpsFilePath))
                {
                    sw.WriteLine("NAME SC_{0}_{1}", ScenId, Code);
                    Log.InfoFormat("MPS file path:\t{0}", mpsFilePath);

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();

                        sw.WriteLine("ROWS");
                        sw.WriteLine(" N UTILITY");
                        /* Budget constraints */

                        sql = $@"SELECT ConstrName, CGroup FROM vw_pb_RemainingScenBudgetConstraints v WITH(NOLOCK)
WHERE ScenID={ScenId} AND Code='{Code}' ORDER BY YearWork, CGroup";
                        Log.Debug(sql);

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int cGroup = Convert.ToInt32(reader["CGroup"]); // cGroup=13 means greater than

                                    if (bIgnoreBPConstraints && cGroup % 10 != 3)
                                    {
                                        continue;    // Do not add equations for bridges and pavements
                                    }
                                    bool bMinimum = (cGroup == 11 || cGroup == 12 || cGroup == 13);

                                    sw.WriteLine($" {(bMinimum ? 'G' : 'L')} {reader["ConstrName"]}");

                                }
                                reader.Close();
                            }
                        }

                        /* Projects */

                        sql = $@"SELECT DISTINCT ProjectNo FROM tbl_pb_Alternatives a WITH (NOLOCK) 
WHERE ScenID={ScenId} AND Code='{Code}'
AND ProjectNo NOT IN 
    (SELECT DISTINCT ProjectNo FROM tbl_pb_Alternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND Selected=1)
ORDER BY ProjectNo";

                        Log.Debug(sql);

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine(" E P{0}", reader[0]);
                                }
                                reader.Close();
                            }
                        }

                        sw.WriteLine("COLUMNS");
#if _NON_MULTI_YEAR
                        /* DIG 03/27/2023 Obsolete */
                        sql = $@"SELECT AltNo, ProjectNo, ConstrName, ISNULL(Cost,0) AS Cost, 
Utility, CGroup
FROM tbl_pb_AlternativeMatrix WITH(NOLOCK) 
WHERE ScenID={ScenId} AND Code='{Code}'
UNION ALL
SELECT AltNo, ProjectNo, NULL, 0.0 AS Cost, 0 AS Utility, 0 AS CGroup
FROM tbl_pb_Alternatives WITH (NOLOCK) 
WHERE ScenID={ScenId} AND Code='{Code}' AND YearWork<0 
AND ProjectNo NOT IN 
    (SELECT DISTINCT ProjectNo FROM tbl_pb_Alternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND Selected=1)
ORDER BY ProjectNo, AltNo, ConstrName";
                        
#endif      
                        Log.Debug(sql);

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                long prevAltNo = 0;
                                while (reader.Read())
                                {
                                    long altNo = reader.GetInt64(0);
                                    int projectNo = reader.GetInt32(1);
                                    double util = reader.GetDouble(4);
                                    int cGroup = reader.GetInt32(5);
                                    if (prevAltNo < altNo)
                                    {
                                        sw.WriteLine($" {altNo} P{projectNo} 1 UTILITY {-util}");
                                        prevAltNo = altNo;
                                    }
                                    if (altNo % 100 > 0)
                                    {
                                        double cost = Convert.ToDouble(reader[3]);

                                        if (bIgnoreBPConstraints && cGroup % 10 != 3)
                                        {
                                            continue;  // Do not add separate constraints for budget or pavement constraints
                                        }

                                        if (cost >= 0.01)
                                        {
                                            sw.WriteLine($" {altNo} {reader[2]} {cost / dMillion}");
                                        }
                                    }
                                }
                                reader.Close();
                            }
                        }

                        sw.WriteLine("RHS");
                        sql = sql = $@"SELECT ConstrName, ConstrValue, cGroup 
FROM vw_pb_RemainingBudgetConstraints v WITH(NOLOCK)
WHERE ScenID={ScenId} AND Code='{Code}' 
ORDER BY YearWork, ConstrName";

                        Log.Debug(sql);

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {

                                    int cGroup = Convert.ToInt32(reader[2]);

                                    if (bIgnoreBPConstraints && cGroup % 10 != 3)
                                    {
                                        continue;   // Do not add constraints for bridges and pavements
                                    }

                                    double cost = Convert.ToDouble(reader[1]);
                                    if (cost >= 0.1)
                                    {
                                        sw.WriteLine($" RHSLE {reader[0]} {cost / dMillion}");
                                    }
                                }
                                reader.Close();
                            }
                        }

                        sql = $@"SELECT DISTINCT ProjectNo FROM tbl_pb_Alternatives a WITH (NOLOCK) 
WHERE ScenID={ScenId} AND Code='{Code}'
AND ProjectNo NOT IN 
    (SELECT DISTINCT ProjectNo FROM tbl_pb_Alternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND Selected=1)
ORDER BY ProjectNo";

                        Log.Debug(sql);

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" RHSLE P{reader[0]} 1");
                                }
                                reader.Close();
                            }
                        }


                        if (AddBinaryBounds)
                        {
                            sw.WriteLine("BOUNDS");

                            sql = $@"SELECT DISTINCT AltNo 
FROM tbl_pb_Alternatives WITH (NOLOCK) WHERE ScenID={ScenId} AND Code='{Code}' 
AND ProjectNo NOT IN 
    (SELECT DISTINCT ProjectNo FROM tbl_pb_Alternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND Selected=1)
ORDER BY AltNo";

                            Log.Debug(sql);

                            using (SqlCommand command = new SqlCommand(sql, conn))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        sw.WriteLine($" UI BOUND {reader[0]} 1");
                                    }
                                    reader.Close();
                                }
                            }
                        }
                    }

                    sw.WriteLine("ENDATA");
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.InfoFormat("GenerateMpsFile - ended\tOK={0};\tError message: {1}", ok, errorMessage);
            return ok;
        }

#endregion

        /// <summary>
        /// Runs Synphony
        /// </summary>
        /// <param name="mpsFilePath">full path to the MPS file</param>
        /// <param name="resultFilePath">(out) full path to the results file (*.txt)</param>
        /// <param name="errorFilePath">(out) full path to the error file (*.err).  Must be empty on success.</param>
        /// <param name="errorMessage">(out) Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool RunSymphony(string mpsFilePath, out string resultFilePath, out string errorFilePath, out string errorMessage)
        {
            resultFilePath = null;
            errorMessage = null;
            errorFilePath = null;
            bool ok = true;
            int millisecWait = 24 * 3600 * 1000;

            Log.Info("RunSymphony - started");

            try
            {
                string dirName = Path.GetDirectoryName(mpsFilePath);
                string fileName = Path.GetFileNameWithoutExtension(mpsFilePath);
                resultFilePath = Path.Combine(dirName, fileName + ".txt");
                errorFilePath = Path.Combine(dirName, fileName + ".err");
                string symphFilePath = Path.Combine(dirName, "symphony.exe");
                File.Delete(resultFilePath);
                File.Delete(errorFilePath);


                System.Diagnostics.ProcessStartInfo pInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    CreateNoWindow = false,
#if DEBUG
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
#else
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized,
#endif
                    FileName = @"CMD.EXE",
                    Arguments = $@"/C {symphFilePath} -F ""{mpsFilePath}"" >""{resultFilePath}"" 2>""{errorFilePath}"""
                };
                Log.Info($"Arguments:\t{pInfo.Arguments}");

                System.Diagnostics.Process process = System.Diagnostics.Process.Start(pInfo);

                ok = process.WaitForExit(millisecWait);
                if (!ok)
                {
                    throw new Exception($"Process timed out after {millisecWait / 10000} seconds");
                }
                if (File.Exists(errorFilePath))
                {
                    errorMessage = File.ReadAllText(errorFilePath);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        ok = false;
                    }
                }

            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"RunSymphony - ended\t OK={ok}, Error message: {errorMessage}");

            return ok;
        }

        /// <summary>
        /// Import iotimization results from Sumphony results file into the database
        /// </summary>
        /// <param name="resultFilePath">Full path to the Symphony results file</param>
        /// <param name="code">Code of the utility combination</param>
        /// <param name="errorMessage">(out) Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool ImportOptimizationResults(string resultFilePath, string code, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            string header = "Column names and values of nonzeros in the solution";
            string allzeros = "All columns are zero in the solution!";
            string infeasible = "The problem is infeasible!";

            long recordsProcessed = 0;

            if (code == null)
            {
                code = this.Code;
            }
            Log.InfoFormat("ImportOptimizationResults - started");

            if (!File.Exists(resultFilePath))
            {
                errorMessage = string.Format("File {0} not found.", resultFilePath);
                Log.Error(errorMessage);
                return (false);
            }

            try
            {
                Log.Info($"Results file path:\t{resultFilePath}");

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string sql = $"DELETE FROM tbl_pb_LPResults WHERE ScenID={ScenId} AND Code='{code}'";
                    Log.Debug($"SQL:\t{sql}");

                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        int N = command.ExecuteNonQuery();
                        Log.Debug($"Number of records deleted:\t{N}");
                    }

                    using (BulkInserter bulkInserter = new BulkInserter())
                    {

                        if (!bulkInserter.Configure(conn, "dbo.tbl_pb_LPResults", "dt_pb_LPResults", null, 60, out errorMessage))
                        {
                            Log.Error(errorMessage);
                            return (false);
                        }

                        bulkInserter.Clear();

                        int state = 0;
                        bool solutionFound = false;
                        string s = null;
                        using (StreamReader sr = new StreamReader(resultFilePath))
                        {
                            while (!sr.EndOfStream && state >= 0)
                            {
                                s = sr.ReadLine().Trim();
                                if (string.IsNullOrEmpty(s))
                                {
                                    if (solutionFound)
                                    {
                                        break;
                                    }
                                    continue;
                                }
                                if (s.StartsWith("+++++"))
                                {
                                    state++;
                                    if (solutionFound)
                                    {
                                        break;
                                    }
                                    continue;
                                }
                                else if (s.StartsWith(header))
                                {
                                    state = 2;
                                    continue;
                                }
                                else if (s.StartsWith(allzeros))
                                {
                                    if (solutionFound)
                                    {
                                        break;
                                    }
                                    throw new Exception(allzeros);
                                }
                                else if (s.StartsWith(infeasible))
                                {
                                    if (solutionFound)
                                    {
                                        break;
                                    }
                                    state = -1;
                                    throw new Exception(infeasible);
                                }
                                else if (state == 3 && !string.IsNullOrEmpty(s))
                                {
                                    string[] sV = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    long V = Convert.ToInt64(sV[0]);
                                    double fraction = Convert.ToDouble(sV[1]);
                                    DataRow r = bulkInserter.NewRow();
                                    r["ScenID"] = ScenId;
                                    r["Code"] = code;
                                    r["AltNo"] = V;
                                    r["ProjectNo"] = (int)(V / 100);
                                    r["Fraction"] = fraction;
                                    r["Selected"] = 0;
                                    ok = bulkInserter.AddRow(r, out errorMessage);
                                    if (!ok)
                                    {
                                        throw new Exception(errorMessage);
                                    }
                                    recordsProcessed++;
                                    solutionFound = true;
                                }
                            }
                        }
                        if (state >= 0 && state != 3)
                        {
                            throw new Exception(string.Format("Header line '{0}' was not found in the file {1}", header, resultFilePath));
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

            Log.InfoFormat("Records processed: {0}", recordsProcessed);
            Log.InfoFormat("ImportOptimizationResults - ended.\t Ok={0};\tErrorMessage: {1}", ok, errorMessage);
            return (ok);
        }

        /// <summary>
        /// Deletes intermediate files (MPS, results, error) after solution results have been imported
        /// </summary>
        /// <param name="errorMessage">(out) Error message on failure;  null on success</param>
        /// <returns>True on failure, False on success</returns>
        public bool DeleteIntermediateFiles(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("Delete intermediate files - started...");

            try
            {
                string LPDir = Path.Combine(HomeDirectory, "LP");
                Directory.GetFiles(LPDir, $"*SC_{ScenId}*.*", SearchOption.TopDirectoryOnly).ToList().ForEach(File.Delete);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info("Delete intermediate files - ended.");

            return ok;
        }

        /// <summary>
        /// Deletes intermediate database records after each utility combination run of a scenario
        /// </summary>
        /// <param name="errorMessage">(out) Error message on failure;  null on success</param>
        /// <returns>True on success, false on failure</returns>
        public bool DeleteIntermediateRecords(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            int N;

            Log.Info("DeleteIntermediateRecords - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                 
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = $"DELETE FROM tbl_pb_ExtendedAlternatives WHERE ScenID={ScenId}";
                        command.CommandTimeout = 60000;
                        N = command.ExecuteNonQuery();
                        Log.Info($"{N} records deleted from tbl_pb_ExtendedAlternatives");
                    }
                
                                   
                    using (SqlCommand command = new SqlCommand(
                        $"DELETE FROM tbl_pb_ExtendedAlternativeMatrix WHERE ScenID={ScenId}", conn))
                    {
                        command.CommandTimeout = 60000;
                        N = command.ExecuteNonQuery();
                        Log.Info($"{N} records deleted from tbl_pb_AlternativeMatrix");
                    }
                  
                     using (SqlCommand command = new SqlCommand(
                      $"DELETE FROM tbl_pb_UtilityRanks WHERE ScenID={ScenId}", conn))
                    {
                        command.CommandTimeout = 60000;
                        N = command.ExecuteNonQuery();
                        Log.Info($"{N} records deleted from tbl_pb_UtilityRanks");
                    }

                    using (SqlCommand command = new SqlCommand(
                       $"DELETE FROM tbl_pb_LPResults WHERE ScenID={ScenId}", conn))
                    {
                        command.CommandTimeout = 60000;
                        N = command.ExecuteNonQuery();
                        Log.Info($"{N} records deleted from tbl_pb_LPResults");
                    }

                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"DeleteIntermediateRecords - ended.  Ok={ok}\tError message: {errorMessage}");

            return (ok);
        }


      

        /// <summary>
        /// Runs scenario through all its utility combinations
        /// </summary>
        /// <param name="doCleanup">True if forcing cleanup (deleting files, intermediate db records); False if this data is to be kept, e.g., for troubleshooting</param>
        /// <param name="doTraining">When True scenario is run through all available U-proxy codes</param>
        /// <param name="doBinary">Run LP with strict binary constraints. Ignored in training mode.</param>
        /// <param name="errorMessage">(out) Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool RunScenario(bool doCleanup, bool doTraining, bool doBinary, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"RunScenario for ScenID={ScenId} started...");

            bool bIgnoreBPConstraints = false;

            using (DataTable dtProxies = new DataTable())
            {
                dtProxies.Columns.Add(new DataColumn("Code", typeof(string)));

                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();


                        using (SqlCommand command = new SqlCommand(
                            $"SELECT ISNULL(ParmValue,0) FROM tbl_pb_ScenParm WITH (NOLOCK) WHERE ScenID={ScenId} AND ParmID='IGNR'",
                            conn))
                        {
                            object o = command.ExecuteScalar();
                            bIgnoreBPConstraints = Convert.ToInt32(o) > 0;
                        }

                        Log.Info($"Ignore separate bridge and pavement budget constraints: {bIgnoreBPConstraints}");

                        string selCode = null;

                        if (!doTraining)
                        {
                            using (SqlCommand command = new SqlCommand(
                                $"SELECT ISNULL(RecommendedUtilityProxy,'') FROM tbl_pb_Scenarios WITH (NOLOCK) WHERE ScenId={ScenId}",
                                conn)
                                )
                            {
                                selCode = command.ExecuteScalar().ToString();
                            }

                            if (string.IsNullOrEmpty(selCode))
                            {
                                throw new Exception($"Scenario with ScenId={ScenId} does not have a preferred utility proxy code set.\nIt needs to be run in the training mode first.");
                            }
                        }

                        using (SqlCommand command = new SqlCommand(
                           $"SELECT Locked FROM tbl_pb_Scenarios WHERE ScenID={ScenId}", conn))
                        {
                            object o = command.ExecuteScalar();
                            if (o == null || o == DBNull.Value)
                            {
                                throw new Exception($"Scenario {ScenId} not found in the dtabase.");
                            }

                            if (Convert.ToInt32(o) != 0)
                            {
                                throw new Exception($"Scenario {ScenId} is currently locked by another user. " +
                                    "Please try again later.");
                            }
                        }

                        using (SqlCommand command = new SqlCommand(
                            $"UPDATE tbl_pb_Scenarios SET CreatedBy=ISNULL(CreatedBy,USER), " +
                            $"CreatedAt=ISNULL(CreatedAt,CURRENT_TIMESTAMP), " +
                            $"LastRunBy=USER, LastRunAt=CURRENT_TIMESTAMP, " +
                            $"LastStarted=CURRENT_TIMESTAMP, Locked=1, " +
                            $"LastFinished=NULL, Notes='In progress' " +
                            $"WHERE ScenID={ScenId}", conn))
                        {
                            command.ExecuteNonQuery();
                        }

                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }


                        if (doTraining)
                        {

                            doBinary = false;

                            string sqlUtilities = "SELECT Code FROM vw_pb_UtilityProxies WITH (NOLOCK) WHERE LEN(Code)=3 ORDER BY Code";

                            using (SqlCommand command = new SqlCommand(sqlUtilities, conn))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        dtProxies.Load(reader);
                                        reader.Close();
                                    }
                                    else
                                    {
                                        throw new Exception($"No utility codes could be identified for scenario with ScenID={ScenId}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            dtProxies.Rows.Clear();
                            DataRow r = dtProxies.NewRow();
                            r["Code"] = selCode;
                            dtProxies.Rows.Add(r);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ok = false;
                    errorMessage = ex.Message;
                    Log.Error(errorMessage);
                }


                for (int i = 0; ok && i < dtProxies.Rows.Count; i++)
                {
                    Code = dtProxies.Rows[i].ItemArray[0].ToString();
                    Log.Info($"Code='{Code}'");

                    ok = RunScenarioCode(doBinary, doCleanup, bIgnoreBPConstraints, out errorMessage);
                }
            }

            if (ok)
            {
                ok = CollectScenarioStatistics(doTraining, out errorMessage);
            }

            string sql = $"UPDATE tbl_pb_Scenarios SET LastFinished=CURRENT_TIMESTAMP, Locked={(string.IsNullOrEmpty(errorMessage) ? 0 : 1)}, " +
                      $"Notes='{(errorMessage ?? "Success").Replace("'", "''")}' WHERE ScenID={ScenId}";
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    if (ok && doTraining)
                    {
                        using (SqlCommand command = new SqlCommand($"SELECT Code FROM vw_pb_ScenarioCodeSelection WITH (NOLOCK) WHERE ScenID={ScenId}",
                            conn))
                        {
                            object o = command.ExecuteScalar();
                            if (o != null && o != DBNull.Value)
                            {
                                sql = $"UPDATE tbl_pb_Scenarios SET RecommendedUtilityProxy='{o}', LastFinished=CURRENT_TIMESTAMP, " +
                                      $"Locked={(string.IsNullOrEmpty(errorMessage) ? 0 : 1)}, " +
                                      $"Notes='{(errorMessage ?? "Success").Replace("'", "''")}' WHERE ScenID={ScenId}";
                            }
                        }
                    }

                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message + $"\nWhen trying to run {sql.Replace("'", "''")}";
                Log.Error(errorMessage);
            }

            Log.Info($"RunScenario for ScenID={ScenId} ended. OK={ok}\tErrorMessage: {errorMessage}");

            return ok;
        }


        /// <summary>
        /// Runs scenario/code combination
        /// </summary>
        /// <param name="doBinary">If True runs strict binary LP for the projects that got fractional decisions in relaxed mode.</param>
        /// <param name="doCleanup">If true erases intermediate files and database records</param>
        /// <param name="bIgnoreBPConstraints">If true ignores separate bridge and pavement constraints and operates only with totals</param>
        /// <param name="errorMessage">WError message, null if no errors</param>
        /// <returns>True on scuccess, False on failure</returns>
        private bool RunScenarioCode(bool doBinary, bool doCleanup, bool bIgnoreBPConstraints, out string errorMessage)
        {
            errorMessage = null;

            bool ok = CreateAlternativesTable(out errorMessage);

            if (ok)
            {
                ok = PrepareAndSolveLPForProjects(false, doBinary, bIgnoreBPConstraints, out errorMessage);
            }

            if (ok && doBinary)
            {
                ok = PrepareAndSolveLPForProjects(true, true, bIgnoreBPConstraints, out errorMessage);
            }

            return (ok);
        }



        /// <summary>
        /// Prepares LP formulation, runs Symphony and interpretes results.
        /// </summary>
        /// <param name="doBinary">If true runs applies binary constraints to decision variables.</param>
        /// <param name="doBinaryThen">If true ProcessLPSolution is still called with doBinary=true even if the doBinary argument here is false</param>
        /// <param name="doCleanup">If true erases intermediate files and database records</param>
        /// <param name="bIgnoreBPConstraints">If true ignores separate bridge and pavement constraints and operates only with totals</param>
        /// <param name="errorMessage">WError message, null if no errors</param>
        /// <returns>True on scuccess, False on failure</returns>
        private bool PrepareAndSolveLPForProjects(bool doBinary, bool doBinaryThen, bool bIgnoreBPConstraints, out string errorMessage)
        {
            string mpsFilePath = null, resultFilePath = null, errorFilePath = null;
            errorMessage = null;

            bool ok = true;


            ok = CreateAlternativesMatrix(false, 0, out errorMessage);

            if (ok)
            {
                ok = GenerateMpsFile(bIgnoreBPConstraints, doBinary, out mpsFilePath, out errorMessage);
            }


            if (ok)
            {
                ok = RunSymphony(mpsFilePath, out resultFilePath, out errorFilePath, out errorMessage);
            }

            if (ok)
            {
                ok = ImportOptimizationResults(resultFilePath, null, out errorMessage);
            }

            if (ok)
            {
                ok = ProcessLPSolution(doBinary || doBinaryThen, 1, out errorMessage);
            }
                       

            return (ok);
        }



#region StaticMethods

    
        /// <summary>
        /// Wraps object's RunScenario method
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="homeDirectory">Path to the home directory.  The LP directory must exists as its subfolder</param>
        /// <param name="scenId">ScendID of the scenario to be run</param>
        /// <param name="doTraining">When set to True scenario will be run through all utility proxies</param>
        /// <param name="doCleanup">True if forcing cleanup (deleting files, intermediate db records); False if this data is to be kept, e.g., for troubleshooting</param>
        /// <param name="errorMessage">(out) Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public static bool RunScenario(string connectionString, string homeDirectory, int scenId, bool doBinary, bool doTraining, bool doCleanup, out string errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            _log.Info($"RunScenario (static) started...");

            try
            {
                SymphonyConductor conductor = new SymphonyConductor
                {
                    ConnectionString = connectionString,
                    HomeDirectory = homeDirectory,
                    ScenId = scenId,
                };

                conductor.LogParameters();

                ok = conductor.RunScenario(doCleanup, doTraining, doBinary, out errorMessage);
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                _log.Error(errorMessage);
            }

            _log.Info($"RunScenario (static) ended.  OK={ok}\tErrorMessage: {errorMessage}");

            return ok;
        }
    }


#endregion
}
