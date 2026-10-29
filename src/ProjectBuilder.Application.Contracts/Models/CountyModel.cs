using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models
{
    [ExcludeFromCodeCoverage]
    public class CountyModel
    {
        public string CountyName { get; set; }
        public int? District { get; set; }
        public int? CountyId { get; set; }
        public string CountyFullName { get { return $"{CountyId}-{CountyName}"; } }
        public override string ToString()
        {
            return CountyFullName;
        }
    }
}
