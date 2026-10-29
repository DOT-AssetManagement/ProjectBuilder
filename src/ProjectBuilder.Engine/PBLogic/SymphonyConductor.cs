using log4net;
using log4net.Appender;
using log4net.Layout;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;

#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625


namespace PBLogic
{
    public partial class SymphonyConductor : IDisposable
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
            get { return _log ??= LogManager.GetLogger("PB-LOGGER"); }
            set { _log = value; }
        }

        public string ConnectionString { get; set; }
        public int ScenId { get; set; }

        public string Code = "NONE";

        public string HomeDirectory { get; set; }



        public SymphonyConductor()
        {
            Projects = new Dictionary<int, TimewiseConstrainedProject>();
        }

        public void Dispose()
        {
            Projects.Clear();
            OriginalBudget.Clear();
            BudgetAttributePairs.Clear();
            BudgetConstraints.Clear();
            BudgetAttributePairsReversed.Clear();
        }

        /// <summary>
        /// Logs parameters.  Does not log connection string in Release mode
        /// </summary>
        private void LogParameters()
        {
#if DEBUG
            Log.Info($"Connection string:\t{ConnectionString}");
#endif
            Log.Info($"Home directory:\t{HomeDirectory}");
            Log.Info($"ScenID:\t{ScenId}");
        }


        /// <summary>
        /// Runs Synphony
        /// </summary>
        /// <param name="mpsFilePath">full path to the MPS file</param>
        /// <param name="resultFilePath">(out) full path to the results file (*.txt)</param>
        /// <param name="errorFilePath">(out) full path to the error file (*.err).  Must be empty on success.</param>
        /// <param name="errorMessage">(out) Error message on failure; null on success</param>
        /// <returns>True on success, False on failure</returns>
        public bool RunSymphony(string mpsFilePath, out string resultFilePath, out string errorFilePath, out string errorMessage)
        {
            resultFilePath = null;
            errorMessage = null;
            errorFilePath = null;
            bool ok = true;
            int millisecWait = 24 * 3600 * 1000;

            Log.Info("RunSymphony - started");

            try
            {
                string dirName = Path.GetDirectoryName(mpsFilePath);
                string fileName = Path.GetFileNameWithoutExtension(mpsFilePath);
                resultFilePath = Path.Combine(dirName, fileName + ".txt");
                errorFilePath = Path.Combine(dirName, fileName + ".err");
                string symphFilePath = Path.Combine(dirName, "symphony.exe");
                File.Delete(resultFilePath);
                File.Delete(errorFilePath);


                // cmd.exe's /C parsing strips only the outermost quote pair when a command
                // line contains several separately-quoted segments (exe path, -F path, and
                // the two redirection targets here) — with 4 separate pairs it mis-parses
                // and splits on the first space inside the first path instead. The fix is to
                // wrap the whole inner command in one more enclosing quote pair so cmd strips
                // that one and leaves the 4 inner pairs intact.
                string innerCommand = $@"""{symphFilePath}"" -F ""{mpsFilePath}"" >""{resultFilePath}"" 2>""{errorFilePath}""";

                System.Diagnostics.ProcessStartInfo pInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    CreateNoWindow = false,
#if DEBUG
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
#else
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized,
#endif
                    FileName = @"CMD.EXE",
                    Arguments = $@"/C ""{innerCommand}"""
                };
                Log.Info($"Arguments:\t{pInfo.Arguments}");

                System.Diagnostics.Process process = System.Diagnostics.Process.Start(pInfo);

                ok = process.WaitForExit(millisecWait);
                if (!ok)
                {
                    throw new Exception($"Process timed out after {millisecWait / 10000} seconds");
                }
                if (File.Exists(errorFilePath))
                {
                    errorMessage = File.ReadAllText(errorFilePath);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        ok = false;
                    }
                }

            }
            catch (Exception ex)
            {
                ok = false;
                errorMessage = ex.Message;
                Log.Error(errorMessage);
            }

            Log.Info($"RunSymphony - ended\t OK={ok}, Error message: {errorMessage}");

            return ok;
        }


        /// <summary>
        /// Deletes intermediate files (MPS, results, error) after solution results have been imported
        /// </summary>
        /// <param name="errorMessage">(out) Error message on failure;  null on success</param>
        /// <returns>True on failure, False on success</returns>
        public bool DeleteIntermediateFiles(out string errorMessage)
        {
            bool ok = true;
            errorMessage = null;

            Log.Info("Delete intermediate files - started...");

            try
            {
                string LPDir = Path.Combine(HomeDirectory, "LP");
                Directory.GetFiles(LPDir, $"*SC_{ScenId}*.*", SearchOption.TopDirectoryOnly).ToList().ForEach(File.Delete);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                ok = false;
                Log.Error(errorMessage);
            }

            Log.Info("Delete intermediate files - ended.");

            return ok;
        }


    }


}
