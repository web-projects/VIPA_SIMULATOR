using Devices.Common.Interfaces;

namespace Devices.Sdk.Features.Interrupt
{
    internal interface IInterruptFeatureValidator
    {
        bool CanInterruptFeatureHandle(IDeviceInterruptFeature feature, ICardDevice cardDevice);
    }
}
