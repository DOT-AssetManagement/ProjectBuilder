using ProjectBuilder.Models.VisualizationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public class ChartsNeedsRepository : IChartsNeedsRepository, IChartsPotentialBenefitsUnitOfWork
    {
        public ChartsNeedsRepository(ICustomRepository<AllNeedsModel> allNeedsRepo, ICustomRepository<BridgeNeedsModel> bridgeNeedsRepo,
                                   ICustomRepository<PavementNeedsModel> pavementNeedsRepo)
        {
            AllNeedsRepo = allNeedsRepo;
            BridgeNeedsRepo = bridgeNeedsRepo;
            PavementNeedsRepo = pavementNeedsRepo;
        }
        public ICustomRepository<AllNeedsModel> AllNeedsRepo { get; set; }
        public ICustomRepository<BridgeNeedsModel> BridgeNeedsRepo { get; set; }
        public ICustomRepository<PavementNeedsModel> PavementNeedsRepo { get; set; }
        public ChartsNeedsRepository(ICustomRepository<AllPotentialBenefitsModel> allPotentialBenefitsRepo,
                                                ICustomRepository<BridgePotentialBenefitsModel> bridgePotentialBenefitsRepo,
                                                ICustomRepository<PavementPotentialBenefitsModel> pavementPotentialBenefitsRepo)
        {
            AllPotentialBenefitsRepo = allPotentialBenefitsRepo;
            BridgePotentialBenefitsRepo = bridgePotentialBenefitsRepo;
            PavementPotentialBenefitsRepo = pavementPotentialBenefitsRepo;
        }
        public ICustomRepository<AllPotentialBenefitsModel> AllPotentialBenefitsRepo { get; set; }
        public ICustomRepository<BridgePotentialBenefitsModel> BridgePotentialBenefitsRepo { get; set; }
        public ICustomRepository<PavementPotentialBenefitsModel> PavementPotentialBenefitsRepo { get; set; }
    }

}
