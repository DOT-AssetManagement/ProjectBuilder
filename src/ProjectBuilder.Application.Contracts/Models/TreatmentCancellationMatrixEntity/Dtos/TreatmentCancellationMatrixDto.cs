using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos
{
    public class TreatmentCancellationMatrixDto : EntityDto<string>
    {
        public char AssetTypeA { get; set; }
        public char AssetTypeB { get; set; }
        public string TreatmentB { get; set; }
    }
}
