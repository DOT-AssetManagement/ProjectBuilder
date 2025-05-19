using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface ITreatmentUnitOfWork
    {     
        Task<Guid?> CreateUserTreatment(UserTreatmentModel userTreatment);
        Task<string> ImportTreatmentsFromExcelFile(string src,string assetType,string excelPath,string excelTabName,Guid? targetLibraryId, bool fromScratch, bool keepAll);
        Task<string> ExportProjectsToExcelFile(string excelPath, int scenid, string source);
        Task<string> ImportMASAndB2PFromExcelFile(bool isMas, string excelPath, string excelTabName);
        void DeleteExcelFile(string excelPath);
        Task<bool> UnAssignTreatmentsFromProject(int projectId);
    }
}
