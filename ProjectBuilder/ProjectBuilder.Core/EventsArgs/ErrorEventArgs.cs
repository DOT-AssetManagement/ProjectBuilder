using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class ErrorEventArgs : EventArgs
    {
        public static ErrorEventArgs None { get => (ErrorEventArgs)Empty; }
        public string  ErrorMessage { get;}
        public LogLevel Level { get; }
        public ErrorEventArgs(string errorMessage,LogLevel level)
        {
            ErrorMessage = errorMessage;
            Level = level;
        }
    }
    public class ScenarioEventArgs : ErrorEventArgs
    {
        public int ScenarioId { get; }
        public ScenarioEventArgs(string errorMessage, LogLevel level,int scenarioId) : base(errorMessage,level)
        {
            ScenarioId = scenarioId;
        }
    }
}
