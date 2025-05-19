using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class CargoAttributesModel
    {
        public int AttributeNo { get; set; }

        public char AssetType { get; set; }

        public string AttributeName { get; set; }

        public string AttributeType { get; set; }

        public string Notes { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
