using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using System.Data.SqlClient;
using System.Data;


namespace PBLogic
{
    public class DataImport
    {
#if DEBUG
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

                File = @".\Logs\ProjectBuilder.NET.log" // all logs will be created in the subdirectory logs 
            };

            // Configure filter to accept log messages of any level.
            log4net.Filter.LevelMatchFilter traceFilter = new log4net.Filter.LevelMatchFilter
            {
#if DEBUG
                LevelToMatch = log4net.Core.Level.Debug
#else
                LevelToMatch = log4net.Core.Level.Info
#endif
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

#else
        private static ILog _log;
#endif

        public static ILog Log
        {
            get { return _log; }
            set { _log = value; }
        }

        public static string PAMSConnectionString { get; set; }
        public static string BAMSConnectionString { get; set; }
        public static string PBConnectionString { get; set; }

        public static bool ImportPAMSSectionSegmentation(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;
            Log.Info("ImportPAMSSectionInfo - started...");

            try
            {
                using (SqlConnection conn = new SqlConnection(PAMSConnectionString))
                {
                    conn.Open();

                    using (SqlConnection PBConne = new SqlConnection(PBConnectionString))
                    {
                        PBConne.Open();
                        
                        using(SqlCommand c =  new SqlCommand("TRUNCATE TABLE tbl_pb_PAMSSectionSegmentation", PBConne))
                        {
                            int N  = c.ExecuteNonQuery();
                        }

                        using (BulkInserter bi = new BulkInserter())
                        {

                            bi.Configure(PBConne, "tbl_pb_PAMSSectionSegmentation",
                                                    "PAMSSectionSegmentation", null, null, out errorMessage);

                            if (!ok)
                            {
                                throw new Exception(errorMessage);
                            }

                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = conn;
                                command.CommandText = @"SELECT d.ID AS AssetID, 
	CONVERT(INT,dd.TextValue) AS District,
	CONVERT(INT,dc.TextValue) AS Cnty,
	CONVERT(INT,dr.TextValue) AS [Route],
	a.FacilityName, 
	CONVERT(INT,a.SectionName) AS [Seg], 
	a.Area, 
	d.NumericValue AS [Length],
	dw.NumericValue AS [Width],
	dn.NumericValue AS [Lanes],
    dinter.TextValue AS Interstate
FROM AttributeDatum d WITH (NOLOCK)
INNER JOIN Attribute t WITH (NOLOCK)
		ON t.Id = d.AttributeId AND (t.[Name] LIKE '%LENGTH%')
INNER JOIN MaintainableAsset a WITH (NOLOCK)
	ON a.ID = d.MaintainableAssetId
INNER JOIN AttributeDatum dw WITH (NOLOCK)
		ON dw.MaintainableAssetId = a.Id
INNER JOIN Attribute tw WITH (NOLOCK)
		ON tw.Id = dw.AttributeId AND (tw.[Name] = 'WIDTH')
INNER JOIN AttributeDatum dn WITH (NOLOCK)
		ON dn.MaintainableAssetId = a.Id
INNER JOIN Attribute tn WITH (NOLOCK)
		ON tn.Id = dn.AttributeId AND (tn.[Name] = 'LANES')
INNER JOIN AttributeDatum dd WITH (NOLOCK)
		ON dd.MaintainableAssetId = a.Id
INNER JOIN Attribute td WITH (NOLOCK)
		ON td.Id = dd.AttributeId AND (td.[Name] = 'DISTRICT')
INNER JOIN AttributeDatum dc WITH (NOLOCK)
		ON dc.MaintainableAssetId = a.Id
INNER JOIN Attribute tc WITH (NOLOCK)
		ON tc.Id = dc.AttributeId AND (tc.[Name] = 'CNTY')
INNER JOIN AttributeDatum dr WITH (NOLOCK)
		ON dr.MaintainableAssetId = a.Id
INNER JOIN Attribute tr WITH (NOLOCK)
		ON tr.Id = dr.AttributeId AND (tr.[Name] = 'SR')
INNER JOIN AttributeDatum dinter WITH (NOLOCK)
		ON dinter.MaintainableAssetId = a.Id
INNER JOIN Attribute tinter WITH (NOLOCK)
		ON tinter.Id = dinter.AttributeId AND (tinter.[Name] = 'INTERSTATE')
ORDER BY CONVERT(INT,SectionName)";
                                command.CommandTimeout = 600;
                                using (IDataReader r = command.ExecuteReader())
                                {
                                    bi.DTable.Load(r);
                                    r.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info("ImportPAMSSectionInfo - ended.");
            return (ok);
        }


    }
}
