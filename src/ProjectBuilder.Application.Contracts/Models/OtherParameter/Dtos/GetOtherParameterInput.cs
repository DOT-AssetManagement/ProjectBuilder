using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.OtherParameter.Dtos
{
    public class GetOtherParameterInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
