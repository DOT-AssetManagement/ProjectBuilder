using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using PAMSDataImporter;
using System.IO;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using PBLogic;


namespace BAMSDataImporter
{
    public static class BAMSImportManager
    {
        private static ILog _log;
        public static ILog Log
        {
            get { return _log; }
            set { _log = value; }
        }

        public static List<string> YearList = new List<string>();

        public static BAMSJSONOutput JsonOutput = new BAMSJSONOutput();
        public static Dictionary<string, BulkInserter> Inserters = new Dictionary<string, BulkInserter>();

        public static string[] TableNames = new string[] {
            "NetworkCondition",
            "SectionNumericAttributes",
            "Sections",
            "SectionTextAttributes",
            "SectionTreatment",
            "SectionTreatmentBudgetUsage",
            "SectionTreatmentCashFlowConsiderations",
            "SectionTreatmentConsiderations",
            "SectionTreatmentOptions",
            "SectionTreatmentRejections",
            "SectionTreatmentSchedulingCollisions",
            "YearlyBudgets",
            "YearlyDeficientConditionGoals",
            "YearlyTargetConditionGoals"
            };

        private static bool ClearInserters(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            try
            {
                foreach (string key in Inserters.Keys)
                {
                    BulkInserter inserter = Inserters[key];
                    if (inserter != null)
                    {
                        inserter.Dispose();
                    }
                }
                Inserters.Clear();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
            }
            return (ok);
        }

        private static bool CreateInserters(SqlConnection sqlConnection, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            try
            {
                foreach (string tableName in TableNames)
                {
                    BulkInserter inserter = new BulkInserter();
                    ok = inserter.Configure(sqlConnection, "tbl_jb_" + tableName, tableName, null, null, out errorMessage);
                    if (!ok)
                    {
                        throw new Exception(errorMessage);
                    }
                    Inserters.Add(tableName, inserter);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
            }

            return ok;
        }

        private static bool SaveSectionDataToDb(int year, string assetName,
                  Dictionary<string, double> dictNumeric, Dictionary<string, string> dictText,
                  out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            BulkInserter bi;
            DataRow r;

            try
            {
                bi = Inserters["Sections"];
                r = bi.NewRow();
                r["Year"] = year;
                r["Asset"] = assetName;
                ok = bi.AddRow(r, out errorMessage);
                if (!ok)
                {
                    return ok;
                }

                bi = Inserters["SectionNumericAttributes"];
                foreach (var key in dictNumeric.Keys)
                {
                    r = bi.NewRow();
                    r["Year"] = year;
                    r["Asset"] = assetName;
                    r["AttributeName"] = key;
                    r["NumericValue"] = dictNumeric[key]; ;
                    ok = bi.AddRow(r, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }
                }

                bi = Inserters["SectionTextAttributes"];
                foreach (string key in dictText.Keys)
                {
                    string val = dictText[key];
                    r = bi.NewRow();
                    r["Year"] = year;
                    r["Asset"] = assetName;
                    r["AttributeName"] = key;
                    r["TextValue"] = val;
                    ok = bi.AddRow(r, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
            }

            return ok;
        }


        private static bool ImportAdditionalTable(JsonConfig config, string bamsTableName, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"ImportAdditionalTable {bamsTableName} - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(config.TargetConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand($"TRUNCATE TABLE tbl_bams_{bamsTableName}", conn))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (BulkInserter inserter = new BulkInserter())
                    {

                        ok = inserter.Configure(conn, $"tbl_bams_{bamsTableName}", bamsTableName, null, null, out errorMessage);
                        if (ok)
                        {
                            using (SqlConnection BAMSconn = new SqlConnection(config.SourceConnectionString))
                            {
                                BAMSconn.Open();
                                using (SqlCommand command = new SqlCommand())
                                {
                                    command.Connection = BAMSconn;
                                    command.CommandType = System.Data.CommandType.Text;
                                    command.CommandText = $"SELECT * FROM {bamsTableName} WITH (NOLOCK)";
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

            Log.Info($"ImportAdditionalTable {bamsTableName} - ended");

            return ok;
        }

        public static bool ImportMaintainableAssets(JsonConfig config, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("ImportMaintainableAssets - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(config.TargetConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand("TRUNCATE TABLE tbl_bams_MaintainableAssets", conn))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (BulkInserter inserter = new BulkInserter())
                    {

                        ok = inserter.Configure(conn, "tbl_bams_MaintainableAssets", "MaitainableAssets", null, null, out errorMessage);

                        if (ok)
                        {
                            using (SqlConnection BAMSconn = new SqlConnection(config.SourceConnectionString))
                            {
                                BAMSconn.Open();
                                using (SqlCommand command = new SqlCommand())
                                {
                                    command.Connection = BAMSconn;
                                    command.CommandTimeout = 50000;
                                    command.CommandText = @"
SELECT ma.ID AS AssetID, 
    ma.AssetName AS Asset, 
	adr.NumericValue AS RiskScore,
	CONVERT(INT,ad.TextValue) AS District, 
    CASE WHEN COALESCE(adi.TextValue,'N') = 'N' THEN 1 ELSE 0 END AS Interstate,
	mal.Direction, 
    mal.Discriminator, 
    mal.LocationIdentifier, 
    mal.[Start] AS StartLoc, 
    mal.[End] AS EndLoc,
	adc.TextValue AS County,
    adsr.TextValue AS RouteNum
FROM MaintainableAsset ma  WITH (NOLOCK)
INNER JOIN MaintainableAssetLocation mal WITH (NOLOCK)
	ON mal.MaintainableAssetId = ma.ID
INNER JOIN AttributeDatum ad WITH (NOLOCK)
	ON ad.MaintainableAssetId = ma.ID 
INNER JOIN Attribute attrd  WITH (NOLOCK) 
	ON attrd.Id = ad.AttributeId AND attrd.Name='DISTRICT'
INNER JOIN AttributeDatum adi 
	ON adi.MaintainableAssetId = ma.ID 
INNER JOIN Attribute attri  WITH (NOLOCK) 
	ON attri.Id = adi.AttributeId AND attri.Name='INTERSTATE'
INNER JOIN AttributeDatum adr 
	ON adr.MaintainableAssetId = ma.ID 
INNER JOIN Attribute attrr  WITH (NOLOCK) 
	ON attrr.Id = adr.AttributeId AND attrr.Name='RISK_SCORE'
INNER JOIN AttributeDatum adc WITH (NOLOCK)
    ON adc.MaintainableAssetId = ma.ID
INNER JOIN Attribute attrc WITH (NOLOCK)
    ON attrc.ID = adc.AttributeID AND attrc.Name='COUNTY'
INNER JOIN AttributeDatum adsr WITH (NOLOCK)
    ON adsr.MaintainableAssetId = ma.ID
INNER JOIN Attribute attrsr WITH (NOLOCK)
    ON attrsr.ID = adsr.AttributeID AND attrsr.Name='ROUTENUM'
WHERE AssetName IS NOT NULL;
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

            Log.Info($"ImportMaintainableAssets - ended. OK={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        private static bool SaveBAMSYearOfPlanningHorizon(BAMSYearOfPlanningHorizon yp, bool doFullImport, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            BulkInserter bi;
            DataRow r;

            try
            {
                int year = yp.Year;
                double cv = yp.ConditionOfNetwork;
                bi = Inserters["NetworkCondition"];
                r = bi.NewRow();
                r["Year"] = year;
                r["ConditionValue"] = cv;
                ok = bi.AddRow(r, out errorMessage);
                if (!ok)
                {
                    return ok;
                }

                bi = Inserters["YearlyBudgets"];
                foreach (Budget bdg in yp.Budgets)
                {
                   if (ok)
                   {
                       r = bi.NewRow();
                       r["Year"] = year;
                       r["BudgetName"] = bdg.Name;
                       r["Amount"] = bdg.Funding;
                       ok = bi.AddRow(r, out errorMessage);
                       if (!ok)
                       {
                           return ok;
                       }
                   }
                }

                if (ok && doFullImport)
                {
                    bi = Inserters["YearlyDeficientConditionGoals"];
                    foreach (DeficientConditionGoal g in yp.DeficientConditionGoals)
                    {
                        r = bi.NewRow();
                        r["Year"] = year;
                        r["AttributeName"] = string.IsNullOrEmpty(g.AttributeName) ? "" : g.AttributeName;
                        r["GoalsMet"] = g.GoalIsMet ? 1 : 0;
                        r["GoalName"] = string.IsNullOrEmpty(g.GoalName) ? "" : g.GoalName;
                        r["ActualDeficientPct"] = g.ActualDeficientPercentage;
                        r["AllowedDeficientPct"] = g.AllowedDeficientPercentage;
                        ok = bi.AddRow(r, out errorMessage);
                        if (!ok)
                        {
                            return ok;
                        }
                    }
                }

                bi = Inserters["YearlyTargetConditionGoals"];
                foreach (TargetConditionGoal g in yp.TargetConditionGoals)
                {
                    r = bi.NewRow();
                    r["Year"] = year;
                    r["AttributeName"] = string.IsNullOrEmpty(g.AttributeName) ? "" : g.AttributeName;
                    r["GoalsMet"] = g.GoalIsMet ? 1 : 0;
                    r["GoalName"] = string.IsNullOrEmpty(g.GoalName) ? "" : g.GoalName;
                    r["ActualValue"] = g.ActualValue;
                    r["TargetValue"] = g.TargetValue;
                    ok = bi.AddRow(r, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }
                }

                foreach (BAMSSectionTreatment st in yp.Assets)
                {
                    ok = SaveSectionDataToDb(year, st.Name,
                            st.NumericAttributes, st.TextAttributes, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }

                    bi = Inserters["SectionTreatment"];
                    r = bi.NewRow();
                    r["Year"] = year;
                    r["Asset"] = st.Name;
                    r["AppliedTreatment"] = st.AppliedTreatment;
                    r["TreatmentCause"] = st.TreatmentCause;
                    r["TreatmentStatus"] = st.TreatmentStatus;
                    r["IgnoresSpendingLimit"] = st.TreatmentFundingIgnoresSpendingLimit ? 1 : 0;
                    ok = bi.AddRow(r, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }

                    if (ok && doFullImport)
                    {

                        #region TreatmentConsiderations
                        foreach (TreatmentConsideration tc in st.TreatmentConsiderations)
                        {
                            bi = Inserters["SectionTreatmentConsiderations"];
                            r = bi.NewRow();
                            r["Year"] = year;
                            r["Asset"] = st.Name;
                            r["TreatmentName"] = tc.TreatmentName;
                            if (tc.BudgetPriorityLevel.HasValue)
                            {
                                r["BudgetPriorityLevel"] = tc.BudgetPriorityLevel.Value;
                            }
                            ok = bi.AddRow(r, out errorMessage);
                            if (!ok)
                            {
                                return ok;
                            }

                            bi = Inserters["SectionTreatmentBudgetUsage"];
                            foreach (BudgetUsage bu in tc.BudgetUsages)
                            {
                                r = bi.NewRow();
                                r["Year"] = year;
                                r["Asset"] = st.Name;
                                r["TreatmentName"] = tc.TreatmentName;
                                r["CoveredCost"] = bu.Cost;
                                r["Status"] = bu.Status;
                                r["BudgetName"] = bu.Name;
                                ok = bi.AddRow(r, out errorMessage);
                                if (!ok)
                                {
                                    return ok;
                                }
                            }

                            bi = Inserters["SectionTreatmentCashFlowConsiderations"];
                            foreach (CashFlowConsideration cfc in tc.CashFlowConsiderations)
                            {
                                r = bi.NewRow();
                                r["Year"] = year;
                                r["Asset"] = st.Name;
                                r["TreatmentName"] = tc.TreatmentName;
                                r["CashFlowRuleName"] = cfc.CashFlowRuleName;
                                r["Reason"] = cfc.Reason;
                                ok = bi.AddRow(r, out errorMessage);
                                if (!ok)
                                {
                                    return ok;
                                }
                            }
                        }

                        #endregion
                    }

                    #region TreatmentOptions
                    bi = Inserters["SectionTreatmentOptions"];
                    foreach (BAMSTreatmentOption to in st.TreatmentOptions)
                    {
                        r = bi.NewRow();
                        r["Year"] = year;
                        r["Asset"] = st.Name;
                        r["TreatmentName"] = to.TreatmentName;
                        r["Cost"] = to.Cost;
                        r["Benefit"] = to.Benefit;
                        if (to.RemainingLife.HasValue)
                        {
                            r["RemainingLife"] = to.RemainingLife.Value;
                        }
                        ok = bi.AddRow(r, out errorMessage);
                        if (!ok)
                        {
                            return ok;
                        }
                    }
                    #endregion

                    if (ok && doFullImport)
                    {
                        #region TreatmentRejections
                        bi = Inserters["SectionTreatmentRejections"];
                        foreach (TreatmentRejection tr in st.TreatmentRejections)
                        {
                            r = bi.NewRow();
                            r["Year"] = year;
                            r["Asset"] = st.Name;
                            r["TreatmentName"] = tr.TreatmentName;
                            r["RejectionReason"] = tr.Reason;
                            ok = bi.AddRow(r, out errorMessage);
                            if (!ok)
                            {
                                return ok;
                            }
                        }
                        #endregion

                        #region SectionTreatmentSchedulingCollisions
                        bi = Inserters["SectionTreatmentSchedulingCollisions"];
                        foreach (TreatmentSchedulingCollision tsc in st.TreatmentSchedulingCollisions)
                        {
                            r = bi.NewRow();
                            r["Year"] = year;
                            r["Asset"] = st.Name;
                            r["CollisionYear"] = tsc.Year;
                            r["UnscheduledTreatmentName"] = tsc.NameOfUnscheduledTreatment;
                            ok = bi.AddRow(r, out errorMessage);
                            if (!ok)
                            {
                                return ok;
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            return ok;
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

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connSource;
                    command.CommandText = sql;
                    command.CommandTimeout = 3600;
                    using (SqlDataReader reader = command.ExecuteReader())
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
            foreach (string yr in YearList)
            {
                Log.Debug($"{yr}");
            }
#endif

            foreach (string Year in YearList)
            {
                Log.Info($"Importing year {Year} - started...");
                sql = $"SELECT [Output] FROM dbo.SimulationOutput WITH (NOLOCK) WHERE LEFT(RIGHT([Output], 5),4) = '{Year}'";
                if (!string.IsNullOrEmpty(config.SimulationId))
                {
                    sql += $@"
AND SimulationId='{config.SimulationId}'";
                }

                BAMSYearOfPlanningHorizon yp = new BAMSYearOfPlanningHorizon();

                using (SqlConnection connSource = new SqlConnection(config.SourceConnectionString))
                {
                    connSource.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connSource;
                        command.CommandText = sql;
                        command.CommandTimeout = 3600;
                        object o = command.ExecuteScalar();
                        yp = JsonConvert.DeserializeObject<BAMSYearOfPlanningHorizon>(o.ToString());
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

                    ok = SaveBAMSYearOfPlanningHorizon(yp, config.DoFullJsonImport, out errorMessage);

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

        public static bool TruncateJpAndBamsTables(string configFileName, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("TruncateJpAndBamsTables - started...");

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
	(table_name LIKE 'tbl_jb_%' OR table_name LIKE 'tbl_bams_%')
  ORDER BY table_name";

                        Log.Debug("SQL SELECT: " + selCommand.CommandText);

                        using (SqlDataReader reader = selCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                using (SqlCommand truncCommand = new SqlCommand())
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
            catch (Exception x)
            {
                ok = false;
                errorMessage = x.Message;
                Log.Error(errorMessage);
            }

            Log.Info("TruncateJpAndBamsTables - ended...");

            return (ok);
        }


        private static bool SaveBAMSJsonOutputToDb(BAMSJSONOutput jo, bool doSaveYears, bool doFullImport, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            DataRow r;
            BulkInserter bi;

            Log.Info("SaveBAMSJsonOutputToDb - started...");

            try
            {
                bi = Inserters["NetworkCondition"];
                r = bi.NewRow();
                r["Year"] = 0;
                r["ConditionValue"] = jo.InitialConditionOfNetwork;
                ok = bi.AddRow(r, out errorMessage);

                foreach (Asset asset in jo.InitialSectionSummaries)
                {
                    ok = SaveSectionDataToDb(0, asset.Name, asset.NumericAttributes,
                                    asset.TextAttributes, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }
                }


                if (ok && doSaveYears)
                {
                    foreach (BAMSYearOfPlanningHorizon yp in jo.Years)
                    {
                        ok = SaveBAMSYearOfPlanningHorizon(yp, doFullImport, out errorMessage);
                        if (!ok)
                        {
                            return ok;
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

            Log.Info("SaveBAMSJsonOutputToDb - ended");

            return ok;
        }

        public static bool ImportJsonOutputObject(JsonConfig config, BAMSJSONOutput JsonOutput, out string errorMessage)
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
                        using (SqlCommand command = new SqlCommand($"TRUNCATE TABLE tbl_jb_{tableName}", conn))
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
                        ok = SaveBAMSJsonOutputToDb(JsonOutput, !config.DoYearByYear, config.DoFullJsonImport, out errorMessage);
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

        public static bool ImportSimulationOutput(JsonConfig config, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            BAMSJSONOutput JsonOutput = new BAMSJSONOutput();

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
                                    JsonOutput.InitialSectionSummaries = JsonConvert.DeserializeObject<List<Asset>>(output);
                                    Log.Info($"{outputType} loaded");
                                }
                                else if (outputType.StartsWith("YearlySection"))
                                {
                                    BAMSYearOfPlanningHorizon yrph = new BAMSYearOfPlanningHorizon();
                                    yrph = JsonConvert.DeserializeObject<BAMSYearOfPlanningHorizon>(output);
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

        public static bool ImportFromBAMS(JsonConfig config, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("Import from BAMS - started...");

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
                    foreach (string bamsTableName in config.AdditionalTableList)
                    {
                        ok = ImportAdditionalTable(config, bamsTableName, out errorMessage);
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

            Log.Info("Import from BAMS - ended");

            return (ok);
        }

        public static bool BAMS2JBAMS(string configFile, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("BAMS2JBAMS - started...");
            try
            {
                if (string.IsNullOrEmpty(configFile))
                {
                    JsonConfig config = new JsonConfig();
                    config.SourceConnectionString = @"Data Source = .\SQL2019;Initial Catalog = IAMv2;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=True";
                    config.TargetConnectionString = @"Data Source = .\SQL2019;Initial Catalog = JBAMS;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=True";
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
                    ok = ImportFromBAMS(config, out errorMessage);
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
