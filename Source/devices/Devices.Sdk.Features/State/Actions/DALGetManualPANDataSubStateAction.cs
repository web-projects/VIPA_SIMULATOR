using Common.XO.Device;
using Common.XO.Requests;
using Devices.Common.Constants;
using Devices.Common.Helpers;
using Devices.Common.Interfaces;
using Devices.Common.State;
using Devices.Sdk.Features.Cancellation;
using System.Threading.Tasks;
using static Devices.Sdk.Features.State.DALSubWorkflowState;

namespace Devices.Sdk.Features.State.Actions
{
    internal class DALGetManualPANDataSubStateAction : DALBaseSubStateAction
    {
        public override DALSubWorkflowState WorkflowStateType => GetManualPANData;

        public DALGetManualPANDataSubStateAction(IDALSubStateController _) : base(_) { }

        public override SubStateActionLaunchRules LaunchRules => new SubStateActionLaunchRules
        {
            RequestCancellationToken = true
        };

        public override async Task DoWork()
        {
            if (StateObject is null)
            {
                //_ = Controller.LoggingClient.LogErrorAsync("Unable to find a state object while attempting to obtain manual PAN data.");
                _ = Error(this);
            }
            else
            {
                CommunicationObject commObject = StateObject as CommunicationObject;
                LinkRequest linkRequest = commObject.LinkRequest;
                LinkDeviceIdentifier deviceIdentifier = linkRequest.GetDeviceIdentifier();
                IDeviceCancellationBroker cancellationBroker = Controller.GetDeviceCancellationBroker();

                IPaymentDevice targetDevice = FindTargetDevice(deviceIdentifier);
                if (targetDevice is ICardDevice cardDevice)
                {
                    var timeoutPolicy = await cancellationBroker.ExecuteWithTimeoutAsync<LinkRequest>(
                        _ => cardDevice.GetManualPANData(linkRequest, _),
                        linkRequest.GetAppropriateManualEntryTimeoutSeconds(Timeouts.DALManualCaptureTimeout),
                        this.CancellationToken);

                    if (timeoutPolicy.Outcome == Polly.OutcomeType.Failure)
                    {
                        //_ = Controller.LoggingClient.LogErrorAsync($"Unable to obtain manual PAN Data from device - '{Controller.DeviceEvent}'.", StatusType.DALTimeOuts);
                        BuildSubworkflowErrorResponse(linkRequest, cardDevice.DeviceInformation, Controller.DeviceEvent);
                    }
                }
                else
                {
                    UpdateRequestDeviceNotFound(linkRequest, deviceIdentifier);
                }

                Controller.SaveState(linkRequest);

                _ = Complete(this);
            }
        }
    }
}
