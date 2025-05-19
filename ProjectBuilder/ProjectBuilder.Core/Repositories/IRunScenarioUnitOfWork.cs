using ProjectBuilder.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface IRunScenarioUnitOfWork : IUnitOfWork
    {      
        public int ScenarioId { get; set; }
        public IRepository<ScenarioParameterModel> ScenarioParametersRepo { get;}
        public IRepository<ScenarioModel> ScenariosRepo { get; }
        public IRepository<ScenarioBudgetFlatModel> ScenariosBudgetsRepo { get; }
        public Task<ScenarioEventArgs> CreateScenario(string scenarioName,Guid libraryId,int firstYear,int lastYear, int setDefault, string CreatedBy = "");
        public Task<ScenarioEventArgs> RunScenario(int scenarioId, string name, bool doCleanup);
        public Task<ScenarioEventArgs> DeleteScenario(int scenarioId);
        public Task<MapsResultModel> ExportScenarioResultsToJson(int scenarioId, bool indent, int? district = null, int? cnty = null, int? route = null, string section = null, string appliedTreatment = null, int? selectedYear = null);
    }
}
