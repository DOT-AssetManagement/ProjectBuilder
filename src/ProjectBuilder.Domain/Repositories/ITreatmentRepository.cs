using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface ITreatmentRepository : IRepository<UserTreatment>
    {
        Task<bool> DeleteTreatments(Guid modelId);
        Task<bool> EditTreatments(UserTreatment customTreatment);
        Task<List<UserTreatment>> FilterTreatments(TreatmentSearchModel model);
        Task<Dictionary<string, int?>> GetMinMaxYears();
        Task<List<byte?>> GetDistricts();
        Task<List<byte?>> GetTreatmentCountyIds();
        Task<List<CountyEntity>> GetCounties(int? district);
        Task<List<int?>> GetRoutes(int? district, int? county);
        Task<List<string>> GetSections(int? district, int? county, int? route);
        Task<List<string>> GetTreatmentTypes(int? district, int? county, int? route);
        Task<List<int?>> GetTreatmentsAvailibleYears(Guid? libraryid);
        Task<DefaultSlack> GetAssetTypeDefaultSlack(string assetType);
        Task<SegmentationModel> GetDirectionInterstate(int? district, int? county, int? route, int? fromsection, int? toSection);
        Task<List<CountyEntity>> GetAllCounties();
        Task<Guid?> CreateUserTreatment(UserTreatment userTreatment);
        string ValidateTreatmentExcelFile(string excelPath, string tabName, string importType);
        Task<string> ImportTreatmentsFromExcelFile(string src, string assetType, string excelPath, string excelTabName, Guid? targetLibraryId, bool fromScratch, bool keepAll);
        Task<string> ExportProjectsToExcelFile(string excelPath, int scenid, string source);
        Task<string> ImportMASAndB2PFromExcelFile(bool isMas, string excelPath, string excelTabName);
        void DeleteExcelFile(string excelPath);
        Task<bool> UnAssignTreatmentsFromProject(int projectId);
        Task<bool> DeleteTreatmentAsync(Guid libraryId, Guid treatmentId);

    }
}
