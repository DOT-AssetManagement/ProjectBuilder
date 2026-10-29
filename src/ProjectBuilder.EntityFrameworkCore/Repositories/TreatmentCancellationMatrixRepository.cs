using Microsoft.EntityFrameworkCore;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class TreatmentCancellationMatrixRepository : EfCoreRepository<ProjectBuilderDbContext, TreatmentCancellationMatrixEntity, string>, ITreatmentCancellationMatrixRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public TreatmentCancellationMatrixRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }

        public async Task<TreatmentCancellationMatrixEntity> FindAsync(char assettype, string treatment)
        {
            return await _context.TreatmentCancellationMatrix
                .Where(t => t.AssetTypeA == assettype && t.TreatmentB == treatment)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Dictionary<string, object> pairs, char assettype, string id)
        {
            var treatment = await _context.TreatmentCancellationMatrix
                .Where(c => c.AssetTypeA == assettype && c.Id == id)
                .FirstOrDefaultAsync();

            if (treatment != null)
            {
                foreach (var pair in pairs)
                {
                    var property = treatment.GetType().GetProperty(pair.Key);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(treatment, pair.Value);
                    }
                }
            }
        }

        public async Task<bool> DeleteAsync(TreatmentCancellationMatrixDto input)
        {
            var treatment = await _context.TreatmentCancellationMatrix.FindAsync(input.AssetTypeA, input.TreatmentB);

            if (treatment == null)
            {
                return false;
            }

            _context.TreatmentCancellationMatrix.Remove(treatment);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
