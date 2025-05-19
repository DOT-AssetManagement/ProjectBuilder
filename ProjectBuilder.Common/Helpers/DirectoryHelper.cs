using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Common.Helpers
{
    public static class DirectoryHelper
    {
        public static string MainApplicationDirectory { get; private set; }
        static DirectoryHelper()
        {
            CreateMainDirectory();
        }

        private static void CreateMainDirectory()
        {
            MainApplicationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Project Builder");
            if (Directory.Exists(MainApplicationDirectory))
                return;
            Directory.CreateDirectory(MainApplicationDirectory);
        }
        public static string GetDirectory(string directoryName)
        {
            var directory = Path.Combine(MainApplicationDirectory, directoryName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
