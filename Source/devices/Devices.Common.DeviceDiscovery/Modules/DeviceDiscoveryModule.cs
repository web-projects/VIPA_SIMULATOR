using Devices.Common.DeviceDiscovery.Providers;
using Ninject.Modules;

namespace Devices.Common.DeviceDiscovery.Modules
{
    public sealed class DeviceDiscoveryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDeviceFilterSelectorProvider>().To<DeviceFilterSelectorProvider>();
            Bind<IDeviceDiscoveryProvider>().To<DeviceDiscoveryProviderImpl>();
        }
    }
}
