using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class CountyRepository : EfCoreRepository<ProjectBuilderDbContext, CountyEntity, int?>, ICountyRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public CountyRepository(
            IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext,
            ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }
        public async Task<CountyEntity> FindAsync(int? district, int? id)
        {
            return await _context.Counties
                .Where(c => c.District == district && c.Id == id)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> DeleteCountyAsync(int countyId, int district)
        {

            var county = await _context.Counties.FindAsync(district,countyId);

            if (county == null)
            {
                return false;
            }

            _context.Counties.Remove(county);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<CountyEntity> UpdateCountyAsync(Dictionary<string, object> updateValues, int? district, int? countyId)
        {
            var county = await _context.Counties
                .Where(c => c.District == district && c.Id == countyId)
                .FirstOrDefaultAsync();

            if (county != null)
            {
                foreach (var pair in updateValues)
                {
                    var property = county.GetType().GetProperty(pair.Key);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(county, pair.Value);
                    }
                }
                await _context.SaveChangesAsync();
            }
            return county;
        }
        public async Task<List<CountyEntity>> GetAllDistricts()
        {
            var districts = await _context.Counties
                                           .AsNoTracking()
                                           .Select(c => c.District)
                                           .Distinct()
                                           .ToListAsync();

            return districts.Select(d => new CountyEntity { District = d }).ToList();
        }

        public async Task<List<CountyEntity>> GetCountiesByDistrict(int id)
        {
            return await _context.Counties
                                 .AsNoTracking()
                                 .Where(c => c.District == id)
                                 .ToListAsync();
        }

        public async Task<List<CountyEntity>> GetCounties()
        {
            return await _context.Counties
                                 .AsNoTracking()
                                 .ToListAsync();
        }
    }
}
