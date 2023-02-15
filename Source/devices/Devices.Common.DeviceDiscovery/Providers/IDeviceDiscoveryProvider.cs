using System.Collections.Generic;
using static Devices.Common.DeviceDiscovery.DeviceTypeEnum;

namespace Devices.Common.DeviceDiscovery.Providers
{
    public interface IDeviceDiscoveryProvider
    {
        IDeviceDiscovery GetDeviceDiscovery(DeviceType deviceType);
        List<IDeviceDiscovery> GetDeviceDiscoveryList();
    }
}
