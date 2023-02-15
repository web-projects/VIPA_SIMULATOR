using IPA5.Core.Extensions;
using IPA5.Core.Providers;
using Microsoft.Extensions.Configuration;
using System;
using static IPA5.LoggingService.Client.LoggingConstants;

namespace LoggingService.Client.Config
{
    internal sealed class LoggingConfigProviderImpl : ConfigProviderBase, ILoggingConfigProvider
    {
        private static LoggingConfigSection appConfig;

        public LoggingClientConfig GetClientConfig()
        {
            if (appConfig != null)
            {
                return appConfig.Configuration;
            }

            return GetClientConfig(GetConfiguration());
        }

        public LoggingClientConfig GetClientConfig(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (appConfig != null)
            {
                return appConfig.Configuration;
            }

            appConfig = configuration.GetSection(LoggingSectionKey).GetByEnv<LoggingConfigSection>();

            if (appConfig == null)
            {
                throw new Exception("Unable to find a configuration section for the logging service client.");
            }

            return appConfig.Configuration;
        }
    }
}
