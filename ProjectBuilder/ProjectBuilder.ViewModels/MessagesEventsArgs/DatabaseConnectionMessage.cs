using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class DatabaseConnectionMessage
    {
        public string DatabaseName { get;  }
        public bool HasConnected { get; }

        public DatabaseConnectionMessage(string databaseName,bool hasConnected)
        {
            DatabaseName = databaseName;
            HasConnected = hasConnected;
        }
    }
}
