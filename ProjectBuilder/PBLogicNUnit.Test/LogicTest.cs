using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using PBLogic;

namespace PBLogicTest
{
    [TestClass]
    public class LogicTest
    {

        private string _ConnectionString = @"Server=DESKTOP-EUKV3FS\SQL2019;Database=EPOS_2;User Id=ProjectBuilder;password=PennDOT;MultipleActiveResultSets=True";
        private int _scenId = 1;
        private string _homeDirectory = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PBLogic";

        private string _mpsFilePath;
        private string _resultFilePath;
        private string _errorFilePath;

        [TestMethod]
        public void TestLogging()
        {
            SymphonyConductor conductor = new SymphonyConductor();
            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = _scenId;
            conductor.HomeDirectory = _homeDirectory;
        }

        [TestMethod]
        public void TestLPFormulationCreation()
        {
            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 24;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "RB";

            string errorMessage = null;
            bool ok = conductor.CreateAlternativesTable(out errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestMPSFileGeneration()
        {
            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 24;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "RB";

            string errorMessage = null;

            bool ok = conductor.GenerateMpsFile(false, false, out _mpsFilePath, out errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestMPSFileGenerationFilter()
        {
            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 1;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "HPB";

            string errorMessage = null;

            bool ok = conductor.GenerateMpsFile(new ScenarioCommunique() { Commitment = false, District = null, MaxPriority = 10, ProjectsOnly = false },
                false, out _mpsFilePath, out errorMessage, out bool degenerateCase);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestTreatementAlternativesGeneration()
        {
            SymphonyConductor conductor = new SymphonyConductor()
            {
                ConnectionString = _ConnectionString,
                ScenId = 1,
                Code = "HPB",
                HomeDirectory = _homeDirectory
            };

            bool ok = conductor.CreateTreatmentsAlternativesTable(0, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestTreatementAlternativeMatrixGeneration()
        {
            SymphonyConductor conductor = new SymphonyConductor()
            {
                ConnectionString = _ConnectionString,
                ScenId = 1,
                Code = "HPB",
                HomeDirectory = _homeDirectory
            };

            bool ok = conductor.CreateAlternativesMatrix(true, 8, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

#if _TREATMENTS
        [TestMethod]
        public void TestTreatmentOptCycle0()
        {
            SymphonyConductor conductor = new SymphonyConductor()
            {
                ConnectionString = _ConnectionString,
                ScenId = 1,
                Code = "HPB",
                HomeDirectory = _homeDirectory
            };
            int minClusterSize = 0;
            bool ok = conductor.RunTreatmentOptimization(0, true, 8, out string errorMessage, out bool degenerateCase, ref minClusterSize);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestTreatmentOptCycle1()
        {
            SymphonyConductor conductor = new SymphonyConductor()
            {
                ConnectionString = _ConnectionString,
                ScenId = 1,
                Code = "HPB",
                HomeDirectory = _homeDirectory
            };
            int minClusterSize = 0;
            bool ok = conductor.RunTreatmentOptimization(1, false, 8, out string errorMessage, out bool degenerateCase, ref minClusterSize);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestTreatmentOptCycle2()
        {
            SymphonyConductor conductor = new SymphonyConductor()
            {
                ConnectionString = _ConnectionString,
                ScenId = 1,
                Code = "HPB",
                HomeDirectory = _homeDirectory
            };
            int minClusterSize = 0;
            bool ok = conductor.RunTreatmentOptimization(2, false, 8, out string errorMessage, out bool degenerateCase, ref minClusterSize);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestTreatmentOptCycle3()
        {
            SymphonyConductor conductor = new SymphonyConductor()
            {
                ConnectionString = _ConnectionString,
                ScenId = 1,
                Code = "HPB",
                HomeDirectory = _homeDirectory
            };
            int minClusterSize = 0;
            bool ok = conductor.RunTreatmentOptimization(3, false, 8, out string errorMessage, out bool degenerateCase, ref minClusterSize);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestTreatmentOptCycle4()
        {
            SymphonyConductor conductor = new SymphonyConductor()
            {
                ConnectionString = _ConnectionString,
                ScenId = 1,
                Code = "HPB",
                HomeDirectory = _homeDirectory
            };
            int minClusterSize = 0;
            bool ok = conductor.RunTreatmentOptimization(4, false, 8, out string errorMessage, out bool degenerateCase, ref minClusterSize);
            Assert.IsTrue(ok, errorMessage);
        }


        [TestMethod]
        public void TestTreatmentOptCycles()
        {
            SymphonyConductor conductor = new SymphonyConductor()
            {
                ConnectionString = _ConnectionString,
                ScenId = 1,
                Code = "HPB",
                HomeDirectory = _homeDirectory
            };

            string errorMessage = null;
            bool ok = true;
            bool degenerateCase = false;
            int minClusterSize = 0;
            for (int i = 1; ok && !degenerateCase && i <= 41; i++)
            {
                ok = conductor.RunTreatmentOptimization(i, false, 8, out errorMessage, out degenerateCase, ref minClusterSize);
                if (ok && degenerateCase)
                {
                    minClusterSize++;
                    degenerateCase = false;
                }
            }
            Assert.IsTrue(ok, errorMessage);
        }

#endif
        [TestMethod]
        public void TestSymphonyRun()
        {
            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = _scenId;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "HPB";

            string errorMessage = null;

            bool ok = conductor.GenerateMpsFile(new ScenarioCommunique()
            {
                Commitment = false,
                ProjectsOnly = false,
                MaxPriority = 10,
                District = null
            }, false, out _mpsFilePath, out errorMessage, out bool degenerateCase);
            Assert.IsTrue(ok, errorMessage);

            if (ok)
            {
                ok = conductor.RunSymphony(_mpsFilePath, out _resultFilePath, out _errorFilePath, out errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }
        }

        [TestMethod]
        public void TestFullOptimizationCycle()
        {
            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = 1;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "HPB";

            string errorMessage = null;

            bool ok = conductor.CreateAlternativesTable(out errorMessage);
            Assert.IsTrue(ok, errorMessage);

            if (ok)
            {
                ok = conductor.CreateAlternativesMatrix(out errorMessage);
                Assert.IsTrue(ok, errorMessage);

            }


            if (ok)
            {
                ok = conductor.GenerateMpsFile(new ScenarioCommunique()
                {
                    District = null,
                    Commitment = false,
                    ProjectsOnly = false,
                    SingleTreatmentsOnly = false,
                    MaxPriority = 10
                }
                    , false, out _mpsFilePath, out errorMessage, out bool degenerateCase);
                Assert.IsTrue(ok, errorMessage);

                if (!degenerateCase)
                {
                    if (ok)
                    {
                        ok = conductor.RunSymphony(_mpsFilePath, out _resultFilePath, out _errorFilePath, out errorMessage);
                        Assert.IsTrue(ok, errorMessage);
                    }

                    if (ok)
                    {
                        ok = conductor.ImportOptimizationResults(_resultFilePath, null, out errorMessage);
                        Assert.IsTrue(ok, errorMessage);
                    }

                    if (ok)
                    {
                        ok = conductor.ProcessLPSolution(true, 1,  out errorMessage);
                        Assert.IsTrue(ok, errorMessage);
                    }

                }

                if (ok)
                {
                    ok = conductor.DeleteIntermediateFiles(out errorMessage);
                    Assert.IsTrue(ok, errorMessage);
                }

                if (ok)
                {
                    ok = conductor.DeleteIntermediateRecords(out errorMessage);
                    Assert.IsTrue(ok, errorMessage);
                }


            }
        }

        [TestMethod]
        public void TestDeleteIntermediateFiles()
        {
            SymphonyConductor conductor = new SymphonyConductor();

            conductor.ConnectionString = _ConnectionString;
            conductor.ScenId = _scenId;
            conductor.HomeDirectory = _homeDirectory;
            conductor.Code = "BCR";

            string errorMessage = null;

            bool ok = conductor.DeleteIntermediateFiles(out errorMessage);

            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestScenarioRunStatic()
        {
            string errorMessage = null;

            bool ok = SymphonyConductor.RunScenario(_ConnectionString, _homeDirectory, 25, true, false, true, out errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestScenarioTrainingStatic()
        {
            string errorMessage = null;
            bool ok = SymphonyConductor.RunScenario(_ConnectionString, _homeDirectory, 24, true, true, false, out errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }
    }
}
