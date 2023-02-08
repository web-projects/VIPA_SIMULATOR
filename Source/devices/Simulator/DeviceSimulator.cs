using Common.XO.Requests;
using Common.XO.Responses;
using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Common.Helpers;
using Devices.Common.Interfaces;
using Devices.Simulator.Connection;
using Ninject;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;

namespace Devices.Simulator
{
    [Export(typeof(ICardDevice))]
    [Export("Simulator Device", typeof(ICardDevice))]
    internal class DeviceSimulator : ICardDevice, IComparable
    {
        public string Name => StringValueAttribute.GetStringValue(DeviceType.Simulator);

        public int SortOrder { get; set; } = -1;

        [Inject]
        private ISerialConnection serialConnection { get; set; } = new SerialConnection();

        private bool IsConnected { get; set; }

        //public event PublishEvent PublishEvent;
        public event DeviceEventHandler DeviceEventOccured;
        public event PublishEvent PublishEvent;

        public List<LinkRequest> GetDeviceResponse(LinkRequest deviceInfo)
        {
            return null;
        }

        public int CompareTo(object obj)
        {
            var device = obj as ICardDevice;

            if (SortOrder > device.SortOrder)
                return 1;

            if (SortOrder < device.SortOrder)
                return -1;

            return 0;
        }

        public string ManufacturerConfigID => DeviceType.Simulator.ToString();

        public DeviceInformation DeviceInformation { get; private set; }

        public DeviceSimulator()
        {

        }

        public object Clone()
        {
            DeviceSimulator clonedObj = new DeviceSimulator();
            return clonedObj;
        }

        public void Dispose()
        {
            serialConnection?.Dispose();
        }

        bool ICardDevice.IsConnected(object request)
        {
            return IsConnected;
        }

        public List<DeviceInformation> DiscoverDevices()
        {
            List<DeviceInformation> deviceInformation = new List<DeviceInformation>();
            deviceInformation.Add(new DeviceInformation()
            {
                ComPort = "COM3",
                Manufacturer = ManufacturerConfigID,
                Model = "SimCity",
                SerialNumber = "CEEEDEADBEEF",
                ProductIdentification = "SIMULATOR",
                VendorIdentifier = "BADDCACA"

            });

            return deviceInformation;
        }

        public void Disconnect()
        {

        }

        public void SetDeviceSectionConfig(DeviceSection config)
        {

        }

        public List<LinkErrorValue> Probe(DeviceConfig config, DeviceInformation deviceInfo, out bool active)
        {
            DeviceInformation = new DeviceInformation()
            {
                ComPort = config.SerialConfig.CommPortName,
                Manufacturer = ManufacturerConfigID,
                Model = "SimCity",
                SerialNumber = "CEEEDEADBEEF",
                ProductIdentification = "SIMULATOR",
                VendorIdentifier = "BADDCACA"
            };
            deviceInfo = DeviceInformation;
            active = IsConnected = serialConnection.Connect(config.SerialConfig.CommPortName);

            return null;
        }

        public void DeviceSetIdle()
        {
        }

        public bool DeviceRecovery()
        {
            return true;
        }

        // ------------------------------------------------------------------------
        // Methods that are mapped for usage in their respective sub-workflows.
        // ------------------------------------------------------------------------
        #region --- subworkflow mapping
        public LinkRequest GetStatus(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            Console.WriteLine($"simulator: GET STATUS for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");
            return linkRequest;
        }

        public LinkRequest AbortCommand(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            Console.WriteLine($"simulator: ABORT COMMAND for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");
            return linkRequest;
        }

        public LinkRequest ManualCardEntry(LinkRequest linkRequest, CancellationToken cancellationToken)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            Console.WriteLine($"simulator: MANUAL CARD ENTRY for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");
            return linkRequest;
        }

        #endregion --- subworkflow mapping
    }
}
