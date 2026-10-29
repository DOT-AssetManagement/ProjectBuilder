using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.UserTreatmentType.Dtos
{
    public class UserTreatmentTypeDto : EntityDto<int>
    {
        public string UserTreatmentName { get; set; }
    }
}
