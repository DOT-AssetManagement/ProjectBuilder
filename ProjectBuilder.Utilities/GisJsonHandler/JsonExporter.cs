using log4net;
using System;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace GisJsonHandler
{
    public class JsonExporter
    {
        private static ILog _log;

        public static ILog Log
        {
            get { return _log; }
            set { _log = value; }
        }

        private static readonly string sqlSelScenarioFmt = @"SELECT s.ScenId, s.ScenarioName, ul.[Name] AS LibraryName, ul.Id as LibraryId, LastRunBy, LastRunAt, Notes
FROM tbl_pb_Scenarios s WITH(NOLOCK)
LEFT OUTER JOIN tbl_lib_UserLibraries ul WITH(NOLOCK)
ON ul.Id = s.LibraryId
WHERE ScenId = {0}";

       /// <summary>
       /// Gnerates JSON string of projects and treatments of selected scenario for GIS
       /// </summary>
       /// <param name="dbConnectionString">Database connection string.</param>
       /// <param name="scenarioId">Scenario ID</param>
       /// <param name="indent">True if JSON string is to be delivered in indetnetd format, false otherwise.</param>
       /// <param name="jsonString">(out) generated JSON string.</param>
       /// <param name="errorMessage">(out) error message, null if no errors</param>
       /// <param name="district">Optional district number.</param>
       /// <param name="cnty">Optional county code.</param>
       /// <param name="route">Optional route number.</param>
       /// <returns>True on success, false on failure.</returns>
        public static bool ExportScenarioResultsToJson(string dbConnectionString, int scenarioId, bool indent, out string jsonString, out string errorMessage, 
                int? district=null, int? cnty=null, int? route=null,
                string section=null, string appliedTreatment=null, 
                int? selectedYear=null, string treatmentType=null)
        {
            bool ok = true;
            jsonString = null;
            errorMessage = null;

            Log.Info($"ExportScenarioResultsToJson ({scenarioId},{indent}) - started...");

            try
            {
                GisJsonHandler.GisOutput gisOutput = new GisOutput();
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = string.Format(sqlSelScenarioFmt, scenarioId);
                        command.CommandType = CommandType.Text;
                       
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                reader.Close();
                                throw new Exception($"Scenario {scenarioId} not found.");
                            }

                            if (reader.Read())
                            {
                                gisOutput.ScenHeader.ScenId = Convert.ToInt32(reader["ScenId"]);
                                gisOutput.ScenHeader.Name = reader["ScenarioName"] == DBNull.Value ? null : reader["ScenarioName"].ToString();
                                gisOutput.ScenHeader.LibraryName = reader["LibraryName"] == DBNull.Value ? null : reader["LibraryName"].ToString();
                                gisOutput.ScenHeader.LibraryId = reader["LibraryId"] == DBNull.Value ? Guid.Empty : (Guid)reader["LibraryId"];
                                gisOutput.ScenHeader.LastRunBy = reader["LastRunBy"] == DBNull.Value ? null : reader["LastRunBy"].ToString();
                                gisOutput.ScenHeader.LastRunTime = reader["LastRunAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastRunAt"]);
                                gisOutput.ScenHeader.Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString();
                            }

                            reader.Close();
                        }
                    }

                    int nProj = 0;

                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = "sp_pb_SelectProjectsForGIS";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 12000;
                        command.Parameters.Add("@ScenId", SqlDbType.Int).Value = scenarioId;
                        if (district.HasValue)
                        {
                            command.Parameters.Add("@District", SqlDbType.TinyInt).Value = district;
                        }
                        if (cnty.HasValue)
                        {
                            command.Parameters.Add("@Cnty", SqlDbType.TinyInt).Value = cnty;
                        }
                        if (route.HasValue)
                        {
                            command.Parameters.Add("@Route", SqlDbType.Int).Value = route;
                        }
                        if (section != null)
                        {
                            command.Parameters.Add("@Section", SqlDbType.VarChar, 20).Value = section;
                        }
                        if (selectedYear.HasValue)
                        {
                            command.Parameters.Add("@Year", SqlDbType.Int).Value = selectedYear;
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                reader.Close();
                                throw new Exception($"No selected projects found for scenario {scenarioId}");
                            }

                            while (reader.Read())
                            {
                                GisJsonHandler.Project project = new Project();
                                project.UserId = reader["UserId"] == DBNull.Value ? null: (string)reader["UserId"];
                                project.UserNotes = reader["UserNotes"] == DBNull.Value ? null :  (string)reader["UserNotes"];
                                project.ProjectId = Convert.ToInt32(reader["ProjectId"]);
                                project.SchemaId = Convert.ToInt64(reader["SchemaId"]);
                                project.ProjectType = Convert.ToInt16(reader["ProjectType"]);
                                project.Year = reader["Year"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Year"]);
                                project.NumBridges = reader["NumBridges"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NumBridges"]);
                                project.NumPaveSections = reader["NumPaves"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NumPaves"]);
                                project.TotalCost = reader["TotalCost"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TotalCost"]);
                                gisOutput.Projects.Add(project);
                                nProj++;
                            }

                            Log.Info($"Number of loaded projects: {nProj}");

                            reader.Close();
                        }
                    }

                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = "sp_pb_SelectTreatmentsForGIS";
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 12000;
                        command.Parameters.Add("@ScenId", SqlDbType.Int).Value = scenarioId;
                        if (district.HasValue)
                        {
                            command.Parameters.Add("@District", SqlDbType.TinyInt).Value = district;
                        }
                        if (cnty.HasValue)
                        {
                            command.Parameters.Add("@Cnty", SqlDbType.TinyInt).Value = cnty;
                        }
                        if (route.HasValue)
                        {
                            command.Parameters.Add("@Route", SqlDbType.Int).Value = route;
                        }
                        if (section != null)
                        {
                            command.Parameters.Add("@Section", SqlDbType.VarChar, 20).Value = section;
                        }
                        if (appliedTreatment != null)
                        {
                            command.Parameters.Add("@AppliedTreatment", SqlDbType.VarChar, 100).Value = appliedTreatment;
                        }
                        if (selectedYear.HasValue)
                        {
                            command.Parameters.Add("@Year", SqlDbType.Int).Value = selectedYear;
                        }
                        if (treatmentType != null)
                        {
                            command.Parameters.Add("@TreatmentType", SqlDbType.Char, 1).Value = treatmentType[0];
                        }

                        int nTreat = 0;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                reader.Close();
                                throw new Exception($"No selected treatments found for scenario {scenarioId}");
                            }

                            while (reader.Read())
                            {
                                
                                GisJsonHandler.Treatment treatment = new Treatment();
                                treatment.ProjectId = Convert.ToInt32(reader["ProjectId"]);
                                treatment.ProjectType = Convert.ToInt16(reader["ProjectType"]);
                                treatment.TreatmentId = Convert.ToInt64(reader["TreatmentId"]);
                                treatment.ImportTimeGeneratedId = reader["ImportTimeGeneratedId"] is DBNull ? Guid.Empty : (Guid)reader["ImportTimeGeneratedId"];

                                treatment.AppliedTreatment = reader["AppliedTreatment"].ToString();
                                treatment.TreatmentType = reader["TreatmentType"].ToString();
                                treatment.District = Convert.ToInt16(reader["District"]);
                                treatment.Cnty = Convert.ToInt16(reader["Cnty"]);
                                treatment.Route = Convert.ToInt32(reader["Route"]);
                                treatment.Direction = Convert.ToInt16(reader["Direction"]);
                                treatment.FromSection = Convert.ToInt32(reader["FromSection"]);
                                treatment.ToSection = Convert.ToInt32(reader["ToSection"]);
                                treatment.BRKEY = reader["BRKEY"] == DBNull.Value ? null : reader["BRKEY"].ToString();
                                treatment.BRIDGE_ID = reader["BRIDGE_ID"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["BRIDGE_ID"]);
                                treatment.OwnerCode = reader["OWNER_CODE"] == DBNull.Value ? null : reader["OWNER_CODE"].ToString();
                                treatment.County = reader["County"] == DBNull.Value ? null : reader["County"].ToString();
                                treatment.Year = reader["Year"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Year"]);
                                treatment.Cost = reader["Cost"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["Cost"]);
                                treatment.Benefit = reader["Benefit"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["Benefit"]);


                                treatment.PreferredYear = reader["PreferredYear"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PreferredYear"]);
                                treatment.MinYear = reader["MinYear"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MinYear"]);
                                treatment.MaxYear = reader["MaxYear"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MaxYear"]);
                                treatment.PriorityOrder = reader["PriorityOrder"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PriorityOrder"]);
                                treatment.IsCommitted = Convert.ToBoolean(reader["IsCommitted"]);
                                treatment.Risk = reader["Risk"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["Risk"]);
                                treatment.IndirectCostDesign = reader["IndirectCostDesign"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["IndirectCostDesign"]);
                                treatment.IndirectCostOther = reader["IndirectCostOther"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["IndirectCostOther"]);
                                treatment.IndirectCostROW = reader["IndirectCostROW"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["IndirectCostROW"]);
                                treatment.IndirectCostUtilities = reader["IndirectCostUtilities"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["IndirectCostUtilities"]);

                                treatment.MPMSID = reader["MPMSID"] == DBNull.Value ? null : (string)reader["MPMSID"];

                                if (treatment.Benefit.HasValue && treatment.Cost.HasValue && treatment.Cost > 0)
                                {
                                    treatment.BenefitCostRatio = treatment.Benefit.Value / treatment.Cost.Value;
                                }
                                else
                                {
                                    treatment.BenefitCostRatio = null;
                                }

                                treatment.MPO = null;
                                if (reader["Budget"] != DBNull.Value)
                                {
                                    string bud = reader["Budget"].ToString().Trim();
                                    int ix = bud.IndexOf(' ');
                                    if (ix > 0)
                                    {
                                        bud = bud.Substring(0, ix);
                                    }
                                    treatment.MPO = bud;
                                }

                                gisOutput.Treatments.Add(treatment);
                                nTreat++;
                            }

                            Log.Info($"Number of loaded treatments: {nTreat}");

                            reader.Close();
                        }
                    }
                }

                jsonString = JsonConvert.SerializeObject(gisOutput, indent ? Formatting.Indented : Formatting.None);
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                jsonString = null;
                Log.Error(errorMessage);
            }


            Log.Info($"ExportScenarioResultsToJson ({scenarioId},{indent}) - finished.");

            return (ok);
        }
    }
}
