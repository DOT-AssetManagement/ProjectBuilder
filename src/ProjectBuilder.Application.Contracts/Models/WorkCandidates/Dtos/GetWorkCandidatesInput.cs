using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.WorkCandidates.Dtos
{
    public class GetWorkCandidatesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
