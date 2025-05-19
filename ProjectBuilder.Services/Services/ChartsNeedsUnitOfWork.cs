using ProjectBuilder.Core;
using ProjectBuilder.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Services
{
     public class ChartsNeedsUnitOfWork : IChartsNeedsUnitOfWork
    {
        public ChartsNeedsUnitOfWork(IRepository<AllNeedsModel> allNeedsRepo, IRepository<BridgeNeedsModel> bridgeNeedsRepo, 
                                     IRepository<PavementNeedsModel> pavementNeedsRepo)
        {
            AllNeedsRepo = allNeedsRepo;
            BridgeNeedsRepo = bridgeNeedsRepo;
            PavementNeedsRepo = pavementNeedsRepo;
        }

        public IRepository<AllNeedsModel> AllNeedsRepo { get; set; }
        public IRepository<BridgeNeedsModel> BridgeNeedsRepo { get; set; }
        public IRepository<PavementNeedsModel> PavementNeedsRepo { get; set; }
    }
    public class ChartsPotentialBenefitsUnitOfWork : IChartsPotentialBenefitsUnitOfWork
    {
        public ChartsPotentialBenefitsUnitOfWork(IRepository<AllPotentialBenefitsModel> allPotentialBenefitsRepo, 
                                                 IRepository<BridgePotentialBenefitsModel> bridgePotentialBenefitsRepo, 
                                                 IRepository<PavementPotentialBenefitsModel> pavementPotentialBenefitsRepo)
        {
            AllPotentialBenefitsRepo = allPotentialBenefitsRepo;
            BridgePotentialBenefitsRepo = bridgePotentialBenefitsRepo;
            PavementPotentialBenefitsRepo = pavementPotentialBenefitsRepo;
        }

        public IRepository<AllPotentialBenefitsModel> AllPotentialBenefitsRepo { get; set; }
        public IRepository<BridgePotentialBenefitsModel> BridgePotentialBenefitsRepo { get; set; }
        public IRepository<PavementPotentialBenefitsModel> PavementPotentialBenefitsRepo { get ; set; }
    }
}
