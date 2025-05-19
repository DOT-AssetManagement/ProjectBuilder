using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using System.Data.SqlClient;
using System.Data;

namespace PBLogic
{

    public enum ClusteringSource
    {
        csPAMS = 1,
        csBAMS = 2,
        csPB = 3,
    }

    public static class Projector
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

        public static string PBConnectionString;

        public static bool BuildProjects(int scenId, string code, int district, string assetType, 
            ClusteringSource clusteringSource, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"BuildProjects ScenId={scenId}, Code={code}, District={district}, AssetType={assetType}, ClusteringSource = {clusteringSource} - started...");
            try
            {
                string targetTableName = "Projects";
                string source = null;
                string spName = null;

                switch(clusteringSource)
                {
                    case ClusteringSource.csPAMS:
                        source = "PAMSPreferred";
                        spName = "dbo.sp_pb_CollectPAMSProjectsAsPreferred";
                        break;

                    case ClusteringSource.csBAMS:
                        // To be continued DIG 6/30/2022
                         break;

                    case ClusteringSource.csPB:
                        break;

                    default:
                        throw new Exception($"Unknown clustering source {clusteringSource}");
                }

                string dbTargetTableName = "tbl_pb_" + targetTableName;

                using (SqlConnection conn = new SqlConnection(PBConnectionString))
                {
                    conn.Open();
                    using(SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = $"TRUNCATE TABLE {dbTargetTableName}";
                        command.CommandTimeout = 600;
                        int N = command.ExecuteNonQuery();
                        Log.Info($"{N} records deleted from {dbTargetTableName}");
                    }
                    using(BulkInserter bi = new BulkInserter())
                    {
                        ok = bi.Configure(conn, dbTargetTableName, targetTableName, null, null, out errorMessage);
                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }
                        using(SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = spName;
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandTimeout = 1200;
                           
                            command.Parameters.Add("@ScenID", SqlDbType.Int).Value = DBNull.Value;
                            command.Parameters.Add("@Code", SqlDbType.NVarChar, 4).Value = DBNull.Value;
                            command.Parameters.Add("@District", SqlDbType.Int).Value = DBNull.Value;
                            command.Parameters.Add("@AssetType", SqlDbType.NVarChar, 1).Value = DBNull.Value;

                            if (scenId > 0)
                            {
                                command.Parameters["@ScenID"].Value = scenId;
                            }
                            if (!string.IsNullOrEmpty(code))
                            {
                                command.Parameters["@Code"].Value = code;
                            }
                            if (district > 0)
                            {
                                command.Parameters["@District"].Value = district;
                            }
                            if (!string.IsNullOrEmpty(assetType))
                            {
                                command.Parameters["@AssetType"].Value = assetType;
                            }

                            int prevDist = 0;
                            int prevCnty = 0;
                            int prevRte = 0;
                            int prevYr = 0;
                            bool prevNull = false;

                            using (SqlDataReader r = command.ExecuteReader())
                            {
                                int N = 0;
                                int M = 0;
                                int P = 0;

                                while(ok && r.HasRows && r.Read())
                                {
                                    N++;

                                    if (r["PAMSSeg"] == DBNull.Value)
                                    {
                                        continue;
                                    }

                                    M++;

                                    DataRow dr = bi.NewRow();

                                    dr["ClusteringSource"] = source;
                                    dr["AssetType"] = assetType;
                                    dr["ScenID"] = scenId;
                                    dr["Code"] = code;

                                    int dist = Convert.ToInt32(r["District"]);
                                    int cnty = Convert.ToInt32(r["Cnty"]);
                                    int rte = Convert.ToInt32(r["Route"]);
                                    int yr = Convert.ToInt32(r["Year"]);

                                    if (dist != prevDist || cnty!= prevCnty || rte!=prevRte || yr != prevYr)
                                    {
                                        P = 0;
                                    }

                                    if (r["AnotherYear"] == DBNull.Value && r["OutOfScope"] == DBNull.Value)
                                    {
                                        if (prevNull)
                                        {
                                            P++;
                                        }
                                        else
                                        {
                                            P = 1;
                                        }
                                        prevNull = true;
                                    }
                                    else
                                    {
                                        P = 1;
                                        prevNull = false;
                                    }

                                    dr["District"] = dist;
                                    dr["Cnty"] = cnty;
                                    dr["Route"] =rte;
                                    dr["Year"] = yr;
                                    dr["Seg"] = r["QSeg"].ToString();
                                    dr["TreatmentNo"] = r["TreatmentId"];
                                    dr["ProjectNo"] = P;
                                    dr["ProjectName"] = $"{dist}-{cnty}-{rte}-{yr}-{P}";

                                    ok = bi.AddRow(dr, out errorMessage);

                                    prevDist = dist;
                                    prevCnty = cnty;
                                    prevRte = rte;
                                    prevYr = yr;
                                }

                                Log.Info($"Records read: {N}.  Records written: {M}");

                                r.Close();
                                if (!ok)
                                {
                                    throw new Exception(errorMessage);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

         
            Log.Info($"BuildProjects ScenId={scenId}, Code={code}, District={district}, AssetType={assetType}, ClusteringSource = {clusteringSource} - ended.  Ok={ok}, ErrorMessage='{errorMessage}'");

            return (ok);
        }
    }
}
