using Devices.Core.State.Interfaces;
using Devices.Core.State.SubWorkflows;

namespace Devices.Core.State.Visitors
{
    internal interface IStateControllerVisitable<TVisitableController, TVisitorAcceptor>
        where TVisitableController : ISubWorkflowHook
        where TVisitorAcceptor : IDALSubStateController
    {
        void Accept(IStateControllerVisitor<TVisitableController, TVisitorAcceptor> visitor);
    }
}
