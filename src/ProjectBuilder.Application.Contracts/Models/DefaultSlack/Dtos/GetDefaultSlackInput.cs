using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.DefaultSlack.Dtos
{
    public class GetDefaultSlackInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
