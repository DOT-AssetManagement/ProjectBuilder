using ProjectBuilder.Models.CandidatePool.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Scenario.Dtos
{
    public class ScenarioDto : EntityDto<int>
    {
        public Guid? LibraryId { get; set; }
        public string ScenarioName { get; set; }
        public string CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public string CreatorEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastRunBy { get; set; }
        public DateTime? LastRunAt { get; set; }
        public DateTime? LastStarted { get; set; }
        public DateTime? LastFinished { get; set; }
        public bool? Locked { get; set; }
        public string Notes { get; set; }
        public IEnumerable<ScenarioDto> Scenarios { get; set; } = Enumerable.Empty<ScenarioDto>();
        public bool Stale { get; set; }
        public bool IsShared { get; set; }
        public string CandidatePool { get; set; }
        public bool ReRun { get; set; }
        public string SearchString { get; set; }
        public int LibrariesCount { get; set; }
        public string ScenarioFullName { get { return $"{ScenarioName}"; } }
        public string ScenName => ScenarioName;
        public Guid? PoolId => LibraryId;
        public long? BridgesCount { get; set; }
        public long? PavementsCount { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public string PoolName { get; set; }
    }
}
