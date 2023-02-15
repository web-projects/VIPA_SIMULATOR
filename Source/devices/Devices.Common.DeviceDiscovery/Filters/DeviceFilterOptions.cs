namespace Devices.Common.DeviceDiscovery.Filters
{
    public sealed class DeviceFilterOptions
    {
        public const int DefaultDelayForIdTechHidSwitchSeconds = 15;

        public string PortNumber { get; set; }
        public bool SuppressIdTechDevices { get; set; }
        public int DelayForIdTechHidSwitchSeconds { get; set; }
    }
}
