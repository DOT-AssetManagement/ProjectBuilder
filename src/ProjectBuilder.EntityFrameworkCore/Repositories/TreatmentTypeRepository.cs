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
    public class TreatmentTypeRepository : EfCoreRepository<ProjectBuilderDbContext, TreatmentType, string>, ITreatmentTypeRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public TreatmentTypeRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }

        public async Task<TreatmentType> FindAssetType(string id)
        {
            return await _context.TreatmentTypes.Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<TreatmentType> UpdateTreatmnetTypeAsync(Dictionary<string, object> pairs, string id)
        {
            var treatmentType = await _context.TreatmentTypes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (treatmentType == null)
            {
                throw new UserFriendlyException("TreatmentType record not found.");
            }

            foreach (var pair in pairs)
            {
                var property = treatmentType.GetType().GetProperty(pair.Key);
                if (property != null && property.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(pair.Value, property.PropertyType);
                    property.SetValue(treatmentType, convertedValue);
                }
            }

            await _context.SaveChangesAsync();
            return treatmentType;
        }
    }
}
