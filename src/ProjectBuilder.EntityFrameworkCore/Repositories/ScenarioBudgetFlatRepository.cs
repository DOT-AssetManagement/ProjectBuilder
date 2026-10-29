using AutoMapper;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class ScenarioBudgetFlatRepository : EfCoreRepository<ProjectBuilderDbContext, ScenarioBudgetFlat, int>, IScenarioBudgetFlatRepository
    {
        private readonly ProjectBuilderDbContext _context;
        public ScenarioBudgetFlatRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext,
                        ILogger<EfCoreRepository<ProjectBuilderDbContext, ScenarioBudget, int>> logger, ProjectBuilderDbContext dbContext) : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }
        public async Task UpdateFieldsAsync(ScenarioBudgetFlat entity, params string[] updatedProperties)
        {
            var dbEntity = await _context.ScenariosBudgetFalts.FindAsync(entity.Id);
            if (dbEntity == null)
            {
                throw new BusinessException("Scenario budget entry not found.");
            }

            foreach (var property in updatedProperties)
            {
                var entityProperty = typeof(ScenarioBudgetFlat).GetProperty(property);
                if (entityProperty != null)
                {
                    entityProperty.SetValue(dbEntity, entityProperty.GetValue(entity));
                    DbContext.Entry(dbEntity).Property(property).IsModified = true;
                }
            }

            await _context.SaveChangesAsync();
        }

    }
}
