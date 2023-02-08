using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;
using XO;

namespace Common.XO.Requests.DAL
{
    public partial class LinkDALActionRequest : LinkFutureCompatibility
    {
        public XO.Responses.LinkErrorValue Validate(bool synchronousRequest = true)
        {
            // No validation on these values
            return null;
        }

        public LinkDALActionType? DALAction { get; set; }
    }

    //DAL action selection
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LinkDALActionType
    {
        EndADAMode,
        StartADAMode,
        SendReset,
        GetStatus,
        GetPayment,
        GetCreditOrDebit,
        GetPIN,
        GetZIP,
        RemoveCard,
        GetIdentifier,
        GetIdentifierAndPayment,
        GetIdentifierAndHoldData,
        UseHeldData,
        DeviceUI,
        CancelPayment,
        StartPreSwipeMode,
        WaitForCardPreSwipeMode,
        EndPreSwipeMode,
        PurgeHeldCardData,
        StartManualPayment,
        MonitorMessageUpdate,
        GetTestPayment,
        GetTestStatus,
        DeviceUITest,
        GetVerifyAmount,
        SaveRollCall,
        GetSignature,
        EndSignatureMode
    }
}
