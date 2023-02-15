using XO.ProtoBuf;
using LinkRequest = Common.XO.Requests.LinkRequest;

namespace Devices.Sdk.Features.State.Providers
{
    internal interface IDALPreProcessor
    {
        bool CanHandleRequest(LinkRequest request, IDALSubStateController subStateController);
        void HandleRequest(CommunicationHeader header, LinkRequest request, IDALSubStateController subStateController);
    }
}
