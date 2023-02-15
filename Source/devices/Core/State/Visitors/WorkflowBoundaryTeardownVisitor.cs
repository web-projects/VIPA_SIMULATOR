using Devices.Core.State.Interfaces;
using Devices.Core.State.SubWorkflows;

namespace Devices.Core.State.Visitors
{
    internal class WorkflowBoundaryTeardownVisitor : IStateControllerVisitor<ISubWorkflowHook, IDALSubStateController>
    {
        public void Visit(ISubWorkflowHook context, IDALSubStateController visitorAcceptor) => context.UnHook();
    }
}
