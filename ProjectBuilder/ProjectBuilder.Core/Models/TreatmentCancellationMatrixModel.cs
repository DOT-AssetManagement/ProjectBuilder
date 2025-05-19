using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core.Models
{
    public class TreatmentCancellationMatrixModel
    {
        public char AssetTypeA { get; set; }
        public string TreatmentA { get; set; }
        public char AssetTypeB { get; set; }
        public string TreatmentB { get; set; }
    }
}
