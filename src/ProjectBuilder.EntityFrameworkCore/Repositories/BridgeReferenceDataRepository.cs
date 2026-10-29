using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System.Linq;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class BridgeReferenceDataRepository : EfCoreRepository<ProjectBuilderDbContext, BridgeReferenceData, string>, IBridgeReferenceDataRepository
    {
        public BridgeReferenceDataRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public IQueryable<BridgeReferenceData> GetQueryable()
        {
            return GetQueryableAsync().Result;
        }
    }
}
