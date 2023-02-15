using Devices.Core.State.SubWorkflows;

namespace Devices.Core.State.Interfaces
{
    internal interface ISubWorkflowHook
    {
        void Hook(IDALSubStateController controller);
        void UnHook();
    }
}
