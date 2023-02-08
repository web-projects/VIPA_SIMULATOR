namespace Config.Application
{
    public class Verifone
    {
        public int SortOrder { get; set; }
        public string[] SupportedDevices { get; set; }
        public string ConfigurationHostId { get; set; }
        public string OnlinePinKeySetId { get; set; }
        public string ADEKeySetId { get; set; }
        public string ConfigurationPackageActive { get; set; }
        public string ActiveCustomerId { get; set; }
        public string Reboot24Hour { get; set; }
        public bool AllowDebugCommands {get; set; }
        public string HealthStatusValidationRequired { get; set; }
    }
}
