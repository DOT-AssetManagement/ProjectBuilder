using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Goal.Dtos
{
    public class GetGoalInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
