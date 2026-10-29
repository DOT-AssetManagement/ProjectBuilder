using Hangfire;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ProjectBuilder.Web.Jobs
{
    // Fetches Pennsylvania's public bridge inventory from PennDOT's PennShare ArcGIS
    // service and replaces tbl_pb_BridgeReferenceDataFromPennshare with the result.
    // Source: https://gis.penndot.pa.gov/gis/rest/services/opendata/pennsylvaniabridges/MapServer/0
    // ("Pennsylvania Bridges" — item f396d79f7c3646b1a033b18eabb666f7 on
    // data-pennshare.opendata.arcgis.com), maxRecordCount=2000, so results are paged.
    public class FetchBridgeReferenceDataJob : ITransientDependency
    {
        private const string ServiceUrl = "https://gis.penndot.pa.gov/gis/rest/services/opendata/pennsylvaniabridges/MapServer/0/query";
        private const string OutFields = "BRKEY,DISTRICT,CTY_CODE,BRIDGE_ID,ST_RT_NO,SEG_NO,OFFSET";
        private const int PageSize = 2000;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPoolRepository _poolRepository;
        private readonly ILogger<FetchBridgeReferenceDataJob> _logger;

        public FetchBridgeReferenceDataJob(
            IHttpClientFactory httpClientFactory,
            IPoolRepository poolRepository,
            ILogger<FetchBridgeReferenceDataJob> logger)
        {
            _httpClientFactory = httpClientFactory;
            _poolRepository = poolRepository;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task RunAsync(string requestedBy)
        {
            _logger.LogInformation("Bridge reference data fetch started (requested by {RequestedBy})", requestedBy);

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);

            var rowsByKey = new Dictionary<string, BridgeReferenceData>();
            int offset = 0;
            int skipped = 0;

            while (true)
            {
                var url = $"{ServiceUrl}?where=1%3D1&outFields={OutFields}&returnGeometry=false&f=json" +
                           $"&resultOffset={offset}&resultRecordCount={PageSize}";

                using var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (doc.RootElement.TryGetProperty("error", out var error))
                {
                    throw new Exception($"ArcGIS service returned an error: {error.GetRawText()}");
                }

                var features = doc.RootElement.GetProperty("features");
                int count = 0;
                foreach (var feature in features.EnumerateArray())
                {
                    count++;
                    var attributes = feature.GetProperty("attributes");
                    if (!TryMapRow(attributes, out var row))
                    {
                        skipped++;
                        continue;
                    }
                    rowsByKey[row.Id] = row; // last one wins if the source repeats a BRKEY across pages
                }

                _logger.LogInformation("Bridge reference data fetch: offset={Offset} returned {Count} features", offset, count);

                if (count < PageSize)
                    break;

                offset += count;
            }

            var rows = new List<BridgeReferenceData>(rowsByKey.Values);
            var inserted = await _poolRepository.ReplaceBridgeReferenceDataAsync(rows);

            _logger.LogInformation(
                "Bridge reference data fetch completed: {Inserted} rows inserted, {Skipped} rows skipped (missing/invalid BRKEY)",
                inserted, skipped);
        }

        private static bool TryMapRow(JsonElement attributes, out BridgeReferenceData row)
        {
            row = null;

            var brkey = FormatBrKey(attributes, "BRKEY");
            if (string.IsNullOrWhiteSpace(brkey))
                return false;

            row = new BridgeReferenceData
            {
                Id = brkey,
                District = ParseInt(attributes, "DISTRICT"),
                CountyCode = ParseInt(attributes, "CTY_CODE"),
                BridgeId = GetString(attributes, "BRIDGE_ID"),
                Route = ParseInt(attributes, "ST_RT_NO"),
                Segment = ParseInt(attributes, "SEG_NO"),
                OffSet = ParseInt(attributes, "OFFSET"),
                CreatedBy = "PennShare Fetch",
                CreatedAt = DateTime.UtcNow
            };
            return true;
        }

        // BRKEY arrives as a JSON number (esriFieldTypeDouble); format without scientific
        // notation so it round-trips as the same digit string PennDOT uses elsewhere.
        private static string FormatBrKey(JsonElement attributes, string fieldName)
        {
            if (!attributes.TryGetProperty(fieldName, out var value) || value.ValueKind == JsonValueKind.Null)
                return null;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
                return number.ToString("F0", CultureInfo.InvariantCulture);

            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        private static string GetString(JsonElement attributes, string fieldName)
        {
            if (!attributes.TryGetProperty(fieldName, out var value) || value.ValueKind == JsonValueKind.Null)
                return null;

            return value.ValueKind == JsonValueKind.String ? value.GetString().Trim() : value.GetRawText();
        }

        private static int? ParseInt(JsonElement attributes, string fieldName)
        {
            if (!attributes.TryGetProperty(fieldName, out var value) || value.ValueKind == JsonValueKind.Null)
                return null;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                return number;

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
                return parsed;

            return null;
        }
    }
}
