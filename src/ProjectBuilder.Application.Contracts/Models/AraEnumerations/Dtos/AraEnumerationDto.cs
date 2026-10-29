using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.AraEnumerations.Dtos
{
    public class AraEnumerationDto : EntityDto<string>
    {
        public string EnumFamily { get; set; } 
        public string EnumName { get; set; }
        public int EnumInt { get; set; }
    }
}
