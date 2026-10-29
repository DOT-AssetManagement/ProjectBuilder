using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Repositories
{
    public interface ICustomRepository<TModel> : IDisposable where TModel : class
    {
        Task<List<TModel>> GetAllAsync(CancellationToken token = default);
        void ApplyFilter(Dictionary<string, object> propertyValuePairs);
        void ApplyFilter(TModel filter);
    }
}
