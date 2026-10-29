using ProjectBuilder.Entities;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PAMSSectionSegment;
using ProjectBuilder.Models.PAMSSectionSegment.Dtos;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Repositories;
using System;
using System.Linq;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    public class PAMSSectionSegmentAppService : ApplicationService, IPAMSSectionSegmentAppService
    {
        private readonly IPAMSSectionSegmentRepository _pamsSectionSegmentRepository;
        private readonly ProjectBuilderMapper _mapper;

        public PAMSSectionSegmentAppService(IPAMSSectionSegmentRepository pamsSectionSegmentRepository, ProjectBuilderMapper mapper)
        {
            _pamsSectionSegmentRepository = pamsSectionSegmentRepository;
            _mapper = mapper;
        }

        public PaginatedDto<PAMSSectionSegmentDto> GetPagedMaintainableAssetsSegmentation(JqueryDatatableParamDto param)
        {
            var query = _pamsSectionSegmentRepository.GetQueryable();

            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                // Same SQL-translatable search strategy as BridgeToPavments: substring match on
                // strings, exact match on numeric columns, applied before Skip/Take against a
                // table with 700k+ rows.
                var search = param.sSearch.Trim();
                var searchIsNumber = long.TryParse(search, out var searchNumber);

                query = query.Where(t =>
                    t.Id.Contains(search) ||
                    (t.NetworkId != null && t.NetworkId.Contains(search)) ||
                    (t.Asset != null && t.Asset.Contains(search)) ||
                    (t.SurfaceName != null && t.SurfaceName.Contains(search)) ||
                    (t.PopulatedBy != null && t.PopulatedBy.Contains(search)) ||
                    (searchIsNumber && (
                        t.District == searchNumber ||
                        t.CountyId == searchNumber ||
                        t.Route == searchNumber ||
                        t.FromSection == searchNumber ||
                        t.ToSection == searchNumber ||
                        t.Area == searchNumber ||
                        t.Lanes == searchNumber ||
                        t.Width == searchNumber ||
                        t.Length == searchNumber))
                );
            }

            var totalRecords = query.Count();

            var displayResult = ApplySort(query, param.SortField, param.SortDir)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();

            var mappedResults = _mapper.Map(displayResult);

            return new PaginatedDto<PAMSSectionSegmentDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };
        }

        private static IQueryable<PAMSSectionSegment> ApplySort(IQueryable<PAMSSectionSegment> query, string sortField, string sortDir)
        {
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortField ?? "").ToLowerInvariant() switch
            {
                "district" => desc ? query.OrderByDescending(t => t.District) : query.OrderBy(t => t.District),
                "countyid" => desc ? query.OrderByDescending(t => t.CountyId) : query.OrderBy(t => t.CountyId),
                "route" => desc ? query.OrderByDescending(t => t.Route) : query.OrderBy(t => t.Route),
                "asset" => desc ? query.OrderByDescending(t => t.Asset) : query.OrderBy(t => t.Asset),
                "direction" => desc ? query.OrderByDescending(t => t.Direction) : query.OrderBy(t => t.Direction),
                "fromsection" => desc ? query.OrderByDescending(t => t.FromSection) : query.OrderBy(t => t.FromSection),
                "tosection" => desc ? query.OrderByDescending(t => t.ToSection) : query.OrderBy(t => t.ToSection),
                "area" => desc ? query.OrderByDescending(t => t.Area) : query.OrderBy(t => t.Area),
                "interstate" => desc ? query.OrderByDescending(t => t.Interstate) : query.OrderBy(t => t.Interstate),
                "lanes" => desc ? query.OrderByDescending(t => t.Lanes) : query.OrderBy(t => t.Lanes),
                "width" => desc ? query.OrderByDescending(t => t.Width) : query.OrderBy(t => t.Width),
                "length" => desc ? query.OrderByDescending(t => t.Length) : query.OrderBy(t => t.Length),
                "surfacename" => desc ? query.OrderByDescending(t => t.SurfaceName) : query.OrderBy(t => t.SurfaceName),
                "risk" => desc ? query.OrderByDescending(t => t.Risk) : query.OrderBy(t => t.Risk),
                // Falls back to the existing IX_tbl_pams_MaintainableAssetsSegmentation_DCRS index
                // columns so paging over 700k+ heap rows still gets a stable order without an extra sort.
                _ => query.OrderBy(t => t.District).ThenBy(t => t.CountyId).ThenBy(t => t.Route).ThenBy(t => t.FromSection)
            };
        }
    }
}
