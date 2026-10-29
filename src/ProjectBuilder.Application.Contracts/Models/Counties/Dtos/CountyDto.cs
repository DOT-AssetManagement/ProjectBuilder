using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Counties.Dtos
{
    public class CountyDto : EntityDto<int?>
    {
        public string CountyName { get; set; }
        public int? District { get; set; }
        public string CountyFullName { get; set; }
        public bool IsDeleteable { get; set; }
        public int? Cnty => Id;
    }
}
