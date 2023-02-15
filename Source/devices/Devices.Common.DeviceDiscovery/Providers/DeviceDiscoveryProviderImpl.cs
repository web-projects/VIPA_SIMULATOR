using Devices.MagTek.Connection;
using System.Collections.Generic;
using static Devices.Common.DeviceDiscovery.DeviceTypeEnum;

namespace Devices.Common.DeviceDiscovery.Providers
{
    public sealed class DeviceDiscoveryProviderImpl : IDeviceDiscoveryProvider
    {
        public IDeviceDiscovery GetDeviceDiscovery(DeviceType deviceType) => deviceType switch
        {
            DeviceType.Verifone => new VerifoneDeviceDiscovery(),
            DeviceType.MagTek => new MagTekDeviceDiscovery(),
            DeviceType.IdTech => new IdTechDeviceDiscovery(),
            _ => new NoDeviceDeviceDiscovery()
        };

        public List<IDeviceDiscovery> GetDeviceDiscoveryList()
        {
            List<IDeviceDiscovery> deviceDiscoveryList = new List<IDeviceDiscovery>();

            deviceDiscoveryList.Add(GetDeviceDiscovery(DeviceType.MagTek));
            deviceDiscoveryList.Add(GetDeviceDiscovery(DeviceType.Verifone));
            deviceDiscoveryList.Add(GetDeviceDiscovery(DeviceType.IdTech));

            return deviceDiscoveryList;
        }
    }
}
