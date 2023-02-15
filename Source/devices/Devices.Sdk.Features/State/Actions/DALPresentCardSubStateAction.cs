﻿using Common.XO.Device;
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
    internal class DALPresentCardSubStateAction : DALBaseSubStateAction
    {
        public override DALSubWorkflowState WorkflowStateType => PresentCard;

        public DALPresentCardSubStateAction(IDALSubStateController _) : base(_) { }

        public override SubStateActionLaunchRules LaunchRules => new SubStateActionLaunchRules
        {
            RequestCancellationToken = true
        };

        public override async Task DoWork()
        {
            if (StateObject is null)
            {
                //_ = Controller.LoggingClient.LogErrorAsync("Unable to find a state object while attempting to get card information.");
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
                    cardDevice.SetRequestHeader(commObject.Header);
                    var timeoutPolicy = await cancellationBroker.ExecuteWithTimeoutAsync<LinkRequest>(
                        _ => cardDevice.PresentCard(linkRequest, _),
                        linkRequest.GetAppropriateTimeoutSeconds(Timeouts.DALCardCaptureTimeout),
                        this.CancellationToken);

                    if (timeoutPolicy.Outcome == Polly.OutcomeType.Failure)
                    {
                        //_ = Controller.LoggingClient.LogErrorAsync($"Unable to obtain Card Information from device - '{Controller.DeviceEvent}'.", StatusType.DALTimeOuts);
                        BuildSubworkflowErrorResponse(linkRequest, cardDevice.DeviceInformation, Controller.DeviceEvent);
                    }
                }
                else
                {
                    UpdateRequestDeviceNotFound(linkRequest, deviceIdentifier);
                }

                Controller.SaveState(new CommunicationObject(commObject.Header, linkRequest));

                _ = Complete(this);
            }
        }
    }
}
