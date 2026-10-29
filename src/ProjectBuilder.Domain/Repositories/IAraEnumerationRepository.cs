using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IAraEnumerationRepository : IRepository<AraEnumerations>
    {
        List<AraEnumerations> GetList();
        IQueryable<AraEnumerations> GetQueryable();
        Task<bool> UpdateAraEnumerationAsync(string enumFamily, string originalEnumName, string enumName, int enumInt);
        Task<bool> DeleteAraEnumerationAsync(string enumFamily, string enumName);
    }
}
