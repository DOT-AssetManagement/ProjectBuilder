using log4net;
using log4net.Appender;
using log4net.Layout;
using GisJsonHandler;
using Newtonsoft.Json;
using System.Security.Cryptography;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS0414

namespace PBLogic.MSTest
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

                File = @"C:\GIT_Sandbox\PennDOT\Logs\DataTestV2.log" // all logs will be created in the subdirectory logs 
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

        private string _ConnectionString = @"Server=XPS8960\MSS2022;Database=SPP_PBv1;Trusted_Connection=True;MultipleActiveResultSets=True;MultipleActiveResultSets=True;Encrypt=False";
        private int _scenId = 1;
        private string _homeDirectory = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PBLogic";
        private string _ConnectionString_12_4_bacpac = @"Server=XPS8960\MSS2022;Database=12_4_bacpac;Trusted_Connection=True;MultipleActiveResultSets=True;MultipleActiveResultSets=True;Encrypt=False";
        private string _ConnectionString_1_27_26_bacpac = @"Server=XPS8960\MSS2022;Database=1_27_26bacpac;Trusted_Connection=True;MultipleActiveResultSets=True;MultipleActiveResultSets=True;Encrypt=False";
        private string _ConnectionString_proddb = @"Server=XPS8960\MSS2022;Database=proddb;Trusted_Connection=True;MultipleActiveResultSets=True;MultipleActiveResultSets=True;Encrypt=False";
        private string _ConnectionString_3_31_PB = @"Server=XPS8960\MSS2022;Database=proddb;Trusted_Connection=True;MultipleActiveResultSets=True;MultipleActiveResultSets=True;Encrypt=False";
        private string _ConnectionString_localdb = @"Server=XPS8960\MSS2022;Database=localdb;Trusted_Connection=True;MultipleActiveResultSets=True;MultipleActiveResultSets=True;Encrypt=False";

        [TestMethod]
        public void A1_CopyScenario()
        {
            bool ok = true;
            int newScenId = -1;

            ok = DataManager.CopyScenario(_ConnectionString, 81, out newScenId, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsTrue(newScenId > _scenId, $"newScenId={newScenId}");
        }

        [TestMethod]
        public void A2_PopulateFilteredTreatmentsForScenario85()
        {
            ScenarioFilter.Log = Log;
            bool ok = ScenarioFilter.PopulateFilteredTreatmentsForScenario(_ConnectionString, 85, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void A21_PopulateFilteredTreatmentsForScenario86()
        {
            ScenarioFilter.Log = Log;
            bool ok = ScenarioFilter.PopulateFilteredTreatmentsForScenario(_ConnectionString, 86, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        [Ignore]        // This library already exists, that's why [Ignore]
        public void A3_LIB_TestCreateNewUserLibrary()
        {
            DataManager.Log = Log;
            bool ok = DataManager.CreateNewUserLibrary(_ConnectionString, Guid.Empty, "Small PAMS Run 2", "Library for treatments imported from small run_PAMSPBExportReport.xlsx", true, out string libraryId, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsNotNull(libraryId, "Library ID has not been retrieved.");
            // '3DB5E069-08F6-4C7A-BD0C-E812DC7C4104'
            // '74449648-E986-4CD7-A85A-F4088F6C8FCF'
        }

        [TestMethod]
        public void A4_ImportPAMSTreatmentsToLibraryFromScratch()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string excelFile = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\Version_2\small run_PAMSPBExportReport.xlsx";
            Guid targetLibraryId = Guid.Parse("3DB5E069-08F6-4C7A-BD0C-E812DC7C4104");
            bool ok = ExcelWrapper.ExcelHandler.ImportTreatments(_ConnectionString,
                "PAMS", "P",
                excelFile,
                "PAMS_TREATMENTS",
                false,
                targetLibraryId,
                true,
                true,
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void A5_ImportSuperAssetsMaster()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string excelFile = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\Version_2\small run_PAMSPBExportReport.xlsx";
             bool ok = ExcelWrapper.ExcelHandler.ImportPAMSSuperAssets(_ConnectionString,
                excelFile,
                "SUPER_ASSETS",
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void A6_ImportMaintainableAssetSegmentationMaster()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string excelFile = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\Version_2\small run_PAMSPBExportReport.xlsx";
            bool ok = ExcelWrapper.ExcelHandler.ImportMaintainableAssetSegmentation(_ConnectionString, excelFile, "PAMS_MAS", false,
                out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void A7_TestPopulateScenarioTreatmentsForScenario86()
        {
            DataManager.Log = Log;
            bool ok = DataManager.PopulateScenarioTreatmentsTable(_ConnectionString, 86, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void A71_TestPopulateScenarioTreatmentsAndProjectsForScenario86()
        {
            DataManager.Log = Log;
            bool ok = DataManager.PopulateScenarioTreatmentsTable(_ConnectionString, 86, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);

            if (ok)
            {
                ok = DataManager.CreateExtendedScenarioProjects(_ConnectionString, 86, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }
        }

        [TestMethod]
        public void A8_PopulateProjectListAndBudgetConstraintsForScenario86()
        {
            SymphonyConductor.Log = Log;

            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 86;
            conductor.HomeDirectory = _homeDirectory;
           
            bool ok = true;
            string? errorMessage = null;

            if (ok)
            {
                ok = conductor.PopulateProjectListAndBudgetConstraints(conductor.ConnectionString, conductor.ScenId, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }
        }

        [TestMethod]
        public void A9_GenerateMPSFileForScenario86()
        {
            SymphonyConductor.Log = Log;

            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 86;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "HPB";
          
            bool ok = true;
            string? errorMessage = null;

          

            if (ok)
            {
                ok = conductor.PopulateProjectListAndBudgetConstraints(conductor.ConnectionString, conductor.ScenId, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }

            if (ok)
            {
                conductor.PopulateBudgetConstraintsDictionary(new ScenarioCommunique());
            }

            if (ok)
            {
                ok = conductor.GenerateMpsFileForMultiYearProjectsV2(new ScenarioCommunique() { MixAssetBudgets=false } , 
                            true, out string mpsFilePath,
                            out errorMessage, out bool degenerateCase);
                Assert.IsTrue(ok, errorMessage);
            }

           
        }

        [TestMethod]
        public void B1_RunMixedOptimization()
        {
            SymphonyConductor.Log = Log;

            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 86;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "HPB";

            bool ok = true;
            string? errorMessage = null;

            ScenarioCommunique filter = new ScenarioCommunique() { MixAssetBudgets = false };

            if (ok)
            {
                ok = conductor.RecordProjectAndTreatmentSelections(conductor.ScenId, out errorMessage, true);
                Assert.IsTrue(ok, errorMessage);
            }

            if (ok)
            {
                ok = conductor.PopulateProjectListAndBudgetConstraints(conductor.ConnectionString, conductor.ScenId, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }

            if (ok)
            {
                ok = conductor.RunMixedOptimizationV2(filter, 2, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }

            if (ok)
            {
                ok = conductor.RecordProjectAndTreatmentSelections(conductor.ScenId, out errorMessage, false);
                Assert.IsTrue(ok, errorMessage);
            }
        }

        [TestMethod]
        public void B2_RunFullOptimizationCycleForScenarioWithCleanup()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = conductor.RunFullOptimizationCycle(_ConnectionString, null, 86, _homeDirectory, true, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void B3_RunFullOptimizationCycleForScenarioWithoutCleanup()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = conductor.RunFullOptimizationCycle(_ConnectionString, null, 86, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void B4_RunExtendedScenario()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString, 86, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void C1_ImportPAMSTreatmentsToLibraryWithOwnCargo()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string excelFile = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\Version_2\small run_PAMSPBExportReport.xlsx";
            Guid targetLibraryId = Guid.Parse("74449648-E986-4CD7-A85A-F4088F6C8FCF");
            bool ok = ExcelWrapper.ExcelHandler.ImportTreatments(_ConnectionString,
                "PAMS", "P",
                excelFile,
                "PAMS_TREATMENTS",
                false,
                targetLibraryId,
                true,
                true,
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void C2_CreateNewScenario()
        {

            bool ok = DataManager.CreateNewScenario(_ConnectionString, "74449648-E986-4CD7-A85A-F4088F6C8FCF", "Scenario_SmallRun_2025-09-05-D", 2025, 2030, 1, out int newScenId, out string errorMessage, "dmitry@gurenich.com");
            Assert.IsTrue(ok, errorMessage);
            Assert.IsTrue(newScenId > 0, $"newScen");
            // ScenId=1090
        }

        [TestMethod]
        public void C3_PopulateFilteredTreatmentsForScenario1090()
        {
            ScenarioFilter.Log = Log;
            bool ok = ScenarioFilter.PopulateFilteredTreatmentsForScenario(_ConnectionString, 1090, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void C4_CopyScenario1090()
        {
            bool ok = true;
            int newScenId = -1;

            ok = DataManager.CopyScenario(_ConnectionString, 1090, out newScenId, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsTrue(newScenId > _scenId, $"newScenId={newScenId}");
        }

        [TestMethod]
        public void C5_PopulateProjectListAndBudgetConstraintsForScenario1090()
        {
            SymphonyConductor.Log = Log;

            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 1090;
            conductor.HomeDirectory = _homeDirectory;

            bool ok = true;
            string? errorMessage = null;

            if (ok)
            {
                ok = conductor.PopulateProjectListAndBudgetConstraints(conductor.ConnectionString, conductor.ScenId, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }
        }

        [TestMethod]
        public void C6_GenerateMPSFileForScenario1090()
        {
            SymphonyConductor.Log = Log;

            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 1090;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "HPB";

            bool ok = true;
            string? errorMessage = null;

            if (ok)
            {
                ok = conductor.PopulateProjectListAndBudgetConstraints(conductor.ConnectionString, conductor.ScenId, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }

            if (ok)
            {
                conductor.PopulateBudgetConstraintsDictionary(new ScenarioCommunique());
            }

            if (ok)
            {
                ok = conductor.GenerateMpsFileForMultiYearProjectsV2(new ScenarioCommunique() { MixAssetBudgets = false },
                            false, out string mpsFilePath,
                            out errorMessage, out bool degenerateCase);
                Assert.IsTrue(ok, errorMessage);
            }


        }

        [TestMethod]
        public void C7_RunExtendedScenario1090()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString, 1090, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void C8_ImportPAMSTreatmentsToLibraryWithOwnCargo()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string excelFile = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\BAMSPBExportReport.xlsx";
            Guid targetLibraryId = Guid.Parse("74449648-E986-4CD7-A85A-F4088F6C8FCF");
            bool ok = ExcelWrapper.ExcelHandler.ImportTreatments(_ConnectionString,
                "BAMS", "B",
                excelFile,
                null,
                false,
                targetLibraryId,
                false,
                true,
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }


        [TestMethod]
        public void D1_GenerateMPSFileForScenario116()
        {
            SymphonyConductor.Log = Log;

           string _connectionString = @"Server=XPS8960\MSS2022;Database=pb;Trusted_Connection=True;MultipleActiveResultSets=True;MultipleActiveResultSets=True;Encrypt=False";

         SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _connectionString;
            conductor.ScenId = 116;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "HPB";

            bool ok = true;
            string? errorMessage = null;

            if (ok)
            {
                ok = conductor.PopulateProjectListAndBudgetConstraints(conductor.ConnectionString, conductor.ScenId, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }

            if (ok)
            {
                conductor.PopulateBudgetConstraintsDictionary(new ScenarioCommunique());
            }

            if (ok)
            {
                ok = conductor.GenerateMpsFileForMultiYearProjectsV2(new ScenarioCommunique() { MixAssetBudgets = false },
                            false, out string mpsFilePath,
                            out errorMessage, out bool degenerateCase);
                Assert.IsTrue(ok, errorMessage);
            }


        }

        [TestMethod]
        public void D2_RunFullOptimizationCycleForScenarioWithoutCleanup()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();
            string _connectionString = @"Server=XPS8960\MSS2022;Database=pb;Trusted_Connection=True;MultipleActiveResultSets=True;MultipleActiveResultSets=True;Encrypt=False";

            bool ok = conductor.RunFullOptimizationCycle(_connectionString, null, 116, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void E1_LIB_TestCreateNewUserLibrary()
        {
            DataManager.Log = Log;
            bool ok = DataManager.CreateNewUserLibrary(_ConnectionString, Guid.Empty, "Current_PAMS_PB_Report", "Library for treatments imported from Current_PAMS_PB_Report.xlsx", true, out string libraryId, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsNotNull(libraryId, "Library ID has not been retrieved.");
            // 0381BC50-33A8-465D-898A-B45AF9B89691
            // LibNo = 1110
        }

        [TestMethod]
        public void E2_ImportPAMSTreatmentsToLibrary1110FromScratch()
        {
            ExcelWrapper.ExcelHandler.Log = Log;
            string excelFile = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\Version_2\Current_PAMS_PB_Report.xlsx";
            Guid targetLibraryId = Guid.Parse("0381BC50-33A8-465D-898A-B45AF9B89691");
            bool ok = ExcelWrapper.ExcelHandler.ImportTreatments(_ConnectionString,
                "PAMS", "P",
                excelFile,
                "PAMS_TREATMENTS",
                false,
                targetLibraryId,
                true,
                true,
                out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void E3_CreateNewScenarioForLibrary1110()
        {

            bool ok = DataManager.CreateNewScenario(_ConnectionString, "0381BC50-33A8-465D-898A-B45AF9B89691", "Current_PAMS_PB_Report", 2025, 2035, 1, out int newScenId, out string errorMessage, "dmitry@gurenich.com");
            Assert.IsTrue(ok, errorMessage);
            Assert.IsTrue(newScenId > 0, $"newScen");
            // ScenId=1094
        }

        [TestMethod]
        public void E4_PopulateScenarioTreatmentsForScenario1094()
        {
            DataManager.Log = Log;
            bool ok = DataManager.PopulateScenarioTreatmentsTable(_ConnectionString, 1094, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void E5_TestPopulateScenarioProjectsForScenario1094()
        {
            DataManager.Log = Log;
            bool ok = DataManager.CreateExtendedScenarioProjects(_ConnectionString, 1094, out string?  errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void E6_GenerateMPSFileForScenario1094()
        {
            SymphonyConductor.Log = Log;

            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 1094;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "HPB";

            bool ok = true;
            string? errorMessage = null;

            if (ok)
            {
                ok = conductor.PopulateProjectListAndBudgetConstraints(conductor.ConnectionString, conductor.ScenId, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }

            if (ok)
            {
                conductor.PopulateBudgetConstraintsDictionary(new ScenarioCommunique());
            }

            if (ok)
            {
                ok = conductor.GenerateMpsFileForMultiYearProjectsV2(new ScenarioCommunique() { MixAssetBudgets = false },
                            false, out string mpsFilePath,
                            out errorMessage, out bool degenerateCase);
                Assert.IsTrue(ok, errorMessage);
            }


        }

        [TestMethod]
        public void E7_RunFullOptimizationCycleForScenarioWithoutCleanupScenario1094()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = conductor.RunFullOptimizationCycle(_ConnectionString, null, 1094, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void E8_RunExtendedScenario1094()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString, 1094, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void F1_CopyUserLibrary()
        {
            DataManager.Log = Log;

            bool ok = DataManager.CopyUserLibrary(_ConnectionString, "0381BC50-33A8-465D-898A-B45AF9B89691", "3B9F0CAB-AB18-4AAB-B004-4C13846DF261", out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void F2_CreateNewScenarioForLibrary_aaaa()
        {

            bool ok = DataManager.CreateNewScenario(_ConnectionString, "3B9F0CAB-AB18-4AAB-B004-4C13846DF261", "Current_PAMS_NoSuperAssets", 2025, 2037, 1, out int newScenId, out string errorMessage, "dmitry@gurenich.com");
            Assert.IsTrue(ok, errorMessage);
            Assert.IsTrue(newScenId > 0, $"newScen");
            // ScenId=1098
        }

        [TestMethod]
        public void F3_RunExtendedScenario1098()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString, 1098, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void F4_CopyScenario1094()
        {
            bool ok = true;
            int newScenId = -1;

            ok = DataManager.CopyScenario(_ConnectionString, 1094, out newScenId, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            Assert.IsTrue(newScenId > _scenId, $"newScenId={newScenId}");
            // 1099
        }

        [TestMethod]
        public void F5_RunExtendedScenario1099()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString, 1099, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void G1_RunExtendedScenario119_on_Bacpac()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString_12_4_bacpac, 119, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }


        [TestMethod]
        public void G2_RunExtendedScenario121_on_1_27_26Bacpac()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString_1_27_26_bacpac, 121, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void G3_RunExtendedScenario4_on_proddb()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString_proddb, 4, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void G4_RunExtendedScenario2_on_3_31_PB()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString_3_31_PB, 2, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void G5_RunScenario16_on_localdb()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString_localdb, 16, null, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void G6_RunScenario16_on_localdb_mixing_budgets()
        {
            SymphonyConductor.Log = Log;
            SymphonyConductor conductor = new SymphonyConductor();

            bool ok = SymphonyConductor.RunExtendedScenario(_ConnectionString_localdb, 16, new ScenarioCommunique{MixAssetBudgets = true}, _homeDirectory, false, out string? errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }
    }
}