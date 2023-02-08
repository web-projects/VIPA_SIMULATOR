using Common.XO.Requests;
using Common.XO.Responses;
using Devices.Common.Constants;
using Devices.Common.Helpers;
using Devices.Common.State;
using Devices.Sdk.Features.Cancellation;
using Newtonsoft.Json;
using Polly;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Devices.Sdk.Features.State.DALSubWorkflowState;

namespace Devices.Sdk.Features.State.Actions
{
    internal class DALGetStatusSubStateAction : DALBaseSubStateAction
    {
        public override DALSubWorkflowState WorkflowStateType => GetStatus;

        public DALGetStatusSubStateAction(IDALSubStateController _) : base(_) { }

        public override SubStateActionLaunchRules LaunchRules => new SubStateActionLaunchRules
        {
            RequestCancellationToken = true
        };

        public override async Task DoWork()
        {
            if (StateObject is null)
            {
                //_ = Controller.LoggingClient.LogErrorAsync("Unable to find a state object while attempting to get device status.");
                _ = Error(this);
            }
            else
            {
                bool atLeastOneSuccess = false;
                Dictionary<string, LinkErrorValue> errors = null;
                CommunicationObject commObject = StateObject as CommunicationObject;
                LinkRequest linkRequest = commObject.LinkRequest;
                IDeviceCancellationBroker cancellationBroker = Controller.GetDeviceCancellationBroker();
                PolicyResult<LinkRequest> timeoutPolicy = null;

                if (Controller.TargetDevice != null)
                {
                    Controller.TargetDevice.SetRequestHeader(commObject.Header);
                    timeoutPolicy = await cancellationBroker.ExecuteWithTimeoutAsync<LinkRequest>(
                        _ => Controller.TargetDevice.GetStatus(linkRequest, _),
                        linkRequest.GetAppropriateTimeoutSeconds(Timeouts.DALGetStatusTimeout),
                        System.Threading.CancellationToken.None);

                    if (timeoutPolicy.Outcome == OutcomeType.Failure)
                    {
                        //_ = Controller.LoggingClient.LogErrorAsync($"Unable to obtain device status - '{Controller.DeviceEvent}'.", StatusType.DALTimeOuts);
                        BuildSubworkflowErrorResponse(linkRequest, Controller.TargetDevice.DeviceInformation, Controller.DeviceEvent);
                        if (Controller.TargetDevice.DeviceInformation?.SerialNumber is { })
                        {
                            errors ??= new Dictionary<string, LinkErrorValue>();
                            errors[Controller.TargetDevice.DeviceInformation.SerialNumber] = CreateSubworkflowError(linkRequest, Controller.DeviceEvent);
                        }
                    }
                    else if (linkRequest.LinkObjects.LinkActionResponseList[0].Errors == null)
                    {
                        atLeastOneSuccess = true;
                        linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices[0].Configurations = Controller.TargetDevice.TransactionConfigurations;
                    }
                }
                else
                {
                    List<LinkRequest> devicesRequest = new List<LinkRequest>();

                    foreach (var device in Controller.TargetDevices)
                    {
                        devicesRequest.Add(JsonConvert.DeserializeObject<LinkRequest>(JsonConvert.SerializeObject(linkRequest)));

                        timeoutPolicy = await cancellationBroker.ExecuteWithTimeoutAsync<LinkRequest>(
                            _ => device.GetStatus(devicesRequest.LastOrDefault(), _),
                            linkRequest.GetAppropriateTimeoutSeconds(Timeouts.DALGetStatusTimeout),
                            System.Threading.CancellationToken.None);

                        if (timeoutPolicy.Outcome == OutcomeType.Failure)
                        {
                            //_ = Controller.LoggingClient.LogErrorAsync($"Unable to obtain device status - '{Controller.DeviceEvent}'.", StatusType.DALTimeOuts);
                            BuildSubworkflowErrorResponse(linkRequest, device.DeviceInformation, Controller.DeviceEvent);
                            if (device.DeviceInformation?.SerialNumber is { })
                            {
                                errors ??= new Dictionary<string, LinkErrorValue>();
                                errors[device.DeviceInformation.SerialNumber] = CreateSubworkflowError(linkRequest, Controller.DeviceEvent);
                            }
                        }
                        else
                        {
                            atLeastOneSuccess = true;
                            devicesRequest.LastOrDefault().LinkObjects.LinkActionResponseList[0].DALResponse.Devices[0].Configurations = device.TransactionConfigurations;
                        }
                    }

                    if (linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse == null)
                    {
                        linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse = new LinkDALResponse();
                    }

                    if (linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices == null)
                    {
                        linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices = new List<LinkDeviceResponse>();
                    }

                    foreach (var response in devicesRequest)
                    {
                        if (response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices != null)
                        {
                            linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices.Add(new LinkDeviceResponse
                            {
                                Manufacturer = response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices[0].Manufacturer,
                                Model = response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices[0].Model,
                                SerialNumber = response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices[0].SerialNumber,
                                Port = response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices[0].Port,
                                Configurations = response.LinkObjects.LinkActionResponseList[0].DALResponse?.Devices[0].Configurations
                            });
                        }
                    }
                }

                // update payload with feature list
                if (atLeastOneSuccess)
                {
                    if (linkRequest.LinkObjects.LinkActionResponseList[0].Errors == null)
                    {
                        foreach (var deviceResponse in linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices)
                        {
                            List<string> deviceFeatures = new List<string>();
                            foreach (var feature in Controller.AvailableFeatures)
                            {
                                if (feature.Value.Contains($"{deviceResponse.Manufacturer}-{deviceResponse.Model}"))
                                {
                                    deviceFeatures.Add(Regex.Replace(feature.Key, "WorkflowFeature", "", RegexOptions.IgnoreCase));
                                }
                            }
                            if (deviceFeatures.Count > 0)
                            {
                                deviceResponse.Features = deviceFeatures;
                            }
                        }
                    }
                }

                // update payload with errors
                if (errors is { })
                {
                    foreach (var deviceResponse in linkRequest.LinkObjects.LinkActionResponseList[0].DALResponse.Devices)
                    {
                        if (deviceResponse.SerialNumber is { } && errors.Keys.Contains(deviceResponse.SerialNumber))
                        {
                            deviceResponse.Errors ??= new List<LinkErrorValue>();
                            deviceResponse.Errors.Add(errors[deviceResponse.SerialNumber]);
                        }
                    }
                }

                CommunicationObject saveObject = new CommunicationObject(commObject.Header, linkRequest);
                Controller.SaveState(saveObject);
                Controller.LastGetStatus = saveObject;

                _ = Complete(this);
            }
        }
    }
}
