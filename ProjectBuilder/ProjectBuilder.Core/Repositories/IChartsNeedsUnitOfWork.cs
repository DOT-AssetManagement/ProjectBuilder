using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core.Repositories
{
    public interface IChartsNeedsUnitOfWork
    {
        public IRepository<AllNeedsModel> AllNeedsRepo { get; set; }
        public IRepository<BridgeNeedsModel> BridgeNeedsRepo { get; set; }
        public IRepository<PavementNeedsModel> PavementNeedsRepo { get; set; }
    }
    public interface IChartsPotentialBenefitsUnitOfWork
    {
        public IRepository<AllPotentialBenefitsModel> AllPotentialBenefitsRepo { get; set; }
        public IRepository<BridgePotentialBenefitsModel> BridgePotentialBenefitsRepo { get; set; }
        public IRepository<PavementPotentialBenefitsModel> PavementPotentialBenefitsRepo { get; set; }
    }
}
