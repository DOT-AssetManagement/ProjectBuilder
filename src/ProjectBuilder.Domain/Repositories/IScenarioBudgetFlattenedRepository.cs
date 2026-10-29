using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IScenarioBudgetFlattenedRepository : IRepository<ScenarioBudget>
    {
        Task<List<ScenarioBudgetFlat>> GetRangeAsync(int startIndex, int count, CancellationToken token = default);
        Task<List<ScenarioBudgetFlat>> GetAllAsync(CancellationToken token = default);
        Task UpdateAsync(ScenarioBudgetFlatModel newValue, params string[] properties);
        Task<int> DeleteAsync(ScenarioBudgetFlat targetbudget);
    }
}
