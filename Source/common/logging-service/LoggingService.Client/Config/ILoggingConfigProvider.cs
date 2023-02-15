using Microsoft.Extensions.Configuration;

namespace LoggingService.Client.Config
{
    interface ILoggingConfigProvider
    {
        LoggingClientConfig GetClientConfig();
        LoggingClientConfig GetClientConfig(IConfiguration configuration);
    }
}
