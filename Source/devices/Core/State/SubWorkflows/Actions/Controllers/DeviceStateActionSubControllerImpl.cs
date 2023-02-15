using Devices.Core.State.SubWorkflows.Management;
using Devices.Core.State.Enums;
using System;
using System.Collections.Generic;

using static Devices.Core.State.Enums.DeviceSubWorkflowState;

namespace Devices.Core.State.SubWorkflows.Actions.Controllers
{
    internal class DeviceStateActionSubControllerImpl : IDeviceSubStateActionController
    {
        private readonly IDeviceSubStateManager manager;

        private Dictionary<DeviceSubWorkflowState, Func<IDALSubStateController, IDeviceSubStateAction>> workflowMap =
            new Dictionary<DeviceSubWorkflowState, Func<IDALSubStateController, IDeviceSubStateAction>>(
                new Dictionary<DeviceSubWorkflowState, Func<IDALSubStateController, IDeviceSubStateAction>>
                {
                    [GetStatus] = (IDALSubStateController _) => new DeviceGetStatusSubStateAction(_),
                    [AbortCommand] = (IDALSubStateController _) => new DeviceAbortCommandSubStateAction(_),
                    [ManualCardEntry] = (IDALSubStateController _) => new DeviceManualCardEntrySubStateAction(_),
                    [SanityCheck] = (IDALSubStateController _) => new DeviceSanityCheckSubStateAction(_),
                    [RequestComplete] = (IDALSubStateController _) => new DeviceRequestCompleteSubStateAction(_)
                }
        );

        private IDeviceSubStateAction currentStateAction;

        public DeviceStateActionSubControllerImpl(IDeviceSubStateManager manager) => (this.manager) = (manager);

        public IDeviceSubStateAction GetFinalState()
            => workflowMap[RequestComplete](manager as IDALSubStateController);

        public IDeviceSubStateAction GetNextAction(IDeviceSubStateAction stateAction)
            => GetNextAction(stateAction.WorkflowStateType);

        public IDeviceSubStateAction GetNextAction(DeviceSubWorkflowState state)
        {
            IDALSubStateController controller = manager as IDALSubStateController;
            if (currentStateAction == null)
            {
                return (currentStateAction = workflowMap[state](controller));
            }

            DeviceSubWorkflowState proposedState = DeviceSubStateTransitionHelper.GetNextState(state, currentStateAction.LastException != null);
            if (proposedState == currentStateAction.WorkflowStateType)
            {
                return currentStateAction;
            }

            return (currentStateAction = workflowMap[proposedState](controller));
        }
    }
}
