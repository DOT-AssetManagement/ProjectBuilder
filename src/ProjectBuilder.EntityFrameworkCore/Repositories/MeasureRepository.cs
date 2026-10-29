
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

public class MeasureRepository : EfCoreRepository<ProjectBuilderDbContext, MeasureEntity, Guid>, IMeasureRepository
{
    public MeasureRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
