using ProjectBuilder.Models.VisualizationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using static ProjectBuilder.Models.Charts.BridgeNeedsDto;

namespace ProjectBuilder.Repositories
{
    public interface IChartsNeedsRepository
    {
        public ICustomRepository<ProjectBuilder.Models.VisualizationModels.AllNeedsModel> AllNeedsRepo { get; set; }
        public ICustomRepository<ProjectBuilder.Models.VisualizationModels.BridgeNeedsModel> BridgeNeedsRepo { get; set; }
        public ICustomRepository<ProjectBuilder.Models.VisualizationModels.PavementNeedsModel> PavementNeedsRepo { get; set; }
    }
    public interface IChartsPotentialBenefitsUnitOfWork
    {
        public ICustomRepository<AllPotentialBenefitsModel> AllPotentialBenefitsRepo { get; set; }
        public ICustomRepository<BridgePotentialBenefitsModel> BridgePotentialBenefitsRepo { get; set; }
        public ICustomRepository<PavementPotentialBenefitsModel> PavementPotentialBenefitsRepo { get; set; }
    }

}
