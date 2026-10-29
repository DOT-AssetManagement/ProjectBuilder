using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Pool.Dtos
{
    public class GetPoolInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
