
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Users;

public class ImportSessionRepository : EfCoreRepository<ProjectBuilderDbContext, ImportSessions, Guid>, IImportSessionRepository
{
    private readonly string _connectionString = string.Empty;
    private readonly ICurrentUser _curruntUser;
    private readonly ProjectBuilderDbContext _dbContext;
    public ImportSessionRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider, IConfiguration configuration, ICurrentUser curruntUser, ProjectBuilderDbContext dbContext)
        : base(dbContextProvider)
    {
        _connectionString = configuration.GetConnectionString("Default");
        _curruntUser = curruntUser;
        _dbContext = dbContext;
    }

    public async Task<ImportSessions> GetByPoolId(Guid poolId)
    {
        var result = await _dbContext.ImportSessions.AsNoTracking()
        .FirstOrDefaultAsync(x => x.LibraryId == poolId);
        return result;
    }

}
