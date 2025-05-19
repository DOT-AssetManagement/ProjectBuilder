using System;
using BAMSDataImporter;
using log4net;
using log4net.Config;
using log4net.Appender;
using System.IO;
using log4net.Layout;

namespace BAMSImportCL
{
    class Program
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

                File = @".\Logs\BAMSImport.log" // all logs will be created in the subdirectory logs 
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

            log4net.ILog log = log4net.LogManager.GetLogger("BAMSImport");

            log.Debug("BAMSImport logger created.");

            return log;
        }

        public static ILog Log
        {
            get { return _log; }
            set { _log = value; }
        }

        private static bool _showHelp = false;
        private static bool _truncate = false;
        private static bool _import = false;

        private static string _helpMessage = $@"
The program expects its configuration file the config.json to exist in its home directory.
Please edit this file appropriately before calling the program.

The program runs from the command line with the following optional arguments:

>BAMSImport[.exe] [/T] [/I] [/?|/H]
- /T - to truncate tables in the target JBAMS database;
- /I - to import data from BAMS database into JBAMS;
- /? or /H- to display this message.

When both  /T and /I arguments are used then target tables are truncated and then populated importing data from BAMS.
This is also the default behavior for the case when no arguments are supplied.

If /T is used without /I then only truncation occurs.
If /I us used without /T then import occurs into non-truncated tables, which may cause primary key violations.
If /? or /H is used then other arguments are ignored.
";

        private static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static bool ParseArguments(string[] args, out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            if (args.Length < 1)
            {
                _truncate = true;
                _import = true;
                _showHelp = false;
            }

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string a = args[i];

                    if (a[0] != '-' && a[0] != '/')
                    {
                        throw new Exception($"{a} is an invalid command-line argument.\n");
                    }

                    switch (a.Substring(0, 2).ToUpper())
                    {
                        case "/I":
                        case "-I":
                            _import = true;
                            break;
                        case "/T":
                        case "-T":
                            _truncate = true;
                            break;
                        case "/?":
                        case "-?":
                        case "/H":
                        case "-H":
                            _showHelp = true;
                            _import = false;
                            _truncate = false;
                            break;
                        default:
                            throw new Exception($"Unexpected argument {a} in the command line\n");
                    }
                }

                if (!_truncate && !_import && !_showHelp)
                {
                    _truncate = true;
                    _import = true;
                }
            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
            }

            return (ok);
        }

        static void Main(string[] args)
        {
            Log.Info("BAMSImport started...");
            string configFileName = "config.json";

            BAMSDataImporter.BAMSImportManager.Log = Log;

            if (!File.Exists(configFileName))
            {
                WriteError("\nconfig.json file not found.");
                return;
            }

            bool ok = ParseArguments(args, out string errorMessage);
            if (!ok)
            {
                WriteMessage(_helpMessage);
                WriteError(errorMessage);
                return;
            }

            if (ok && _showHelp)
            {
                WriteMessage(_helpMessage);
                return;
            }

            if (ok && _truncate)
            {
                ok = BAMSDataImporter.BAMSImportManager.TruncateJpAndBamsTables(configFileName, out errorMessage);
            }

            if (ok && _import)
            {
                ok = BAMSDataImporter.BAMSImportManager.BAMS2JBAMS(configFileName, out errorMessage);
            }

            if (ok)
            {
                WriteMessage("\nAll done!");
            }
            else
            {
                WriteError("\n" + errorMessage);
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadKey();

            return;
        }
    }

}
