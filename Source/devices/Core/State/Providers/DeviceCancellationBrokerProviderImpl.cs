using Devices.Core.Providers;
using Devices.Core.Cancellation;

namespace DAL.Core.State.Providers
{
    internal class DeviceCancellationBrokerProviderImpl : IDeviceCancellationBrokerProvider
    {
        public IDeviceCancellationBroker GetDeviceCancellationBroker() => new DeviceCancellationBrokerImpl();
    }
}
