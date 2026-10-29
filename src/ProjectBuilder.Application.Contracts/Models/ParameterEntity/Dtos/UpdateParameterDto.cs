using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ParameterEntity.Dtos
{
    public class UpdateParameterDto : EntityDto<string>
    {
        public string Parmfamily { get; set; }
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        public double? DefaultValue { get; set; }
    }
}
