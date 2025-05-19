using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PBLogic;
using System.Data;
using log4net;
using log4net.Appender;
using log4net.Layout;
using GisJsonHandler;
using Newtonsoft.Json;

namespace PBLogicTest
{
    [TestClass]
    public class DataTest
    {

        private static ILog _log = ConfigureLogger();

        private static log4net.ILog ConfigureLogger()
        {
            // Programmatic configuration
            // follows (with some streamlining) the example from Brendan Long and Ron Grabowski
            // org.apache.logging.log4net-user
            // These config statements create a RollingFile Appender.  Rolling File Appenders rollover on each execution of the test harness, 
            // in this case, following the Composite RollingMode.  Alternative log4net appenders may be added  or replace this default appender at the programmer's discretion.

            // PatternLayout layout = new PatternLayout("%d [%t] %-5p %c - %m%n");

            PatternLayout layout = new PatternLayout("%d %-5p %c - %m%n");
            log4net.Appender.RollingFileAppender appender = new RollingFileAppender
            {
                Layout = layout,
                AppendToFile = true,
                MaxFileSize = 10000000,
                RollingStyle = RollingFileAppender.RollingMode.Composite,
                StaticLogFileName = true,

                File = @".\Logs\DataTest.NET.log" // all logs will be created in the subdirectory logs 
            };

            // Configure filter to accept log messages of any level.
            log4net.Filter.LevelMatchFilter traceFilter = new log4net.Filter.LevelMatchFilter
            {

                LevelToMatch = log4net.Core.Level.Info
            };
            appender.ClearFilters();
            appender.AddFilter(traceFilter);

            appender.ImmediateFlush = true;
            appender.ActivateOptions();

            // Attach appender into hierarchy
            log4net.Repository.Hierarchy.Logger root =
                ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root;
            root.AddAppender(appender);
            root.Repository.Configured = true;

            log4net.ILog log = log4net.LogManager.GetLogger("PB-LOGGER");

            log.Debug("PB-LOGGER created.");

            return log;
        }

        public static ILog Log
        {
            get { return _log; }
            set { _log = value; }
        }

        private string _ConnectionString = @"Server=DESKTOP-EUKV3FS\SQL2019NEW;Database=SPP_PBv1;Trusted_Connection=True;MultipleActiveResultSets=True";
        private int _scenId = 1;
        private string _homeDirectory = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PBLogic";

        [TestMethod]
        public void TestCopyScenario()
        {
            bool ok = true;
            int newScenId = -1;

            ok = DataManager.CopyScenario(_ConnectionString, 1, out newScenId, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsTrue(newScenId > _scenId, $"newScenId={newScenId}");

        }

        [TestMethod]
        public void TestDeleteScenario()
        {
            bool ok = DataManager.DeleteScenario(_ConnectionString, 29,  out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestSaveScenarioToXml()
        {
            bool ok = DataManager.SaveScenarioInputsToXml(_ConnectionString, _homeDirectory, "Scenario25.xml", 25, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestSavescenarioStisticsToXml()
        {
            bool ok = DataManager.SaveScenarioStatisticsToXmlFile(_ConnectionString, _homeDirectory,
                24, "Statistics24_RB.xml", out string errorMessage);

            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestCreateNewScenario()
        {
           
            bool ok = DataManager.CreateNewScenario(_ConnectionString, "67940CF9-7689-4ABA-8DED-349D0A5782D1",  "Scenario_(BAMS/PAMS all districts)_20230615", 2020, 2034, out int newScenId, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsTrue(newScenId > 0, $"newScen");
        }

        [TestMethod]
        public void TestUnlockScenario()
        {

            bool ok = DataManager.UnlockScenario(_ConnectionString, 25, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        
        }

        [TestMethod]
        public void TesGetScenarioStatistics()
        {

            string xmlFilePath = _homeDirectory += @"\Data\ScenarioStatistics1.xml";
            bool ok = DataManager.GetScenarioStatistics(_ConnectionString, 1, out DataSet ds, xmlFilePath, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);

        }

        [TestMethod]
        public void TesGetScenarioStatisticsByDistrict()
        {

            string xmlFilePath = _homeDirectory += @"\Data\ScenarioStatistics1050_8_All";
            bool ok = DataManager.GetScenarioStatisticsByDistrict(_ConnectionString, 1, null, out DataSet ds, xmlFilePath, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);

        }

    
        [TestMethod]
        public void TestPopulateScenarioTreatments()
        {
            bool ok = DataManager.PopulateScenarioTreatmentsTable(_ConnectionString, 3, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }


        [TestMethod]
        public void TestExcelRead()
        {
            string path = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PAMS-BAMS-TO-PB.xlsx";
            bool ok = ExcelWrapper.ExcelHandler.Read(path, out DataSet dataSet, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsNotNull(dataSet, errorMessage);
        }

        #region IMPORT_FROM_EXCEL

        [TestMethod]
        public void THE_TestImportBAMSTreatments()
        {
            bool ok = ExcelWrapper.ExcelHandler.ImportTreatments(_ConnectionString,
                "BAMS", "B",
                @"C:\Projects\FHWA-SpyPond\PennDOT-2022\BMSID District 8 Test BAMSPBExportReport.xlsx", 
                null,
                false,
                null,
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void THE_TestImportPAMSTreatments()
        {
            bool ok = ExcelWrapper.ExcelHandler.ImportTreatments(_ConnectionString,
                "PAMS", "P",
                @"C:\Projects\FHWA-SpyPond\PennDOT-2022\TD_Statewide PAMSPBExportReport.xlsx", 
                "PAMS_TREATMENTS",
                false,
                null,
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void THE_TestImportBAMSTreatmentsToLibrary()
        {
            Guid targetLibraryId = Guid.Parse("67940CF9-7689-4ABA-8DED-349D0A5782D1");
            bool ok = ExcelWrapper.ExcelHandler.ImportTreatments(_ConnectionString,
                "BAMS", "B",
                @"C:\Projects\FHWA-SpyPond\PennDOT-2022\BAMSPBExportReport.xlsx",
                null,
                false,
                targetLibraryId,
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void THE_TestImportPAMSTreatmentsToLibrary()
        {
            Guid targetLibraryId =  Guid.Parse("67940CF9-7689-4ABA-8DED-349D0A5782D1");
            bool ok = ExcelWrapper.ExcelHandler.ImportTreatments(_ConnectionString,
                "PAMS", "P",
                @"C:\Projects\FHWA-SpyPond\PennDOT-2022\TD_Statewide PAMSPBExportReportUPDATE.xlsx",
                "PAMS_TREATMENTS",
                false,
                targetLibraryId,
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }
    
     
        [TestMethod]
        public void THE_TestImportMaintainableAssetSegmentation()
        {
            // string path = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PAMSPBExportReportUpdatedForDirection.xlsx";
            string path = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\TD_Statewide PAMSPBExportReport.xlsx";
            ExcelWrapper.ExcelHandler.Log = Log;

            bool ok = ExcelWrapper.ExcelHandler.ImportMaintainableAssetSegmentation(_ConnectionString, path,  "PAMS_MAS", false, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }
    
        [TestMethod]
        public void THE_TestImportBridgeToPavement()
        {
            string path = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\Bridge ID To Pavement Location Crosswalk.xlsx";
            bool ok = ExcelWrapper.ExcelHandler.ImportBridgeToPavement(_ConnectionString, path, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

       
        #endregion

         #region ExcelExport
        [TestMethod]
        public void THE_TestExportTreatmentsToBAMS()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string path = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PBJ_TREATMENTS_TO_BAMS_SCEN_8_20230523.xlsx";
            bool ok = ExcelWrapper.ExcelHandler.ExportTreatmentsToBAMS(_ConnectionString, path,  8,  out long recordsWritten, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void THE_TestExportProjectsToBAMS()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string path = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PBJ_PROJECTS_TO_BAMS_SCEN_8_20230615.xlsx";
            bool ok = ExcelWrapper.ExcelHandler.ExportProjectsToBAMS(_ConnectionString, path, 8, out long recordsWritten, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void THE_TestExportTreatmentsToPAMS()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string path = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PBJ_TREATMENTS_TO_PAMS_SCEN_8_20230615.xlsx";
            bool ok = ExcelWrapper.ExcelHandler.ExportTreatmentsToPAMS(_ConnectionString, path, 8, out long recordsWritten, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void THE_TestExportProjectsToPAMS()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string path = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PBJ_PROJECTS_TO_PAMS_SCEN_8_20230615.xlsx";
            bool ok = ExcelWrapper.ExcelHandler.ExportProjectsToPAMS(_ConnectionString, path, 8, out long recordsWritten, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }
        #endregion

        [TestMethod]
        public void LIB_TestCreateNewUserLibrary()
        {
            bool ok = DataManager.CreateNewUserLibrary(_ConnectionString, 1, "Library_20230614", "Description for the new library", true, out string libraryId, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsNotNull(libraryId, "Library ID has not been retrieved.");
        }

        [TestMethod]
        public void LIB_TestDeactivateLibrary()
        {
            bool ok = DataManager.DeactivateUserLibrary(_ConnectionString, "E9144692-3BA3-4997-BD37-2453B460EF31", out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void LIB_TestReactivateLibrary()
        {
            bool ok = DataManager.ReactivateUserLibrary(_ConnectionString, "E9144692-3BA3-4997-BD37-2453B460EF31", out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void LIB_TestPopulateUserLibraryFromMaster()
        {
            bool ok = DataManager.PopulateUserLibrary(_ConnectionString, 
                "E9144692-3BA3-4997-BD37-2453B460EF31", 
                out string errorMessage,
                fromScratch: true,
                sourceLibraryId: null,
                assetType: null, 
                district: null, 
                cnty: null, 
                route: null, 
                simulationId: null, 
                networkId: null, 
                minYear: null, 
                maxYear: null 
                );
            Assert.IsTrue(ok, errorMessage);
        }


        [TestMethod]
        public void LIB_TestCopyLibrary()
        {
            bool ok = DataManager.PopulateUserLibrary(_ConnectionString,
                "27D73A4C-BFB4-4903-AD75-8F0AA182AE22",
                out string errorMessage,
                fromScratch: true,
                sourceLibraryId: "E9144692-3BA3-4997-BD37-2453B460EF31",
                assetType: null,
                district: null,
                cnty: null,
                route: null,
                simulationId: null,
                networkId: null,
                minYear: null,
                maxYear: null
                );
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestGisExportIndented()
        {
            GisJsonHandler.JsonExporter.Log = _log;
            bool ok = JsonExporter.ExportScenarioResultsToJson(_ConnectionString, 2, true, out string jsonString, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsNotNull(jsonString, "JSON string returned as null.");
            if (ok)
            {
                System.IO.File.WriteAllText(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Scenario_2_Indented.json", jsonString);
            }

            GisOutput gis = JsonConvert.DeserializeObject<GisOutput>(jsonString);

            Assert.IsNotNull(gis, "gis is null");
        }

        [TestMethod]
        public void TestGisExportIndentedFiltered()
        {
            GisJsonHandler.JsonExporter.Log = _log;
            bool ok = JsonExporter.ExportScenarioResultsToJson(_ConnectionString, 
                    2, true, out string jsonString, out string errorMessage,
                    district: 8, 
                    cnty: 66, 
                    route: 83, 
                    section: "24-24",
                    appliedTreatment: "Superstructure Rep/Rehab",
                    selectedYear: 2028
                    );
            Assert.IsTrue(ok, errorMessage);
            Assert.IsNotNull(jsonString, "JSON string returned as null.");
            if (ok)
            {
                System.IO.File.WriteAllText(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Scenario_2_IndentedFiltered.json", jsonString);
            }

            if (ok)
            {
                ok = JsonExporter.ExportScenarioResultsToJson(_ConnectionString,
                       2, true, out jsonString, out errorMessage,
                       district: 8,
                       cnty: 66,
                       route: 83,
                       section: "24",   // Alternative way of passing the section parameter if from=to
                       appliedTreatment: "Superstructure Rep/Rehab",
                       selectedYear: 2028
                       );
                Assert.IsTrue(ok, errorMessage);
                Assert.IsNotNull(jsonString, "JSON string returned as null.");
                if (ok)
                {
                    System.IO.File.WriteAllText(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Scenario_2_Alt_IndentedFiltered.json", jsonString);
                }

            }

        }

        [TestMethod]
        public void TestGisExportNonIndented()
        {
            GisJsonHandler.JsonExporter.Log = _log;
            bool ok = JsonExporter.ExportScenarioResultsToJson(_ConnectionString, 2, false, out string jsonString, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsNotNull(jsonString, "JSON string returned as null.");
            if (ok)
            {
                System.IO.File.WriteAllText(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Scenario_2_NonIndented.json", jsonString);
            }

            GisOutput gis = JsonConvert.DeserializeObject<GisOutput>(jsonString);

            Assert.IsNotNull(gis, "gis is null");
        }

        [TestMethod]
        public void LIB_TestNewUserTreatmentWithDefaults()
        {
            DataManager.Log = Log;
            bool ok = DataManager.CreateUserTreatment(
                _ConnectionString,
                "27D73A4C-BFB4-4903-AD75-8F0AA182AE22", // LibraryId
                2,          // Mobility
                8,          // District
                62,         // Cnty
                1020,       // Route
                12,         // From Section
                2025,       // Preferred year
                1000000,    // Direct cost
                2000000,    // Benefit
                out Guid? treatmentTimeGneratedId,
                out string errorMessage
                );
            Assert.IsTrue(true, errorMessage);
            Assert.IsNull(errorMessage, errorMessage);
            Assert.IsNotNull(treatmentTimeGneratedId, "out Guid is null");
        }

        [TestMethod]
        public void LIB_TestNewUserTreatmentWithDetails()
        {
            DataManager.Log = Log;
            bool ok = DataManager.CreateUserTreatment(
                _ConnectionString,
                "27D73A4C-BFB4-4903-AD75-8F0AA182AE22", // LibraryId
                3,          // Capacity adding
                8,          // District
                28,         // Cnty
                401,        // Route
                2005,       // From Section
                2027,       // Preferred year
                2000000,    // Direct cost
                3000000,    // Benefit
                out Guid? treatmentTimeGneratedId,
                out string errorMessage,
                assetType: 'C',     // 'C' for capacity
                treatmentStatus: 2, // Progressed
                treatmentCause: 5,  // Cash-flow
                minYear: 2025,
                maxYear: 2029,
                toSection: 2005,
                direction: 1,
                offset: 4004,
                isInterstate: false,
                isIsolatedBridge: false,
                priorityOrder: 3,
                riskScore: 1234.56789,
                brkey: "17740",
                assetName: "17740",
                remainingLife: 100,
                isCommitted: true,
                ignoresSpendingLimit: false,
                indirectDesignCost: 300000,
                indirectROWCost: 200000,
                indirectUtilitiesCost: 100000,
                indirectOtherCost: 50000
                );
            Assert.IsTrue(true, errorMessage);
            Assert.IsNull(errorMessage, errorMessage);
            Assert.IsNotNull(treatmentTimeGneratedId, "out Guid is null");
        }
    }
}
