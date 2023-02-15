using XO.ProtoBuf;
using LinkRequest = Common.XO.Requests.LinkRequest;

namespace Devices.Common.State
{
    public sealed class CommunicationObject
    {
        public LinkRequest LinkRequest { get; set; }
        public CommunicationHeader Header { get; set; }

        public CommunicationObject(CommunicationHeader header, LinkRequest request)
        {
            Header = header;
            LinkRequest = request;
        }
    }
}
