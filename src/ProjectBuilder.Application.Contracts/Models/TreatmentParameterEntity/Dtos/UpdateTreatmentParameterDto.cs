using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.TreatmentParameterEntity.Dtos
{
    public class UpdateTreatmentParameterDto : EntityDto<int>
    {
        public double? UserTreatmentBenefitWeight { get; set; }
    }
}
