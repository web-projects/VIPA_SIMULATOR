using IPA5.Core.Constants;
using Devices.Common;
using Devices.Common.Helpers;
using Devices.Common.State;
using Devices.Sdk.Features.Cancellation;
using Common.XO.Common.DAL;
using Common.XO.Enums.Legacy;
using Common.XO.ProtoBuf;
using Common.XO.Requests.DAL;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Devices.Sdk.Features.State.DALSubWorkflowState;
using LinkRequest = XO.Requests.LinkRequest;

namespace Devices.Sdk.Features.State.Actions
{
    internal class DALGetCardDataSubStateAction : DALBaseSubStateAction
    {
        public override DALSubWorkflowState WorkflowStateType => GetCardData;

        public DALGetCardDataSubStateAction(IDALSubStateController _) : base(_)
        {
        }

        public override SubStateActionLaunchRules LaunchRules => new SubStateActionLaunchRules
        {
            RequestCancellationToken = true,
            DisableRequestPreProcessing = true
        };

        // TODO: figure out a better way to check for user cancellation?
        private bool UserCanceled(LinkRequest linkRequest)
            => linkRequest.Actions?.FirstOrDefault()?.DALRequest?.LinkObjects?.DALResponseData?.Errors?.FirstOrDefault()?.Code is { };

        private LinkRequest SynchronizeLinkRequestWithManualPaymentResponse(LinkRequest linkRequest)
        {
            LinkRequest request = JsonConvert.DeserializeObject<LinkRequest>(JsonConvert.SerializeObject(linkRequest));

            if (UserCanceled(Controller.Register.LinkRequest))
            {
                request.Actions[0].DALRequest.LinkObjects = new LinkDALRequestIPA5Object();
                request.Actions[0].DALRequest.LinkObjects.DALResponseData = Controller.Register.LinkRequest.Actions[0].DALRequest.LinkObjects.DALResponseData;
            }
            else
            {
                LinkCardWorkflowControls cardWorkflowControls = Controller.Register.LinkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices[0].CardWorkflowControls;
                request.LinkObjects.LinkActionResponseList[0].DALResponse = Controller.Register.LinkRequest.LinkObjects.LinkActionResponseList.First().DALResponse;
                request.Actions[0].PaymentRequest.CardWorkflowControls.EMVEnabled = cardWorkflowControls.EMVEnabled;
                request.Actions[0].PaymentRequest.CardWorkflowControls.ContactlessEnabled = cardWorkflowControls.ContactlessEnabled;
                request.Actions[0].PaymentRequest.CardWorkflowControls.ContactlessEMVEnabled = cardWorkflowControls.ContactlessEMVEnabled;
                request.Actions[0].DALRequest.LinkObjects = Controller.Register.LinkRequest.Actions.First().DALRequest.LinkObjects;
            }

            return request;
        }

        public override async Task DoWork()
        {
            if (StateObject is null)
            {
                _ = Controller.LoggingClient.LogErrorAsync("Unable to find a state object while attempting to obtain card data.");
                _ = Error(this);
            }
            else
            {
                Controller.Register.LinkRequest = null;
                CommunicationObject commObject = StateObject as CommunicationObject;
                LinkRequest linkRequest = commObject.LinkRequest;
                LinkDeviceIdentifier deviceIdentifier = linkRequest.GetDeviceIdentifier();
                IDeviceCancellationBroker cancellationBroker = Controller.GetDeviceCancellationBroker();

                IPaymentDevice targetDevice = FindTargetDevice(deviceIdentifier);
                targetDevice?.SetRequestHeader(commObject.Header);
                if (targetDevice is ICardDevice cardDevice)
                {
                    var timeoutPolicy = await cancellationBroker.ExecuteWithTimeoutAsync<LinkRequest>(
                        _ => cardDevice.GetCardData(linkRequest, _),
                        linkRequest.GetAppropriateTimeoutSeconds(Timeouts.DALCardCaptureTimeout),
                        CancellationToken);

                    if (Controller.InterruptManager.InterruptJobSize > 0)
                    {
                        //Wait for any interrupt task to completed (if any), very important to ensure manual entry is done all the way
                        using CancellationTokenSource cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(Timeouts.DALCardCaptureTimeout));
                        while (Controller.InterruptManager.InterruptJobSize > 0 && !cancellationToken.Token.IsCancellationRequested)
                        {
                            await Task.Delay(50);
                        }
                    }

                    if (Controller.Register.LinkRequest != null)
                    {
                        linkRequest = SynchronizeLinkRequestWithManualPaymentResponse(linkRequest);

                        Controller.SaveState(new CommunicationObject(commObject.Header, linkRequest));

                        _ = Complete(this);

                        return;
                    }

                    if (timeoutPolicy.Outcome == Polly.OutcomeType.Failure)
                    {
                        _ = Controller.LoggingClient.LogErrorAsync($"Unable to obtain data from card device - '{Controller.DeviceEvent}'.", StatusType.DALTimeOuts);
                        BuildSubworkflowErrorResponse(linkRequest, cardDevice.DeviceInformation, Controller.DeviceEvent, false);
                    }
                }
                else if (targetDevice is ICheckDevice checkDevice)
                {
                    var timeoutPolicy = await cancellationBroker.ExecuteWithTimeoutAsync<LinkRequest>(
                        _ => checkDevice.GetCheckData(linkRequest, _),
                        linkRequest.GetAppropriateTimeoutSeconds(Timeouts.DALACHCaptureTimeoutSec),
                        CancellationToken);
                    if (timeoutPolicy.Outcome == Polly.OutcomeType.Failure)
                    {
                        _ = Controller.LoggingClient.LogErrorAsync($"Unable to obtain data from check device - '{Controller.DeviceEvent}'.", StatusType.DALTimeOuts);
                        BuildSubworkflowErrorResponse(linkRequest, checkDevice.DeviceInformation, Controller.DeviceEvent, false);
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

        public override bool RequestSupported(LinkRequest request)
        {
            bool result = request.Actions.First().DALActionRequest?.DALAction switch
            {
                LinkDALActionType.StartManualPayment => true,
                _ => false
            };

            return result;
        }

        public override void RequestReceived(CommunicationHeader header, LinkRequest request)
        {
            _ = Controller.LoggingClient.LogInfoAsync($"Received from {request}");

            switch (request.Actions.First().DALActionRequest.DALAction)
            {
                case LinkDALActionType.StartManualPayment:
                    {
                        LinkDeviceIdentifier deviceIdentifier = request.GetDeviceIdentifier();
                        IPaymentDevice targetDevice = FindTargetDevice(deviceIdentifier);

                        if (targetDevice is ICardDevice cardDevice)
                        {
                            InterruptFeatureOptions options = new InterruptFeatureOptions()
                                 .SetRequest(header, request)
                                 .SetController(Controller)
                                 .SetTargetDevice(cardDevice);

                            IDeviceInterruptFeature manualPaymentFeature = Controller.GetInterruptFeature(SupportedFeatures.ManualPaymentFeature);
                            Task.Run(async () =>
                            {
                                await Controller.ExecuteFeature(manualPaymentFeature, options);
                                // LinkRequest could be null in a Manual Entry selection via Monitor before device is ready to process transaction
                                Controller.RequestWorkflowCancellation();       //This will only cancel the main workflow!
                            });
                        }
                        else if (targetDevice is ICheckDevice checkDevice)
                        {
                            checkDevice.DeviceSetIdle();        //Don't have a manual payment mode for this, simple cancel waiting for check
                        }

                        break;
                    }
                default:
                    {
                        _ = Controller.LoggingClient.LogWarnAsync($"DALAction {request.Actions.First().DALActionRequest.DALAction} not implemented.");
                        break;
                    }
            }         
        }
    }
}
