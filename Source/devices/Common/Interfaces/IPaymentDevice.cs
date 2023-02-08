using Common.XO.Requests;
using Common.XO.Responses;
using Devices.Common.AppConfig;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Devices.Common.Interfaces
{
    public interface IPaymentDevice : ICloneable, IDisposable
    {
        event PublishEvent PublishEvent;
        event PublishEvent PublishMonitor;
        event DeviceLogHandler DeviceLogOccured;
        event DeviceEventHandler DeviceEventOccured;

        DeviceInformation DeviceInformation { get; }

        string Name { get; }

        string ManufacturerConfigID { get; }

        int SortOrder { get; set; }

        List<string> TransactionConfigurations { get; }

        bool IsConnected(LinkRequest request);

        List<DeviceInformation> DiscoverDevices();

        List<LinkErrorValue> Probe(DeviceConfig config, DeviceInformation deviceInfo, out bool dalActive);

        int GetDeviceHealthStatus();

        //int SetDeviceConfigFromCDB(MultiCDBDataResponse cdbDataResponse);

        void DeviceSetIdle();

        bool DeviceRecovery();

        bool DeviceRecoveryWithMessagePreservation();

        void Disconnect();

        void SetDeviceSectionConfig(DeviceSection config);

        List<LinkDeviceResponse> GetDeviceResponse(LinkDeviceResponse deviceInfo);

        //void SetRequestHeader(CommunicationHeader header);

        LinkRequest GetStatus(LinkRequest request, CancellationToken cancellationToken);
    }

}
