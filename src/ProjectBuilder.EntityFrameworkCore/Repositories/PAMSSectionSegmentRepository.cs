using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System.Linq;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class PAMSSectionSegmentRepository : EfCoreRepository<ProjectBuilderDbContext, PAMSSectionSegment, string>, IPAMSSectionSegmentRepository
    {
        public PAMSSectionSegmentRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public IQueryable<PAMSSectionSegment> GetQueryable()
        {
            return GetQueryableAsync().Result;
        }
    }
}
