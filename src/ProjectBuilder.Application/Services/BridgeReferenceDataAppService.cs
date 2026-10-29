using ProjectBuilder.Entities;
using ProjectBuilder.Models.BridgeReferenceData;
using ProjectBuilder.Models.BridgeReferenceData.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Repositories;
using System;
using System.Linq;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    public class BridgeReferenceDataAppService : ApplicationService, IBridgeReferenceDataAppService
    {
        private readonly IBridgeReferenceDataRepository _bridgeReferenceDataRepository;
        private readonly ProjectBuilderMapper _mapper;

        public BridgeReferenceDataAppService(IBridgeReferenceDataRepository bridgeReferenceDataRepository, ProjectBuilderMapper mapper)
        {
            _bridgeReferenceDataRepository = bridgeReferenceDataRepository;
            _mapper = mapper;
        }

        public PaginatedDto<BridgeReferenceDataDto> GetPagedBridgeReferenceData(JqueryDatatableParamDto param)
        {
            var query = _bridgeReferenceDataRepository.GetQueryable();

            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                // Same SQL-translatable search strategy as BridgeToPavments: substring match on
                // strings, exact match on numeric columns, applied before Skip/Take.
                var search = param.sSearch.Trim();
                var searchIsNumber = long.TryParse(search, out var searchNumber);

                query = query.Where(t =>
                    t.Id.Contains(search) ||
                    (t.BridgeId != null && t.BridgeId.Contains(search)) ||
                    (t.CreatedBy != null && t.CreatedBy.Contains(search)) ||
                    (searchIsNumber && (
                        t.District == searchNumber ||
                        t.CountyCode == searchNumber ||
                        t.Route == searchNumber ||
                        t.Segment == searchNumber ||
                        t.OffSet == searchNumber))
                );
            }

            var totalRecords = query.Count();

            var displayResult = ApplySort(query, param.SortField, param.SortDir)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();

            var mappedResults = _mapper.Map(displayResult);

            return new PaginatedDto<BridgeReferenceDataDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };
        }

        private static IQueryable<BridgeReferenceData> ApplySort(IQueryable<BridgeReferenceData> query, string sortField, string sortDir)
        {
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortField ?? "").ToLowerInvariant() switch
            {
                "id" => desc ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id),
                "bridgeid" => desc ? query.OrderByDescending(t => t.BridgeId) : query.OrderBy(t => t.BridgeId),
                "district" => desc ? query.OrderByDescending(t => t.District) : query.OrderBy(t => t.District),
                "countycode" => desc ? query.OrderByDescending(t => t.CountyCode) : query.OrderBy(t => t.CountyCode),
                "route" => desc ? query.OrderByDescending(t => t.Route) : query.OrderBy(t => t.Route),
                "segment" => desc ? query.OrderByDescending(t => t.Segment) : query.OrderBy(t => t.Segment),
                "offset" => desc ? query.OrderByDescending(t => t.OffSet) : query.OrderBy(t => t.OffSet),
                "createdby" => desc ? query.OrderByDescending(t => t.CreatedBy) : query.OrderBy(t => t.CreatedBy),
                "createdat" => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                // Falls back to the existing IX_tbl_pb_BridgeReferenceDataFromPennshare_DCRS index
                // columns for stable, index-friendly paging.
                _ => query.OrderBy(t => t.District).ThenBy(t => t.CountyCode).ThenBy(t => t.Route).ThenBy(t => t.Segment)
            };
        }
    }
}
