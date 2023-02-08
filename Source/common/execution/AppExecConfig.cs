using Common.Execution;
using System;

namespace Execution
{
    public class AppExecConfig
    {
        public bool TerminalBypassHealthRecord { get; set; }
        public bool DisplayProgressBar {get; set; }
        public ConsoleColor ForeGroundColor { get; set; }
        public ConsoleColor BackGroundColor { get; set; }
        public Modes.Execution ExecutionMode { get; set; }
        public string HealthCheckValidationMode { get; set; }
    }
}
