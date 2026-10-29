using System;

namespace ProjectBuilder.Web.Pages.Shared
{
    public class PoolCandidatesNavViewModel
    {
        public Guid PoolId { get; set; }
        public string PoolName { get; set; }
        public string ActiveMenu { get; set; }
        public bool CanWorkCandidates { get; set; }
        public bool CanConfiguration { get; set; }
        public bool CanRun { get; set; }
    }
}
