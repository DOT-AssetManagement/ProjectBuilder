using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class CargoDataModel
    {
        public Guid ImportTimeGeneratedGuid { get; set; }

        public Guid ImportSessionId { get; set; }

        public int AttributeNo { get; set; }

        public string? TextValue { get; set; }

        public double? NumericValue { get; set; }
    }
}
