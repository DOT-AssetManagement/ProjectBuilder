using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.PAMSSectionSegment.Dtos
{
    public class GetPAMSSectionSegmentInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
