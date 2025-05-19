using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class SegmentationModel
    {
        public bool isInterstate { get; set; }
        public bool Direction { get; set; }
    }
}
