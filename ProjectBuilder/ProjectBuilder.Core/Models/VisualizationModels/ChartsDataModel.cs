using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public class ChartsDataModel
    {
        public Dictionary<string, Dictionary<string, object>> SeriesPoint { get; set; } = new Dictionary<string, Dictionary<string, object>>();
        public List<int?> Labels { get; set; }
    }
}
