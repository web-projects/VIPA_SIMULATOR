using Devices.Common.DeviceDiscovery.SerialDevice;
using System;
using System.Collections.Generic;
using System.Management;

namespace Devices.Common.DeviceDiscovery.Providers
{
    public sealed class IdTechDeviceDiscovery : IDeviceDiscovery
    {
        public static readonly string IDTECH_VID = "0ACD";

        public string VID => IDTECH_VID;

        public static readonly string DeviceId = "DeviceID";
        private static readonly string pnpDeviceId = "PNPDeviceID";
        private static readonly string description = "Description";
        private static readonly string caption = "Caption";
        public static readonly string Usb = "USB\\";
        private static readonly string pid = "PID_";
        private static readonly string specificSearcherString = $"Select * From Win32_PnPEntity WHERE DeviceID Like \"%usb%{SerialDeviceVid.IdTechVid}%\" ";

        public List<USBDeviceInfo> DeviceInfo { get; } = new List<USBDeviceInfo>();

        public bool FindDevices() => GetUSBDevices() > 0;

        private int GetUSBDevices()
        {
            DeviceInfo.Clear();

            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(specificSearcherString);

            using ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementBaseObject device in collection)
            {
                string deviceIDStr = device.GetPropertyValue(DeviceId)?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(deviceIDStr))
                {
                    continue;
                }

                if (deviceIDStr.Contains(Usb, StringComparison.OrdinalIgnoreCase)
                    && deviceIDStr.Contains(SerialDeviceVid.IdTechVid, StringComparison.OrdinalIgnoreCase))
                {
                    USBDeviceInfo usbDeviceInfo = new USBDeviceInfo(
                        deviceIDStr,
                        device.GetPropertyValue(pnpDeviceId)?.ToString(),
                        device.GetPropertyValue(description)?.ToString(),
                        device.GetPropertyValue(caption)?.ToString()
                    );

                    usbDeviceInfo.ProductID = usbDeviceInfo.DeviceID.Substring(usbDeviceInfo.DeviceID.IndexOf(pid, 0, StringComparison.OrdinalIgnoreCase) + 4, 4);
                    usbDeviceInfo.ComPort = $"{SerialDeviceVid.IdTechVid}_{pid}{usbDeviceInfo.ProductID}";

                    DeviceInfo.Add(usbDeviceInfo);
                }
            }

            return DeviceInfo.Count;
        }
    }
}
