using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IScenarioBudgetFlatRepository  : IRepository<ScenarioBudgetFlat>
    {
        Task UpdateFieldsAsync(ScenarioBudgetFlat entity, params string[] updatedProperties);
    }
}
