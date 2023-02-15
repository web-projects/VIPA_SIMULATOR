using Devices.Common.DeviceDiscovery.Filters;

namespace Devices.Common.DeviceDiscovery.Providers
{
    public sealed class DeviceFilterSelectorProvider : IDeviceFilterSelectorProvider
    {
        public IDeviceFilterSelector GetDeviceFilterSelector() => new DeviceFilterSelector();
    }
}
