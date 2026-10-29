using ProjectBuilder.Entities;
using System.Linq;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IPAMSSectionSegmentRepository : IRepository<PAMSSectionSegment, string>
    {
        IQueryable<PAMSSectionSegment> GetQueryable();
    }
}
