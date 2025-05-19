using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.IO;
using PBLogic;



namespace PAMSDataImporter
{

    public static partial class ImportManager
    {
        public static string PAMSDbConnectionString = null;
        public static string TargetDbConnectionString = null;
        public static string JsonFilePath = null;

        public static PAMSJSONOutput JsonOutput = new PAMSJSONOutput();
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
                    ok = inserter.Configure(sqlConnection, "tbl_jp_" + tableName, tableName, null, null, out errorMessage);
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

        private static bool ParseBudgetName(string name, out int district, out bool interstate, out string errorMessage)
        {
            bool ok = true;
            district = 0;
            interstate = false;
            errorMessage = null;

            try
            {
                string[] ss = name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length < 3)
                {
                    interstate = false;
                }
                else
                {
                    interstate = ss[2].ToUpper() == "INTERSTATE";
                }
                district = Int32.Parse(ss[1]);
            }
            catch(Exception ex)
            {
                errorMessage = $"Imbavlid budget name: '{name}'\n" + ex.Message;
                ok = false;
            }
            return (ok);
        }
      

        private static bool SaveSectionDataToDb(int year, string section, string facilityName, 
                        Dictionary<string, double> dictNumeric, Dictionary<string,string> dictText, 
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
                r["Section"] = section;
                r["Facility"] = facilityName;
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
                    r["Section"] = section;
                    r["Facility"] = facilityName;
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
                    r["Section"] = section;
                    r["Facility"] = facilityName;
                    r["AttributeName"] = key;
                    r["TextValue"] = val;
                    ok = bi.AddRow(r, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }
                }
            }
            catch(Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
            }

            return ok;
        }


        private static bool SavePAMSYearOfPlanningHorizon(YearOfPlanningHorizon yp, bool doFullImport, out string errorMessage)
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
                    double amt = bdg.Funding;
                    int district = 0;
                    bool interstate = false;
                    ok = ParseBudgetName(bdg.Name, out district, out interstate, out errorMessage);
                    if (ok)
                    {
                        r = bi.NewRow();
                        r["Year"] = year;
                        r["District"] = district;
                        r["Interstate"] = interstate ? 1 : 0;
                        r["Amount"] = amt;
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

                foreach (SectionTreatment st in yp.Sections)
                {
                    ok = SaveSectionDataToDb(year, st.Section, st.Name,
                            st.NumericAttributes, st.TextAttributes, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }

                    bi = Inserters["SectionTreatment"];
                    r = bi.NewRow();
                    r["Year"] = year;
                    r["Section"] = st.Section;
                    r["Facility"] = st.Name;
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
                            r["Section"] = st.Section;
                            r["Facility"] = st.Name;
                            r["TreatmentName"] = tc.TreatmentName;
                            r["BudgetPriorityLevel"] = tc.BudgetPriorityLevel;
                            ok = bi.AddRow(r, out errorMessage);
                            if (!ok)
                            {
                                return ok;
                            }

                            bi = Inserters["SectionTreatmentBudgetUsage"];
                            foreach (BudgetUsage bu in tc.BudgetUsages)
                            {

                                ok = ParseBudgetName(bu.Name, out int district, out bool interstate, out errorMessage);
                                if (!ok)
                                {
                                    return ok;
                                }
                                r = bi.NewRow();
                                r["Year"] = year;
                                r["Section"] = st.Section;
                                r["Facility"] = st.Name;
                                r["TreatmentName"] = tc.TreatmentName;
                                r["District"] = district;
                                r["Interstate"] = interstate ? 1 : 0;
                                r["CoveredCost"] = bu.Cost;
                                r["Status"] = bu.Status;
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
                                r["Section"] = st.Section;
                                r["Facility"] = st.Name;
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
                    foreach (TreatmentOption to in st.TreatmentOptions)
                    {
                        r = bi.NewRow();
                        r["Year"] = year;
                        r["Section"] = st.Section;
                        r["Facility"] = st.Name;
                        r["TreatmentName"] = to.TreatmentName;
                        r["Cost"] = to.Cost;
                        r["Benefit"] = to.Benefit;
                        double remainingLife = 0;
                        if (to.RemainingLife < 0.01)
                        {
                            remainingLife = 0.0;
                        }
                        else
                        {
                            try
                            {
                                remainingLife = Convert.ToInt32(to.RemainingLife);
                            }
                            catch (Exception)
                            {
                                remainingLife = 999999.99;
                                r["Notes"] = $"{to.RemainingLife}";
                            }
                        }


                        r["RemainingLife"] = remainingLife;
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
                            r["Section"] = st.Section;
                            r["Facility"] = st.Name;
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
                            r["Section"] = st.Section;
                            r["Facility"] = st.Name;
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
            catch(Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            return ok;
        }

        private static bool SavePAMSJsonOutputToDb(PAMSJSONOutput jo, bool doSaveYears, bool doFullImport, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            DataRow r;
            BulkInserter bi;

            Log.Info("SavePAMSJsonOutputToDb - started...");

            try
            {
                bi = Inserters["NetworkCondition"];
                r = bi.NewRow();
                r["Year"] = 0;
                r["ConditionValue"] = jo.InitialConditionOfNetwork;
                ok = bi.AddRow(r, out errorMessage);

                foreach (Facility facility in jo.InitialSectionSummaries)
                {
                    ok = SaveSectionDataToDb(0, facility.Section, facility.Name, facility.NumericAttributes,
                                    facility.TextAttributes, out errorMessage);
                    if (!ok)
                    {
                        return ok;
                    }
                }


                if (ok && doSaveYears)
                {
                    foreach (YearOfPlanningHorizon yp in jo.Years)
                    {
                        ok = SavePAMSYearOfPlanningHorizon(yp, doFullImport, out errorMessage);
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

            Log.Info("SavePAMSJsonOutputToDb - ended");

            return ok;
        }

        public static bool ImportMaintainableAssets(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(TargetDbConnectionString))
                {
                    conn.Open();

                    using(SqlCommand command = new SqlCommand("TRUNCATE TABLE tbl_pams_MaintainableAssets", conn))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (BulkInserter inserter = new BulkInserter())
                    {

                        ok = inserter.Configure(conn, "tbl_pams_MaintainableAssets", "MaitainableAssets", null, null,  out errorMessage);

                        if (ok)
                        {
                           using (SqlConnection PAMSconn = new SqlConnection(PAMSDbConnectionString))
                            {
                                PAMSconn.Open();
                                using (SqlCommand command = new SqlCommand())
                                {
                                    command.Connection = PAMSconn;
                                    command.CommandTimeout = 50000;
                                    command.CommandText = @"
SELECT ma.ID AS AssetID, 
    -- ma.NetworkID, 
    ma.SectionName AS Section, 
    ma.FacilityName AS Facility, 
	ma.Area, 
    -- ma.AreaUnit, 
    adr.NumericValue AS RiskScore,
	CONVERT(INT,ad.TextValue) AS District, 
    CASE WHEN COALESCE(adi.TextValue,'N') = 'N' THEN 1 ELSE 0 END AS Interstate,
	-- ma.SpatialWeighting,
	mal.Direction, 
    mal.Discriminator, 
    mal.LocationIdentifier, 
    mal.[Start] AS StartLoc, 
    mal.[End] AS EndLoc,
    adc.TextValue AS County,
    adsr.TextValue AS SR
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
	ON attrr.Id = adr.AttributeId AND attrr.Name='RISKSCORE'
INNER JOIN AttributeDatum adc WITH (NOLOCK)
    ON adc.MaintainableAssetId = ma.ID
INNER JOIN Attribute attrc WITH (NOLOCK)
    ON attrc.ID = adc.AttributeID AND attrc.Name='CNTY'
INNER JOIN AttributeDatum adsr WITH (NOLOCK)
    ON adsr.MaintainableAssetId = ma.ID
INNER JOIN Attribute attrsr WITH (NOLOCK)
    ON attrsr.ID = adsr.AttributeID AND attrsr.Name='SR'

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
            catch(Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
            }
            return (ok);
        }

        public static bool ImportJsonFile(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            try
            {
                ok = ClearInserters(out errorMessage);
                if (!ok)
                {
                    return ok;
                }

                string jsonText = File.ReadAllText(JsonFilePath);
                JsonOutput = JsonConvert.DeserializeObject<PAMSJSONOutput>(jsonText);

                using(SqlConnection conn = new SqlConnection(TargetDbConnectionString))
                {
                    conn.Open();

                    foreach(string tableName in TableNames)
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
                        ok = SavePAMSJsonOutputToDb(JsonOutput, true, false, out errorMessage);
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
            catch(Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
            }
          
            return (ok);
        }
    }
}
