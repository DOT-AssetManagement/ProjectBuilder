using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.AraEnumerations.Dtos;
using ProjectBuilder.Models.DefaultSlack;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Services
{
    public class DefaultSlackAppService : ApplicationService, IDefaultSlackAppService
    {
        private readonly IDefaultSlackrepository _defaultSlackrepository;
        private readonly ProjectBuilderMapper _mapper;

        public DefaultSlackAppService(IDefaultSlackrepository defaultSlackrepository, ProjectBuilderMapper mapper)
        {
            _defaultSlackrepository = defaultSlackrepository;
            _mapper = mapper;
        }
        public async Task<PaginatedDto<DefaultSlackDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var defaultSlacks = await _defaultSlackrepository.GetQueryableAsync();
            var displayResult = defaultSlacks
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();
            var totalRecords = defaultSlacks.Count();

            var mappedResults = _mapper.Map(displayResult);
            return new PaginatedDto<DefaultSlackDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };

        }
        public async Task<DefaultSlackDto> GetDefaultSlack(string AssetType)
        {
            var defaultSlack = await _defaultSlackrepository.FindAsync(AssetType);
            var defaultSlackViewModel = new DefaultSlackDto()
            {
                Id = defaultSlack.Id,
                MoveAfter = defaultSlack.MoveAfter,
                MoveBefore = defaultSlack.MoveBefore
            };  
            return defaultSlackViewModel;
        }
        public async Task<DefaultSlackDto> EditDefaultSlack(UpdateDefaultSlackDto model)
        {
            if (model.MoveBefore == null || model.MoveAfter == null)
            {
                throw new UserFriendlyException("The provided information is not valid to edit a county.");
            }

            var pairs = new Dictionary<string, object>
            {
                { nameof(model.MoveAfter), model.MoveAfter },
                { nameof(model.MoveBefore), model.MoveBefore }
            };

            await _defaultSlackrepository.UpdateDefaultSlackAsync(pairs, model.Id);
            var updatedEntity = await _defaultSlackrepository.FindAsync(model.Id);
            var result = _mapper.Map(updatedEntity);
            return result;

        }
        public async Task<bool> DeleteDefaultSlack(string AssetType)
        {
            return await _defaultSlackrepository.DeleteDefaultSlackAsync(AssetType);
        }
        public async Task<List<DefaultSlackDto>> GetAllAsync()
        {
            var items = await _defaultSlackrepository.GetQueryableAsync();
            return items.Select(x => new DefaultSlackDto
            {
                Id = x.Id,
                MoveBefore = x.MoveBefore,
                MoveAfter = x.MoveAfter
            }).ToList();
        }
    }
}
