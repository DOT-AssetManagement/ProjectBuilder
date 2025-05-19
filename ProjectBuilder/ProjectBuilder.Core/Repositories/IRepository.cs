using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface IRepository<TModel> : IDisposable where TModel : class
    {
        Task<List<TModel>> GetAllAsync(CancellationToken token = default);
        Task<List<TModel>> GetRangeAsync(int startIndex, int count, CancellationToken token = default);
        void Insert(TModel model);
        Task<TModel> InsertAndSave(TModel model);
        Task UpdateAsync<Id>(Id modelId,Dictionary<string,object> propertiesValuesPairs);
        Task UpdateAsync(TModel newValue, params string[] properties);
        Task UpdateAsync(Dictionary<string, object> propertiesValuesPairs, params object[] parameters);
        Task<bool> DeleteAsync<Id>(Id modelId, CancellationToken token = default);
        Task<bool> DeleteAsync(params object[]parameters);
        Task<int> DeleteAsync(TModel model);
        Task SaveChangesAsync(CancellationToken token = default);
        Task<TModel> FindAsync(params object[] parameters);
        Task<TModel> FindAsync(CancellationToken token, params object[] parameters);
        void ClearPendingChanges();
        Task Refresh();
        bool IsPending { get; }
        Task<long> GetCountAsync(CancellationToken token = default);
        void ApplyFilter(Dictionary<string, object> propertyValuePairs);
        void ApplyFilter(TModel filter);

        event Action<ErrorEventArgs> ErrorOccured;
    }
}
