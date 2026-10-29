using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IDefaultSlackrepository : IRepository<DefaultSlack>
    {
        Task<DefaultSlack> FindAsync(string assettype);
        Task<bool> DeleteDefaultSlackAsync(string assettype);
        Task<DefaultSlack> UpdateDefaultSlackAsync(Dictionary<string, object> pairs, string id);
    }
}
