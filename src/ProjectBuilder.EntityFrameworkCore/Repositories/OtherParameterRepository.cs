
using Microsoft.EntityFrameworkCore;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Repositories;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DSS.Repositories
{
    public class OtherParameterRepository : EfCoreRepository<ProjectBuilderDbContext, OtherParameterEntity>, IOtherParameterRepository
    {
        private readonly ProjectBuilderDbContext _dbContext;
        public OtherParameterRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider, ProjectBuilderDbContext dbContext)
            : base(dbContextProvider)
        {
            _dbContext = dbContext;
        }
        public async Task<OtherParameterEntity> GetAsync(string code)
        {
            return await _dbContext.Set<OtherParameterEntity>()
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(op => op.Id == code);
        }

        public async Task<OtherParameterEntity> UpdateAsync(OtherParameterEntity entity)
        {
            try
            {
                _dbContext.Update(entity);
                await _dbContext.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw;
            }
        }      
    }
}
