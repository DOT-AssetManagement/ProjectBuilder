using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class BridgeToPavementsRepository : EfCoreRepository<ProjectBuilderDbContext, BridgeToPavements, string>, IBridgeToPavementsRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public BridgeToPavementsRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ILogger<EfCoreRepository<ProjectBuilderDbContext, BridgeToPavements, string>> logger, IConfiguration configuration, ProjectBuilderDbContext dbContext)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
        }
        public List<BridgeToPavements> GetList()
        {
            return GetListAsync().Result;
        }

        public IQueryable<BridgeToPavements> GetQueryable()
        {
            return GetQueryableAsync().Result;
        }
    }
}
