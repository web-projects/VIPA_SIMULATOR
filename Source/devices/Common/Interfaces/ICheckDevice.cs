using Common.XO.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Devices.Common.Interfaces
{
    public interface ICheckDevice : IPaymentDevice
    {
        // ------------------------------------------------------------------------
        // FEATURE: PAYMENT
        // ------------------------------------------------------------------------

        LinkRequest PresentCheck(LinkRequest request, CancellationToken cancellationToken);

        LinkRequest GetCheckData(LinkRequest request, CancellationToken cancellationToken);

        Task<LinkRequest> RemoveCheck(LinkRequest request, CancellationToken cancellationToken);
    }
}
