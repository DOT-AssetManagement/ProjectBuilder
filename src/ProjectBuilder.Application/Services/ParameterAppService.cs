using ProjectBuilder.Entities;
using ProjectBuilder.Models.ParameterEntity;
using ProjectBuilder.Models.ParameterEntity.Dtos;
using ProjectBuilder.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ProjectBuilder.Permissions;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    [Authorize(ConfigurationPermissions.ConfigurationPageActions.Default)]
    public class ParameterAppService : ApplicationService, IParameterAppService
    {
        private readonly IParameterRepository _repository;

        public ParameterAppService(IParameterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ParameterDto>> GetAllAsync()
        {
            var items = await _repository.GetQueryableAsync();
            return items.Select(x => new ParameterDto
            {
                Id           = x.Id,
                Parmfamily   = x.Parmfamily,
                ParmName     = x.ParmName,
                ParmDescription = x.ParmDescription,
                DefaultValue = x.DefaultValue
            }).ToList();
        }

        public async Task<ParameterDto> EditParameterAsync(string id, string parmfamily, string parmName, string parmDescription, double? defaultValue)
        {
            var updated = await _repository.UpdateAsync(id, parmfamily, parmName, parmDescription, defaultValue);
            if (updated == null) return null;
            return new ParameterDto
            {
                Id              = updated.Id,
                Parmfamily      = updated.Parmfamily,
                ParmName        = updated.ParmName,
                ParmDescription = updated.ParmDescription,
                DefaultValue    = updated.DefaultValue
            };
        }

        public async Task<bool> DeleteParameterAsync(string id, string parmfamily)
        {
            return await _repository.DeleteAsync(id, parmfamily);
        }
    }
}
