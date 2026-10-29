using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ScenarioBudget.Dtos
{
    public class ScenarioBudgetDto : EntityDto<int>
    {
        public int District { get; set; }
        public int ScenarioId { get; set; }
        public bool IsInterstate { get; set; }
        public string AssetType { get; set; }
        public decimal? Budget { get; set; }
    }
}
