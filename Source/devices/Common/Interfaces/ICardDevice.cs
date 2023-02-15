using Common.XO.Requests;
using Common.XO.Responses;
using Devices.Common.AppConfig;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static Common.XO.Responses.LinkEventResponse;

namespace Devices.Common.Interfaces
{
    public delegate void PublishEvent(EventTypeType eventType, EventCodeType eventCode,
            List<LinkDeviceResponse> devices, object request, string message);

    public interface ICardDevice : IPaymentDevice
    {
        event PublishEvent PublishEvent;
        event DeviceEventHandler DeviceEventOccured;

        string Name { get; }

        string ManufacturerConfigID { get; }

        int SortOrder { get; set; }

        DeviceInformation DeviceInformation { get; }

        bool IsConnected(object request);

        void SetDeviceSectionConfig(DeviceSection config);

        List<DeviceInformation> DiscoverDevices();

        List<LinkErrorValue> Probe(DeviceConfig config, DeviceInformation deviceInfo, out bool dalActive);

        void DeviceSetIdle();

        bool DeviceRecovery();

        void Disconnect();

        List<LinkRequest> GetDeviceResponse(LinkRequest deviceInfo);

        // ------------------------------------------------------------------------
        // Methods that are mapped for usage in their respective sub-workflows.
        // ------------------------------------------------------------------------
        LinkRequest GetStatus(LinkRequest linkRequest);
        LinkRequest AbortCommand(LinkRequest linkRequest);
        LinkRequest ManualCardEntry(LinkRequest linkRequest, CancellationToken cancellationToken);

        // ------------------------------------------------------------------------
        // FEATURE: PAYMENT
        // ------------------------------------------------------------------------
        LinkRequest PresentCard(LinkRequest request, CancellationToken cancellationToken);
        ValueTask<LinkRequest> GetCardDataAsync(LinkRequest request, CancellationToken cancellationToken);
        LinkRequest GetManualPANData(LinkRequest request, CancellationToken cancellationToken);
        LinkRequest GetVerifyAmount(LinkRequest request, CancellationToken cancellationToken);
        LinkRequest GetCreditOrDebit(LinkRequest request, CancellationToken cancellationToken);
        LinkRequest GetPin(LinkRequest request, CancellationToken cancellationToken);
        LinkRequest GetZip(LinkRequest request, CancellationToken cancellationToken, CancellationToken timeoutCancellationToken);
        LinkRequest RemoveCard(LinkRequest request, CancellationToken cancellationToken);
        LinkRequest DeviceUI(LinkRequest request, CancellationToken cancellationToken);

        // ------------------------------------------------------------------------
        // FEATURE: Pre-Swipe
        // ------------------------------------------------------------------------
        LinkRequest StartPreSwipeMode(LinkRequest request, CancellationToken cancellationToken);
        LinkRequest EndPreSwipeMode(LinkRequest request, CancellationToken cancellationToken);
        LinkRequest PurgeHeldCardData(LinkRequest request, CancellationToken cancellationToken);
    }
}
