using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos
{
    public class CreateTreatmentCancellationMatrixDto
    {
        public char AssetTypeA { get; set; }
        public string Id { get; set; }
        public char AssetTypeB { get; set; }
        public string TreatmentB { get; set; }
    }
}
