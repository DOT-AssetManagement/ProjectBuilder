using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.ReviseBudgetConstraints
{
    public class ReviseBudgetConstraintsDto
    {
        public int? YearOfWork { get; set; }
        public int? District { get; set; }
        public int? BridgeInterstate { get; set; }
        public int? BridgeNonInterstate { get; set; }
        public int? PavementInterstate { get; set; }
        public int? PavementNonInterstate { get; set; }
    }
}
