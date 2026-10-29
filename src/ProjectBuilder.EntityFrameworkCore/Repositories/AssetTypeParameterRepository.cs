
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class AssetTypeParameterRepository: EfCoreRepository<ProjectBuilderDbContext, AssetTypeParameterDataEntity, Guid>, IAssetTypeParameterRepository
    {
        public AssetTypeParameterRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider)
       : base(dbContextProvider)
        {
        }
    }
}
