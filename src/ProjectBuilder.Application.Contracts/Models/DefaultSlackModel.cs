using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models
{

    [ExcludeFromCodeCoverage]
    public class DefaultSlackModel
    {
        public string AssetType { get; set; }
        public int MoveBefore { get; set; }
        public int MoveAfter { get; set; }
    }
}
