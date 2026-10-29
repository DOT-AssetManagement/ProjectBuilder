using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Models.DssMap;
using ProjectBuilder.Models.DssMap.Dtos;
using ProjectBuilder.Permissions;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    [Authorize(SupplementalCandidatesPermissions.PoolsPageActions.Map)]
    public class DssMapAppService : ApplicationService, IDssMapAppService
    {
        private readonly string _connectionString;

        public DssMapAppService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        // ── POST /api/app/dss-map/generate-map ───────────────────────────────
        // Returns the full project + work-candidate dataset for a DSS scenario's pool.
        // The client resolves geometry from PennDOT's ArcGIS feature services, same as /Map.
        public async Task<DssMapGisDataDto> GenerateMapAsync(DssMapGenerateRequestDto input)
        {
            if (input.ScenId == Guid.Empty)
                throw new UserFriendlyException("Scenario is required.");

            return await BuildGisDataAsync(input);
        }

        // ── Build GIS data from dbo.UspGetScenarioResultsForGIS ──────────────
        private async Task<DssMapGisDataDto> BuildGisDataAsync(DssMapGenerateRequestDto input)
        {
            var result = new DssMapGisDataDto();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("dbo.UspGetScenarioResultsForGIS", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 12000
            };
            cmd.Parameters.Add("@ScenId", SqlDbType.UniqueIdentifier).Value = input.ScenId;
            cmd.Parameters.Add("@EffThreshold", SqlDbType.Float).Value = input.RelativeEfficiencyThreshold ?? 0d;

            using var reader = await cmd.ExecuteReaderAsync();

            // Result set 1: scenario header
            if (!reader.HasRows)
                throw new UserFriendlyException($"Scenario {input.ScenId} not found.");
            if (await reader.ReadAsync())
            {
                result.Scenario.ScenId = input.ScenId;
                result.Scenario.Name = Str(reader["ScenName"]);
                result.Scenario.PoolName = Str(reader["PoolName"]);
                result.Scenario.User = Str(reader["User"]);
                result.Scenario.LastRun = reader["LastRun"] == DBNull.Value
                    ? null : (DateTime?)Convert.ToDateTime(reader["LastRun"]);
                result.Scenario.EfficiencyThreshold = input.RelativeEfficiencyThreshold ?? 0d;
            }

            // Result set 2: project candidates
            await reader.NextResultAsync();
            if (!reader.HasRows)
                throw new UserFriendlyException(
                    $"No project candidates for scenario {input.ScenId}. Run the scenario first.");
            while (await reader.ReadAsync())
            {
                result.Projects.Add(new DssMapGisProjectDto
                {
                    ProjectId = RequireGuid(reader["ProjectId"], "ProjectId"),
                    ProjectNo = Str(reader["ProjectNo"]),
                    PlanningPartner = Str(reader["PlanningPartner"]),
                    District = reader["District"] == DBNull.Value ? null : (byte?)Convert.ToByte(reader["District"]),
                    Cnty = reader["Cnty"] == DBNull.Value ? null : (byte?)Convert.ToByte(reader["Cnty"]),
                    County = Str(reader["County"]),
                    Description = Str(reader["Description"]),
                    Treatment = Str(reader["Treatment"]),
                    MinYear = reader["MinYear"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["MinYear"]),
                    MaxYear = reader["MaxYear"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["MaxYear"]),
                    NumPave = RequireInt32(reader["NumPave"], "NumPave"),
                    NumBridge = RequireInt32(reader["NumBridge"], "NumBridge"),
                    Cost = reader["Cost"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Cost"]),
                    Benefit = reader["Benefit"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Benefit"]),
                    Efficiency = reader["Efficiency"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Efficiency"])
                });
            }

            // Result set 3: work candidates
            await reader.NextResultAsync();
            if (!reader.HasRows)
                throw new UserFriendlyException(
                    $"No work candidates for scenario {input.ScenId}. Run the scenario first.");
            while (await reader.ReadAsync())
            {
                result.WorkCandidates.Add(new DssMapGisWorkCandidateDto
                {
                    ProjectId = RequireGuid(reader["ProjId"], "ProjId"),
                    TreatmentId = RequireGuid(reader["TreatId"], "TreatId"),
                    TreatNo = RequireInt32(reader["TreatNo"], "TreatNo"),
                    Partner = Str(reader["Partner"]),
                    AssetType = Str(reader["AssetType"]),
                    TreatmentType = Str(reader["TreatmentType"]),
                    TreatmentName = Str(reader["TreatmentName"]),
                    District = reader["District"] == DBNull.Value ? null : (byte?)Convert.ToByte(reader["District"]),
                    County = reader["Cnty"] == DBNull.Value ? null : (byte?)Convert.ToByte(reader["Cnty"]),
                    CountyName = Str(reader["County"]),
                    Route = reader["Route"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["Route"]),
                    FromSection = reader["FromSection"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["FromSection"]),
                    ToSection = reader["ToSection"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["ToSection"]),
                    BRKEY = Str(reader["BRKEY"]),
                    BRIDGE_ID = reader["BMSID"] == DBNull.Value ? null : (long?)Convert.ToInt64(reader["BMSID"]),
                    CRS = Str(reader["CRS"]),
                    Direction = reader["Direction"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["Direction"]),
                    Year = reader["Year"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["Year"]),
                    Cost = reader["Cost"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Cost"]),
                    Benefit = reader["Benefit"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Benefit"]),
                    BCRatio = reader["BCRatio"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["BCRatio"])
                });
            }

            return result;
        }

        private static string Str(object val) =>
            val == null || val == DBNull.Value ? null : val.ToString();

        private static Guid RequireGuid(object val, string fieldName) =>
            val == null || val == DBNull.Value
                ? throw new UserFriendlyException($"Unexpected NULL for required field '{fieldName}' returned by UspGetScenarioResultsForGIS.")
                : Guid.Parse(val.ToString());

        private static int RequireInt32(object val, string fieldName) =>
            val == null || val == DBNull.Value
                ? throw new UserFriendlyException($"Unexpected NULL for required field '{fieldName}' returned by UspGetScenarioResultsForGIS.")
                : Convert.ToInt32(val);
    }
}
