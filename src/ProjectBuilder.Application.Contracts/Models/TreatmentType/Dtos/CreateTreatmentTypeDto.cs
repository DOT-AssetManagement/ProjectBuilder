using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.TreatmentType.Dtos
{
    public class CreateTreatmentTypeDto
    {
        public string Id { get; set; }
        public int? MoveEarlier { get; set; }
        public int? MoveLater { get; set; }
        public string AssetType { get; set; }
    }
}
