using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Counties.Dtos
{
    public class GetCountyInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
