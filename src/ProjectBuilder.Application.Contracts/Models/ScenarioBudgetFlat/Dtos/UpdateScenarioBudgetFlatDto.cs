using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ScenarioBudgetFlat.Dtos
{
    public class UpdateScenarioBudgetFlatDto : EntityDto<int>
    {
        public int ScenarioId { get; set; }
        public int District { get; set; }
        public decimal? BridgeInterstateBudget { get; set; }
        public decimal? BridgeNonInterstateBudget { get; set; }
        public decimal? PavementInterstateBudget { get; set; }
        public decimal? PavementNonInterstateBudget { get; set; }
    }
}
