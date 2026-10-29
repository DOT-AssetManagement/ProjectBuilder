using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.TreatmentParameterEntity.Dtos
{
    public class GetTreatmentParameterInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
