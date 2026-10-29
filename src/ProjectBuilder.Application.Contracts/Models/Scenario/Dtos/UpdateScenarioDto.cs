using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Scenario.Dtos
{
    public class UpdateScenarioDto : EntityDto<int>
    {
        public Guid? LibraryId { get; set; }
        public string ScenarioName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastRunBy { get; set; }
        public string Description { get; set; }
        public DateTime? LastRunAt { get; set; }
        public DateTime? LastStarted { get; set; }
        public DateTime? LastFinished { get; set; }
        public bool? Locked { get; set; }
        public string Notes { get; set; }
        public bool Stale { get; set; }
 
    }
}
