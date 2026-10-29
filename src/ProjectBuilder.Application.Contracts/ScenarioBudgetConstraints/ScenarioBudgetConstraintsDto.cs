using ProjectBuilder.Models;
using ProjectBuilder.Models.ScenarioBudgetFlat.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ScenarioBudgetConstraints
{
    public class ScenarioBudgetConstraintsDto
    {
        public IEnumerable<ScenarioBudgetFlatDto> BudgetConstraints { get; set; }
        public string SelectedScenario { get; set; }
        public int? ScenarioId { get; set; }
    }
}
