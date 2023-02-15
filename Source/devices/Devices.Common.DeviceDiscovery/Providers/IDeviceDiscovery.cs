using System.Collections.Generic;

namespace Devices.Common.DeviceDiscovery.Providers
{
    public interface IDeviceDiscovery
    {
        public string VID { get; }
        public List<USBDeviceInfo> DeviceInfo { get; }
        public bool FindDevices();
    }
}
