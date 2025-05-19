using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface IProjectTreatmentRepository : IRepository<ProjectTreatmentModel>,IFilterData
    {
        Task<bool> DeleteProjectTreatments(long modelId);
        Task<bool> EditProjectTreatments(ProjectTreatmentModel customTreatments);
        Task<bool> CheckIsUserCreated(int? scenId);
        Task<bool> CheckStale(int? scenId);
        Task<Guid> GetProjectTreatmentImportTimeGeneratedId(long ProjectTreatmentId);


        Task<List<ProjectTreatmentModel>> FilterProjectTreatments(TreatmentSearchModel model);
    }
    public interface IFilterData
    {
        Task<List<byte?>> GetDistricts(int? scenarioId);
        Task<List<CountyModel>> GetCounties(int? scenarioId, int? district);
        Task<List<int?>> GetRoutes(int? scenarioId, int? district, int? county);
        Task<List<string>> GetSections(int? scenarioId, int? district, int? county, int? route);
        Task<List<int?>> GetYears(int? scenarioId);
    }
}
