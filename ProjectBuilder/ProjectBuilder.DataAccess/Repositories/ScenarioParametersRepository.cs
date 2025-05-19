using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class ScenarioParametersRepository : ProjectBuilderRepository<ScenarioParamater, ScenarioParameterModel, string>
    {
        public ScenarioParametersRepository(ProjectBuilderDbContext projectBuilderDbContext, IMapper mapper, ILogger<ProjectBuilderRepository<ScenarioParamater, ScenarioParameterModel, string>> logger) 
                                     : base(projectBuilderDbContext, mapper, logger)
        {

        }
        public override async Task UpdateAsync(ScenarioParameterModel newValue, params string[] properties)
        {
            var newEntity = Mapper.Map<ScenarioParamater>(newValue);
            var target = await ProjectBuilderDbContext.ScenarioParameters.FindAsync(newValue.ScenarioId, newValue.ParameterId);
            if (target is null)
            {
                RaiseOnErrorOccured("Could not update the value in the database.", LogLevel.Error);
                return;
            }
            target.ParameterValue = newValue.ParameterValue;
        }
        protected override async Task<bool> HandleUpdatingConflicts(IReadOnlyList<EntityEntry> entries,CancellationToken token= default)
        {
            foreach (var entry in entries)
            {
                if (entry.Entity as ScenarioParamater is null)
                    continue;
                ProjectBuilderDbContext.ScenarioParameters.Add(entry.Entity as ScenarioParamater);               
            }
            try
            {
               await ProjectBuilderDbContext.SaveChangesAsync(token);
               return true;
            }
            catch
            {
               return false;
            }
        }
    }
}
