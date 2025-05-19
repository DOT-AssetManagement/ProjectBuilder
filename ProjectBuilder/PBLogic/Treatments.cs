using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace PBLogic
{
    public partial class  SymphonyConductor
    {
        /// <summary>
        /// Populates tbl_pb_LPAlternatives table
        /// </summary>
        /// <param name="District">Number of the district, for whichto create the alternatives table, 0 if for all districts</param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on success, False on falure</returns>
        public bool CreateTreatmentsAlternativesTable(int District, out string errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            Log.Info($"CreateTreatmentAlternativesTable (ScenID={ScenId}, Code='{Code}, District={District}' - Started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand("dbo.sp_pb_CreateTreatmentAlternativesTable", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 240000;
                        command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
                        command.Parameters.Add("@Code", SqlDbType.VarChar, 4).Value = Code;
                        command.Parameters.Add("@District", SqlDbType.Int).Value = District;
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

            Log.Info($"CreateTreatmentAlternativesTable (ScenID={ScenId}, Code='{Code}, District={District}' ended.  Error message: {errorMessage}");
            return ok;
        }

#if _TREATMENTS
        /// <summary>
        /// Prepares LP formulation, runs Symphony and interpretes results.
        /// </summary>
        /// <param name="doBinary">If true runs applies binary constraints to decision variables.</param>
        /// <param name="doBinaryThen">If true ProcessLPSolution is still called with doBinary=true even if the doBinary argument here is false</param>
        /// <param name="doCleanup">If true erases intermediate files and database records</param>
        /// <param name="bIgnoreBPConstraints">If true ignores separate bridge and pavement constraints and operates only with totals</param>
        /// <param name="errorMessage">WError message, null if no errors</param>
        /// <returns>True on scuccess, False on failure</returns>
        private bool PrepareAndSolveLPForTreatments(int iterationNo, int district, bool doBinary, bool doBinaryThen, bool doCleanup, out string errorMessage,
            out bool DegenerateCase)
        {
            string mpsFilePath = null, resultFilePath = null, errorFilePath = null;
            errorMessage = null;
            DegenerateCase = false;

            bool ok = true;



            if (iterationNo == 0 && !doBinary)
            {
                ok = CreateAlternativesMatrix(true, district, out errorMessage);
            }

            if (ok)
            {
                ok = GenerateMpsFileForTreatmentsDistrictIteration(iterationNo, district, doBinary, out mpsFilePath, out errorMessage, out DegenerateCase);
            }

            if (ok && !DegenerateCase)
            {
                ok = RunSymphony(mpsFilePath, out resultFilePath, out errorFilePath, out errorMessage);
            }

            if (ok && !DegenerateCase)
            {
                ok = ImportOptimizationResults(resultFilePath, null, out errorMessage);
            }

            if (ok && !DegenerateCase)
            {
                ok = ProcessLPSolution(doBinary || doBinaryThen, 1, true, out errorMessage);
            }
            else if (!DegenerateCase)
            {
                if (doBinary && errorMessage.ToUpper().Contains("INFEASIBLE"))
                {
                    Log.Info("Recovering");
                    Log.Info("Seting selection to year -1 for all 'fractional' treatments");
                    ok = true;
                    errorMessage = null;
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(ConnectionString))
                        {
                            conn.Open();
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = conn;
                                command.CommandText = $@"UPDATE tbl_pb_TreatmentAlternatives SET Selected=1
WHERE ScenID={ScenId} AND Code='{Code}' AND District={district} AND YearWork < 0
 AND TreatmentNo NOT IN (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
    WHERE ScenID={ScenId} AND Code='{Code}' AND District={district} AND Selected=1)";
                                command.CommandTimeout = 600;
                                Log.Debug(command.CommandText);
                                int N = command.ExecuteNonQuery();
                                Log.Info($"{N} treatments processed");
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        errorMessage = ex.Message;
                        ok = false;
                        Log.Error(errorMessage);
                    }
                }
            }

            if (ok && doCleanup)
            {
                ok = DeleteIntermediateFiles(out errorMessage);
            }

            if (ok && doCleanup)
            {
                ok = DeleteIntermediateRecords(true, out errorMessage);
            }

            return (ok);
        }

        public bool GenerateMpsFileForTreatmentsDistrictIteration(int Iteration, int District, 
            bool AddBinaryBounds, out string mpsFilePath, 
            out string errorMessage, out bool degenerateCase)
        {
            bool ok = true;
            errorMessage = null;
            mpsFilePath = null;
            string sql;
            double dMillion = 1000000;
            degenerateCase = false;

            Log.Info("GenerateMpsFile - Treatements - Iteration - started");
            LogParameters();

            mpsFilePath = Path.Combine(HomeDirectory, $"LP\\TSC_{ScenId}_{Code}_{District}_{Iteration}_{AddBinaryBounds}.mps");
            Log.Info($"Generating file {mpsFilePath}");

            try
            {
                using (StreamWriter sw = File.CreateText(mpsFilePath))
                {
                    sw.WriteLine($"NAME LP\\TrSC_{ScenId}_{Code}_{District}_{Iteration}");
                    Log.InfoFormat("MPS file path:\t{0}", mpsFilePath);

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();

                        sw.WriteLine("ROWS");
                        sw.WriteLine(" N UTILITY");
                        /* Budget constraints */


                        sql = $@"SELECT ConstrName, CGroup 
FROM vw_pb_RemainingTreatmentDistrictBudgetConstraints v WITH(NOLOCK)
WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} ORDER BY YearWork, CGroup";

                        Log.Debug(sql);

                        int N = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            command.CommandTimeout = 600;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" L {reader["ConstrName"]}");
                                    N++;
                                }
                                reader.Close();
                            }
                        }

                        Log.Info($"ROWS - Constraints - written: {N}");
                        /* Treatments */

                        if (Iteration <= 0)
                        {
                            sql = $@"SELECT DISTINCT TreatmentNo 
FROM tbl_pb_TreatmentAlternatives a WITH (NOLOCK) 
WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} 
AND TreatmentNo NOT IN 
    (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
ORDER BY TreatmentNo";
                        }
                        else
                        {
                            sql = $@"SELECT DISTINCT TreatmentNo 
FROM tbl_pb_TreatmentPreviousSelections a WITH (NOLOCK)
WHERE IterationNo={Iteration - 1} AND ScenID={ScenId} AND Code='{Code}' AND District={District}
AND TreatmentNo NOT IN 
    (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
ORDER BY TreatmentNo";
                        }


                        Log.Debug(sql);
                        N = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            command.CommandTimeout = 600;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine(" E T{0}", reader[0]);
                                    N++;
                                }
                                reader.Close();
                            }
                        }

                        Log.Info($"ROWS - Treatments - written: {N}");
                        if (N == 0)
                        {
                            degenerateCase = true;
                            throw new Exception("No treatment alternatives left for selection.");
                        }

                        sw.WriteLine("COLUMNS");

                        if (Iteration <= 0)
                        {
                            sql = $@"SELECT AltNo, ProjectNo AS TretamentNo, ConstrName, ISNULL(Cost,0) AS Cost, 
Utility, CGroup
FROM tbl_pb_AlternativeMatrix WITH(NOLOCK) 
WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND CGroup IN (1,2)
AND  ProjectNo NOT IN (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
UNION ALL
SELECT AltNo, TreatmentNo, NULL, 0.0 AS Cost, 0 AS Utility, 0 AS CGroup
FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK) 
WHERE ScenID={ScenId} AND Code='{Code}' AND YearWork<0 AND District={District}
AND TreatmentNo NOT IN 
    (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND Selected=1 AND District={District})
ORDER BY 2, 1, 3";
                        }
                        else
                        {
                            sql = $@"SELECT AltNo, ProjectNo AS TretamentNo, ConstrName, 
ISNULL(Cost,0) AS Cost, CONVERT(FLOAT,p.Rnk) AS Utility, CGroup
FROM tbl_pb_AlternativeMatrix x WITH(NOLOCK) 
INNER JOIN tbl_pb_TreatmentClusteringAnalysis p WITH(NOLOCK)
    ON p.IterationNo={Iteration} 
    AND p.ScenID = x.ScenID AND p.Code=x.Code AND p.District=x.District 
    AND p.TreatmentNo=x.ProjectNo 
    AND x.YearWork=p.AltYearWork
WHERE x.ScenID={ScenId} AND x.Code='{Code}' AND x.District={District} 
  AND x.CGroup IN (1,2)
  AND p.TreatmentNo NOT IN 
    (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
UNION ALL
SELECT AltNo, a.TreatmentNo, NULL, 0.0 AS Cost, 0.0 AS Utility, 0 AS CGroup
FROM tbl_pb_TreatmentAlternatives a WITH (NOLOCK) 
INNER JOIN tbl_pb_TreatmentPreviousSelections p WITH(NOLOCK)
    ON p.IterationNo={Iteration - 1} 
    AND p.ScenID = a.ScenID AND p.Code=a.Code AND p.District=a.District
    AND p.TreatmentNo=a.TreatmentNo
WHERE a.ScenID={ScenId} AND a.Code='{Code}' AND a.YearWork<0 
  AND a.District={District} 
  AND a.TreatmentNo NOT IN 
    (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
ORDER BY 2, 1, 3";
                        }


                        Log.Debug(sql);
                        N = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            command.CommandTimeout = 600;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                long prevAltNo = 0;
                                while (reader.Read())
                                {
                                    long altNo = reader.GetInt64(0);
                                    int entityNo = reader.GetInt32(1);
                                    double util = reader.GetDouble(4);
                                    int cGroup = reader.GetInt32(5);
                                    if (prevAltNo < altNo)
                                    {
                                        sw.WriteLine($" {altNo} T{entityNo} 1 UTILITY {-util}");
                                        prevAltNo = altNo;
                                        N++;
                                    }
                                    if (altNo % 100 > 0)
                                    {
                                        double cost = Convert.ToDouble(reader[3]);


                                        if (cost >= 0.01)
                                        {
                                            sw.WriteLine($" {altNo} {reader[2]} {cost / dMillion}");
                                            N++;
                                        }
                                    }
                                }
                                reader.Close();
                            }
                        }

                        Log.Info($"COLUMNS - written: {N}");

                        sw.WriteLine("RHS");


                        sql = $@"SELECT ConstrName, ConstrValue, cGroup 
FROM vw_pb_RemainingTreatmentDistrictBudgetConstraints v WITH(NOLOCK)
WHERE ScenID={ScenId} AND Code='{Code}' AND District={District}
ORDER BY YearWork, ConstrName";


                        Log.Debug(sql);
                        N = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            command.CommandTimeout = 600;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {

                                    int cGroup = Convert.ToInt32(reader[2]);

                                    double cost = Convert.ToDouble(reader[1]);
                                    if (cost >= 0.1)
                                    {
                                        sw.WriteLine($" RHSLE {reader[0]} {cost / dMillion}");
                                        N++;
                                    }
                                }
                                reader.Close();
                            }
                        }

                        Log.Info($"RHS - Constraints - written: {N}");
                        if (Iteration <= 0)
                        {
                            sql = $@"SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives a WITH (NOLOCK) 
WHERE ScenID={ScenId} AND Code='{Code}' AND District={District}
AND TreatmentNo NOT IN 
    (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
ORDER BY TreatmentNo";
                        }
                        else
                        {
                            sql = $@"SELECT DISTINCT TreatmentNo 
FROM tbl_pb_TreatmentPreviousSelections a WITH (NOLOCK)
WHERE IterationNo={Iteration - 1} AND ScenID={ScenId} AND Code='{Code}' 
    AND District={District}
    AND TreatmentNo NOT IN 
        (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
     WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
";
                        }
                        Log.Debug(sql);
                        N = 0;

                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            command.CommandTimeout = 600;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine($" RHSLE T{reader[0]} 1");
                                    N++;
                                }
                                reader.Close();
                            }
                        }
                        Log.Info($"RHS - Treatments - written: {N}");

                        if (AddBinaryBounds)
                        {
                            sw.WriteLine("BOUNDS");

                            if (Iteration <= 0)
                            {
                                sql = $@"SELECT DISTINCT AltNo 
FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK) WHERE ScenID={ScenId} AND Code='{Code}' AND District={District}
AND TreatmentNo NOT IN 
    (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
ORDER BY AltNo";
                            }
                            else
                            {
                                sql = $@"SELECT DISTINCT AltNo 
FROM tbl_pb_TreatmentAlternatives a WITH (NOLOCK) 
INNER JOIN tbl_pb_TreatmentClusteringAnalysis p WITH (NOLOCK) 
    ON p.IterationNo={Iteration} AND p.ScenID=a.ScenID AND p.Code=a.Code AND p.District=a.District
    AND p.TreatmentNo = a.TreatmentNo AND (p.AltYearWork = a.YearWork OR a.YearWork<0)
WHERE a.ScenID={ScenId} AND a.Code='{Code}' AND a.District={District}
AND a.TreatmentNo NOT IN 
    (SELECT DISTINCT TreatmentNo FROM tbl_pb_TreatmentAlternatives WITH (NOLOCK)
        WHERE ScenID={ScenId} AND Code='{Code}' AND District={District} AND Selected=1)
ORDER BY AltNo
";
                            }


                            Log.Debug(sql);
                            N = 0;

                            using (SqlCommand command = new SqlCommand(sql, conn))
                            {
                                command.CommandTimeout = 600;

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        sw.WriteLine($" UI BOUND {reader[0]} 1");
                                        N++;
                                    }
                                    reader.Close();
                                }
                            }

                            Log.Info($"BOUNDS - written: {N}");
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

            Log.InfoFormat("GenerateMpsFile - ended\tOK={0};\tError/Warning message: {1}", ok, errorMessage);
            return ok;
        }

        /// <summary>
        /// Runs scenario/code/dustrict combination for treatments
        /// expects table tbl_TreatementAlternatives to be populated by that time
        /// </summary>
        /// <param name="reset">If true causes resetting all previous selections to zero</param>
        /// <param name="district">District number for which to run the optimization</param>
        /// <param name="errorMessage">WError message, null if no errors</param>
        /// <returns>True on scuccess, False on failure</returns>
        public bool RunTreatmentOptimization(int iterationNo, bool reset, int district, out string errorMessage, out bool degenerateCase, ref int minClusterSize)
        {
            errorMessage = null;
            bool ok = true;
            degenerateCase = false;

            Log.Info($"Run treatment optimization, Iteration={iterationNo}, Reset={reset}, District={district} - started...");

            if (ok && (reset || iterationNo==0))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = $@"UPDATE tbl_pb_TreatmentAlternatives SET Selected=0 
 WHERE ScenID={ScenId} AND Code='{Code}' AND District={district} AND Selected<>0";
                            int N = command.ExecuteNonQuery();
                            Log.Info($"Treatments selection reset: {N}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Log.Error(errorMessage);
                    ok = false;
                }
            }


            if (ok && iterationNo > 0)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = $@"UPDATE tbl_pb_TreatmentAlternatives SET Selected=0 
 WHERE ScenID={ScenId} AND Code='{Code}' AND District={district} AND Selected<>0
  AND TreatmentNo IN (SELECT TreatmentNo FROM tbl_pb_TreatmentPreviousSelections WITH (NOLOCK)
                      WHERE IterationNo={iterationNo-1} AND ScenID = {ScenId} AND Code='{Code}' 
                            AND District={district})";
                            int N = command.ExecuteNonQuery();
                            Log.Info($"Treatment selections reset from previous iteration: {N}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Log.Error(errorMessage);
                    ok = false;
                }
            }


            if (ok) // Fist solve the relaxed model, then the strict one.
            {
                ok = PrepareAndSolveLPForTreatments(iterationNo, district, false, true, true, out errorMessage, out degenerateCase);
                if (ok && degenerateCase)
                {
                    Log.Warn($@"Degenerate case achieved at iteration={iterationNo}, with NO binary bounds.
Iteration loop interrupted at this point.");
                }
            }

       

            if (ok && !degenerateCase)
            {
                ok = PrepareAndSolveLPForTreatments(iterationNo, district, true, true, true, out errorMessage, out degenerateCase);
                if (ok && degenerateCase)
                {
                    Log.Warn($@"Degenerate case achieved at iteration={iterationNo}, WITH binary bounds.
It is okay to continue iterations loop.");
                    degenerateCase = false;
                }
            }

            if (ok)
            {
                ok = AnalyzeTreatmentClustering(iterationNo + 1, district, out errorMessage, ref minClusterSize);
            }

            Log.Info($"Run treatment optimization, Iteration={iterationNo}, Reset={reset}, District={district} - ended. Ok={ok}, ErrorMessage: {errorMessage}");

            return (ok);
        }

#endif

        private bool AnalyzeTreatmentClustering(int iterationNo, int district, out string errorMessage, ref int minClusterSizeApplied)
        {
            bool ok = true;
            errorMessage = null;
            
            Log.Info($"AnalyzeTreatmentClustering Iteration={iterationNo}, District={district} - started...");
            try
            {
                using(SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using(SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.sp_pb_AnalyzeTreatmentClustering";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@Iter", SqlDbType.Int).Value = iterationNo;
                        command.Parameters.Add("@ScenID", SqlDbType.Int).Value = ScenId;
                        command.Parameters.Add("@Code", SqlDbType.NVarChar, 4).Value = Code;
                        command.Parameters.Add("@District", SqlDbType.Int).Value = district;
                        command.Parameters.Add("@MinImposed", SqlDbType.Int).Value = minClusterSizeApplied;

                        SqlParameter parmMinUsed = new SqlParameter
                        {
                            ParameterName = "@MinUsed",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(parmMinUsed);

                        command.CommandTimeout = 600;
                        command.ExecuteNonQuery();

                        minClusterSizeApplied = Convert.ToInt32(command.Parameters["@MinUsed"].Value);
                        Log.Info($"minClusterSizeApplied={minClusterSizeApplied}");
                    }
                }
             }
            catch(Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }
            Log.Info($"AnalyzeTreatmentClustering Iteration={iterationNo}, District={district} - ended. OK={ok}, ErrorMessage='{errorMessage}'");

            return (ok);
        }

    }

}