using Common.Ninject;
using Devices.Sdk.Features.Modules;
using Devices.SDK.Modules;
using Ninject;
using Ninject.Modules;
using System.Collections.Generic;

namespace Devices.Sdk.Features
{
    internal sealed class SdkFeatureResolver : IKernelModuleResolver
    {
        private const int NumberOfKnownModules = 6;

        public IKernel ResolveKernel(params NinjectModule[] modules)
        {
            List<NinjectModule> moduleList;

            if (modules != null && modules.Length > 0)
            {
                moduleList = new List<NinjectModule>(NumberOfKnownModules + modules.Length);
                moduleList.AddRange(modules);
            }
            else
            {
                moduleList = new List<NinjectModule>(NumberOfKnownModules);
            }

            moduleList.Add(new DeviceSdkModule());
            moduleList.Add(new DALSdkFeaturesModule());
            //moduleList.Add(new BrokerConnectorModule());
            //moduleList.Add(new LoggingServiceClientModule());
            //moduleList.Add(new IPA5CoreModule());
            //moduleList.Add(new BridgeModule());

            IKernel kernel = new StandardKernel(moduleList.ToArray());

            kernel.Settings.InjectNonPublic = true;
            kernel.Settings.InjectParentPrivateProperties = true;

            return kernel;
        }
    }
}
