namespace Devices.Common
{
    public sealed class ActionContainer
    {
        public string ActionName { get; set; }
        public string DisplayName { get; set; }
        public DeviceInformation DeviceInformation { get; set; }
        public bool ActionRegistered { get; set; }
        public string Request { get; set; }
        public string ConfirmationMessage { get; set; }
    }
}
