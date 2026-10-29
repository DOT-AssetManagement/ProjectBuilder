using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.Charts
{
    public class ChartsDto
    {
        public int? ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public IEnumerable<int?> Districts { get; set; } = Enumerable.Empty<int?>();
    }
}
