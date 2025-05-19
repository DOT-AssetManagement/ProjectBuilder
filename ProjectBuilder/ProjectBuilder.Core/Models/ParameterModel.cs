using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core.Models
{
    public class ParameterModel
    {
        public string ParameterId { get; set; }
        public string Parmfamily { get; set; }
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        public double? DefaultValue { get; set; }
    }
}
