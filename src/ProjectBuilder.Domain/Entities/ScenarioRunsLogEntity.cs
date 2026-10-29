using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Entities
{
    public class ScenarioRunsLogEntity
    {
        public Guid Id { get; set; }
        public int RunNo { get; set; }
        public Guid? ScenId { get; set; }
        public int? ScenNo { get; set; }
        public DateTime StartedAt { get; set; }
        public string? StartedBy { get; set; }
        public string? Status { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? Notes { get; set; }
    }
}
