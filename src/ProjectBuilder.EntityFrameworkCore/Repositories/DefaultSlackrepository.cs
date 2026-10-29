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
    public class DefaultSlackrepository : EfCoreRepository<ProjectBuilderDbContext, DefaultSlack, string>, IDefaultSlackrepository
    {
        private readonly ProjectBuilderDbContext _context;

        public DefaultSlackrepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }

        public async Task<DefaultSlack> FindAsync(string assettype)
        {
            return await _context.DefaultSlacksValues
                .Where(c => c.Id == assettype)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteDefaultSlackAsync(string assettype)
        {
            var defaultSlack = await _context.DefaultSlacksValues.FindAsync(assettype);

            if (defaultSlack == null)
            {
                return false;
            }

            _context.DefaultSlacksValues.Remove(defaultSlack);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<DefaultSlack> UpdateDefaultSlackAsync(Dictionary<string, object> pairs, string id)
        {
            var defaultSlack = await _context.DefaultSlacksValues
                .FirstOrDefaultAsync(c => c.Id == id);

            if (defaultSlack == null)
            {
                throw new UserFriendlyException("Default Slack record not found.");
            }

            foreach (var pair in pairs)
            {
                var property = defaultSlack.GetType().GetProperty(pair.Key);
                if (property != null && property.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(pair.Value, property.PropertyType);
                    property.SetValue(defaultSlack, convertedValue);
                }
            }
            await _context.SaveChangesAsync();
            return defaultSlack;
        }
    }
}
