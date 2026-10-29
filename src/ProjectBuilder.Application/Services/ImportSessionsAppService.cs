using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.ImportSessions;
using ProjectBuilder.Models.ImportSessions.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    public class ImportSessionsAppService : ApplicationService, IImportSessionsAppService
    {
        private readonly IImportSessionsRepository _importSessionsRepository;
        private readonly IImportSessionRepository _importSessionRepository;
        private readonly IObjectMapper _ObjectMapper;

        public ImportSessionsAppService(IImportSessionsRepository importSessionsRepository, IImportSessionRepository importSessionRepository, IObjectMapper objectMapper)
        {
            _importSessionsRepository = importSessionsRepository;
            _importSessionRepository = importSessionRepository;
            _ObjectMapper = objectMapper;
        }

        public PaginatedDto<ImportSessionDto> GetPagedImportSessions(JqueryDatatableParamDto param)
        {
            var importSessions = _importSessionsRepository.GetQueryable();
            var displayResult = importSessions
              .Skip(param.iDisplayStart)
              .Take(param.iDisplayLength)
              .ToList();
            var totalRecords = importSessions.Count();

            var mappedResults = _ObjectMapper.Map<List<ImportSessions>, List<ImportSessionDto>>(displayResult);
            return new PaginatedDto<ImportSessionDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };
        }

        public async Task<ImportSessionDto> GetByPoolId(Guid poolId)
        {
            var response = await _importSessionRepository.GetByPoolId(poolId);
            return _ObjectMapper.Map<ImportSessions, ImportSessionDto>(response);
        }

        public List<ImportSessionDto> GetAllAsync()
        {
            var items = _importSessionsRepository.GetQueryable();
            return items.Select(x => new ImportSessionDto
            {
                Id = x.Id,
                NoId = x.NoId,
                ImportSource = x.ImportSource,
                DataSourceType = x.DataSourceType,
                DataSourceName = x.DataSourceName,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                CompletedAt = x.CompletedAt,
                CompletedStatus = x.CompletedStatus,
                Notes = x.Notes
            }).ToList();
        }
    }
}
