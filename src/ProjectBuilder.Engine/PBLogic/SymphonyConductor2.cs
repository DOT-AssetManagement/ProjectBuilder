using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;
using Newtonsoft;
using Newtonsoft.Json;

#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8625

namespace PBLogic
{
    public partial class SymphonyConductor
    {

        public Dictionary<int, TimewiseConstrainedProject> Projects { get; set; }
        public MultiYearBudget OriginalBudget = new MultiYearBudget();
        public Dictionary<string, int> BudgetAttributePairs = new Dictionary<string, int>();
        public Dictionary<int, string> BudgetAttributePairsReversed = new Dictionary<int,string>();
        private Dictionary<string, double> BudgetConstraints = new();
        private Dictionary<string, double> BudgetSpent = new Dictionary<string, double>();

        public bool PopulateProjectListAndBudgetConstraints(string connectionString, int scenId, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"PopulateProjectListAndBudgetConstraints (scenId={scenId}) - started...");

            int firstScenarioYear = 0;
            int lastScenarioYear = 0;
            int maxLeftShift = 99;
            int maxRightShift = 99;

            try
            {
                Projects.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT TOP 1 ParmValue FROM tbl_pb_ScenParm WITH (NOLOCK) WHERE ScenId={scenId} AND ParmID='YFST'";
                        cmd.CommandType = CommandType.Text;
                        object o = cmd.ExecuteScalar();
                        if (o == null && o == DBNull.Value)
                        {
                            throw new Exception($"Valid entry not found in tbl_pb_ScenParm for ScenId={scenId} AND ParmID='YFST'");
                        }
                        firstScenarioYear = (int)(Convert.ToDouble(o));
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT TOP 1 ParmValue FROM tbl_pb_ScenParm WITH (NOLOCK) WHERE ScenId={scenId} AND ParmID='YLST'";
                        cmd.CommandType = CommandType.Text;
                        object o = cmd.ExecuteScalar();
                        if (o == null && o == DBNull.Value)
                        {
                            throw new Exception($"Valid entry not found in tbl_pb_ScenParm for ScenId={scenId} AND ParmID='YLST'");
                        }
                        lastScenarioYear = (int)(Convert.ToDouble(o));
                    }

                    Log.Info($"First scenario year: {firstScenarioYear}");
                    Log.Info($"Last scenario year: {lastScenarioYear}");

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT TOP 1 ParmValue FROM tbl_pb_ScenParm WITH (NOLOCK) WHERE ScenId={scenId} AND ParmID='HFTL'";
                        cmd.CommandType = CommandType.Text;
                        object o = cmd.ExecuteScalar();
                        if (o == null && o == DBNull.Value)
                        {
                            throw new Exception($"Valid entry not found in tbl_pb_ScenParm for ScenId={scenId} AND ParmID='HFTL'");
                        }
                       maxLeftShift = (int)(Convert.ToDouble(o));
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT TOP 1 ParmValue FROM tbl_pb_ScenParm WITH (NOLOCK) WHERE ScenId={scenId} AND ParmID='HFTR'";
                        cmd.CommandType = CommandType.Text;
                        object o = cmd.ExecuteScalar();
                        if (o == null && o == DBNull.Value)
                        {
                            throw new Exception($"Valid entry not found in tbl_pb_ScenParm for ScenId={scenId} AND ParmID='HFTR'");
                        }
                        maxRightShift = (int)(Convert.ToDouble(o));
                    }

                    Log.Info($"Maximum left shift (push forward years): {maxLeftShift}");
                    Log.Info($"Maximum right shift (delay years): {maxRightShift}");

                    // DIG 08/30/25
                    // The syntax for Utility=RANK() OVER(...) corresponds to the expression from vw_pb_UtilityProxies
                    // for Code = 'HPB' (see sp_pb_CreateExtendedAltrnatives)
                    // sp_pb_CreateAlternatives is not used in version 2.  
                    // All alternatives are created in C# code

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"SELECT p.ProjectId, TotalCost, MinYear, MaxYear, IsUserCreated
	, PreferredStartingYear, PreferredEndingYear, r.Utility 
FROM tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK) 
INNER JOIN (
	SELECT OriginId AS ProjectId,
	Utility = RANK() OVER(ORDER BY CONVERT(INT,HasCommitted) ASC, PriorityOrder DESC, Benefit ASC, 
			(CASE IsUserCreated WHEN 1 THEN 'U' ELSE 'P' END), OriginId)
	FROM
	(
		SELECT 
			CASE IsUserCreated WHEN 1 THEN 'U' ELSE 'P' END AS Origin,
				ProjectId AS OriginId,
				District, 
				Benefit, Risk, HasCommitted, PriorityOrder,
				IsUserCreated
			FROM tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
			WHERE p.ScenId={scenId} AND p.IsSuppressed=0
		)q 
	) r
ON r.ProjectId = p.ProjectId
WHERE p.ScenId={scenId} AND IsSuppressed=0 AND IsSelected=0
ORDER BY 1";
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 60000;

                        Log.Debug(cmd.CommandText);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int projectId = Convert.ToInt32(reader["ProjectId"]);
                                double totalCost = Convert.ToDouble(reader["TotalCost"]);
                                int MinYear = Convert.ToInt32(reader["MinYear"]);
                                int MaxYear = Convert.ToInt32(reader["MaxYear"]);
                                bool isUserCreated = Convert.ToBoolean(reader["IsUserCreated"]);
                                int preferredStartingYear = Convert.ToInt32(reader["PreferredStartingYear"]);
                                int preferredEndingYear = Convert.ToInt32(reader["PreferredEndingYear"]);
                                double Utility = Convert.ToDouble(reader["Utility"]);
                                Projects.Add(projectId, new TimewiseConstrainedProject
                                {
                                    MinYear = MinYear,
                                    MaxYear = MaxYear,
                                    IsUserCreated = isUserCreated,
                                    PreferredEndingYear = preferredEndingYear,
                                    PreferredStartingYear = preferredStartingYear,
                                    UtilityPreferred = Utility
                                }
                                );
                            }
                            reader.Close();
                        }
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"SELECT AssignedTimewiseConstrainedProjectId
    , ExtendedTreatmentId 
    , Cost = (ISNULL(Cost,0) + ISNULL(IndirectCostDesign,0) + ISNULL(IndirectCostROW,0) + ISNULL(IndirectCostUtilities,0) + ISNULL(IndirectCostOther,0))
    , District
    , Interstate
    , AssetType
    , PreferredYear
    , IsCommitted
    , PriorityOrder
FROM tbl_pb_ExtendedImportedTreatments WITH (NOLOCK)
WHERE ScenId={scenId} AND AssignedTimewiseConstrainedProjectId IS NOT NULL";
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 60000;

                        Log.Debug(cmd.CommandText);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int projectId = Convert.ToInt32(reader["AssignedTimewiseConstrainedProjectId"]);
                                int treatmentId = Convert.ToInt32(reader["ExtendedTreatmentId"]);

                                if (Projects.ContainsKey(projectId))
                                {
                                    bool isCommitted = false;   // These are defaults
                                    int priorityOrder = 4;

                                    double cost = Convert.ToDouble(reader["Cost"]);
                                    int district = Convert.ToInt32(reader["District"]);
                                    int isInterstate = Convert.ToInt32(reader["Interstate"]);
                                    string assetType = reader["AssetType"].ToString().ToUpper().Substring(0, 1);
                                    int preferredYear = Convert.ToInt32(reader["PreferredYear"]);

                                    if (reader["IsCommitted"] != null && reader["IsCommitted"] != DBNull.Value)
                                    {
                                        isCommitted = Convert.ToBoolean(reader["IsCommitted"]);
                                    }

                                    if (reader["PriorityOrder"] != null && reader["PriorityOrder"] != DBNull.Value)
                                    {
                                        priorityOrder = Convert.ToInt32(reader["PriorityOrder"]);
                                    }

                                    Projects[projectId].Treatments.Add(treatmentId, new NarrowLibraryTreatment()
                                    {
                                        Cost = cost,
                                        District = district,
                                        IsInterstate = isInterstate,
                                        AssetType = assetType,
                                        PreferredYear = preferredYear,
                                        IsCommitted = isCommitted,
                                        PriorityOrder = priorityOrder,
                                    }
                                    );
                                }
                            }
                            reader.Close();
                        }
                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"SELECT t.ExtendedTreatmentId, t.AssignedTimewiseConstrainedProjectId,
	 a.AttributeName, a.AttributeType, c.TextValue, c.NumericValue 
FROM tbl_pb_ExtendedImportedTreatments t WITH (NOLOCK)
INNER JOIN tbl_lib_CargoData c WITH (NOLOCK)
   ON c.AssetType=t.AssetType
  AND c.ImportTimeGeneratedGuid=t.ImportTimeGeneratedId
INNER JOIN tbl_lib_CargoAttributes a WITH (NOLOCK)
   ON  a.LibraryId=c.LibraryId
   AND a.AttributeNo=c.AttributeNo
   AND a.AssetType=t.AssetType
WHERE t.ScenId={scenId} 
  AND t.AssignedTimewiseConstrainedProjectId IS NOT NULL
  AND c.TextValue IS NOT NULL
  AND AttributeName IN (SELECT DISTINCT AttributeName 
                        FROM tbl_pb_ExtendedScenarioBudgetWithAttributes WITH (NOLOCK)
						WHERE ScenId=t.ScenId AND AttributeName IS NOT NULL AND AttributeName <>'*'
						)
ORDER BY ExtendedTreatmentId, AttributeName";

                        Log.Debug(cmd.CommandText);

                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 60000;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int projectId = Convert.ToInt32(reader["AssignedTimewiseConstrainedProjectId"]);
                                if (Projects.ContainsKey(projectId))
                                {

                                    TimewiseConstrainedProject project = Projects[projectId];
                                    long treatmentId = Convert.ToInt64(reader["ExtendedTreatmentId"]);
                                    if (project.Treatments.ContainsKey(treatmentId))
                                    {
                                        NarrowLibraryTreatment treatment = project.Treatments[treatmentId];
                                        CargoValue cargo = new CargoValue()
                                        {
                                            CargoType = reader["AttributeType"].ToString(),
                                            TextValue = reader["TextValue"].ToString()
                                        };

                                        if (reader["NumericValue"] != null && reader["NumericValue"] != DBNull.Value)
                                        {
                                            cargo.NumericValue = Convert.ToDouble(reader["NumericValue"]);
                                        }
                                        treatment.Cargo.Add(reader["AttributeName"].ToString().ToUpper(), cargo);
                                    }
                                }
                            }
                            reader.Close();
                        }

                        ok = PopulateOriginalRelevantBudgetConstraints(conn, scenId, out errorMessage);

                        if (ok)
                        {
                            ok = PopulateBudgetAttributePairs(conn, scenId, out errorMessage);
                        }

                        if (ok)
                        {
                         
                            Log.Info("Original budget structure:");
                            Log.Info(JsonConvert.SerializeObject(OriginalBudget, Formatting.Indented));
                            Log.Info("Relevant budget attributes pairs:");
                            Log.Info(JsonConvert.SerializeObject(BudgetAttributePairs, Formatting.Indented));
                            Log.Info("Relevant budget attributes pairs (reversed):");
                            Log.Info(JsonConvert.SerializeObject(BudgetAttributePairsReversed, Formatting.Indented));
                        }
                    }
                }
                foreach (int projectKey in Projects.Keys)
                {
                    TimewiseConstrainedProject project = Projects[projectKey];
                    project.PopulateProjectDetail(BudgetAttributePairs, firstScenarioYear, lastScenarioYear, maxLeftShift, maxRightShift);
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Log.Error(ex.Message);
                errorMessage = ex.Message;
            }

            Log.Info($"PopulateProjectListAndBudgetConstraints (scenId={scenId}) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        public bool PopulateOriginalRelevantBudgetConstraints(SqlConnection connection, int scenId, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"PopulateOriginalRelevantBudgetConstraints (scenId={scenId}) - started...");

            try
            {
                OriginalBudget.Clear();

                string sql = $@"SELECT YearWork, District, IsInterstate, AssetType, AttributeName, AttributeValue, Budget
FROM vw_pb_ExtendedScenarioBudgetWithAttributes b WITH (NOLOCK)
WHERE b.ScenId = {scenId} AND b.District<>'*' AND b.IsInterstate <>'*' AND b.AssetType <> '*'
	AND 
EXISTS (
        SELECT TOP 1 1 FROM tbl_pb_ExtendedImportedTreatments x WITH (NOLOCK)
				WHERE x.ScenId = b.ScenId
				  AND x.District = b.District
				  AND x.Interstate = b.IsInterstate
				  AND (x.AssetType=b.AssetType OR b.AssetType='R')
        )
UNION ALL 
SELECT YearWork, District, IsInterstate, AssetType, AttributeName, AttributeValue, Budget
FROM vw_pb_ExtendedScenarioBudgetWithAttributes b WITH (NOLOCK)
WHERE b.ScenId = {scenId} 
  AND (b.District='*' OR b.IsInterstate='*' OR b.AssetType='*')
ORDER BY YearWork, District, IsInterstate, AttributeName, AttributeValue;";

                Log.Debug(sql);

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int yr = Convert.ToInt32(reader["YearWork"]);

                            string district = "*";
                            string assetType = "*'";
                            string isInterstate = "*";

                            object oDistrict = reader["District"];
                            object oAssetType = reader["AssetType"];
                            object oIsInterstate = reader["IsInterstate"];

                            if (oDistrict !=null && oDistrict != DBNull.Value)
                            {
                                int dist = Convert.ToInt32(oDistrict);
                                district = dist.ToString();
                            }
                            if (oAssetType != null &&  oAssetType != DBNull.Value)
                            {
                                assetType = oAssetType.ToString().ToUpper()[0].ToString();
                            }
                            if (oIsInterstate !=null &&  oIsInterstate != DBNull.Value)
                            {
                                isInterstate = oIsInterstate.ToString().ToUpper()[0].ToString();   
                            }

                            string attrName = "*";
                            string attrValue = "*";
                            if (reader["AttributeName"] != null && reader["AttributeName"] != DBNull.Value &&
                                    reader["AttributeValue"] != null && reader["AttributeValue"] != DBNull.Value &&
                                        reader["AttributeName"].ToString().Trim() != "*" &&
                                            reader["AttributeValue"].ToString().Trim() != "*")
                            {
                                attrName = reader["AttributeName"].ToString().Trim();
                                attrValue = reader["AttributeValue"].ToString().Trim();
                            }
                            double budget = Convert.ToDouble(reader["Budget"]);

                            Dictionary<string, BudgetPrefix> prefixDict = new Dictionary<string, BudgetPrefix>();

                            if (OriginalBudget.Budget.ContainsKey(yr))
                            {
                                prefixDict = OriginalBudget.Budget[yr];
                            }
                            else
                            {
                                OriginalBudget.Budget.Add(yr, prefixDict);
                            }

                            BudgetPrefix prefix = new BudgetPrefix() { AssetType = assetType, District = district, IsInterstate = isInterstate };

                            if (prefixDict.ContainsKey(prefix.ToString()))
                            {
                                string key = prefix.ToString();
                                prefix = prefixDict[key];
                            }
                            else
                            {
                                prefixDict.Add(prefix.ToString(), prefix);
                            }

                            if (attrName == "*" || attrValue == "*")
                            {
                                prefix.Cost = budget;
                            }
                            else
                            {
                                Dictionary<string, double> myValueCosts = new Dictionary<string, double>();

                                if (prefix.AttributeCosts.ContainsKey(attrName))
                                {
                                    myValueCosts = prefix.AttributeCosts[attrName];
                                }
                                else
                                {
                                    prefix.AttributeCosts.Add(attrName, myValueCosts);
                                }

                                if (myValueCosts.ContainsKey(attrValue))
                                {
                                    myValueCosts[attrValue] = budget;  // should not happen in principle
                                }
                                else
                                {
                                    myValueCosts.Add(attrValue, budget);
                                }
                            }
                        }
                        reader.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                ok = false;
                Log.Error(ex.Message);
                errorMessage = ex.Message;
            }

            Log.Info($"PopulateOriginalRelevantBudgetConstraints (scenId={scenId}) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }


        public bool PopulateBudgetAttributePairs(SqlConnection conn, int scenId, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"PopulateBudgetAttributePairs (scenId={scenId}) - started...");

            try
            {
                string sql = $@"SELECT AttributePairCode, ROW_NUMBER() OVER (ORDER BY AttributePairCode) AS AttributePairNo
FROM (
	SELECT DISTINCT'[' + UPPER(RTRIM(LTRIM(AttributeName))) + '].[' +
		UPPER(RTRIM(LTRIM(AttributeValue))) +']' AS AttributePairCode
	FROM tbl_pb_ExtendedScenarioBudgetWithAttributes WITH (NOLOCK)
	WHERE AttributeName IS NOT NULL AND AttributeName <> '*'
      AND AttributeValue IS NOT NULL AND AttributeValue <> '*'
	  AND ScenId={scenId}
  ) q";
                Log.Debug(sql);

                BudgetAttributePairs.Clear();
                BudgetAttributePairsReversed.Clear();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string pairCode = reader["AttributePairCode"].ToString();
                            int pairNo = Convert.ToInt32(reader["AttributePairNo"]);
                            BudgetAttributePairs.Add(pairCode, pairNo);
                            BudgetAttributePairsReversed.Add(pairNo, pairCode);
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Log.Error(ex.Message);
                errorMessage = ex.Message;
            }


            Log.Info($"PopulateBudgetAttributePairs (scenId={scenId}) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }


        public bool GenerateMpsFileForMultiYearProjectsV2(ScenarioCommunique filter, bool addBinaryBounds, out string? mpsFilePath,
          out string? errorMessage, out bool degenerateCase)
        {
            bool ok = true;
            errorMessage = null;
            mpsFilePath = null;
            degenerateCase = false;


            Log.Info("GenerateMpsFile - started");
            LogParameters();

            string mode = addBinaryBounds ? "Strict" : "Relaxed";
            mpsFilePath = Path.Combine(HomeDirectory, $"LP\\SC_{ScenId}_{mode}.mps");
                   Log.Info($"Generating file {mpsFilePath}");
            Log.Info($"Filter: UserOrCommitment={filter.Commitment}, ProjectsOnly={filter.ProjectsOnly}, District={filter.District}, MaxPriority={filter.MaxPriority}");

            List<int> QualifyingProjectIds = new List<int>();
            List<int> AlternativeIds = new List<int>();

            try
            {

                foreach (var proj in Projects)
                {
                    int projectId = proj.Key;
                    TimewiseConstrainedProject project = proj.Value;

                    if (project.IsSelected)
                    {
                        continue;
                    }

                    // DIG 4/7/2026
                    // Added this additional check to avoid running 'strict' model on projects
                    // that had their selected alternatives explicitly set as 'do-nothing'
                    // during the 'relaxed' phase
                    if (addBinaryBounds && project.DoNothingAlternativeExplicitlySelected)
                    {
                        continue;
                    }

                    if (filter.District.HasValue && project.Treatments.Count > 0)
                    {
                        bool qualifies = true;
                        foreach (var t in project.Treatments)
                        {

                            int? district = t.Value.District;
                            if (district == null || district.Value != filter.District.Value)
                            {
                                qualifies = false;
                                break;
                            }
                        }
                        if (!qualifies)
                        {
                            continue;
                        }
                    }

                    if (filter.Commitment && !project.HasCommitted && !project.IsUserCreated)
                    {
                        continue;
                    }

                    if (filter.MaxPriority.HasValue && project.PriorityOrder > filter.MaxPriority.Value)
                    {
                        continue;
                    }

                    QualifyingProjectIds.Add(projectId);
                }

                Log.Info($"# of qualifying projects: {QualifyingProjectIds.Count}");

                if (QualifyingProjectIds.Count < 1)
                {
                    degenerateCase = true;
                    throw new Exception("No treatment alternatives left for selection.");
                }

   
                using (StreamWriter sw = File.CreateText(mpsFilePath))
                {
                    sw.WriteLine($"NAME SC_{ScenId}_{mode}");
                    Log.InfoFormat("MPS file path:\t{0}", mpsFilePath);

                    sw.WriteLine($"* {DateTime.Now}");
                    sw.WriteLine("* Additional (cargo-based) budget attributes being used:");
                    foreach (var pair in BudgetAttributePairs)
                    {
                        sw.WriteLine($"* {pair.Key} => G{pair.Value}");
                    }

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();

                        sw.WriteLine("ROWS");
                        sw.WriteLine(" N UTILITY");

                        /* Budget constraints */

                        int j = 0;
                        foreach(var budgetEntry in BudgetConstraints)
                        {
                            sw.WriteLine($" L {budgetEntry.Key}");
                            j++;
                        }
                        Log.Debug($"# of budget constraints rows written: {j}");


                        /* Projects */

                        j = 0;
                        foreach (int projectId in QualifyingProjectIds)
                        {
                            sw.WriteLine($" E X{projectId}");
                            j++;
                        }

                        Log.Info($"# of project constraints written: {j}");

                        if (j < 1)
                        {
                            degenerateCase = true;
                            throw new Exception("No treatment alternatives left for selection.");
                        }

                        sw.WriteLine("COLUMNS");

                        j = 0;
                        int k = 0;

                        foreach (int projectId in QualifyingProjectIds)
                        {
                            TimewiseConstrainedProject project = Projects[projectId];

                            int altNo = projectId * 100;
                            AlternativeIds.Add(altNo);

                            sw.WriteLine($" {altNo} X{projectId} 1 UTILITY 0");
                            j++;

                            foreach (var altVar in project.Alternatives)
                            {
                                int Id = altVar.Key;
                                ProjectAlternative alt = altVar.Value;
                                altNo = projectId * 100 + Id;
                                AlternativeIds.Add(altNo);

                                sw.WriteLine($" {altNo} X{projectId} 1 UTILITY {-alt.WorkingUtility}");
                                j++;
                                SortedDictionary<string, double> Costs = filter.MixAssetBudgets ? alt.Costs[1] : alt.Costs[0];
                                foreach (var Cost in Costs)
                                {
                                    if (BudgetConstraints.ContainsKey(Cost.Key))    // Not all cargo-related pairs may occur in all years of budget constraints
                                    {
                                        sw.WriteLine($" {altNo} {Cost.Key} {Cost.Value}");
                                        k++;
                                    }
                                }

                            }
                        }

                        Log.Debug($"# of utility columns written: {j}");
                        Log.Debug($"# of cost columns written: {k}");


                        sw.WriteLine("RHS");

                        j = 0;
                        foreach (var constr in BudgetConstraints)
                        {
                            // Pre-funded committed projects can overdraw a constraint; a negative RHS would make the model infeasible
                            sw.WriteLine($" RHSLE {constr.Key} {Math.Max(0.0, constr.Value)}");
                            j++;
                        }

                        Log.Debug($"# of cost RHSLE written: {j}");

                        j = 0;
                        foreach (int projectId in QualifyingProjectIds)
                        {
                            sw.WriteLine($" RHSLE X{projectId} 1");
                            j++;
                        }

                        Log.Debug($"# of project RHSLE written: {j}");


                        /* BOUNDS */

                        if (addBinaryBounds)
                        {
                            j = 0;
                            sw.WriteLine("BOUNDS");
                            foreach (int altId in AlternativeIds)
                            {
                                sw.WriteLine($" UI BOUND {altId} 1");
                                j++;
                            }

                            Log.Debug($"# of UI BOUND written: {j}");
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

        public void PopulateBudgetConstraintsDictionary(ScenarioCommunique filter)
        {
            BudgetConstraints.Clear();
            BudgetSpent.Clear();

            foreach (var budgetEntry in OriginalBudget.Budget)
            {
                int yr = budgetEntry.Key;
                Dictionary<string, BudgetPrefix> budgetPrefixDict = budgetEntry.Value;
                foreach (var budgetPrefix in budgetPrefixDict)
                {
                    string prefixCode = budgetPrefix.Key;
                    BudgetPrefix prefixDetail = budgetPrefix.Value;

                    if (prefixDetail.AssetType.ToUpper() == "R" && !filter.MixAssetBudgets)
                    {
                        continue;
                    }

                    if (prefixDetail.AssetType.ToUpper() != "R" && filter.MixAssetBudgets)
                    {
                        continue;
                    }

                    if (filter.District.HasValue && prefixDetail.District != filter.District.ToString())
                    {
                        continue;
                    }

                    string constrCode = $"Y{yr}{prefixCode}";
                    if (prefixDetail.Cost.HasValue)
                    {
                        BudgetConstraints.Add(constrCode, prefixDetail.Cost.Value);
                        BudgetSpent.Add(constrCode, 0.0);
                    }

                    foreach (var attrDetail in prefixDetail.AttributeCosts)
                    {
                        string attrName = attrDetail.Key;
                        Dictionary<string, double> attrDtail = attrDetail.Value;
                        foreach (var attrValueDetail in attrDetail.Value)
                        {
                            string suffix = $"[{attrName}].[{attrValueDetail.Key}]";
                            int attrNo = BudgetAttributePairs[suffix.ToUpper()];
                            constrCode = $"Y{yr}{prefixCode}G{attrNo}";
                            BudgetConstraints.Add(constrCode, attrValueDetail.Value);
                            BudgetSpent.Add(constrCode , 0.0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import optimization results from Sumphony results file into the database
        /// </summary>
        /// <param name="resultFilePath">Full path to the Symphony results file</param>
        /// <param name="errorMessage">(out) Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool ImportOptimizationResultsV2(string resultFilePath, ScenarioCommunique filter, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            string header = "Column names and values of nonzeros in the solution";
            string allzeros = "All columns are zero in the solution!";
            string infeasible = "The problem is infeasible!";

            long recordsProcessed = 0;
            long explicitDoNothingsCount = 0;

            Log.InfoFormat("ImportOptimizationResultsV2 - started");

            if (!File.Exists(resultFilePath))
            {
                errorMessage = string.Format("File {0} not found.", resultFilePath);
                Log.Error(errorMessage);
                return (false);
            }

            try
            {
                Log.Info($"Results file path:\t{resultFilePath}");

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
                            double fraction = Convert.ToDouble(sV[1]);

                            if (fraction >= 1.0)
                            {
                                long V = Convert.ToInt64(sV[0]);
                                int projectId = (int)(V / 100);
                                int altNo = (int)(V % 100);

                                if (altNo > 0)  // Do-nothings amount to non-selection
                                {
                                    ProjectAlternative alternative = Projects[projectId].Alternatives[altNo];
                                    Projects[projectId].IsSelected = true;
                                    Projects[projectId].SelectedFirstYear =alternative.FirstYearWork;
                                    SortedDictionary<string, double> costs = alternative.Costs[filter.MixAssetBudgets ? 1 : 0];
                                    foreach(var costEntry in costs)
                                    {
                                        string key = costEntry.Key;
                                        if (BudgetConstraints.ContainsKey(key))
                                        {
                                            BudgetConstraints[key] -= costEntry.Value;
                                            BudgetSpent[key] += costEntry.Value;
                                        }
                                    }
                                }
                                else
                                {
                                    Projects[projectId].IsSelected = false;
                                    Projects[projectId].DoNothingAlternativeExplicitlySelected = true;
                                    explicitDoNothingsCount++;
                                }
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
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.InfoFormat("Records processed: {0}", recordsProcessed);
            Log.InfoFormat($"Number of 'Do nothing' alternatives explicitly selected: {explicitDoNothingsCount}");
            Log.InfoFormat("ImportOptimizationResultsV2 - ended.\t Ok={0};\tErrorMessage: {1}", ok, errorMessage);
            return (ok);
        }

        // Selects committed projects at their preferred year regardless of budget; the solver allocates what remains
        public void FundCommittedProjects(ScenarioCommunique filter)
        {
            int funded = 0;

            foreach (var proj in Projects)
            {
                TimewiseConstrainedProject project = proj.Value;

                if (!project.HasCommitted || project.IsSelected || project.Alternatives.Count < 1)
                {
                    continue;
                }

                ProjectAlternative alternative = project.Alternatives.Values.FirstOrDefault(a => a.IsPreferred)
                                                    ?? project.Alternatives.Values.First();

                project.IsSelected = true;
                project.SelectedFirstYear = alternative.FirstYearWork;

                SortedDictionary<string, double> costs = alternative.Costs[filter.MixAssetBudgets ? 1 : 0];
                foreach (var costEntry in costs)
                {
                    string key = costEntry.Key;
                    if (BudgetConstraints.ContainsKey(key))
                    {
                        BudgetConstraints[key] -= costEntry.Value;
                        BudgetSpent[key] += costEntry.Value;
                    }
                }

                funded++;
            }

            Log.Info($"# of committed projects funded ahead of optimization: {funded}");

            foreach (var constr in BudgetConstraints.Where(c => c.Value < 0))
            {
                Log.Warn($"Budget constraint {constr.Key} overdrawn by committed projects: {-constr.Value}");
            }
        }

        public bool RunMixedOptimizationV2(ScenarioCommunique filter, int NumIterations, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            string? mpsFilePath = null;
            string? resultFilePath = null;
            string? errorFilePath = null;

            Log.Info($"RunMixedOptimizationV2 NumIterations={NumIterations}  - started...");

            try
            {
                PopulateBudgetConstraintsDictionary(filter);
                FundCommittedProjects(filter);

                for (int I = 0; ok && I < NumIterations; I++)
                {
                    Log.Info($"RunMixedOptimization, Pass={I + 1}");

                    if (ok)
                    {
                        ok = GenerateMpsFileForMultiYearProjectsV2(filter, (I == 1), out mpsFilePath, out errorMessage, out bool degenerateCase);

                        if (ok && !degenerateCase)
                        {
                            if (ok)
                            {
                                ok = RunSymphony(mpsFilePath, out resultFilePath, out errorFilePath, out errorMessage);
                            }

                            if (ok)
                            {
                                ok = ImportOptimizationResultsV2(resultFilePath, filter, out errorMessage);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Error(errorMessage);
                ok = false;
            }

            Log.Info($"RunMixedOptimizationV2 - ended.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        public bool RecordProjectAndTreatmentSelections(int scenId, out string? errorMessage, bool resetOnly=false)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"RecordProjectAndTreatmentSelections (ScenId={scenId}) - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = $"DELETE tbl_pb_ScenarioProjectSelections WHERE ScenId={scenId}";
                        command.CommandType = CommandType.Text;
                        Log.Debug(command.CommandText);
                        long N = command.ExecuteNonQuery();
                        Log.Info($"# of old records deleted from tbl_pb_ScenarioProjectSelections: {N}");
                    }

                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = $@"UPDATE tbl_pb_ExtendedTimewiseConstrainedProjects 
SET IsSelected = 0, SelectedFirstYear=NULL, SelectedLastYear=NULL
WHERE ScenId={scenId}";
                        command.CommandType = CommandType.Text;
                        Log.Debug(command.CommandText);
                        long N = command.ExecuteNonQuery();
                        Log.Info($"# of projects from tbl_pb_ExtendedTimewiseConstrainedProjects reset, : {N}");
                    }

                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = $@"UPDATE tbl_pb_ExtendedImportedTreatments 
SET IsSelected = 0, SelectedYear=NULL
WHERE ScenId={scenId}";
                        command.CommandType = CommandType.Text;
                        Log.Debug(command.CommandText);
                        long N = command.ExecuteNonQuery();
                        Log.Info($"# of treatments from tbl_pb_ExtendedImportedTreatments reset, : {N}");
                    }

                    if (!resetOnly)
                    {
                        int M = 0;

                        using (BulkInserter bulkInserter = new BulkInserter())
                        {
                            ok = bulkInserter.Configure(conn, "tbl_pb_ScenarioProjectSelections", "dt", null, null, out errorMessage);
                            if (!ok)
                            {
                                throw new Exception(errorMessage);
                            }

                            foreach (var projectEntry in Projects)
                            {
                                TimewiseConstrainedProject project = projectEntry.Value;
                                if (project.IsSelected)
                                {
                                    DataRow r = bulkInserter.NewRow();
                                    r["ScenId"] = scenId;
                                    r["ProjectId"] = projectEntry.Key;
                                    r["SelectedFirstYear"] = project.SelectedFirstYear;
                                    ok = bulkInserter.AddRow(r, out errorMessage);
                                    if (!ok)
                                    {
                                        throw new Exception(errorMessage);
                                    }
                                    M++;
                                }
                            }
                        }

                        Log.Info($"# records added to tbl_pb_ScenarioProjectSelections: {M}");

                        using (SqlCommand command = conn.CreateCommand())
                        {
                            command.CommandText = $@"UPDATE tbl_pb_ExtendedTimewiseConstrainedProjects
SET IsSelected=1, SelectedFirstYear = s.SelectedFirstYear, SelectedLastYear = s.SelectedFirstYear + (PreferredEndingYear - PreferredStartingYear)
FROM tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (ROWLOCK)
INNER JOIN tbl_pb_ScenarioProjectSelections s WITH (NOLOCK)	
	ON s.ScenId=p.ScenId
   AND s.ProjectId=p.ProjectId
WHERE p.ScenId={scenId};";
                            command.CommandType = CommandType.Text;
                            Log.Debug(command.CommandText);
                            long N = command.ExecuteNonQuery();
                            Log.Info($"# of projects from tbl_pb_ExtendedTimewiseConstrainedProjects having been marked as selected: {N}");
                        }

                        using (SqlCommand command = conn.CreateCommand())
                        {
                            command.CommandText = $@"UPDATE tbl_pb_ExtendedImportedTreatments 
		SET SelectedYear = p.SelectedFirstYear + (t.PreferredYear - p.PreferredStartingYear)
		  , IsSelected=1
	FROM tbl_pb_ExtendedImportedTreatments t WITH (ROWLOCK)
	INNER JOIN tbl_pb_ExtendedTimewiseConstrainedProjects p WITH (NOLOCK)
		ON p.ScenID=t.ScenId
		AND p.ProjectId = t.AssignedTimewiseConstrainedProjectId
		AND t.ScenId = {scenId}
		AND p.IsSelected = 1;";
                            command.CommandType = CommandType.Text;
                            Log.Debug(command.CommandText);
                            long N = command.ExecuteNonQuery();
                            Log.Info($"# of treatments from tbl_pb_ExtendedImportedTreatments having been marked as selected: {N}");
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

            Log.Info($"RecordProjectAndTreatmentSelections (ScenId={scenId}) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        public bool RunFullOptimizationCycle(string connectionString, ScenarioCommunique? communique, int scenId, string homeDirectory, bool doCleanup, out string? errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"RunFullOptimizationCycle (scenId={scenId} - started...");

            try
            {
                ConnectionString = connectionString;
                ScenId = scenId;
                HomeDirectory = homeDirectory;

                if (communique == null)
                {
                    communique = new ScenarioCommunique { MixAssetBudgets = false };
                }

                ok = RecordProjectAndTreatmentSelections(scenId, out errorMessage, true);   // Reset project abd treatment selections
                if (ok)
                {
                    ok = PopulateProjectListAndBudgetConstraints(connectionString, scenId, out errorMessage);
                }
                if (ok)
                {
                    ok = RunMixedOptimizationV2(communique, 2, out errorMessage);
                }
                if (ok)
                {
                    ok = RecordProjectAndTreatmentSelections(scenId, out errorMessage, false); // Set selection flags and years for projects and treatments
                }
                if (ok)
                {
                    ok = RecordSpentBudget(scenId, out errorMessage);
                }
                if (ok && doCleanup)
                {
                    ok = DeleteIntermediateFiles(out errorMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message; 
                ok = false; 
                Log.Error(errorMessage);
            }

            Log.Info($"RunFullOptimizationCycle (scenId={scenId} - finished.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        private bool RecordSpentBudget(int scenId, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"RecordSpentBudget (scenId={scenId}) - started...");

            List<string> sql = new List<string>();

            sql.Add($"DELETE FROM tbl_pb_ExtendedScenarioSpentBudgetWithAttributes WHERE ScenId={scenId};");

            foreach(string key in BudgetSpent.Keys)
            {
                double d = BudgetSpent[key];
                ok = ParseCostPrefix(key, out int? yr, out string? district, out char? isInterstate,
                    out char? assetType, out string? attributeName, out string? attributeValue,
                    out errorMessage);
                if (!ok)
                {
                    break;
                }
                sql.Add($@"INSERT INTO tbl_pb_ExtendedScenarioSpentBudgetWithAttributes
 (ScenId, YearWork, District, IsInterstate, AssetType,AttributeName,AttributeValue,FundsSpent)
VALUES ({scenId}, {yr},'{district}', '{isInterstate}', '{assetType}', '{attributeName}', '{attributeValue}', {d})");
            }

            if (ok)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        foreach (string stmt in sql)
                        {
                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = stmt;
                                cmd.CommandType = CommandType.Text;
                                int n = cmd.ExecuteNonQuery();
                                Log.DebugFormat($@"SQL:
{stmt}
Record count = {n}");
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    errorMessage = ex.Message;
                    Log.Error(errorMessage);
                    ok = false;
                }
            }
                
            Log.Info($"RecordSpentBudget (scenId={scenId}) - finished.  Ok={ok}, ErrorMessage: '{errorMessage}");
            return (ok);
        }

        private bool ParseCostPrefix(string costPrefix, out int? yr, out string? district, out char? isInterstate,
            out char? assetType, out string? attributeName,
            out string? attributeValue, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            yr = null; district = null; isInterstate = null;
            attributeName = null; attributeValue = null; assetType = null;

            try
            {
                int iY = costPrefix.IndexOf('Y');
                if (iY != 0)
                {
                    throw new Exception("Letter Y was not found in the beginning of the string.");
                }

                int iD = costPrefix.IndexOf('D');
                if (iD != iY+5)
                {
                    throw new Exception("Letter D was not found where it was expected");
                }

                int iI = costPrefix.IndexOf('I');
                if (iI < iD + 2)
                {
                    throw new Exception("Letter I was not found where it was expected");
                }

                int iT = costPrefix.IndexOf('T');
                if (iT != iI+2)
                {
                    throw new Exception("Letter T was not found where it was expected");
                }

                int iG = costPrefix.IndexOf('G');

                string s = costPrefix.Substring(1, 4);
                if (int.TryParse(s, out int r))
                {
                    yr = r;
                }
                else
                {
                    throw new Exception($"Valid calendar year could not be extracted from '{s}'");
                }

                district = costPrefix.Substring(iD + 1, iI - iD - 1);
                isInterstate = costPrefix[iI + 1];
                assetType = costPrefix[iT + 1];

                if (iG < 0)
                {
                    // Plain constraint code with no cargo-attribute suffix (e.g. "Y2026D3I1TP") —
                    // legacy's strict "G must be present" check would incorrectly throw on these;
                    // treat as "no attribute" instead, matching how PopulateBudgetConstraintsDictionary
                    // creates these entries (plain prefix.Cost, no attribute pair).
                }
                else if (iG != iT + 2)
                {
                    throw new Exception("Letter G was not found where it was expected");
                }
                else
                {
                    s = costPrefix.Substring(iG + 1, costPrefix.Length - iG - 1);
                    if (int.TryParse(s, out int g))
                    {
                        string pairCode = BudgetAttributePairsReversed[g];
                        if (pairCode[0] != '[' || pairCode[pairCode.Length - 1] != ']')
                        {
                            throw new Exception($"Pair code {pairCode} is not in expected format (missing square bracket(s))");
                        }
                        int iX = pairCode.IndexOf("].[");
                        if (iX < 0)
                        {
                            throw new Exception($"The '].[' character combination was not found within the pair code {pairCode}");
                        }
                        attributeName = pairCode.Substring(1, iX - 1);
                        attributeValue = pairCode.Substring(iX + 3, pairCode.Length - iX - 4);
                    }
                    else
                    {
                        throw new Exception($"Valid positive integer could not be extracted from '{s}'");
                    }
                }

            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
                Log.Error($"{errorMessage} when trying to parse '{costPrefix}'");
                ok = false;
            }

            return ok;
        }
    }
}
