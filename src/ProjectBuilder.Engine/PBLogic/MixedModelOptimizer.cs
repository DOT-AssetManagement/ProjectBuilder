using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PBLogic
{
    public partial class SymphonyConductor
    {

        /// <summary>
        /// Shortcut to the next method
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="scenId"></param>
        /// <param name="homeDirectory"></param>
        /// <param name="doCleanup"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool RunExtendedScenario(string connectionString, int scenId, string homeDirectory,
         bool doCleanup, out string? errorMessage)
        {
            return RunExtendedScenario(connectionString, scenId, null, homeDirectory, doCleanup, out errorMessage);
        }

        /// <summary>
        /// Performs treatment budnling int projects and runs optimization using Symphony
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="scenId">Scenario Id</param>
        /// <param name="communique">Optional ScenarioCommunique object, may be null</param>
        /// <param name="homeDirectory">Home directory of the application.  The LP directory must be under it.</param>
        /// <param name="doCleanup">When true the MPS, TXT and ERR files in the .\LP directory get erased</param>
        /// <param name="errorMessage">(out) error message, null if no errors</param>
        /// <returns>True on success, False on failure.</returns>
        public static bool RunExtendedScenario(string connectionString, int scenId, ScenarioCommunique? communique, string homeDirectory,
            bool doCleanup, out string? errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info($"RunMixedScenario ScenId={scenId},  doCleanup={doCleanup} - started...");

            Dictionary<string, double> ParmDict = new Dictionary<string, double>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Guard against concurrent/duplicate runs. Use an atomic UPDATE so two workers
                    // cannot both pass a separate SELECT-then-UPDATE lock check.
                    using (SqlCommand clearStale = new SqlCommand(
                        $@"UPDATE tbl_pb_Scenarios SET Locked=0
                           WHERE ScenID={scenId} AND ISNULL(Locked,0)=1
                             AND LastStarted IS NOT NULL
                             AND LastStarted < DATEADD(hour, -3, CURRENT_TIMESTAMP)
                             AND (Notes LIKE '%progress%' OR Notes LIKE '%In progress%')", conn))
                    {
                        clearStale.CommandTimeout = 6000;
                        clearStale.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = $"UPDATE tbl_pb_Scenarios SET CreatedBy=ISNULL(CreatedBy,USER), " +
                            $"CreatedAt=ISNULL(CreatedAt,CURRENT_TIMESTAMP), " +
                            $"LastRunBy=USER, LastRunAt=CURRENT_TIMESTAMP, " +
                            $"LastStarted=CURRENT_TIMESTAMP, Locked=1, " +
                            $"LastFinished=NULL, Notes='In progress...' " +
                            $"WHERE ScenID={scenId} AND ISNULL(Locked,0)=0";
                        command.CommandTimeout = 6000;
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            using (SqlCommand existsCheck = new SqlCommand(
                                $"SELECT COUNT(*) FROM tbl_pb_Scenarios WHERE ScenID={scenId}", conn))
                            {
                                int exists = Convert.ToInt32(existsCheck.ExecuteScalar());
                                if (exists == 0)
                                {
                                    throw new Exception($"Scenario {scenId} not found in the database.");
                                }
                            }

                            throw new Exception($"Scenario {scenId} is currently locked by another run. Please try again later.");
                        }
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
                if (communique == null)
                {
                    communique = new ScenarioCommunique
                    {
                        Commitment = false,
                        District = null,
                        ProjectsOnly = false,
                        SingleTreatmentsOnly = false,
                        MaxPriority = 10,
                        MixAssetBudgets = false
                    };
                }

                using (SymphonyConductor conductor = new SymphonyConductor())
                {
                    ok = conductor.RunFullOptimizationCycle(connectionString, communique, scenId, homeDirectory, doCleanup, out errorMessage);
                }
            }

            try
            {
                if (ok)
                {
                    // A run can complete every ADO.NET step without throwing yet still leave the scenario's
                    // output table empty (e.g. a concurrent/duplicate run wiping it out mid-flight). Verify
                    // there's actually something to show before reporting Success.
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (SqlCommand countCommand = new SqlCommand(
                            $"SELECT COUNT(*) FROM tbl_pb_ExtendedTimewiseConstrainedProjects WHERE ScenId={scenId}", conn))
                        {
                            int projectCount = Convert.ToInt32(countCommand.ExecuteScalar());
                            if (projectCount == 0)
                            {
                                ok = false;
                                errorMessage = $"Scenario {scenId} finished running but produced no projects. " +
                                    "This can happen if another run for the same scenario started at the same time — please re-run the scenario.";
                                Log.Error(errorMessage);
                            }
                        }
                    }
                }

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
