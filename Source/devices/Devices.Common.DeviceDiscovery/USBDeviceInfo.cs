namespace Devices.Common.DeviceDiscovery
{
    public sealed class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceID, string productID, string serialNum)
        {
            DeviceID = deviceID;
            ProductID = productID;
            SerialNumber = serialNum;
        }

        public USBDeviceInfo(string deviceID, string pnpDeviceID, string description, string caption)
        {
            DeviceID = deviceID;
            PnpDeviceID = pnpDeviceID;
            Description = description;
            Caption = caption;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
        public string Caption { get; private set; }

        public string Identifier { get; set; }
        public string ProductID { get; set; }
        public string SerialNumber { get; set; }
        public string ComPort { get; set; }
    }
}
