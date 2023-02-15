using IPA5.Core.Providers;
using IPA5.LoggingService.Client.Config;
using IPA5.LoggingService.Client.Providers;
using Ninject.Modules;

namespace LoggingService.Client.Modules
{
    public class LoggingServiceClientModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILoggingConfigProvider>().To<LoggingConfigProviderImpl>().InSingletonScope();
            Bind<ILoggingServiceClientProvider>().To<LoggingServiceClientProviderImpl>();
            Bind<IMultiObjectPoolProvider>().To<LoggingClientMultiObjectPoolProvider>().InSingletonScope();
        }
    }
}
