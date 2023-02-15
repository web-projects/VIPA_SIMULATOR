using IPA5.Core.Providers;
using IPA5.LoggingService.Client.Config;
using Ninject;
using System;
using System.Reflection;

namespace LoggingService.Client.Providers
{
    internal sealed class LoggingServiceClientProviderImpl : ILoggingServiceClientProvider
    {
        [Inject]
        public ILoggingConfigProvider ConfigProvider { get; set; }

        [Inject]
        public IProtoBufClientProvider ProtoBufClientProvider { get; set; }

        [Inject]
        public IMultiObjectPoolProvider MultiObjectPoolProvider { get; set; }

        public ILoggingServiceClient GetLoggingServiceClient()
        {
            return new LoggingServiceClient(
                ProtoBufClientProvider.GetHttpClient(),
                ConfigProvider.GetClientConfig(),
                Assembly.GetCallingAssembly().GetName().Name,
                TryGetHostName(),
                MultiObjectPoolProvider.GetMultiTypeObjectPool());
        }

        private string TryGetHostName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch (InvalidOperationException)
            {
                // The name of this computer cannot be obtained.
                return "LOCAL";
            }
        }
    }
}
