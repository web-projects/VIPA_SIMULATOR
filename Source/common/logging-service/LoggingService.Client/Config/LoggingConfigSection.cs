using System;

namespace LoggingService.Client.Config
{
    [Serializable]
    sealed class LoggingConfigSection
    {
        public LoggingClientConfig Configuration { get; } = new LoggingClientConfig();
    }
}
