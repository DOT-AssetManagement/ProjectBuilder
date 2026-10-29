using Microsoft.EntityFrameworkCore;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Repositories;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DSS.Repositories
{
    public class ParameterRepository : EfCoreRepository<ProjectBuilderDbContext, ParameterEntity>, IParameterRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public ParameterRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider, ProjectBuilderDbContext dbContext)
            : base(dbContextProvider)
        {
            _context = dbContext;
        }

        public async Task<ParameterEntity> GetAsync(string id, string parmfamily)
        {
            return await _context.Parameters
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.Parmfamily == parmfamily);
        }

        public async Task<ParameterEntity> UpdateAsync(string id, string parmfamily, string parmName, string parmDescription, double? defaultValue)
        {
            var entity = await _context.Parameters
                .FirstOrDefaultAsync(p => p.Id == id && p.Parmfamily == parmfamily);

            if (entity != null)
            {
                entity.ParmName = parmName;
                entity.ParmDescription = parmDescription;
                entity.DefaultValue = defaultValue;
                await _context.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<bool> DeleteAsync(string id, string parmfamily)
        {
            var entity = await _context.Parameters
                .FirstOrDefaultAsync(p => p.Id == id && p.Parmfamily == parmfamily);

            if (entity == null) return false;

            _context.Parameters.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
