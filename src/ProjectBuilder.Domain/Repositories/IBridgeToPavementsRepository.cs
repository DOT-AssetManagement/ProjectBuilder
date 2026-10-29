using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IBridgeToPavementsRepository : IRepository<BridgeToPavements>
    {
        List<BridgeToPavements> GetList();
        IQueryable<BridgeToPavements> GetQueryable();
    }
}
