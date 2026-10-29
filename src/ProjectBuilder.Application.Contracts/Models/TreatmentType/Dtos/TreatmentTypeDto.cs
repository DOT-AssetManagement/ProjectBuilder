using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.TreatmentType.Dtos
{
    public class TreatmentTypeDto : EntityDto<string>
    {
        public int? MoveEarlier { get; set; }
        public int? MoveLater { get; set; }
        public string AssetType { get; set; }
    }
}
