using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Counties.Dtos
{
    public class UpdateCountyDto : EntityDto<int?>
    {
        public string CountyName { get; set; }

        public int? District { get; set; }
    }
}
