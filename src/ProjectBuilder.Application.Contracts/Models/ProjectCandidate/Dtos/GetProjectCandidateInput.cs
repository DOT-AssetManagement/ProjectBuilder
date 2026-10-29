using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ProjectCandidate.Dtos
{
    public class GetProjectCandidateInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
