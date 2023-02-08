using Common.XO.Private;
using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Verifone.Connection;
using Devices.Verifone.Helpers;
using Devices.Verifone.VIPA.Helpers;
using Devices.Verifone.VIPA.TagLengthValue;
using System;
using System.Collections.Generic;
using static Devices.Verifone.VIPA.VIPAImpl;

namespace Devices.Verifone.VIPA.Interfaces
{
    public interface IVipa : IDisposable
    {
        DeviceInformation DeviceInformation { get; }

        bool Connect(VerifoneConnection connection, DeviceInformation deviceInformation);

        void ConnectionConfiguration(SerialDeviceConfig serialConfig, DeviceEventHandler deviceEventHandler, DeviceLogHandler deviceLogHandler);

        int CloseContactlessReader(bool forceClose = false);

        void ResponseCodeHandler(List<TLV> tags, int responseCode, bool cancelled = false);

        bool DisplayMessage(VIPADisplayMessageValue displayMessageValue = VIPADisplayMessageValue.Idle, bool enableBacklight = false, string customMessage = "");

        (DeviceInfoObject deviceInfoObject, int VipaResponse) VIPARestart();

        (DeviceInfoObject deviceInfoObject, int VipaResponse) DeviceCommandReset();

        (DeviceInfoObject deviceInfoObject, int VipaResponse) DeviceExtendedReset();

        (DevicePTID devicePTID, int VipaResponse) DeviceReboot();

        (int VipaResult, int VipaResponse) GetActiveKeySlot();

        (DeviceInfoObject deviceInfoObject, int VipaResponse) GetDeviceHealth(SupportedTransactions supportedTransactions);

        (SecurityConfigurationObject securityConfigurationObject, int VipaResponse) GetSecurityConfiguration(byte hostID, byte vssSlot);

        (KernelConfigurationObject kernelConfigurationObject, int VipaResponse) GetEMVKernelChecksum();

        string GetContactlessEMVKernelVersions();

        int ConfigurationPackage(string deviceModel, bool activeSigningMethodIsSphere);

        int ValidateConfiguration(string deviceModel, bool activeSigningMethodIsSphere);

        int FeatureEnablementToken();

        int LockDeviceConfiguration0(string deviceModel, string activeConfiguration, bool activeSigningMethodIsSphere);

        int LockDeviceConfiguration8(string deviceModel, string activeConfiguration, bool activeSigningMethodIsSphere);

        int UnlockDeviceConfiguration();

        void LoadDeviceSectionConfig(DeviceSection deviceSectionConfig);

        (LinkDALRequestIPA5Object LinkActionRequestIPA5Object, int VipaResponse) DisplayCustomScreen(string displayMessage);

        (LinkDALRequestIPA5Object LinkActionRequestIPA5Object, int VipaResponse) DisplayCustomScreenHTML(string displayMessage);

        LinkDALRequestIPA5Object VIPAVersions(string deviceModel, bool hmacEnabled, string activeCustomerId);

        (string Timestamp, int VipaResponse) Get24HourReboot();

        (string Timestamp, int VipaResponse) Reboot24Hour(string timestamp);

        (string Timestamp, int VipaResponse) GetTerminalDateTime();

        (string Timestamp, int VipaResponse) SetTerminalDateTime(string timestamp);

        string GetDeviceHealthTimeZone(string deviceSerialNumber);

        (LinkDALRequestIPA5Object linkActionRequestIPA5Object, int VipaResponse) ProcessManualPayment(bool requestCVV);
    }
}