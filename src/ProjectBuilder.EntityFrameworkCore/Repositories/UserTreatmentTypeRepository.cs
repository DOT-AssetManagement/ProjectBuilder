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
    public class UserTreatmentTypeRepository : EfCoreRepository<ProjectBuilderDbContext, UserTreatmentType, int>, IUserTreatmentTypeRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public UserTreatmentTypeRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }
        public async Task<UserTreatmentType> FindAsync(int id)
        {
            return await _context.UserTreatmentTypes
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }
        public async Task UpdateUserTreatmentTypeAsync(Dictionary<string, object> pairs, int id)
        {
            var userTreatment = await _context.UserTreatmentTypes
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userTreatment == null)
            {
                throw new UserFriendlyException("UserTreatmentType record not found.");
            }

            foreach (var pair in pairs)
            {
                var property = userTreatment.GetType().GetProperty(pair.Key);
                if (property != null && property.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(pair.Value, property.PropertyType);
                    property.SetValue(userTreatment, convertedValue);
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteAsync(int id)
        {

            var userTreatment = await _context.UserTreatmentTypes.FindAsync(id);

            if (userTreatment == null)
            {
                return false;
            }

            _context.UserTreatmentTypes.Remove(userTreatment);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
