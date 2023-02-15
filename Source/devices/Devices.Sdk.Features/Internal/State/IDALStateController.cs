using Common.Core.Patterns.Queuing;
using Devices.Common;
using Devices.Common.AppConfig;
using Devices.Common.Helpers;
using Devices.Common.Interfaces;
using Devices.Common.State;
using Devices.Core.SerialPort.Interfaces;
using Devices.Core.State.Actions.Preprocessing;
using Devices.Sdk.Features.Cancellation;
using Devices.Sdk.Features.Internal.ErrorManager;
using Devices.Sdk.Features.Internal.State.Actions;
using Devices.Sdk.Features.Interrupt;
using Devices.Sdk.Features.State;
using Devices.Sdk.Features.State.Providers;
using System.Collections.Generic;

namespace Devices.Sdk.Features.Internal.State
{
    internal interface IDALStateController : IDALStateEventEmitter, IStateControlTrigger<IDALStateAction>, IDeviceFeatureFacade
    {
        string PluginPath { get; }
        bool StartupComplete { get; set; }
        IPaymentDevice TargetDevice { get; }
        List<IPaymentDevice> TargetDevices { get; }
        DeviceSection Configuration { get; }
        IDevicePluginLoader DevicePluginLoader { get; set; }
        ISerialPortMonitor SerialPortMonitor { get; }
        IDeviceFeatureManager FeatureManager { get; }
        IErrorManager ErrorManager { get; }
        IInterruptManager InterruptManager { get; }
        PriorityQueue<PriorityQueueDeviceEvents> PriorityQueue { get; set; }
        //ILoggingServiceClient LoggingClient { get; }
        //IDiagnosticClient DiagnosticClient { get; }
        //IBrokerConnector Connector { get; }
        //ICdbAttributeProvider CdbAttributeProvider { get; set; }
        IDALPreProcessor PreProcessor { get; }
        CommunicationObject LastGetStatus { get; set; }
        //IPA5SharedConfiguration SharedConfiguration { get; }

        void SetTargetDevice(IPaymentDevice targetDevice);
        void SetTargetDevices(List<IPaymentDevice> targetDevices);
        void SetPublishEventHandlerAsTask();
        void SaveState(object stateObject);
        IControllerVisitorProvider GetCurrentVisitorProvider();
        ISubStateManagerProvider GetSubStateManagerProvider();
        IDeviceCancellationBroker GetCancellationBroker();
        IDeviceHighLevelRegister GetFreshHLRegister();
        IPaymentDevice[] GetAvailableDevices();
        void QueueDiagnosticEvent(DeviceEvent deviceEvent, DeviceInformation deviceInformation);
        void PublishDeviceConnectEvent(IPaymentDevice device, string portNumber);
    }
}
