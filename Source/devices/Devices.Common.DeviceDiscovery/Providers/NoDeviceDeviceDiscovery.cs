using System.Collections.Generic;

namespace Devices.Common.DeviceDiscovery.Providers
{
    public class NoDeviceDeviceDiscovery : IDeviceDiscovery
    {
        public static readonly string NoDeviceVID = "caca";

        public string VID => NoDeviceVID;

        public List<USBDeviceInfo> DeviceInfo { get; set; } = new List<USBDeviceInfo>();

        public bool FindDevices() => false;
    }
}
