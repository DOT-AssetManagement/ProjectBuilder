using Microsoft.EntityFrameworkCore;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class TreatmentParameterRepositorty : EfCoreRepository<ProjectBuilderDbContext, TreatmentParameterEntity, int>, ITreatmentParameterRepositorty
    {
        private readonly ProjectBuilderDbContext _context;

        public TreatmentParameterRepositorty(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }

        public async Task<TreatmentParameterEntity> FindTreatmentParameterAsync(int id)
        {
            return await _context.TreatmentParameters
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteTreatmentParameter(int id)
        {
            var treatmentParameter = await _context.TreatmentParameters.FindAsync(id);

            if (treatmentParameter == null)
            {
                return false;
            }

            _context.TreatmentParameters.Remove(treatmentParameter);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task UpdateDeTreatmentParameter(Dictionary<string, object> pairs, int id)
        {
            var treatmentParameter = await _context.TreatmentParameters
                .FirstOrDefaultAsync(t => t.Id == id);

            if (treatmentParameter == null)
            {
                throw new UserFriendlyException("Default Slack record not found.");
            }

            foreach (var pair in pairs)
            {
                var property = treatmentParameter.GetType().GetProperty(pair.Key);
                if (property != null && property.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(pair.Value, property.PropertyType);
                    property.SetValue(treatmentParameter, convertedValue);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
