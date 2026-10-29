using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.CreateBridgeToPavements.Dtos
{
    public class GetBridgeToPavementInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
