using Devices.Common.DeviceDiscovery.SerialDevice;
using System.Threading.Tasks;
using static Devices.Common.DeviceDiscovery.DeviceTypeEnum;

namespace Devices.Common.DeviceDiscovery.Filters
{
    public sealed class DeviceFilterSelector : IDeviceFilterSelector
    {
        public ValueTask<DeviceType> InterrogateDeviceTypeAsync(DeviceFilterOptions deviceFilterOptions)
        {
            if (IsMagTekDevice(deviceFilterOptions.PortNumber))
            {
                return new ValueTask<DeviceType>(DeviceType.MagTek);
            }

            if (IsIdTechDevice(deviceFilterOptions.PortNumber))
            {
                return new ValueTask<DeviceType>(DeviceType.IdTech);
            }

            return new ValueTask<DeviceType>(DeviceType.Verifone);
        }

        private bool IsMagTekDevice(string portNumber)
            => portNumber is { }
            && portNumber.IndexOf("IMAGESAFE", 0, System.StringComparison.OrdinalIgnoreCase) > -1;

        private bool IsIdTechDevice(string portNumber)
            => portNumber is { }
            && portNumber.StartsWith(SerialDeviceVid.IdTechVid, System.StringComparison.OrdinalIgnoreCase);

    }
}
