using Devices.Common.DeviceDiscovery;
using Devices.Common.DeviceDiscovery.Providers;
using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;

namespace Devices.MagTek.Connection
{
    public sealed class MagTekDeviceDiscovery : IDeviceDiscovery
    {
        public static readonly string MAGTEK_VID = "0801";
        private const string PIDImageSafe = "2234";

        public string VID => MAGTEK_VID;

        public List<USBDeviceInfo> DeviceInfo { get; set; } = new List<USBDeviceInfo>();

        public bool FindDevices()
        {
            DeviceInfo.Clear();
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher($"Select * From Win32_PnPEntity WHERE DeviceID Like \"%usb%vid_{MAGTEK_VID}%\" "))
            {
                collection = searcher.Get();    //Filter for USB devices with a specific VID
            }
            foreach (var device in collection)
            {
                string deviceID = device.GetPropertyValue("DeviceID")?.ToString();
                if (string.IsNullOrWhiteSpace(deviceID))
                {
                    continue;
                }
                string[] deviceCfg = Regex.Split(deviceID, @"\\");
                if (deviceCfg.Length == 3)
                {
                    Regex rg = new Regex(@"&PID_[0-9a-zA-Z\s]{0,4}", RegexOptions.IgnoreCase);
                    MatchCollection matched = rg.Matches(deviceCfg[1]);
                    if (matched.Count > 0 && matched[0]?.Value.Substring(1).IndexOf(PIDImageSafe, StringComparison.CurrentCultureIgnoreCase) > 0)
                    {
                        DeviceInfo.Add(new USBDeviceInfo(deviceID, matched[0]?.Value.Substring(1), deviceCfg[2]));
                    }
                }
            }
            collection.Dispose();

            foreach (var device in DeviceInfo)
            {
                device.ComPort = device.DeviceID;
            }

            return DeviceInfo.Count > 0;
        }
    }
}
