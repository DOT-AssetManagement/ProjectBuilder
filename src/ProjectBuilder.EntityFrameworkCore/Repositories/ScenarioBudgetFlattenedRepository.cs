using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;


namespace ProjectBuilder.Repositories
{
    public class ScenarioBudgetFlattenedRepository : EfCoreRepository<ProjectBuilderDbContext, ScenarioBudget, int>, IScenarioBudgetFlattenedRepository
    {
        private IQueryable<ScenarioBudgetFlat> _currentQuery;
        private readonly ProjectBuilderDbContext _context;
        public ScenarioBudgetFlattenedRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext,
                        ILogger<EfCoreRepository<ProjectBuilderDbContext, ScenarioBudget, int>> logger, ProjectBuilderDbContext dbContext) : base(projectBuilderDbContext)
        {
            _context = dbContext;
            _currentQuery = InitializeScenarioBudgetQuery();
        }
        //private IQueryable<ScenarioBudgetFlat> InitializeScenarioBudgetQuery(Expression<Func<ScenarioBudget, bool>> filter = null)
        //{
        //    if (filter == null)
        //        filter = PredicateBuilder.New<ScenarioBudget>(true);
        //    return _context.ScenariosBudgets.Where(filter)
        //                                                   .OrderBy(b => b.Id)
        //                                                   .GroupBy(b => new { b.Id, b.District, b.ScenarioId }, (key, flatten) => new ScenarioBudgetFlat
        //                                                   {
        //                                                       Id = key.Id,
        //                                                       District = key.District,
        //                                                       ScenarioId = key.ScenarioId,
        //                                                       BridgeInterstateBudget = flatten.FirstOrDefault(b => b.AssetType == "B" && b.IsInterstate).Budget,
        //                                                       BridgeNonInterstateBudget = flatten.FirstOrDefault(b => b.AssetType == "B" && !b.IsInterstate).Budget,
        //                                                       PavementInterstateBudget = flatten.FirstOrDefault(b => b.AssetType == "P" && b.IsInterstate).Budget,
        //                                                       PavementNonInterstateBudget = flatten.FirstOrDefault(b => b.AssetType == "P" && !b.IsInterstate).Budget
        //                                                   });
        //}

        private IQueryable<ScenarioBudgetFlat> InitializeScenarioBudgetQuery(Expression<Func<ScenarioBudget, bool>> filter = null)
        {
            if (filter == null)
                filter = PredicateBuilder.New<ScenarioBudget>(true);

            return _context.ScenariosBudgets
                .Where(filter)
                .OrderBy(b => b.Id)
                .GroupBy(
                    b => new { b.Id, b.District, b.ScenarioId },
                    (key, flatten) => new ScenarioBudgetFlat
                    {
                        Id = key.Id,
                        District = key.District,
                        ScenarioId = key.ScenarioId,
                        BridgeInterstateBudget = flatten
                            .Where(b => b.AssetType == "B" && b.IsInterstate)
                            .Select(b => (decimal?)b.Budget)
                            .FirstOrDefault(),
                        BridgeNonInterstateBudget = flatten
                            .Where(b => b.AssetType == "B" && !b.IsInterstate)
                            .Select(b => (decimal?)b.Budget)
                            .FirstOrDefault(),
                        PavementInterstateBudget = flatten
                            .Where(b => b.AssetType == "P" && b.IsInterstate)
                            .Select(b => (decimal?)b.Budget)
                            .FirstOrDefault(),
                        PavementNonInterstateBudget = flatten
                            .Where(b => b.AssetType == "P" && !b.IsInterstate)
                            .Select(b => (decimal?)b.Budget)
                            .FirstOrDefault()
                    });
        }
        public virtual async Task<List<ScenarioBudgetFlat>> GetRangeAsync(int startIndex, int count, CancellationToken token = default)
        {
            return await _currentQuery.Skip(startIndex).Take(count)
                                      .AsNoTracking()
                                      //.AsExpandable()
                                      //.ProjectTo<ScenarioBudgetFlatModel>(Mapper.ConfigurationProvider)
                                      .ToListAsync();
        }
        public async Task<List<ScenarioBudgetFlat>> GetAllAsync(CancellationToken token = default)
        {
            return await _currentQuery.AsNoTracking()
                                      //.AsExpandable()
                                      //.ProjectTo<ScenarioBudgetFlatModel>(Mapper.ConfigurationProvider)
                                      .ToListAsync();
        }
        //public  void ApplyFilter(Dictionary<string, object> propertyValuePairs)
        //{
        //    _currentQuery = InitializeScenarioBudgetQuery();
        //    var expression = BuildExpression(propertyValuePairs);
        //    _currentQuery = InitializeScenarioBudgetQuery(expression);
        //}

        public  async Task UpdateAsync(ScenarioBudgetFlatModel newValue, params string[] properties)
        {
            bool isInterstate = false;
            string assetType = "B";
            switch (properties[0])
            {
                case nameof(newValue.BridgeInterstateBudget):
                    isInterstate = true;
                    assetType = "B";
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
            var target = await _context.ScenariosBudgets.FindAsync(newValue.ScenarioId, newValue.District, newValue.YearWork, isInterstate, assetType);
            var budgetvalue = newValue.GetType().GetProperty(properties[0]).GetValue(newValue) as decimal?;
            if (target is null)
            {
               // RaiseOnErrorOccured("Could not find the updated in the database.", LogLevel.Error);
                return;
            }
            target.Budget = budgetvalue;
        }
        public  async Task<int> DeleteAsync(ScenarioBudgetFlat targetbudget)
        {
            return await _context.ScenariosBudgets
                         .Where(b => b.District == targetbudget.District && b.ScenarioId == targetbudget.ScenarioId && b.Id == targetbudget.YearWork).ExecuteDeleteAsync();
        }
        public  async Task<ScenarioBudgetFlatModel> InsertAndSave(ScenarioBudgetFlatModel model)
        {
            var scenarioBudgets = new List<ScenarioBudget>
            {
                new ScenarioBudget
                {
                    District = model.District,
                    Id = model.YearWork,
                    ScenarioId = model.ScenarioId,
                    IsInterstate = true,
                    AssetType = "B",
                    Budget = model.BridgeInterstateBudget
                },
                new ScenarioBudget
                {
                    District = model.District,
                    Id = model.YearWork,
                    ScenarioId = model.ScenarioId,
                    IsInterstate = false,
                    AssetType = "B",
                    Budget = model.BridgeNonInterstateBudget
                },
                new ScenarioBudget
                {
                    District = model.District,
                    Id = model.YearWork,
                    ScenarioId = model.ScenarioId,
                    IsInterstate = true,
                    AssetType = "P",
                    Budget = model.PavementInterstateBudget
                },
                new ScenarioBudget
                {
                    District = model.District,
                    Id = model.YearWork,
                    ScenarioId = model.ScenarioId,
                    IsInterstate = false,
                    AssetType = "P",
                    Budget = model.PavementNonInterstateBudget
                }
            };

            foreach (var scenarioBudget in scenarioBudgets)
            {
                await _context.ScenariosBudgets.AddAsync(scenarioBudget);
            }
            await _context.SaveChangesAsync();

            return model;
        }
        protected Expression<Func<ScenarioBudget, bool>> BuildExpression(Dictionary<string, object> propertyValuePairs)
        {
            var expression = PredicateBuilder.New<ScenarioBudget>(true);
            if (propertyValuePairs is null || propertyValuePairs.Count < 1)
                return expression;
            var entityType = typeof(Entity);
            var parameter = Expression.Parameter(typeof(Entity), "e");
            foreach (var property in propertyValuePairs.Keys)
            {
                var value = propertyValuePairs[property];
                var propertyExpression = Expression.PropertyOrField(parameter, property);
                var pType = entityType.GetProperty(property).PropertyType;
                var predicate = Expression.Lambda<Func<ScenarioBudget, bool>>(Expression.Equal(propertyExpression, Expression.Convert(Expression.Constant(value), pType)), parameter);
                expression = PredicateBuilder.And(expression, predicate);
            }
            return expression;
        }
    }
}
