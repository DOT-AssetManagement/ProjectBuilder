using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class PAMSSectionSegmentModel
    {
        public byte District { get; set; }
        public byte CountyId { get; set; }
        public int Route { get; set; }
        public int Section { get; set; }
        public bool Interstate { get; set; }
    }
}
