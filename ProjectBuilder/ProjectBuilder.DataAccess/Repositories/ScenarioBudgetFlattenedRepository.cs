using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class ScenarioBudgetFlattenedRepository : ProjectBuilderRepository<ScenarioBudget, ScenarioBudgetFlatModel, int>
    {
        private IQueryable<ScenarioBudgetFlat> _currentQuery;
        public ScenarioBudgetFlattenedRepository(ProjectBuilderDbContext projectBuilderDbContext, IMapper mapper, 
                        ILogger<ProjectBuilderRepository<ScenarioBudget, ScenarioBudgetFlatModel, int>> logger) : base(projectBuilderDbContext, mapper, logger)
        {
            _currentQuery = InitializeScenarioBudgetQuery();
        }
        private IQueryable<ScenarioBudgetFlat> InitializeScenarioBudgetQuery(Expression<Func<ScenarioBudget,bool>> filter = null)
        {
            if (filter == null)
                filter = PredicateBuilder.New<ScenarioBudget>(true);
            return ProjectBuilderDbContext.ScenariosBudgets.Where(filter)
                                                           .OrderBy(b => b.EntityId)
                                                           .GroupBy(b => new { b.EntityId, b.District, b.ScenarioId }, (key, flatten) => new ScenarioBudgetFlat
                                                           {
                                                              EntityId = key.EntityId,
                                                              District = key.District,
                                                              ScenarioId = key.ScenarioId,
                                                              BridgeInterstateBudget = flatten.FirstOrDefault(b => b.AssetType == "B" && b.IsInterstate).Budget,
                                                              BridgeNonInterstateBudget = flatten.FirstOrDefault(b => b.AssetType == "B" && !b.IsInterstate).Budget,
                                                              PavementInterstateBudget = flatten.FirstOrDefault(b => b.AssetType == "P" && b.IsInterstate).Budget,
                                                              PavementNonInterstateBudget = flatten.FirstOrDefault(b => b.AssetType == "P" && !b.IsInterstate).Budget
                                                           });
        }
        public override async Task<List<ScenarioBudgetFlatModel>> GetRangeAsync(int startIndex, int count, CancellationToken token = default)
        {
            return await _currentQuery.Skip(startIndex).Take(count)
                                      .AsNoTracking()
                                      .AsExpandable()
                                      .ProjectTo<ScenarioBudgetFlatModel>(Mapper.ConfigurationProvider)
                                      .ToListAsync();
        }
        public override async Task<List<ScenarioBudgetFlatModel>> GetAllAsync(CancellationToken token = default)
        {
            return await _currentQuery.AsNoTracking()
                                      .AsExpandable()
                                      .ProjectTo<ScenarioBudgetFlatModel>(Mapper.ConfigurationProvider)
                                      .ToListAsync();
        }
        public override void ApplyFilter(Dictionary<string, object> propertyValuePairs)
        {
            _currentQuery = InitializeScenarioBudgetQuery();
            var expression = BuildExpression(propertyValuePairs);
            _currentQuery = InitializeScenarioBudgetQuery(expression);
        }
      
        public override async Task UpdateAsync(ScenarioBudgetFlatModel newValue ,params string[] properties)
        {
            bool isInterstate = false;
            string assetType = "B";
            switch (properties[0])
            {
                case nameof(newValue.BridgeInterstateBudget): 
                    isInterstate= true;
                    assetType= "B";
                    break;
                case nameof(newValue.BridgeNonInterstateBudget):
                    isInterstate = false;
                    assetType = "B";
                    break;
                case nameof(newValue.PavementInterstateBudget):
                    isInterstate = true;
                    assetType = "P";
                    break;
                case nameof(newValue.PavementNonInterstateBudget):
                    isInterstate = false;
                    assetType = "P";
                    break;
            }
            var target = await ProjectBuilderDbContext.ScenariosBudgets.FindAsync(newValue.ScenarioId,newValue.District,newValue.YearWork,isInterstate,assetType);
            var budgetvalue = newValue.GetType().GetProperty(properties[0]).GetValue(newValue) as decimal?;
            if(target is null)
            {
                RaiseOnErrorOccured("Could not find the updated in the database.", LogLevel.Error);
                return;
            }
            target.Budget = budgetvalue;        
        }
        public override async Task<int> DeleteAsync(ScenarioBudgetFlatModel targetbudget)
        {
           return await ProjectBuilderDbContext.ScenariosBudgets
                        .Where(b => b.District == targetbudget.District && b.ScenarioId == targetbudget.ScenarioId && b.EntityId == targetbudget.YearWork).ExecuteDeleteAsync();
        }
        public override async Task<ScenarioBudgetFlatModel> InsertAndSave(ScenarioBudgetFlatModel model)
        {
            var scenarioBudgets = new List<ScenarioBudget>
            {
                new ScenarioBudget
                {
                    District = model.District,
                    EntityId = model.YearWork,
                    ScenarioId = model.ScenarioId,
                    IsInterstate = true,
                    AssetType = "B",
                    Budget = model.BridgeInterstateBudget
                },
                new ScenarioBudget
                {
                    District = model.District,
                    EntityId = model.YearWork,
                    ScenarioId = model.ScenarioId,
                    IsInterstate = false,
                    AssetType = "B",
                    Budget = model.BridgeNonInterstateBudget
                },
                new ScenarioBudget
                {
                    District = model.District,
                    EntityId = model.YearWork,
                    ScenarioId = model.ScenarioId,
                    IsInterstate = true,
                    AssetType = "P",
                    Budget = model.PavementInterstateBudget
                },
                new ScenarioBudget
                {
                    District = model.District,
                    EntityId = model.YearWork,
                    ScenarioId = model.ScenarioId,
                    IsInterstate = false,
                    AssetType = "P",
                    Budget = model.PavementNonInterstateBudget
                }
            };

            foreach (var scenarioBudget in scenarioBudgets)
            {
                await ProjectBuilderDbContext.ScenariosBudgets.AddAsync(scenarioBudget);
            }
            await ProjectBuilderDbContext.SaveChangesAsync();

            return model;
        }
    }
}
