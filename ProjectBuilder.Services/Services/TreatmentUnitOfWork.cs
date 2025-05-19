using ExcelWrapper;
using Microsoft.Extensions.Configuration;
using PBLogic;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Services
{
    public class TreatmentUnitOfWork : ITreatmentUnitOfWork
    {
        private readonly string _connectionString;
       

        public TreatmentUnitOfWork(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }
        public async Task<Guid?> CreateUserTreatment(UserTreatmentModel userTreatment)
        {
            Guid? treatmentId = Guid.Empty;
            string errorMessage = string.Empty;
                await Task.Run(() => DataManager.CreateUserTreatment(_connectionString,userTreatment.LibraryId.ToString(),userTreatment.UserTreatmentTypeNo.Value,userTreatment.District.Value,
                userTreatment.CountyId.Value,userTreatment.Route.Value,userTreatment.FromSection.Value,userTreatment.PreferredYear.Value,userTreatment.Cost.Value,userTreatment.Benefit.Value,out treatmentId,out errorMessage,
                userTreatment.Treatment,userTreatment.AssetType.FirstOrDefault(),0,0,userTreatment.MinYear,userTreatment.MaxYear,userTreatment.ToSection,userTreatment.Direction,
                userTreatment.Offset,userTreatment.Brkey,null,userTreatment.Asset,userTreatment.Interstate.Value,false,userTreatment.PriorityOrder.Value,userTreatment.Risk
                ,null,userTreatment.IsCommitted.Value,false,userTreatment.IndirectCostDesign,userTreatment.IndirectCostRow,userTreatment.IndirectCostUtilities,userTreatment.IndirectCostOther));
            if (string.IsNullOrEmpty(errorMessage))
                return treatmentId;
            return Guid.Empty;
        }

        public async Task<bool> UnAssignTreatmentsFromProject(int projectId)
        {
            string errorMessage = string.Empty;
            await Task.Run(() => DataManager.UnAssignProjectTreatments(_connectionString, projectId, out string errorMessage));
            return string.IsNullOrEmpty(errorMessage);
        }

        public void DeleteExcelFile(string excelPath)
        {
            try
            {
              if(File.Exists(excelPath))
                  File.Delete(excelPath);

            }
            catch 
            {
            }
        }

        public async Task<string> ImportTreatmentsFromExcelFile(string src, string assetType, string excelPath, string excelTabName, Guid? targetLibraryId, bool fromScratch, bool keepAll)
        {
            string errorMessage = "";
            await Task.Run(() => ExcelHandler.ImportTreatments(_connectionString,src,assetType,excelPath,excelTabName,false,targetLibraryId, fromScratch, keepAll,out errorMessage));
            return errorMessage;
        }

        public async Task<string> ExportProjectsToExcelFile(string excelPath, int scenid, string source)
        {
            string errorMessage = "Selected type doesn't have export feature";
            long recordsWritten = 0;
            if(source == "PAMS")
                await Task.Run(() => ExcelHandler.ExportProjectsToPAMS(_connectionString, excelPath, scenid, out recordsWritten, out errorMessage));
            else if(source == "BAMS")
                await Task.Run(() => ExcelHandler.ExportProjectsToBAMS(_connectionString, excelPath, scenid, out recordsWritten, out errorMessage));
            return errorMessage;
        }

        public async Task<string> ImportMASAndB2PFromExcelFile(bool isMas, string excelPath, string excelTabName)
        {
            string errorMessage = "";
            if (isMas)
            {
                await Task.Run(() => ExcelHandler.ImportMaintainableAssetSegmentation(_connectionString, excelPath, excelTabName, false, out errorMessage));
            }
            else
            {
                await Task.Run(() => ExcelHandler.ImportBridgeToPavement(_connectionString, excelPath, out errorMessage));
            }
            return errorMessage;
        }
    }
}
