using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Measure.Dtos
{
    public class GetMeasureInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
