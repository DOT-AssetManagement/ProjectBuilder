using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class ImportSessionsRepository : EfCoreRepository<ProjectBuilderDbContext, ImportSessions, Guid>, IImportSessionsRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public ImportSessionsRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }

        public IQueryable<ImportSessions> GetQueryable()
        {
            return GetQueryableAsync().Result;
        }

    }
}
