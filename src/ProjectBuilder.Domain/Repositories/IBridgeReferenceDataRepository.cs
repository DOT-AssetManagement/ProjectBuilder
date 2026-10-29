using ProjectBuilder.Entities;
using System.Linq;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IBridgeReferenceDataRepository : IRepository<BridgeReferenceData, string>
    {
        IQueryable<BridgeReferenceData> GetQueryable();
    }
}
