using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.AraEnumerations.Dtos;
using ProjectBuilder.Models.BridgeToPavements;
using ProjectBuilder.Models.CreateBridgeToPavements.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    public class BridgeToPavmentsAppService : ApplicationService, IBridgeToPavmentsAppService
    {
        private readonly IBridgeToPavementsRepository _bridgeToPavementsRepository;
        private readonly ProjectBuilderMapper _mapper;

        public BridgeToPavmentsAppService(IBridgeToPavementsRepository bridgeToPavementsRepository, ProjectBuilderMapper mapper)
        {
            _bridgeToPavementsRepository = bridgeToPavementsRepository;
            _mapper = mapper;
        }
        public List<BridgeToPavementDto> GetAllBridgeToPavements()
        {
            var bridgeToPavements = _bridgeToPavementsRepository.GetList();
            return bridgeToPavements.Select(item => new BridgeToPavementDto { Id = item.Id }).ToList();
        }
        public PaginatedDto<BridgeToPavementDto> GetPagedBridgeToPavements(JqueryDatatableParamDto param)
        {
            var query = _bridgeToPavementsRepository.GetQueryable();

            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                // EF Core can't translate ToString()/Contains(string, StringComparison) on
                // every column type reliably — match strings by substring and numeric
                // columns by exact value so the whole predicate stays SQL-translatable
                // (this runs against the query before Skip/Take, not an in-memory list).
                var search = param.sSearch.Trim();
                var searchIsNumber = long.TryParse(search, out var searchNumber);

                query = query.Where(t =>
                    t.Id.Contains(search) ||
                    (t.County != null && t.County.Contains(search)) ||
                    (t.CreatedBy != null && t.CreatedBy.Contains(search)) ||
                    (searchIsNumber && (
                        t.BridgeId == searchNumber ||
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

            return new PaginatedDto<BridgeToPavementDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };
        }

        private static IQueryable<BridgeToPavements> ApplySort(IQueryable<BridgeToPavements> query, string sortField, string sortDir)
        {
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortField ?? "").ToLowerInvariant() switch
            {
                "id" => desc ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id),
                "bridgeid" => desc ? query.OrderByDescending(t => t.BridgeId) : query.OrderBy(t => t.BridgeId),
                "district" => desc ? query.OrderByDescending(t => t.District) : query.OrderBy(t => t.District),
                "county" => desc ? query.OrderByDescending(t => t.County) : query.OrderBy(t => t.County),
                "route" => desc ? query.OrderByDescending(t => t.Route) : query.OrderBy(t => t.Route),
                "segment" => desc ? query.OrderByDescending(t => t.Segment) : query.OrderBy(t => t.Segment),
                "offset" => desc ? query.OrderByDescending(t => t.OffSet) : query.OrderBy(t => t.OffSet),
                "createdby" => desc ? query.OrderByDescending(t => t.CreatedBy) : query.OrderBy(t => t.CreatedBy),
                "createdat" => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                _ => query.OrderBy(t => t.Id)
            };
        }

        public List<BridgeToPavementDto> GetAllAsync()
        {
            var query = _bridgeToPavementsRepository.GetQueryable();
            return query.Select(x => new BridgeToPavementDto
            {
                Id = x.Id,
                BridgeId = x.BridgeId,
                District = x.District,
                County = x.County,
                CountyCode = x.CountyCode,
                Route = x.Route,
                Segment = x.Segment,
                OffSet = x.OffSet,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt
            }).ToList();
        }
    }
}