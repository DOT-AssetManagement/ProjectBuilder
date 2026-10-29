using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Treatment.Dtos
{
    public class GetTreatmentInput : PagedAndSortedResultRequestDto
    {
         public string Filter { get; set; }
    }
}
