using log4net.Appender;
using log4net.Layout;
using log4net;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.IO.MemoryMappedFiles;
using System.Data;

#pragma warning disable CS0618
#pragma warning disable CS8618
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8625  

namespace PBLogic
{
    public class TreatmentFilterCriterion
    {
        public int CargoAttributeNo;
        public string CargoAttributeType;
        public string AssetType;
        public string Operation;
        public string TextOperand = null;
        public double? NumericOperand = null;
    }

    public class ScenarioFilter
    {
#if DEBUG_WITH_OWN_LOG
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

        public Dictionary<int, Dictionary<string, List<TreatmentFilterCriterion>>> Criteria = new Dictionary<int, Dictionary<string, List<TreatmentFilterCriterion>>>();


        public bool LoadFromDatabaseUsingConnection(SqlConnection connection, int scenId, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"LoadFromDatabaseUsingConnection (scenId={scenId}) - started...");
            Criteria.Clear();
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = $@"SELECT f.FilterNo, UPPER(f.AttributeName) AS AttributeName, a.AttributeNo, a.AttributeType, f.AssetType, f.Operation, f.TextValue
FROM tbl_pb_ScenarioFilters f WITH (NOLOCK)
LEFT OUTER JOIN tbl_pb_CargoAttributes a WITH (NOLOCK)
ON a.AttributeName=f.AttributeName
AND (a.AttributeType=f.AssetType OR f.AssetType='*')
WHERE f.ScenId = {scenId}
ORDER BY f.FilterNo, f.AttributeName";
                    command.CommandType = System.Data.CommandType.Text;
                    using (SqlDataReader r = command.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int filterNo = Convert.ToInt32(r["FilterNo"]);
                            Dictionary<string, List<TreatmentFilterCriterion>>? AndClause = null;
                            if (!Criteria.ContainsKey(filterNo))
                            {
                                AndClause = new Dictionary<string, List<TreatmentFilterCriterion>>();
                                Criteria.Add(filterNo, AndClause);
                            }
                            AndClause = Criteria[filterNo];

                            string attrName = r["AttributeName"].ToString();
                            List<TreatmentFilterCriterion> filterList = null;

                            if (AndClause.ContainsKey(attrName))
                            {
                                filterList = AndClause[attrName];
                            }
                            else
                            {
                                filterList = new List<TreatmentFilterCriterion>();
                                AndClause.Add(attrName, filterList);
                            }

                            TreatmentFilterCriterion criterion = new TreatmentFilterCriterion
                            {
                                AssetType = r["AssetType"].ToString(),
                                Operation = r["Operation"].ToString(),
                                TextOperand = r["TextValue"].ToString()
                            };

                            if (r["AttributeNo"] != null && r["AttributeNo"] != DBNull.Value)
                            {
                                criterion.CargoAttributeNo = Convert.ToInt32(r["AttributeNo"]);
                            }

                            if (r["AttributeType"] != null && r["AttributeType"] != DBNull.Value)
                            {
                                criterion.CargoAttributeType = r["AttributeType"].ToString();
                            }

                            double d;



                            if (!double.TryParse(criterion.TextOperand, out d))
                            {
                                if (criterion.CargoAttributeType != null && criterion.CargoAttributeType == "N")
                                {
                                    throw new Exception($"Unable to convert '{criterion.TextOperand}' to numeric type for {attrName}");
                                }
                            }

                            criterion.NumericOperand = d;


                            filterList.Add(criterion);
                        }

                        r.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"LoadFromDatabaseUsingConnection (scenId={scenId}) - finished.  Ok={ok}, ErrorMessage: {errorMessage}");

            return ok;
        }

        public bool LoadFromDatabase(string connectionString, int scenId, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"LoadFromDatabaseUsingConnection (scenId={scenId}) - started...");

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    ok = LoadFromDatabaseUsingConnection(connection, scenId, out errorMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"LoadFromDatabase (scenId={scenId}) - finished.  Ok={ok}, ErrorMessage: {errorMessage}");

            return ok;
        }

        public static bool PopulateFilteredTreatmentsForScenarioUsingConnection(SqlConnection connection, int scenId, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"PopulateFilteredTreatmentsForScenarioUsingConnection (scenId={scenId}) - started...");

            string sql = $@"SELECT t.ImportTimeGeneratedId, d.AttributeNo, UPPER(a.AttributeName) AS AttributeName, a.AssetType AS AttrAssetType, d.TextValue
FROM tbl_lib_LibraryTreatments t WITH (NOLOCK)
INNER JOIN tbl_lib_CargoData d WITH (NOLOCK)
	ON  d.LibraryId=t.LibraryId
    AND d.AssetType=t.AssetType
    AND d.ImportTimeGeneratedGuid = t.ImportTimeGeneratedId
INNER JOIN tbl_lib_CargoAttributes a WITH (NOLOCK)
	ON a.LibraryId=d.LibraryId
   AND a.AssetType = t.AssetType
   AND a.AttributeNo=d.AttributeNo
WHERE t.LibraryId = (SELECT LibraryId FROM tbl_pb_Scenarios WITH (NOLOCK) WHERE ScenId={scenId})
  AND EXISTS(SELECT TOP 1 1 FROM tbl_pb_ScenarioFilters f WITH (NOLOCK)
             WHERE f.ScenId={scenId} AND f.AssetType=t.AssetType AND UPPER(f.AttributeName)=UPPER(a.AttributeName))
ORDER BY ImportTimeGeneratedId, AttributeName";

            Log.Debug(sql);

            try
            {

                using(SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $@"DELETE FROM tbl_pb_ScenarioFilteredTreatments WHERE ScenId={scenId}";
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    long N = cmd.ExecuteNonQuery();
                    Log.Info($"Old records deleted from tbl_pb_ScenarioFilteredTreatments: {N}");
                }

                ScenarioFilter filter = new ScenarioFilter();

                ok = filter.LoadFromDatabaseUsingConnection(connection, scenId, out errorMessage);
                if (!ok)
                {
                    throw new Exception(errorMessage);
                }

                Log.Info("Filter:");
                Log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(filter, Newtonsoft.Json.Formatting.Indented));

                Dictionary<Guid, NarrowLibraryTreatment> treatments = new Dictionary<Guid, NarrowLibraryTreatment>();

                string sqlTreats = $@"SELECT ImportTimeGeneratedId FROM tbl_lib_LibraryTreatments t WITH (NOLOCK)
WHERE t.LibraryId = (SELECT LibraryId FROM tbl_pb_Scenarios WITH (NOLOCK) WHERE ScenId={scenId})";

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlTreats;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid treatmentId = Guid.Parse(reader["ImportTimeGeneratedId"].ToString());
                            if (!treatments.ContainsKey(treatmentId))
                            {
                                treatments.Add(treatmentId, new NarrowLibraryTreatment());
                            }
                        }
                        reader.Close();
                    }

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandTimeout = 60000;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                NarrowLibraryTreatment? treatment = null;
                                Guid treatmentId = Guid.Parse(reader["ImportTimeGeneratedId"].ToString());
                                if (treatments.ContainsKey(treatmentId))
                                {
                                    treatment = treatments[treatmentId];
                                }

                                CargoValue cargoValue = null;

                                string? textValue = null;
                                object oTextValue = reader["TextValue"];
                                if (oTextValue != null && oTextValue != DBNull.Value)
                                {
                                    textValue = oTextValue.ToString();
                                }

                                string fieldName = reader["AttributeName"].ToString();

                                if (textValue != null && double.TryParse(textValue, out double numericValue))
                                {
                                    cargoValue = new CargoValue() { CargoType = "N", NumericValue = numericValue, TextValue = textValue };
                                }
                                else
                                {
                                    cargoValue = new CargoValue() { CargoType = "T", TextValue = textValue };
                                }


                                treatment.Cargo.Add(fieldName, cargoValue);

                            }

                            reader.Close();
                        }
                    }

                    Log.Info($"Total number of treatments in the library: {treatments.Count}");
                    int nWritten = 0;



                    using (BulkInserter bi = new BulkInserter())
                    {

                        ok = bi.Configure(connection, "tbl_pb_ScenarioFilteredTreatments", "dt", null, null, out errorMessage);
                        if (!ok)
                        {
                            throw new Exception(errorMessage);
                        }

                        foreach (var trt in treatments)
                        {
                            Guid treatmentId = trt.Key;
                            NarrowLibraryTreatment treatment = trt.Value;
                            bool treatmentOk = true;

                            foreach (var orClause in filter.Criteria)
                            {
                                bool b = false;
                                foreach (var clause in orClause.Value)
                                {
                                    if (treatment.Cargo.ContainsKey(clause.Key))
                                    {
                                        CargoValue cargoValue = treatment.Cargo[clause.Key];

                                        foreach (TreatmentFilterCriterion criterion in clause.Value)
                                        {
                                            b = CargoSatisfiesCriterion(cargoValue, criterion);
                                            if (!b)
                                            {
                                                treatmentOk = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!b)
                                    {
                                        treatmentOk = false;
                                        break;
                                    }
                                }

                                if (b)
                                {
                                    treatmentOk = true;      // At least on OR-clause is satisfied
                                    break;
                                }
                            }

                            if (treatmentOk)
                            {
                                DataRow r = bi.NewRow();
                                r["ScenId"] = scenId;
                                r["ImportTimeGeneratedId"] = treatmentId;
                                ok = bi.AddRow(r, out errorMessage);
                                if (!ok)
                                {
                                    throw new Exception(errorMessage);
                                }
                                nWritten++;
                            }

                        }
                    }

                    if (ok)
                    {
                        Log.Info($"Filtered treatment IDs written into tbl_pb_ScenarioFilteredTreatments: {nWritten}");
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info($"PopulateFilteredTreatmentsForScenarioUsingConnection (scenId={scenId}) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        public static bool PopulateFilteredTreatmentsForScenario(string connectionString, int scenId, out string? errorMessage)
        {
           bool ok = true;
            errorMessage = null;

            Log.Info($"PopulateFilteredTreatmentsForScenario (scenId={scenId}) - started...");

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    ok = PopulateFilteredTreatmentsForScenarioUsingConnection(connection, scenId, out errorMessage);
                }
            }
             catch (Exception ex)
             {
                 errorMessage=ex.Message;
                ok = false;
                 Log.Error(errorMessage);
             }
 
            Log.Info($"PopulateFilteredTreatmentsForScenario (scenId={scenId}) - finished  Ok={ok}, ErrorMessage: '{errorMessage}'.");

             return ok;
         }


        public static bool CargoSatisfiesCriterion(CargoValue cargo, TreatmentFilterCriterion criterion)
        {
            bool b = false;

            if (cargo.CargoType == "T")
            {
                if (cargo.TextValue == null && criterion.TextOperand == null)
                {
                    return false;
                }

                string arg = cargo.TextValue;
                string rhs = criterion.TextOperand;
                int n = arg.CompareTo(rhs);

                switch (criterion.Operation)
                {
                    case "=":
                        b = n == 0;
                        break;
                    case "<>":
                        b = n != 0;
                        break;
                    case ">":
                        b = n > 0;
                        break;
                    case "<":
                        b = n < 0;
                        break;
                    case ">=":
                        b = n >= 0;
                        break;
                    case "<=":
                        b = n <= 0;
                        break;
                    case "{}":
                        string[] ss = rhs.ToUpper().Split(',');
                        b = (ss.Contains<string>(arg));
                        break;
                    default:
                        throw new Exception($"Ilegal operation {criterion.Operation} encountered in the filtering criterion formulation.");

                }
            }
            else if (cargo.CargoType == "N")
            {
                if (cargo.NumericValue == null || criterion.NumericOperand == null)
                {
                    return false;
                }

                double arg = cargo.NumericValue.Value;
                double d = arg - criterion.NumericOperand.Value;

                switch (criterion.Operation)
                {
                    case "=":
                        b = d == 0;
                        break;
                    case "<>":
                        b = d != 0;
                        break;
                    case ">":
                        b = d > 0;
                        break;
                    case "<":
                        b = d < 0;
                        break;
                    case ">=":
                        b = d >= 0;
                        break;
                    case "<=":
                        b = d <= 0;
                        break;
                    case "{}":
                        string rhs = criterion.TextOperand;
                        string[] ss = rhs.ToUpper().Split(',');
                        foreach (string s in ss)
                        {
                            if (arg == double.Parse(s))
                            {
                                b = true;
                                break;
                            }
                        }
                        break;
                    default:
                        throw new Exception($"Ilegal operation {criterion.Operation} encountered in the filtering criterion formulation.");
                }
            }
            else
            {
                throw new Exception($"Unknown cargo attribute type '{cargo.GetType}'");
            }

            return b;
        }
    }
}
