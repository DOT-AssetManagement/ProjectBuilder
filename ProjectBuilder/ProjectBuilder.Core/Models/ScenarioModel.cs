using System;
using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class ScenarioModel
    {
        public int ScenarioId { get; set; }

        public Guid? LibraryId { get; set; }
        public string ScenarioName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string LastRunBy { get; set; }
        public DateTime? LastRunAt { get; set; }
        public DateTime? LastStarted { get; set; }
        public DateTime? LastFinished { get; set; }
        public bool Locked { get; set; }
        public string Notes { get; set; }
        public bool ReRun { get; set; }
		public bool Stale { get; set; }
        public string CandidatePool { get; set; }
        public string ScenarioFullName { get { return $"{ScenarioName}"; } }
        public override string ToString()
        {
            return ScenarioFullName;
        }
    }
}
