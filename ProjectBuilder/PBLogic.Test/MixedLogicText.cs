using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PBLogic;

namespace PBLogicTest
{
    [TestClass]
    public class MixedLogicText
    {
        private string _ConnectionString = @"Server=DESKTOP-EUKV3FS\SQL2019NEW;Database=SPP_PBv1;Trusted_Connection=True;MultipleActiveResultSets=True";
        private string _homeDirectory = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\PBLogic";


        [TestMethod]
        public void THE_TestExtendedScenario()
        {
           
             bool  ok = SymphonyConductor.RunExtendedScenario(_ConnectionString, _homeDirectory, 8, false,
                   out string errorMessage);
                   

            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestMixedOptimizationFull()
        {
            bool ok = DataManager.PopulateScenarioTreatmentsTable(_ConnectionString, 8, out string errorMessage);

            if (ok)
            {
                 ok = DataManager.CreateExtendedScenarioProjects(_ConnectionString, 8, out errorMessage);
            }

            if (ok)
            {
                ok = SymphonyConductor.RunMixedScenarioFull(_ConnectionString, _homeDirectory, 8,
                    new ScenarioCommunique
                    {
                        Commitment = false,
                        District = null,
                        ProjectsOnly = false,
                        SingleTreatmentsOnly = false,
                        MaxPriority = 10,
                        MixAssetBudgets = false
                    }
                    , 2, false, out errorMessage);
            }

            if (ok)
            Assert.IsTrue(ok, errorMessage);
        }

        /*
        [TestMethod]
        public void TestMixedOptimizationStepByStep()
        {
            int scenId = 1043;

            bool ok = DataManager.PopulateScenarioTreatmentsTable(_ConnectionString, scenId, out string errorMessage);

            if (ok)
            {
                ok = DataManager.CreateScenarioProjects(_ConnectionString, scenId, true,
                   true, true, true, true, true, true, out errorMessage);
            }

            if (ok)
            {
                ok = SymphonyConductor.RunMixedScenario(_ConnectionString, _homeDirectory, scenId,
                                      false, out errorMessage);
            }

            if (ok)
            {
                ok = DataManager.CreateScenarioProjects(_ConnectionString, scenId, false,
                  true, true, true, true, true, true, out errorMessage);
            }

            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void TestSingleTreatmentOptimizationFull()
        {
            bool ok = DataManager.CreateScenarioProjects(_ConnectionString, 1, true,
                    true, true, true, true, true, true, out string errorMessage);

            if (ok)
            {
                ok = SymphonyConductor.RunMixedScenarioFull(_ConnectionString, _homeDirectory, 1, new ScenarioCommunique
                {
                    Commitment = false,
                    District = null,
                    ProjectsOnly = false,
                    SingleTreatmentsOnly = true,
                    MaxPriority = 10,
                    MixAssetBudgets = false
                }, 2, false, out errorMessage);
            }
            Assert.IsTrue(ok, errorMessage);
        }

        [TestMethod]
        public void THE_TestCreateExtendedScenarioProjects()
        {
            bool ok = DataManager.CreateExtendedScenarioProjects(_ConnectionString, 8, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

    
        [TestMethod]
        public void MAIN_TestScenarioRun()
        {
            int scenId = 3;
            //int scenId = 1044;
            //int scenId = 1043;
            //int scenId = 1045;    // exact copy of ScenId=1
            //int scenId = 1046;    // exact copy of ScenId=1044
            //int scenId = 1047;    // exact copy of 1043
            //int scenId = 1048;    // copy of 1043 but with mixed budget
            //int scenId = 1050;      // exact copy of 1048



            bool ok = SymphonyConductor.RunMixedScenario(_ConnectionString, _homeDirectory, scenId, true, out string errorMessage);

            Assert.IsTrue(ok, errorMessage);
        }
    

        [TestMethod]
        public void TestMultipleScenariosRun()
        {
            //int scenId = 1;
            //int scenId = 1044;
            //int scenId = 1043;
            //int scenId = 1045;    // exact copy of ScenId=1
            //int scenId = 1046;    // exact copy of ScenId=1044
            //int scenId = 1047;    // exact copy of 1043
            //int scenId = 1048;    // copy of 1043 but with mixed budget
            //int scenId = 1050;      // exact copy of 1048

            foreach (int scenId in new int[] { 1043, 1044, 1045, 1046, 1047 })
            {
                bool ok = SymphonyConductor.RunMixedScenario(_ConnectionString, _homeDirectory, scenId, true, out string errorMessage);
                Assert.IsTrue(ok, errorMessage);
            }
        }

        **/
    }
}
