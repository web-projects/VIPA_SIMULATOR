using AppCommon.Helpers.EMVKernel;
using Common.Constants;
using Common.XO.Device;
using Common.XO.Private;
using Common.XO.Requests;
using Common.XO.Responses;
using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Common.Config;
using Devices.Common.Interfaces;
using Devices.Verifone.Connection;
using Devices.Verifone.Helpers;
using Devices.Verifone.VIPA;
using Devices.Verifone.VIPA.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using StringValueAttribute = Devices.Common.Helpers.StringValueAttribute;

namespace Devices.Verifone
{
    [Export(typeof(ICardDevice))]
    [Export("Verifone-M400", typeof(ICardDevice))]
    [Export("Verifone-P200", typeof(ICardDevice))]
    [Export("Verifone-P400", typeof(ICardDevice))]
    [Export("Verifone-UX300", typeof(ICardDevice))]
    internal class VerifoneDevice : IDisposable, ICardDevice
    {
        public string Name => StringValueAttribute.GetStringValue(Devices.Common.Helpers.DeviceType.Verifone);

        public event PublishEvent PublishEvent;
        public event DeviceEventHandler DeviceEventOccured;
        public event DeviceLogHandler DeviceLogHandler;

        private VerifoneConnection VerifoneConnection { get; set; }

        private (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceVIPAInfo;
        private bool IsConnected { get; set; }

        DeviceConfig deviceConfiguration;
        DeviceSection deviceSectionConfig;

        [Inject]
        internal IVipa VipaConnection { get; set; } = new VIPAImpl();

        public IVipa VipaDevice { get; private set; }

        public DeviceInformation DeviceInformation { get; private set; }

        public string ManufacturerConfigID => Devices.Common.Helpers.DeviceType.Verifone.ToString();

        public int SortOrder { get; set; } = -1;

        int ConfigurationHostId { get => deviceSectionConfig?.Verifone?.ConfigurationHostId ?? VerifoneSettingsSecurityConfiguration.ConfigurationHostId; }

        int OnlinePinKeySetId { get => deviceSectionConfig?.Verifone?.OnlinePinKeySetId ?? VerifoneSettingsSecurityConfiguration.OnlinePinKeySetId; }

        int ADEKeySetId { get => deviceSectionConfig?.Verifone?.ADEKeySetId ?? VerifoneSettingsSecurityConfiguration.ADEKeySetId; }

        string ConfigurationPackageActive { get => deviceSectionConfig?.Verifone?.ConfigurationPackageActive; }

        string SigningMethodActive { get; set; }

        string ActiveCustomerId { get => deviceSectionConfig?.Verifone?.ActiveCustomerId; }

        bool EnableHMAC { get; set; }

        LinkDALRequestIPA5Object VipaVersions { get; set; }

        public VerifoneDevice()
        {
            string logsDir = Directory.GetCurrentDirectory() + Path.Combine("\\", LogDirectories.LogDirectory);
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }
        }

        public object Clone()
        {
            VerifoneDevice clonedObj = new VerifoneDevice();
            return clonedObj;
        }

        public void Dispose()
        {
            VipaConnection?.Dispose();
            IsConnected = false;
        }

        public void Disconnect()
        {
            VerifoneConnection?.Disconnect();
            IsConnected = false;
        }

        bool ICardDevice.IsConnected(object request)
        {
            return IsConnected;
        }

        private IVipa LocateDevice(LinkDeviceIdentifier deviceIdentifer)
        {
            // If we have single device connected to the work station
            if (deviceIdentifer == null)
            {
                return VipaConnection;
            }

            // get device serial number
            string deviceSerialNumber = DeviceInformation?.SerialNumber;

            if (string.IsNullOrEmpty(deviceSerialNumber))
            {
                // clear up any commands the device might be processing
                //VipaConnection.AbortCurrentCommand();

                //SetDeviceVipaInfo(VipaConnection, true);
                //deviceSerialNumber = deviceVIPAInfo.deviceInfoObject?.LinkDeviceResponse?.SerialNumber;
            }

            if (!string.IsNullOrWhiteSpace(deviceSerialNumber))
            {
                // does device serial number match LinkDeviceIdentifier serial number
                if (deviceSerialNumber.Equals(deviceIdentifer.SerialNumber, StringComparison.CurrentCultureIgnoreCase))
                {
                    return VipaConnection;
                }
                else
                {
                    //VipaConnection.DisplayMessage(VIPADisplayMessageValue.Idle);
                }
            }

            return VipaConnection;
        }

        private int GetDeviceHealthStatus()
        {
            (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceInfo = VipaDevice.GetDeviceHealth(deviceConfiguration.SupportedTransactions);

            if (deviceInfo.VipaResponse == (int)VipaSW1SW2Codes.Success)
            {
                DeviceInformation.ContactlessKernelInformation = VipaDevice.GetContactlessEMVKernelVersions();
                Debug.WriteLine($"EMV KERNEL VERSION: \"{DeviceInformation.ContactlessKernelInformation}\"");
            }

            DeviceInformation.VOSVersions = VipaDevice.DeviceInformation.VOSVersions;

            return deviceInfo.VipaResponse;
        }

        private void ReportEMVKernelInformation()
        {
            if (!string.IsNullOrEmpty(DeviceInformation.EMVL2KernelVersion))
            {
                Debug.WriteLine($"EMV L2 KERNEL VERSION: \"{DeviceInformation.EMVL2KernelVersion}\"");
            }

            if (!string.IsNullOrEmpty(DeviceInformation.ContactlessKernelInformation))
            {
                string[] kernelRevisions = DeviceInformation.ContactlessKernelInformation.Split(';');
                foreach (string version in kernelRevisions)
                {
                    Debug.WriteLine($"EMV KERNEL VERSION: \"{version}\"");
                }
            }
        }

        private void GetBundleSignatures()
        {
            if (VipaDevice != null)
            {
                if (!IsConnected)
                {
                    VipaDevice.Dispose();
                    VerifoneConnection = new VerifoneConnection();
                    IsConnected = VipaDevice.Connect(VerifoneConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = VipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        VipaVersions = VipaDevice.VIPAVersions(deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model, EnableHMAC, ActiveCustomerId);
                    }

                    DeviceSetIdle();
                }
            }

        }

        private string GetWorkstationTimeZone()
        {
            TimeZoneInfo curTimeZone = TimeZoneInfo.Local;
            return curTimeZone.DisplayName;
        }

        private string GetKIFTimeZoneFromDeviceHealthFile(string deviceSerialNumber)
        {
            if (VipaDevice != null)
            {
                return VipaDevice.GetDeviceHealthTimeZone(deviceSerialNumber);
            }

            return string.Empty;
        }

        public void SetDeviceSectionConfig(DeviceSection config)
        {
            // L2 Kernel Information
            int healthStatus = GetDeviceHealthStatus();

            if (healthStatus == (int)VipaSW1SW2Codes.Success)
            {
                ReportEMVKernelInformation();
            }

            deviceSectionConfig = config;

            // BUNDLE Signatures
            GetBundleSignatures();

            SigningMethodActive = "UNSIGNED";

            if (VipaVersions.DALCdbData is { })
            {
                SigningMethodActive = VipaVersions.DALCdbData.VIPAVersion.Signature?.ToUpper() ?? "MISSING";
            }
            EnableHMAC = SigningMethodActive.Equals("SPHERE", StringComparison.CurrentCultureIgnoreCase) ? false : true;

            if (VipaConnection != null)
            {
                Console.WriteLine($"\r\n\r\nACTIVE SIGNATURE _____: {SigningMethodActive.ToUpper()}");
                Console.WriteLine($"ACTIVE CONFIGURATION _: {deviceSectionConfig.Verifone?.ConfigurationPackageActive}");
                string onlinePINSource = deviceSectionConfig.Verifone?.ConfigurationHostId == VerifoneSettingsSecurityConfiguration.DUKPTEngineIPP ? "IPP" : "VSS";
                Console.WriteLine($"ONLINE DEBIT PIN STORE: {onlinePINSource}");
                Console.WriteLine($"HMAC ENABLEMENT ACTIVE: {EnableHMAC.ToString().ToUpper()}");
                Console.WriteLine($"WORKSTATION TIMEZONE _: \"{GetWorkstationTimeZone()}\"");
                Console.WriteLine("");
                VipaConnection.LoadDeviceSectionConfig(deviceSectionConfig);
            }
        }

        public List<LinkErrorValue> Probe(DeviceConfig config, DeviceInformation deviceInfo, out bool active)
        {
            DeviceInformation = deviceInfo;
            DeviceInformation.Manufacturer = ManufacturerConfigID;
            DeviceInformation.ComPort = deviceInfo.ComPort;

            VerifoneConnection = new VerifoneConnection();
            active = IsConnected = VipaConnection.Connect(VerifoneConnection, DeviceInformation);

            if (active)
            {
                (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = VipaConnection.DeviceCommandReset();

                if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                {
                    // check for power on notification: reissue reset command to obtain device information
                    if (deviceIdentifier.deviceInfoObject.LinkDeviceResponse.PowerOnNotification != null)
                    {
                        Console.WriteLine($"\nDEVICE EVENT: Terminal ID={deviceIdentifier.deviceInfoObject.LinkDeviceResponse.PowerOnNotification?.TerminalID}," +
                            $" EVENT='{deviceIdentifier.deviceInfoObject.LinkDeviceResponse.PowerOnNotification?.TransactionStatusMessage}'");

                        deviceIdentifier = VipaConnection.DeviceCommandReset();

                        if (deviceIdentifier.VipaResponse != (int)VipaSW1SW2Codes.Success)
                        {
                            return null;
                        }
                    }

                    if (DeviceInformation != null)
                    {
                        DeviceInformation.Manufacturer = ManufacturerConfigID;
                        DeviceInformation.Model = deviceIdentifier.deviceInfoObject.LinkDeviceResponse.Model;
                        DeviceInformation.SerialNumber = deviceIdentifier.deviceInfoObject.LinkDeviceResponse.SerialNumber;
                        DeviceInformation.FirmwareVersion = deviceIdentifier.deviceInfoObject.LinkDeviceResponse.FirmwareVersion;
                    }
                    VipaDevice = VipaConnection;
                    deviceConfiguration = config;
                    active = true;

                    //Console.WriteLine($"\nDEVICE PROBE SUCCESS ON {DeviceInformation?.ComPort}, FOR SN: {DeviceInformation?.SerialNumber}");
                }
                else
                {
                    //VipaDevice.CancelResponseHandlers();
                    //Console.WriteLine($"\nDEVICE PROBE FAILED ON {DeviceInformation?.ComPort}\n");
                }
            }
            return null;
        }

        public List<DeviceInformation> DiscoverDevices()
        {
            List<DeviceInformation> deviceInformation = new List<DeviceInformation>();
            Connection.DeviceDiscovery deviceDiscovery = new Connection.DeviceDiscovery();
            if (deviceDiscovery.FindVerifoneDevices())
            {
                foreach (var device in deviceDiscovery.deviceInfo)
                {
                    if (string.IsNullOrEmpty(device.ProductID) || string.IsNullOrEmpty(device.SerialNumber))
                        throw new Exception("The connected device's PID or SerialNumber did not match with the expected values!");

                    deviceInformation.Add(new DeviceInformation()
                    {
                        ComPort = device.ComPort,
                        ProductIdentification = device.ProductID,
                        SerialNumber = device.SerialNumber,
                        VendorIdentifier = Connection.DeviceDiscovery.VID,
                        VOSVersions = new VOSVersions(),
                        EMVKernelConfiguration = new EMVKernelConfiguration()
                    });

                    System.Diagnostics.Debug.WriteLine($"device: ON PORT={device.ComPort} - VERIFONE MODEL={deviceInformation[deviceInformation.Count - 1].ProductIdentification}, " +
                        $"SN=[{deviceInformation[deviceInformation.Count - 1].SerialNumber}], PORT={deviceInformation[deviceInformation.Count - 1].ComPort}");
                }
            }

            // validate COMM Port
            if (!deviceDiscovery.deviceInfo.Any() || deviceDiscovery.deviceInfo[0].ComPort == null || !deviceDiscovery.deviceInfo[0].ComPort.Any())
            {
                return null;
            }

            return deviceInformation;
        }

        public void DeviceSetIdle()
        {
            //Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: SET TO IDLE.");
            if (VipaDevice != null)
            {
                VipaDevice.CloseContactlessReader(true);
                VipaDevice.DisplayMessage(VIPAImpl.VIPADisplayMessageValue.Idle);
            }
        }

        public bool DeviceRecovery()
        {
            Console.WriteLine($"DEVICE: ON PORT={DeviceInformation.ComPort} - DEVICE-RECOVERY");
            return false;
        }

        public List<LinkRequest> GetDeviceResponse(LinkRequest deviceInfo)
        {
            throw new NotImplementedException();
        }

        public LinkRequest GetVerifyAmount(LinkRequest request, CancellationToken cancellationToken)
        {
            LinkActionRequest linkActionRequest = request.Actions.First();
            //IVIPADevice device = LocateDevice(linkActionRequest?.DALRequest?.DeviceIdentifier);
            IVipa device = VipaDevice;

            if (device != null)
            {
                //SelectVerifyAmount(device, request, linkActionRequest, cancellationToken);
                DisplayCustomScreen(request);
            }

            return request;
        }

        public string AmountToDollar(string amount)
        {
            if (amount == null)
            {
                return null;
            }

            string dollarAmount = string.Format("{0:#0.00}", Convert.ToDecimal(amount) / 100);

            return dollarAmount;
        }

        // ------------------------------------------------------------------------
        // Methods that are mapped for usage in their respective sub-workflows.
        // ------------------------------------------------------------------------
        #region --- subworkflow mapping
        public LinkRequest GetStatus(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: GET STATUS for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");
            return linkRequest;
        }

        public LinkRequest AbortCommand(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE: ABORT COMMAND for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");
            return linkRequest;
        }

        public LinkRequest ManualCardEntry(LinkRequest linkRequest, CancellationToken cancellationToken)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: MANUAL CARD ENTRY for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (VipaDevice != null)
            {
                if (!IsConnected)
                {
                    VipaDevice.Dispose();
                    VerifoneConnection = new VerifoneConnection();
                    IsConnected = VipaDevice.Connect(VerifoneConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    //bool requestCVV = linkActionRequest.PaymentRequest?.CardWorkflowControls?.CVVEnabled != false;  //This will default to true
                    bool requestCVV = false;
                    (LinkDALRequestIPA5Object LinkDALRequestIPA5Object, int VipaResponse) cardInfo = VipaDevice.ProcessManualPayment(requestCVV);

                    // Check for transaction timeout
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DeviceSetIdle();
                        // Reset contactless reader to hide contactless status bar if device is unplugged and replugged during a payment workflow
                        //_ = Task.Run(() => VipaDevice.CloseContactlessReader(true));
                        //SetErrorResponse(linkActionRequest, EventCodeType.REQUEST_TIMEOUT, cardInfo.VipaResponse, StringValueAttribute.GetStringValue(DeviceEvent.RequestTimeout));
                        return linkRequest;
                    }

                    if (cardInfo.VipaResponse == (int)VipaSW1SW2Codes.UserEntryCancelled)
                    {
                        DeviceSetIdle();
                        //SetErrorResponse(linkActionRequest, EventCodeType.USER_CANCELED, cardInfo.VipaResponse, "Cancelled");
                        return linkRequest;
                    }

                    // set card data
                    linkActionRequest.DALRequest.LinkObjects = cardInfo.LinkDALRequestIPA5Object;

                    //PopulateCapturedCardData(linkActionRequest, LinkCardPaymentResponseEntryMode.Manual);

                    // complete CardData response format
                    //CompleteCardDataResponse(request, linkActionRequest, deviceInfoResponse);
                }
            }

            DeviceSetIdle();

            return linkRequest;
        }

        public LinkRequest DisplayCustomScreen(LinkRequest linkRequest)
        {
            LinkActionRequest linkActionRequest = linkRequest?.Actions?.First();
            Console.WriteLine($"DEVICE[{DeviceInformation.ComPort}]: DISPLAY CUSTOM SCREEN for SN='{linkActionRequest?.DeviceRequest?.DeviceIdentifier?.SerialNumber}'");

            if (VipaDevice != null)
            {
                if (!IsConnected)
                {
                    VipaDevice.Dispose();
                    VerifoneConnection = new VerifoneConnection();
                    IsConnected = VipaDevice.Connect(VerifoneConnection, DeviceInformation);
                }

                if (IsConnected)
                {
                    (DeviceInfoObject deviceInfoObject, int VipaResponse) deviceIdentifier = VipaDevice.DeviceCommandReset();

                    if (deviceIdentifier.VipaResponse == (int)VipaSW1SW2Codes.Success)
                    {
                        long amount = 9999999;
                        string requestedAmount = amount.ToString();

                        // must contain 5 elements: "title|item 1|item 2|item 3|total"
                        // use '-' for vertical spacing when items 2/3 do not exist, otherwise use a space or leave the item field blank
                        string displayMessage = $"VERIFY AMOUNT|item 1 ..... ${AmountToDollar(requestedAmount)}|-| |Total ...... ${AmountToDollar(requestedAmount)}";

                        (LinkDALRequestIPA5Object LinkActionRequestIPA5Object, int VipaResponse) verifyAmountResponse = VipaDevice.DisplayCustomScreenHTML(displayMessage);

                        if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine("DEVICE: CUSTOM SCREEN EXECUTED SUCCESSFULLY - RESPONSE={0}\n", verifyAmountResponse.LinkActionRequestIPA5Object.DALResponseData.Value.Equals("1", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO");
                        }
                        else if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.DeviceNotSupported)
                        {
                            Console.WriteLine(string.Format("DEVICE: UNSUPPORTED DEVICE ERROR=0x{0:X4}\n", verifyAmountResponse.VipaResponse));
                        }
                        else if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.FileNotFoundOrNotAccessible)
                        {
                            displayMessage = $"VERIFY AMOUNT|Total.....${AmountToDollar(requestedAmount)}|YES|NO";
                            verifyAmountResponse = VipaDevice.DisplayCustomScreen(displayMessage);

                            if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.Success)
                            {
                                Console.WriteLine("DEVICE: CUSTOM SCREEN EXECUTED SUCCESSFULLY - RESPONSE={0}\n", verifyAmountResponse.LinkActionRequestIPA5Object.DALResponseData.Value.Equals("1", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO");
                            }
                            else if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.DeviceNotSupported)
                            {
                                Console.WriteLine(string.Format("DEVICE: UNSUPPORTED DEVICE ERROR=0x{0:X4}\n", verifyAmountResponse.VipaResponse));
                            }
                            else
                            {
                                Console.WriteLine(string.Format("DEVICE: FAILED DISPLAY CUSTOM SCREEN REQUEST WITH ERROR=0x{0:X4}\n", verifyAmountResponse.VipaResponse));
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED DISPLAY CUSTOM SCREEN REQUEST WITH ERROR=0x{0:X4}\n", verifyAmountResponse.VipaResponse));
                        }

                        /*(LinkDALRequestIPA5Object LinkActionRequestIPA5Object, int VipaResponse) verifyAmountResponse = VipaDevice.DisplayCustomScreen(displayMessage);

                        if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.Success)
                        {
                            Console.WriteLine("DEVICE: CUSTOM SCREEN EXECUTED SUCCESSFULLY - RESPONSE={0}\n", verifyAmountResponse.LinkActionRequestIPA5Object.DALResponseData.Value.Equals("1", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO");
                        }
                        else if (verifyAmountResponse.VipaResponse == (int)VipaSW1SW2Codes.DeviceNotSupported)
                        {
                            Console.WriteLine(string.Format("DEVICE: UNSUPPORTED DEVICE ERROR=0x{0:X4}\n", verifyAmountResponse.VipaResponse));
                        }
                        else
                        {
                            Console.WriteLine(string.Format("DEVICE: FAILED DISPLAY CUSTOM SCREEN REQUEST WITH ERROR=0x{0:X4}\n", verifyAmountResponse.VipaResponse));
                        }*/
                    }
                }
            }

            DeviceSetIdle();

            return linkRequest;
        }

        #endregion --- subworkflow mapping
    }
}
