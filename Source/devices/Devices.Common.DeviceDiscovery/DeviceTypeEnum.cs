using Common.Helpers;
using static Devices.Common.DeviceDiscovery.SupportedDeviceTypes;

namespace Devices.Common.DeviceDiscovery
{
    public sealed class DeviceTypeEnum
    {
        public enum DeviceType
        {
            [StringValue(VerifoneDeviceType)]
            Verifone = 1,
            [StringValue(IdTechDeviceType)]
            IdTech = 2,
            [StringValue(SimulatorDeviceType)]
            Simulator = 3,
            [StringValue(MockDeviceType)]
            Mock = 4,
            [StringValue(NullDeviceType)]
            NoDevice = 5,
            [StringValue(MagTekDeviceType)]
            MagTek = 6,
        }
    }
}
