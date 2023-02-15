using System.Threading.Tasks;
using static Devices.Common.DeviceDiscovery.DeviceTypeEnum;

namespace Devices.Common.DeviceDiscovery.Filters
{
    public interface IDeviceFilterSelector
    {
        ValueTask<DeviceType> InterrogateDeviceTypeAsync(DeviceFilterOptions deviceFilterOptions);
    }
}
