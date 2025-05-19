using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class ScenarioParameterModel
    {
        public int ScenarioId { get; set; }
        public string ParameterId { get; set; }
        public string ParameterName { get; set; }
        public string ParameterDescription { get; set; }
        public double? ParameterValue { get; set; }
    }
}
