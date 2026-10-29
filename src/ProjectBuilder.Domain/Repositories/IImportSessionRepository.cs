
using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

public interface IImportSessionRepository : IBasicRepository<ImportSessions, Guid>
{
    Task<ImportSessions> GetByPoolId(Guid poolName);
}
