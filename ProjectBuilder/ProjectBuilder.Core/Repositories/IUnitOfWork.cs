using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken token = default);
        public bool IsPending { get;}

        event Action<ErrorEventArgs> ErrorOccured;
        void ClearPendingOperations();
    }
}
