using Devices.Common.DeviceDiscovery.Filters;

namespace Devices.Common.DeviceDiscovery.Providers
{
    public interface IDeviceFilterSelectorProvider
    {
        IDeviceFilterSelector GetDeviceFilterSelector();
    }
}
