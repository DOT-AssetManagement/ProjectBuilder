using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PBLogic
{
    public partial class SymphonyConductor
    {
        public bool RunMixedOptimization(ScenarioCommunique filter, int NumIterations, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            string mpsFilePath = null;
            string resultFilePath = null;
            string errorFilePath = null;

            Log.Info($"RunMixedOptimization NumIterations={NumIterations}  - started...");

            try
            {
                for (int I = 0; ok && I < NumIterations; I++)
                {
                    Log.Info($"RunMixedOptimization, Pass={I + 1}");

                    if (ok)
                    {
                        ok = GenerateMpsFileForMultiYearProjects(filter, (I == 1), out mpsFilePath, out errorMessage, out bool degenerateCase);

                        if (ok && !degenerateCase)
                        {
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
                                ok = ProcessLPSolution(true, I + 1,  out errorMessage);
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

            Log.Info($"RunMixedOptimization - ended.  Ok={ok}, ErrorMessage: '{errorMessage}'");

            return (ok);
        }

        public bool SetSprojectAndTreatmentSelections(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"SetSprojectAndTreatmentSelections - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        conn.InfoMessage += Conn_InfoMessage;
                        command.CommandText = "dbo.sp_pb_SetExtendedProjectSelections";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 2400;
                        command.Parameters.Add("@ScenId", SqlDbType.Int).Value = ScenId;
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

            Log.Info($"SetSprojectAndTreatmentSelections - ended. Ok={ok}, ErrorMessage: '{errorMessage}'");

            return ok;
        }

        private void Conn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
              Log.Info("SQL Messages:\n" + e.Message);
        }

        public bool OptimizeAndSetSelections(ScenarioCommunique filter, int NumIterations, bool doCleanup, out string errorMessage)
        {
            errorMessage = null;

            bool ok = RunMixedOptimization(filter, NumIterations,  out errorMessage);
            if (ok)
            {
                ok = SetSprojectAndTreatmentSelections(out errorMessage);
            }

            return (ok);
        }



        public static bool RunExtendedScenario(string connectionString, string homeDirectory, int scenId, ScenarioCommunique filter, int NumIterations, bool doCleanup, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"RunExtendedScenario ScenId={scenId}, filter={filter},  NumIterations={NumIterations}, doCleanup={doCleanup} - started...");

            try
            {
                SymphonyConductor conductor = new SymphonyConductor
                {
                    ConnectionString = connectionString,
                    HomeDirectory = homeDirectory,
                    ScenId = scenId
                };

                conductor.LogParameters();


                ok = conductor.CreateAlternativesTable(out errorMessage);

                
                if (ok)
                {
                    ok = conductor.RunMixedOptimization(filter, NumIterations, out errorMessage);
                }

                if (ok)
                {
                    ok = conductor.SetSprojectAndTreatmentSelections(out errorMessage);
                }

                if (ok && doCleanup)
                {
                    ok = conductor.DeleteIntermediateFiles(out errorMessage);
                }

                if (ok && doCleanup)
                {
                    ok = conductor.DeleteIntermediateRecords(out errorMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
            }

            Log.Info($"RunExtendedScenario - ended. Ok={ok}, ErrorMessage='{errorMessage}'");


            return (ok);
        }

        public static bool RunMixedScenarioFull(string connectionString, string homeDirectory, int scenId, ScenarioCommunique filter, int NumIterations, bool doCleanup, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"RunMixedScenarioFull ScenId={scenId}, filter={filter},  NumIterations={NumIterations}, doCleanup={doCleanup} - started...");

            try
            {
                SymphonyConductor conductor = new SymphonyConductor
                {
                    ConnectionString = connectionString,
                    HomeDirectory = homeDirectory,
                    ScenId = scenId,

                };

                conductor.LogParameters();


                ok = conductor.CreateAlternativesTable(out errorMessage);

                if (ok)
                {
                    ok = conductor.RunMixedOptimization(filter, NumIterations, out errorMessage);
                }

                if (ok)
                {
                    ok = conductor.SetSprojectAndTreatmentSelections(out errorMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
            }

            Log.Info($"RunMixedScenarioFull - ended. Ok={ok}, ErrorMessage='{errorMessage}'");


            return (ok);
        }

     
        public static bool RunMixedScenarioStepByStep(string connectionString, string homeDirectory, int scenId, ScenarioCommunique filter, int NumIterations, bool doCleanup, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"RunMixedScenarioStepByStep ScenId={scenId}, filter={filter},  NumIterations={NumIterations}, doCleanup={doCleanup} - started...");

            try
            {
                SymphonyConductor conductor = new SymphonyConductor
                {
                    ConnectionString = connectionString,
                    HomeDirectory = homeDirectory,
                    ScenId = scenId,
                    Code = "B",
                };

                conductor.LogParameters();

                ok = conductor.CreateAlternativesTable(out errorMessage);

                if (ok)
                {
                    filter.Commitment = true;
                    filter.MaxPriority = 10;
                    Log.Info($"Step filter: {filter}");
                    ok = conductor.RunMixedOptimization(filter, NumIterations,  out errorMessage);
                }

                for (int j = 0; ok && j < 2; j++)
                {
                    if (j == 0)
                    {
                        filter.ProjectsOnly = true;
                        filter.SingleTreatmentsOnly = false;
                    }
                    else
                    {
                        filter.ProjectsOnly = false;
                        filter.SingleTreatmentsOnly = true;
                    }

                    for (int i = 0; ok && i < 5; i++)
                    {
                        filter.Commitment = false;
                        filter.MaxPriority = i;
                        Log.Info($"Step filter: {filter}");
                        ok = conductor.RunMixedOptimization(filter, NumIterations,  out errorMessage);
                    }
                }

                if (ok)
                {
                    ok = conductor.SetSprojectAndTreatmentSelections(out errorMessage);
                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
            }

            Log.Info($"RunMixedScenarioStepByStep - ended. Ok={ok}, ErrorMessage='{errorMessage}'");


            return (ok);
        }

        public static bool DeleteAlternatives(string connectionString, int scenId, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            int N;

            Log.Info("DeleteAlternatives - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand(
                      $"DELETE FROM tbl_pb_ALternatives WHERE ScenID={scenId}", conn))
                    {
                        command.CommandTimeout = 60000;
                        N = command.ExecuteNonQuery();
                        Log.Info($"{N} records deleted from tbl_pb_ALternatives");
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"DeleteAlternatives - ended.  Ok={ok}\tError message: {errorMessage}");

            return (ok);
        }


        public static bool RunExtendedScenario(string connectionString, string homeDirectory, int scenId, bool doCleanup, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"RunMixedScenario ScenId={scenId},  doCleanup={doCleanup} - started...");

            ScenarioCommunique filter = new ScenarioCommunique();
            Dictionary<string, double> ParmDict = new Dictionary<string, double>();
         

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = $"UPDATE tbl_pb_Scenarios SET CreatedBy=ISNULL(CreatedBy,USER), " +
                            $"CreatedAt=ISNULL(CreatedAt,CURRENT_TIMESTAMP), " +
                            $"LastRunBy=USER, LastRunAt=CURRENT_TIMESTAMP, " +
                            $"LastStarted=CURRENT_TIMESTAMP, Locked=1, " +
                            $"LastFinished=NULL, Notes='In progress...' " +
                            $"WHERE ScenID={scenId}";
                        command.CommandTimeout = 6000;
                        command.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = $@"SELECT ParmId, COALESCE(ParmValue, -1.0) 
 FROM vw_pb_ScenarioParameters WITH (NOLOCK) 
 WHERE ScenId={scenId} ORDER By ParmId ";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ParmDict.Add(reader[0].ToString(), Convert.ToDouble(reader[1]));
                            }
                            reader.Close();
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

            if (ok)
            {
                foreach (string key in ParmDict.Keys)
                {
                    Log.Info($"{key} = {ParmDict[key]}");
                }

                if (ParmDict["YFST"] < 0 || ParmDict["YLST"] < 0)
                {
                    ok = false;
                    errorMessage = $"The planning horizon timeframe for scenario {scenId} is not specified.";
                }
            }

            if (ok)
            {
                ok = DataManager.PopulateScenarioTreatmentsTable(connectionString, scenId, out errorMessage);
            }

            if (ok)
            {
                ok = DataManager.CreateExtendedScenarioProjects(connectionString, scenId, out errorMessage);
            }

            if (ok)
            {
                ok = SymphonyConductor.RunMixedScenarioFull(connectionString, homeDirectory, scenId,
                    new ScenarioCommunique
                    {
                        Commitment = false,
                        District = null,
                        ProjectsOnly = false,
                        SingleTreatmentsOnly = false,
                        MaxPriority = 10,
                        MixAssetBudgets = false
                    }
                    , 2, doCleanup, out errorMessage);
            }


            try
            {
                string Notes = ok ? "Success" : "Failed, please see the log file for more detail.";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = $"UPDATE tbl_pb_Scenarios SET Locked=0, " +
                            $"LastFinished=CURRENT_TIMESTAMP, Notes='{Notes}' " +
                            $"WHERE ScenID={scenId}";
                        command.CommandTimeout = 6000;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message + " when trying to unlock the scenario";
                ok = false;
                Log.Error(errorMessage);
            }


            Log.Info($"RunMixedScenario - ended. Ok={ok}, ErrorMessage='{errorMessage}'");

            return ok;
        }
    }
}
