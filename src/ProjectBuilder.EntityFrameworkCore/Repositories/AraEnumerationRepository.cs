using Microsoft.EntityFrameworkCore;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class AraEnumerationRepository : EfCoreRepository<ProjectBuilderDbContext, AraEnumerations, string>, IAraEnumerationRepository
    {
        private readonly ProjectBuilderDbContext _context;
   
        public AraEnumerationRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }

        public List<AraEnumerations> GetList()
        {
            return GetListAsync().Result;
        }

        public IQueryable<AraEnumerations> GetQueryable()
        {
            return GetQueryableAsync().Result;
        }

        // AraEnumerations is mapped HasNoKey() (its real primary key is the composite
        // EnumFamily+EnumName, which ABP's Entity<string> can't represent), so it can't be
        // change-tracked via the DbSet — mutations go straight to the base table with raw SQL.
        public async Task<bool> UpdateAraEnumerationAsync(string enumFamily, string originalEnumName, string enumName, int enumInt)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE tbl_ara_Enumerations
SET EnumName = {enumName}, EnumInt = {enumInt}
WHERE EnumFamily = {enumFamily} AND EnumName = {originalEnumName}");
            return rows > 0;
        }

        public async Task<bool> DeleteAraEnumerationAsync(string enumFamily, string enumName)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM tbl_ara_Enumerations
WHERE EnumFamily = {enumFamily} AND EnumName = {enumName}");
            return rows > 0;
        }
    }
}
