using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class ParametersRepository : EfCoreRepository<ProjectBuilderDbContext, ParameterEntity, string>, IParametersRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public ParametersRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ILogger<EfCoreRepository<ProjectBuilderDbContext, ParameterEntity, string>> logger, IConfiguration configuration, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }
        public async Task<ParameterEntity> FindAsync(string parameterId,string parmfamily)
        {
            return await _context.Parameters
                .Where(c => c.Id == parameterId && c.Parmfamily == parmfamily)
                .FirstOrDefaultAsync();
        }
        public async Task<ParameterEntity> UpdateAsync(Dictionary<string, object> pairs, string parameterId, string parmfamily)
        {
            var parameter = await _context.Parameters
                .Where(c => c.Id == parameterId && c.Parmfamily == parmfamily)
                .FirstOrDefaultAsync();

            if (parameter != null)
            {
                foreach (var pair in pairs)
                {
                    var property = parameter.GetType().GetProperty(pair.Key);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(parameter, pair.Value);
                    }
                }

                // Save changes after updating the properties
                await _context.SaveChangesAsync();
            }

            return parameter; 
        }
        public async Task<bool> DeleteAsync(string parameterId, string parmfamily)
        {

            var parameter = await _context.Parameters.FindAsync(parameterId, parmfamily);

            if (parameter == null)
            {
                return false;
            }

            _context.Parameters.Remove(parameter);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
