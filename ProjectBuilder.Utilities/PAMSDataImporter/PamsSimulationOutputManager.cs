using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using PBLogic;
using log4net;

namespace PAMSDataImporter
{
    public static partial class ImportManager
    {
        private static ILog _log;
        public static ILog Log
        {
            get { return _log; }
            set { _log = value; }
        }

        public static List<string> YearList = new List<string>();

        public static bool ImportJsonOutputObject(JsonConfig config, PAMSJSONOutput JsonOutput, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("ImportJsonOutputObject - started...");

            try
            {
                ok = ClearInserters(out errorMessage);
                if (!ok)
                {
                    return ok;
                }

                using (SqlConnection conn = new SqlConnection(config.TargetConnectionString))
                {
                    conn.Open();

                    foreach (string tableName in TableNames)
                    {
                        using (SqlCommand command = new SqlCommand($"TRUNCATE TABLE tbl_jp_{tableName}", conn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    if (ok)
                    {
                        ok = CreateInserters(conn, out errorMessage);
                    }

                    if (ok)
                    {
                        ok = SavePAMSJsonOutputToDb(JsonOutput, !config.DoYearByYear, config.DoFullJsonImport, out errorMessage);
                    }

                    bool clok = ClearInserters(out string errMessage);

                    if (!ok)
                    {
                        throw new Exception(errorMessage);
                    }

                    if (!clok)
                    {
                        throw new Exception(errMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info("ImportJsonOutputObject - ended");

            return (ok);
        }

        public static bool TruncateJpAndPamsTables(string configFileName, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("TruncateJpAndPamsTables - started...");

            try
            {
                JsonConfig config = new JsonConfig();
                string s = File.ReadAllText(configFileName);
                config = JsonConvert.DeserializeObject<JsonConfig>(s);

                using (SqlConnection conn = new SqlConnection(config.TargetConnectionString))
                {
                    conn.Open();
                    using (SqlCommand selCommand = new SqlCommand())
                    {
                        selCommand.Connection = conn;
                        selCommand.CommandText = @" SELECT table_name FROM [INFORMATION_SCHEMA].[TABLES] WITH (NOLOCK)
  WHERE TABLE_TYPE = 'BASE TABLE' AND
	(table_name LIKE 'tbl_jp_%' OR table_name LIKE 'tbl_pams_%')
  ORDER BY table_name";

                        Log.Debug("SQL SELECT: " + selCommand.CommandText);

                        using(SqlDataReader reader = selCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                using(SqlCommand truncCommand = new SqlCommand())
                                {
                                    truncCommand.Connection = conn;
                                    truncCommand.CommandText = $"TRUNCATE TABLE {reader[0]}";
                                    Log.Info(truncCommand.CommandText);
                                    truncCommand.CommandTimeout = 10000;
                                    truncCommand.ExecuteNonQuery();
                                }
                            }
                            reader.Close();
                        }
                    }
                }
            }
            catch(Exception x)
            {
                ok = false;
                errorMessage = x.Message;
                Log.Error(errorMessage);
            }

            Log.Info("TruncateJpAndPamsTables - ended...");

            return (ok);
        }



        public static bool ProcessYearlySections(JsonConfig config, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            string sql = null;

            Log.Info("ProcessYearlySections - started...");

            YearList.Clear();

            using (SqlConnection connSource = new SqlConnection(config.SourceConnectionString))
            {
                connSource.Open();
                sql = $@"SELECT LEFT(RIGHT([Output], 5),4) AS [Year] FROM dbo.SimulationOutput WITH (NOLOCK) 
WHERE OutputType LIKE 'Yearly%' ";
                if (!string.IsNullOrEmpty(config.SimulationId))
                {
                    sql += $@"
AND SimulationId='{config.SimulationId}' ";
                }
                sql += @"
ORDER BY 1";
                Log.Debug(sql);

                using(SqlCommand command = new SqlCommand())
                {
                    command.Connection = connSource;
                    command.CommandText = sql;
                    command.CommandTimeout = 3600;
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            YearList.Add(reader[0].ToString());
                        }
                    }
                    command.CommandText = sql;
                }
            }
#if DEBUG
            foreach(string yr in YearList)
            {
                Log.Debug($"{yr}");
            }
#endif

            foreach(string Year in YearList)
            {
                Log.Info($"Importing year {Year} - started...");
                sql = $"SELECT [Output] FROM dbo.SimulationOutput WITH (NOLOCK) WHERE LEFT(RIGHT([Output], 5),4) = '{Year}'";
                if (!string.IsNullOrEmpty(config.SimulationId))
                {
                    sql += $@"
AND SimulationId='{config.SimulationId}'";
                }

                YearOfPlanningHorizon yp = new YearOfPlanningHorizon();

                using (SqlConnection connSource = new SqlConnection(config.SourceConnectionString))
                {
                    connSource.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connSource;
                        command.CommandText = sql;
                        command.CommandTimeout = 3600;
                        object o = command.ExecuteScalar();
                        yp = JsonConvert.DeserializeObject<YearOfPlanningHorizon>(o.ToString());
                    }
                }

                if (ok)
                {
                    ok = ClearInserters(out errorMessage);
                    if (!ok)
                    {
                        throw new Exception(errorMessage);
                    }
                }

                using (SqlConnection connTarget = new SqlConnection(config.TargetConnectionString))
                {
                    connTarget.Open();
                    ok = CreateInserters(connTarget, out errorMessage);
                    if (!ok)
                    {
                        throw new Exception(errorMessage);
                    }

                    ok = SavePAMSYearOfPlanningHorizon(yp, config.DoFullJsonImport, out errorMessage);

                    bool clok = ClearInserters(out string errMessage);

                    if (!ok)
                    {
                        throw new Exception(errorMessage);
                    }

                    if (!clok)
                    {
                        throw new Exception(errMessage);
                    }
                }

                if (!ok)
                {
                    break;
                }

                GC.Collect();

                Log.Info($"Importing year {Year} - ended.");
            }

                 

            Log.Info($"ProcessYearlySections - ended.  OK={ok}, ErrorMessage: '{errorMessage}'");
            return (ok);
        }


        public static bool ImportSimulationOutput(JsonConfig config, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            PAMSJSONOutput JsonOutput = new PAMSJSONOutput();

            Log.Info("ImportSimulationOutput - started...");

            try
            {
                using (SqlConnection connSource = new SqlConnection(config.SourceConnectionString))
                {
                    connSource.Open();

                    string whereClause = string.IsNullOrEmpty(config.SimulationId) ?
                            "" : $"WHERE SimulationId='{config.SimulationId}'";

                    if (config.DoYearByYear)
                    {
                        whereClause += string.IsNullOrEmpty(config.SimulationId) ? "WHERE " : @"
AND ";
                        whereClause += "OutputType LIKE 'Initial%'";
                    }
                    string sql = $@"SELECT [Output], OutputType, LEFT(RIGHT([Output], 5),4) AS [Year]
 FROM dbo.SimulationOutput WITH (NOLOCK) 
{whereClause} 
ORDER BY OutputType , LEFT(RIGHT([Output], 5),4) ";

                    Log.Debug(sql);

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connSource;
                        command.CommandText = sql;
                        command.CommandTimeout = 36000000;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string output = reader[0].ToString();
                                string outputType = reader[1].ToString();
                                
                                if (outputType.StartsWith("InitialConditionNetwork"))
                                {
                                   JsonOutput.InitialConditionOfNetwork = JsonConvert.DeserializeObject<double>(reader[0].ToString());
                                    Log.Info($"{outputType} loaded");
                                }
                                else if (outputType.StartsWith("InitialSummary"))
                                {
                                    JsonOutput.InitialSectionSummaries = JsonConvert.DeserializeObject<List<Facility>>(output);
                                    Log.Info($"{outputType} loaded");
                                }
                                else if (outputType.StartsWith("YearlySection"))
                                {
                                    YearOfPlanningHorizon yrph = new YearOfPlanningHorizon();
                                    yrph = JsonConvert.DeserializeObject<YearOfPlanningHorizon>(output);
                                    JsonOutput.Years.Add(yrph);
                                    Log.Info($"{outputType} {reader[2]} loaded");
                                }
                                else
                                {
                                    throw new Exception("Unexpected OutputType in dbo.SimulationOutput: " + outputType);
                                }
                            }
                            reader.Close();
                        }
                    }
                }

                if (ok)
                {
                    ok = ImportJsonOutputObject(config, JsonOutput, out errorMessage);
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info("ImportSimulationOutput - ended");

            return ok;
        }


        public static bool ImportMaintainableAssetsSegmentation(JsonConfig config, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("ImportMaintainableAssetsSegmentation - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(config.TargetConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand("TRUNCATE TABLE tbl_pams_MaintainableAssetsSegmentation", conn))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (BulkInserter inserter = new BulkInserter())
                    {

                        ok = inserter.Configure(conn, "tbl_pams_MaintainableAssetsSegmentation", "MaitainableAssetsSegmentation", null, null, out errorMessage);

                        if (ok)
                        {
                            using (SqlConnection PAMSconn = new SqlConnection(config.SourceConnectionString))
                            {
                                PAMSconn.Open();
                                using (SqlCommand command = new SqlCommand())
                                {
                                    command.Connection = PAMSconn;
                                    command.CommandTimeout = 50000;
                                    command.CommandText = @"
SELECT d.ID AS [AssetID], 
	CONVERT(TINYINT,dd.TextValue) AS [District],
	CONVERT(TINYINT,dc.TextValue) AS [Cnty],
	CONVERT(INT,dr.TextValue) AS [Route],
	CONVERT(INT,a.SectionName) AS [Section], 
	CONVERT(FLOAT,a.Area) AS [Area], 
	CONVERT(FLOAT,d.NumericValue) AS [Length],
	CONVERT(FLOAT,dw.NumericValue) AS [Width],
	CONVERT(FLOAT,dn.NumericValue) AS [Lanes],
	CONVERT(BIT,CASE WHEN di.TextValue IN ('Y','y','1') THEN 1 ELSE 0 END) AS [Interstate]
FROM dbo.AttributeDatum d WITH (NOLOCK)
INNER JOIN dbo.Attribute t WITH (NOLOCK) 
		ON t.Id = d.AttributeId AND (t.[Name] LIKE '%LENGTH%')
INNER JOIN dbo.MaintainableAsset a WITH (NOLOCK) 
	ON a.ID = d.MaintainableAssetId
INNER JOIN dbo.AttributeDatum dw WITH (NOLOCK) 
		ON dw.MaintainableAssetId = a.Id
INNER JOIN dbo.Attribute tw WITH (NOLOCK) 
		ON tw.Id = dw.AttributeId AND (tw.[Name] = 'WIDTH')
INNER JOIN dbo.AttributeDatum dn WITH (NOLOCK) 
		ON dn.MaintainableAssetId = a.Id
INNER JOIN dbo.Attribute tn WITH (NOLOCK) 
		ON tn.Id = dn.AttributeId AND (tn.[Name] = 'LANES')
INNER JOIN dbo.AttributeDatum dd WITH (NOLOCK) 
		ON dd.MaintainableAssetId = a.Id
INNER JOIN dbo.Attribute td WITH (NOLOCK) 
		ON td.Id = dd.AttributeId AND (td.[Name] = 'DISTRICT')
INNER JOIN dbo.AttributeDatum dc WITH (NOLOCK) 
		ON dc.MaintainableAssetId = a.Id
INNER JOIN dbo.Attribute tc WITH (NOLOCK) 
		ON tc.Id = dc.AttributeId AND (tc.[Name] = 'CNTY')
INNER JOIN dbo.AttributeDatum dr WITH (NOLOCK) 
		ON dr.MaintainableAssetId = a.Id
INNER JOIN dbo.Attribute tr WITH (NOLOCK) 
		ON tr.Id = dr.AttributeId AND (tr.[Name] = 'SR')
INNER JOIN dbo.AttributeDatum di WITH (NOLOCK) 
		ON di.MaintainableAssetId = a.Id
INNER JOIN dbo.Attribute ai WITH (NOLOCK) 
		ON ai.Id = di.AttributeId AND (ai.[Name] = 'INTERSTATE')
";
                                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                                    {
                                        da.Fill(inserter.DTable);
                                    }
                                }
                            }
                        }

                        if (ok)
                        {
                            inserter.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
            }


            Log.Info($"ImportMaintainableAssetsSegmentation - ended. OK={ok}, ErrorMessage: '{errorMessage}");

            return (ok);
        }

        public static bool ImportMaintainableAssets(JsonConfig config, out string errorMessage)
        {
            errorMessage = null;

            Log.Info("ImportMaintainableAssets - started...");

            string target = TargetDbConnectionString;
            string source = PAMSDbConnectionString;

            PAMSDbConnectionString = config.SourceConnectionString;
            TargetDbConnectionString = config.TargetConnectionString;

            bool ok = ImportMaintainableAssets(out errorMessage);

            PAMSDbConnectionString = source;
            TargetDbConnectionString = target;

            Log.Info("ImportMaintainableAssets - ended...");

            return (ok);
        }

        private static bool ImportAdditionalTable(JsonConfig config, string pamsTableName, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"ImportAdditionalTable {pamsTableName} - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(config.TargetConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand($"TRUNCATE TABLE tbl_pams_{pamsTableName}", conn))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (BulkInserter inserter = new BulkInserter())
                    {

                        ok = inserter.Configure(conn, $"tbl_pams_{pamsTableName}", pamsTableName, null, null, out errorMessage);
                        if (ok)
                        {
                            using (SqlConnection PAMSconn = new SqlConnection(config.SourceConnectionString))
                            {
                                PAMSconn.Open();
                                using(SqlCommand command = new SqlCommand())
                                {
                                    command.Connection = PAMSconn;
                                    command.CommandType = System.Data.CommandType.Text;
                                    command.CommandText = $"SELECT * FROM {pamsTableName} WITH (NOLOCK)";
                                    command.CommandTimeout = 50000;
                                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                                    {
                                        da.Fill(inserter.DTable);
                                    }
                                }
                            }
                        }

                        if (ok)
                        {
                            inserter.Flush();
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

            Log.Info($"ImportAdditionalTable {pamsTableName} - ended");

            return ok;
        }

        public static bool ImportFromPAMS(JsonConfig config, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("Import from PAMS - started...");

            try
            {
                ok = ImportSimulationOutput(config, out errorMessage);

                if (ok && config.DoYearByYear)
                {
                    ok = ProcessYearlySections(config, out errorMessage);
                }

                if (ok)
                {
                     ok = ImportMaintainableAssets(config, out errorMessage);
                }

                if (ok)
                {
                    ok = ImportMaintainableAssetsSegmentation(config, out errorMessage);
                }

                if (ok)
                {
                    foreach(string pamsTableName in config.AdditionalTableList)
                    {
                        ok = ImportAdditionalTable(config, pamsTableName, out errorMessage);
                        if (!ok)
                        {
                            break;
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

            Log.Info("Import from PAMS - ended");

            return (ok);
        }

        public static bool PAMS2JPAMS(string configFile, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("PAMS2JPAMS - started...");
            try
            {
                if (string.IsNullOrEmpty(configFile))
                {
                    JsonConfig config = new JsonConfig();
                    config.SourceConnectionString = @"Data Source = .\SQL2019;Initial Catalog = IAMv2; Persist Security Info=True;User ID = ProjectBuilder; Password=PennDOT;MultipleActiveResultSets=True";
                    config.TargetConnectionString = @"Data Source = .\SQL2019;Initial Catalog = JPAMS; Persist Security Info=True;User ID = ProjectBuilder; Password=PennDOT;MultipleActiveResultSets=True";
                    config.SimulationId = "F9EFA037-1FCB-41A6-9D64-0A5A91DE09E3";
                    config.AdditionalTableList.AddRange(new string[] { "Section", "SectionFacility", "Budget", "BudgetAmount" });
                    string s = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Data\config.json", s);
                }
                else
                {
                    JsonConfig config = new JsonConfig();
                    string s = File.ReadAllText(configFile);
                    config = JsonConvert.DeserializeObject<JsonConfig>(s);
#if DEBUG
                    s = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText($"{configFile}-Copy.json", s);
#endif
                    ok = ImportFromPAMS(config, out errorMessage);
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info("PAMS2JPAMS - ended");

            return (ok);
        }
    }
}
