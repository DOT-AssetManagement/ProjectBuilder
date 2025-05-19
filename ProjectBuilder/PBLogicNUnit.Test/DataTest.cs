using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using PBLogic;
using System.Data;

namespace PBLogicTest
{
    [TestClass]
    public class DataTest
    {

        private string _ConnectionString = @"Server=DESKTOP-EUKV3FS\SQL2019;Database=SPP_PBv1;Trusted_Connection=True;MultipleActiveResultSets=True";
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
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(ok, errorMessage);
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
           
          //  bool ok = DataManager.CreateNewScenario(_ConnectionString, "Scenario 10", 2022, 2031, out int newScenId, out string errorMessage);
            //Assert.IsTrue(ok, errorMessage);
            //Assert.IsTrue(newScenId > 0, $"newScen");
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
        public void TestDataImport()
        {
            //bool ok = DataManager.ImportDataFromPAMSandBAMS(_ConnectionString, true, true, out string errorMessage);
            //Assert.IsTrue(ok, errorMessage);
        }
    }
}
