
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

public class GoalRepository : EfCoreRepository<ProjectBuilderDbContext, GoalEntity, Guid>, IGoalRepository
{
    public GoalRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
