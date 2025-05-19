using PAMSDataImporter;
using BAMSDataImporter;
using PBLogic;

namespace PBLogicNUnit.Test
{
    public class PAMSTest
    {
        // string _JsonFilePath = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\ARA_Database\PAMS_IAMV2_ForBimal\PAMS-f9efa037-1fcb-41a6-9d64-0a5a91de09e3.json";

        string _JsonFilePath = @"C:\Projects\FHWA-SpyPond\PennDOT-2022\ARA_Database\PAMS_IAMV2_ForBimal\PAMS-District8.Formatted.json";
        string _PAMSDBConnectionString = @"Data Source = .\SQL2019;Initial Catalog=IAMv2; Persist Security Info=True; Integrated Security = True; MultipleActiveResultSets=True";
        string _TargetDBConnectionString = @"Data Source = .\SQL2019;Initial Catalog = JPAMS; Persist Security Info=True;User ID = ProjectBuilder; Password=PennDOT;MultipleActiveResultSets=True";
        string _EPOSConnextionString = @"Data Source = .\SQL2019;Initial Catalog = EPOS_2; Persist Security Info=True;User ID = ProjectBuilder; Password=PennDOT;MultipleActiveResultSets=True";

        [Test]
        public void TestJSONDeSerialization()
        {
            PAMSDataImporter.ImportManager.JsonFilePath = _JsonFilePath;
            PAMSDataImporter.ImportManager.TargetDbConnectionString = _TargetDBConnectionString;
            bool ok = PAMSDataImporter.ImportManager.ImportJsonFile(out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [Test]
        public void TestMaintainableAssetsImport()
        {
            PAMSDataImporter.ImportManager.TargetDbConnectionString = _TargetDBConnectionString;
            PAMSDataImporter.ImportManager.PAMSDbConnectionString = _PAMSDBConnectionString;
            bool ok = PAMSDataImporter.ImportManager.ImportMaintainableAssets(out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [Test]
        public void TestImportPAMSSegments()
        {
            DataImport.PAMSConnectionString = _PAMSDBConnectionString;
            DataImport.PBConnectionString = _EPOSConnextionString;
            bool ok = DataImport.ImportPAMSSectionSegmentation(out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [Test]
        public void TestPAMSPreferredProjects()
        {
            Projector.PBConnectionString = _EPOSConnextionString;
            bool ok = Projector.BuildProjects(0, null, 8, "P", ClusteringSource.csPAMS, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
        }

        [Test]
        public void TestPAMS2JPAMS()
        {
        /*
            bool ok = ImportManager.PAMS2JPAMS(null, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            */


            bool ok = ImportManager.PAMS2JPAMS(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Data\config.json", out string errorMessage);
            Assert.IsTrue(ok, errorMessage);

        }

        [Test]
        public void TestJPAMSTruncate()
        {
            /*
            bool ok = ImportManager.PAMS2JPAMS(null, out string errorMessage);
            Assert.IsTrue(ok, errorMessage);
            */


            bool ok = ImportManager.TruncateJpAndPamsTables(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Data\PAMSconfig.json", out string errorMessage);
           

        }

        [Test]
        public void TestBAMS2JBAMS()
        {
           /*         
           bool ok = BAMSImportManager.BAMS2JBAMS(null, out string errorMessage);
           Assert.IsTrue(ok, errorMessage);
           */
           
            bool ok = BAMSImportManager.BAMS2JBAMS(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Data\config.json", out string errorMessage);
            Assert.IsTrue(ok, errorMessage);

        }

        [Test]
        public void TestJBAMSTruncate()
        {
         

            bool ok = BAMSImportManager.TruncateJpAndBamsTables(@"C:\Projects\FHWA-SpyPond\PennDOT-2022\Data\config.json", out string errorMessage);


        }
    }
}
