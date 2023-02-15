using System;
using IPA5.Common.Helpers.Config.Binder;
using static IPA5.Common.Helpers.Config.SharedConfigKeys;

namespace LoggingService.Client.Config
{
    [Serializable]
    public sealed class LoggingClientConfig
    {
        [EnvConfigBinder(LoggingServiceUrlKey)]
        public string Url { get; set; }
    }
}
