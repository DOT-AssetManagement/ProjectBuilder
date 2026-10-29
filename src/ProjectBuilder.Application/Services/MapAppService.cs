using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Models.Map;
using ProjectBuilder.Models.Map.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    [Authorize(ScenarioPermissions.Analysis.Maps)]
    public class MapAppService : ApplicationService, IMapAppService
    {
        private readonly string _connectionString;
        private readonly IProjectRepository _projectRepository;

        public MapAppService(IConfiguration configuration, IProjectRepository projectRepository)
        {
            _connectionString = configuration.GetConnectionString("Default");
            _projectRepository = projectRepository;
        }

        // ── GET /api/app/map/scenarios ───────────────────────────────────────
        public async Task<List<MapScenarioDto>> GetScenariosAsync()
        {
            var list = new List<MapScenarioDto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(
                "SELECT ScenId, ScenarioName FROM tbl_pb_Scenarios WITH(NOLOCK) ORDER BY ScenarioName", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new MapScenarioDto
                {
                    ScenId = Convert.ToInt32(reader["ScenId"]),
                    ScenarioName = reader["ScenarioName"].ToString()
                });
            }
            return list;
        }

        // ── GET /api/app/map/districts ───────────────────────────────────────
        public async Task<List<MapDistrictDto>> GetDistrictsAsync()
        {
            var list = new List<MapDistrictDto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("dbo.UspUIGetDistricts", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 24000
            };
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new MapDistrictDto
                    {
                        DistrictId = Convert.ToInt32(row["District"]),
                        DistrictName = row["Descr"].ToString()
                    });
                }
            }
            return list;
        }

        // ── GET /api/app/map/counties?districtId=X ───────────────────────────
        public async Task<List<MapCountyDto>> GetCountiesAsync(int districtId)
        {
            var list = new List<MapCountyDto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("dbo.UspUIGetCounties", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 24000
            };
            cmd.Parameters.Add("@District", SqlDbType.Int).Value = districtId;
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new MapCountyDto
                    {
                        Cnty = Convert.ToInt32(row["Cnty"]),
                        CountyName = row["County"].ToString()
                    });
                }
            }
            return list;
        }

        // ── GET /api/app/map/routes?districtId=X&countyId=Y ─────────────────
        public async Task<List<MapRouteDto>> GetRoutesAsync(int districtId, int countyId)
        {
            var list = new List<MapRouteDto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("dbo.UspUIGetRoutes", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 24000
            };
            cmd.Parameters.Add("@District", SqlDbType.Int).Value = districtId;
            cmd.Parameters.Add("@Cnty", SqlDbType.Int).Value = countyId;
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                adapter.Fill(dt);
                foreach (DataRow row in dt.Rows)
                    list.Add(new MapRouteDto { Rte = row["Rte"].ToString() });
            }
            return list;
        }

        // ── POST /api/app/map/generate-map ───────────────────────────────────
        // Returns the full project + treatment dataset for a scenario.
        // The React client renders this with the Esri ArcGIS SDK directly.
        public async Task<MapGisDataDto> GenerateMapAsync(MapGenerateRequestDto input)
        {
            if (input.ScenId <= 0)
                throw new UserFriendlyException("Scenario is required.");

            return await BuildGisDataAsync(input);
        }

        // ── Build GIS data from sp_pb_SelectProjectsForGIS
        //                    and sp_pb_SelectTreatmentsForGIS ──────────────────
        private async Task<MapGisDataDto> BuildGisDataAsync(MapGenerateRequestDto input)
        {
            var totalTimer = Stopwatch.StartNew();
            var result = new MapGisDataDto();

            // Scenario header first — cheap PK lookup, and it's the thing that tells us the
            // scenario exists at all, so it should still gate the two heavier queries below.
            var headerTimer = Stopwatch.StartNew();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    @"SELECT s.ScenId, s.ScenarioName,
                             ul.[Name] AS LibraryName,
                             s.LastRunBy, s.LastRunAt, s.Notes
                      FROM tbl_pb_Scenarios s WITH(NOLOCK)
                      LEFT JOIN tbl_lib_UserLibraries ul WITH(NOLOCK) ON ul.Id = s.LibraryId
                      WHERE s.ScenId = @ScenId", conn);
                cmd.Parameters.Add("@ScenId", SqlDbType.Int).Value = input.ScenId;
                using var reader = await cmd.ExecuteReaderAsync();
                if (!reader.HasRows)
                    throw new UserFriendlyException($"Scenario {input.ScenId} not found.");
                if (await reader.ReadAsync())
                {
                    result.Scenario.ScenId      = Convert.ToInt32(reader["ScenId"]);
                    result.Scenario.Name        = Str(reader["ScenarioName"]);
                    result.Scenario.LibraryName = Str(reader["LibraryName"]);
                    result.Scenario.LastRunBy   = Str(reader["LastRunBy"]);
                    result.Scenario.LastRunTime = reader["LastRunAt"] == DBNull.Value
                        ? null : (DateTime?)Convert.ToDateTime(reader["LastRunAt"]);
                    result.Scenario.Notes       = Str(reader["Notes"]);
                }
            }
            headerTimer.Stop();

            // Projects and treatments don't depend on each other — load them concurrently,
            // each on its own connection, instead of one after another on a shared connection.
            var projectsTimer = Stopwatch.StartNew();
            var treatmentsTimer = Stopwatch.StartNew();

            async Task<List<MapGisProjectDto>> LoadProjectsTimedAsync()
            {
                var list = await LoadProjectsAsync(input);
                projectsTimer.Stop();
                return list;
            }

            async Task<List<MapGisTreatmentDto>> LoadTreatmentsTimedAsync()
            {
                var list = await LoadTreatmentsAsync(input);
                treatmentsTimer.Stop();
                return list;
            }

            var projectsTask = LoadProjectsTimedAsync();
            var treatmentsTask = LoadTreatmentsTimedAsync();
            await Task.WhenAll(projectsTask, treatmentsTask);

            result.Projects = await projectsTask;
            result.Treatments = await treatmentsTask;

            totalTimer.Stop();
            Logger.LogInformation(
                "GenerateMap scenId={ScenId} header={HeaderMs}ms projects={ProjectsMs}ms treatments={TreatmentsMs}ms total={TotalMs}ms (projects/treatments ran concurrently)",
                input.ScenId, headerTimer.ElapsedMilliseconds, projectsTimer.ElapsedMilliseconds,
                treatmentsTimer.ElapsedMilliseconds, totalTimer.ElapsedMilliseconds);

            return result;
        }

        // ── Projects — sp_pb_SelectProjectsForGIS ─────────────────────────────
        private async Task<List<MapGisProjectDto>> LoadProjectsAsync(MapGenerateRequestDto input)
        {
            var list = new List<MapGisProjectDto>();

            // Overlaps the sproc below, but the two share one DbContext so they must not overlap each other.
            async Task<(Dictionary<int, int> Funded, Dictionary<int, int> Unfunded)> LoadSequenceNumbersAsync()
            {
                var funded = await _projectRepository.GetSequenceNumbersAsync(input.ScenId);
                var unfunded = await _projectRepository.GetUnfundedSequenceNumbersAsync(input.ScenId);
                return (funded, unfunded);
            }

            var sequenceNumbersTask = LoadSequenceNumbersAsync();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("sp_pb_SelectProjectsForGIS", conn)
                   { CommandType = CommandType.StoredProcedure, CommandTimeout = 12000 };
            cmd.Parameters.Add("@ScenId", SqlDbType.Int).Value = input.ScenId;
            if (input.District.HasValue) cmd.Parameters.Add("@District", SqlDbType.TinyInt).Value = (byte)input.District.Value;
            if (input.Cnty.HasValue)     cmd.Parameters.Add("@Cnty",     SqlDbType.TinyInt).Value = (byte)input.Cnty.Value;
            if (input.Route.HasValue)    cmd.Parameters.Add("@Route",    SqlDbType.Int).Value = input.Route.Value;
            // Always pull unselected (not-funded) projects too — the map now shows both and lets
            // the client-side filter/renderer distinguish by Status instead of the server omitting them.
            cmd.Parameters.Add("@IncludeUnselected", SqlDbType.Bit).Value = true;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new MapGisProjectDto
                {
                    ProjectId      = Convert.ToInt32(reader["ProjectId"]),
                    SchemaId       = Convert.ToInt64(reader["SchemaId"]),
                    ProjectType    = Convert.ToInt16(reader["ProjectType"]),
                    Year           = reader["Year"]      == DBNull.Value ? null : (int?)Convert.ToInt32(reader["Year"]),
                    NumBridges     = reader["NumBridges"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NumBridges"]),
                    NumPaveSections= reader["NumPaves"]   == DBNull.Value ? 0 : Convert.ToInt32(reader["NumPaves"]),
                    TotalCost      = reader["TotalCost"]  == DBNull.Value ? 0d : Convert.ToDouble(reader["TotalCost"]),
                    UserNotes      = Str(reader["UserNotes"]),
                    Status         = Str(reader["Status"])
                });
            }

            var (sequenceNumbers, unfundedSequenceNumbers) = await sequenceNumbersTask;
            foreach (var p in list)
            {
                var numbers = p.Status == "Unselected" ? unfundedSequenceNumbers : sequenceNumbers;
                p.SequenceNo = numbers.TryGetValue(p.ProjectId, out var seq) ? seq : (int?)null;
            }

            return list;
        }

        // ── Treatments — sp_pb_SelectTreatmentsForGIS ─────────────────────────
        private async Task<List<MapGisTreatmentDto>> LoadTreatmentsAsync(MapGenerateRequestDto input)
        {
            var list = new List<MapGisTreatmentDto>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("sp_pb_SelectTreatmentsForGIS", conn)
                   { CommandType = CommandType.StoredProcedure, CommandTimeout = 12000 };
            cmd.Parameters.Add("@ScenId", SqlDbType.Int).Value = input.ScenId;
            if (input.District.HasValue)
                cmd.Parameters.Add("@District",      SqlDbType.TinyInt).Value = (byte)input.District.Value;
            if (input.Cnty.HasValue)
                cmd.Parameters.Add("@Cnty",          SqlDbType.TinyInt).Value = (byte)input.Cnty.Value;
            if (input.Route.HasValue)
                cmd.Parameters.Add("@Route",         SqlDbType.Int).Value = input.Route.Value;
            if (!string.IsNullOrEmpty(input.TreatmentType))
                cmd.Parameters.Add("@TreatmentType", SqlDbType.Char, 1).Value = input.TreatmentType[0];
            // Always pull unselected (not-funded) treatments too — see LoadProjectsAsync above.
            cmd.Parameters.Add("@IncludeUnselected", SqlDbType.Bit).Value = true;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                double? cost    = reader["Cost"]    == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Cost"]);
                double? benefit = reader["Benefit"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Benefit"]);
                double? bcr     = (benefit.HasValue && cost.HasValue && cost > 0)
                    ? benefit.Value / cost.Value : null;

                // Total cost = direct cost + all indirect cost components, matching the
                // TotalCost formula in vw_pb_ui_ScenarioTreatments (the DTO behind the bottom
                // "Project Treatments" grid) so the two grids agree on the same treatment.
                double? totalCost = cost.HasValue
                    ? cost.Value + NullableDouble(reader["IndirectCostDesign"])
                                 + NullableDouble(reader["IndirectCostROW"])
                                 + NullableDouble(reader["IndirectCostUtilities"])
                                 + NullableDouble(reader["IndirectCostOther"])
                    : null;

                string mpo = null;
                if (reader["Budget"] != DBNull.Value)
                {
                    string bud = reader["Budget"].ToString().Trim();
                    int ix = bud.IndexOf(' ');
                    mpo = ix > 0 ? bud.Substring(0, ix) : bud;
                }

                list.Add(new MapGisTreatmentDto
                {
                    ProjectId        = Convert.ToInt32(reader["ProjectId"]),
                    ProjectType      = Int16Or(reader["ProjectType"]),
                    TreatmentId      = Convert.ToInt64(reader["TreatmentId"]),
                    AppliedTreatment = reader["AppliedTreatment"].ToString(),
                    TreatmentType    = reader["TreatmentType"].ToString(),
                    District         = Int16Or(reader["District"]),
                    County           = Int16Or(reader["Cnty"]),
                    Route            = Int32Or(reader["Route"]),
                    Direction        = Int16Or(reader["Direction"]),
                    FromSection      = Int32Or(reader["FromSection"]),
                    ToSection        = Int32Or(reader["ToSection"]),
                    BRKEY            = Str(reader["BRKEY"]),
                    BRIDGE_ID        = reader["BRIDGE_ID"] == DBNull.Value ? null : (long?)Convert.ToInt64(reader["BRIDGE_ID"]),
                    OwnerCode        = Str(reader["OWNER_CODE"]),
                    CountyName       = Str(reader["County"]),
                    MPO              = mpo,
                    Year             = reader["Year"]    == DBNull.Value ? null : (int?)Convert.ToInt32(reader["Year"]),
                    Cost             = cost,
                    TotalCost        = totalCost,
                    Benefit          = benefit,
                    BenefitCostRatio = bcr,
                    Status           = Str(reader["Status"])
                });
            }
            reader.Close();

            // The GIS sproc doesn't return the project's HasCommitted, so it's looked up separately and stamped on each treatment.
            var committedByProject = new Dictionary<int, string>();
            using (var committedCmd = new SqlCommand(
                "SELECT ProjectId, HasCommitted FROM tbl_pb_ExtendedTimewiseConstrainedProjects WITH (NOLOCK) WHERE ScenId = @ScenId", conn))
            {
                committedCmd.Parameters.Add("@ScenId", SqlDbType.Int).Value = input.ScenId;
                using var committedReader = await committedCmd.ExecuteReaderAsync();
                while (await committedReader.ReadAsync())
                {
                    var hasCommitted = committedReader["HasCommitted"] != DBNull.Value && Convert.ToBoolean(committedReader["HasCommitted"]);
                    committedByProject[Convert.ToInt32(committedReader["ProjectId"])] = hasCommitted ? "Y" : "N";
                }
            }
            foreach (var t in list)
                t.HasCommitted = committedByProject.TryGetValue(t.ProjectId, out var c) ? c : "N";

            return list;
        }

        private static string Str(object val) =>
            val == null || val == DBNull.Value ? null : val.ToString();

        private static double NullableDouble(object val) =>
            val == null || val == DBNull.Value ? 0d : Convert.ToDouble(val);

        // Location/classification columns (District, Direction, Route, ...) are nullable in
        // tbl_pb_ExtendedImportedTreatments — a NULL here (e.g. a treatment edited before a
        // shared-endpoint field was fixed to stop clobbering it) shouldn't 500 the whole map.
        private static short Int16Or(object val, short fallback = 0) =>
            val == null || val == DBNull.Value ? fallback : Convert.ToInt16(val);

        private static int Int32Or(object val, int fallback = 0) =>
            val == null || val == DBNull.Value ? fallback : Convert.ToInt32(val);
    }
}
