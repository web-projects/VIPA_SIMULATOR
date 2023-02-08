using System;

namespace Devices.Sdk.Features.State
{
    /// <summary>
    /// Represents a set of sub-workflow states that represent certain specific
    /// processes that need to be completed before a transition occurs to send us
    /// back to the Manage state (Idle).
    /// </summary>
    public enum DALSubWorkflowState : int
    {
        /// <summary>
        /// Default state for all SubWorkflows.
        /// </summary>
        Undefined,

        /// <summary>
        /// Represents a state when DAL starts getting status information from the device
        /// </summary>
        GetStatus,

        [Obsolete]
        /// <summary>
        /// Represents a state when DAL asks user to present the card either by inserting, swiping or tapping it
        /// </summary>
        PresentCard,

        /// <summary>
        /// Represents a state when DAL tries to get card data from the device
        /// </summary>
        GetCardData,

        /// <summary>
        /// Represents a state when DAL tries to get PAN data from the device
        /// </summary>
        GetManualPANData,

        /// <summary>
        /// Represents a state when DAL asks user to verify payment amount
        /// </summary>
        GetVerifyAmount,

        /// <summary>
        /// Represents a state when DAL asks user to select the card type (debit/credit)
        /// </summary>
        GetCreditOrDebit,

        /// <summary>
        /// Respresents a state when DAL asks user to input the Zip Code into the device
        /// </summary>
        GetZip,

        /// <summary>
        /// Represents a state when DAL asks user to input PIN into the device
        /// </summary>
        GetPin,

        /// <summary>
        /// Represents a state when DAL directs user to remove EMV card from the device
        /// </summary>
        RemoveCard,

        /// <summary>
        /// Represents a state when DAL asks the device for UI processing
        /// </summary>
        DeviceUI,

        /// <summary>
        /// Represents a state when DAL starts ADA Mode on the device
        /// </summary>
        StartADAMode,

        /// <summary>
        /// Represents a state when DAL waits for a command after initializing ADA mode
        /// </summary>
        ADAIdleState,

        /// <summary>
        /// Represents a state when DAL ends ADA Mode on the device
        /// </summary>
        EndADAMode,

        /// <summary>
        /// Represents a state where a sanity check is performed to ensure that the DAL
        /// is in an operational state ready to receive the next command before a response
        /// is sent back to the caller.
        /// </summary>
        SanityCheck,

        /// <summary>
        /// Represents a state when SubWorkflow Completes
        /// </summary>
        RequestComplete,

        /// <summary>
        /// Represents a state when DAL starts PreSwipe mode.
        /// </summary>
        StartPreSwipeMode,

        /// <summary>
        /// Represents a state when DAL ends PreSwipe mode.
        /// </summary>
        EndPreSwipeMode,

        /// <summary>
        /// Represents a state when DAL enters a mode to wait for a cardswipe.
        /// </summary>
        WaitForCardPreSwipeMode,

        /// <summary>
        /// State where held card data is purged (either by request or timeout).
        /// </summary>
        PurgeHeldCardData,

        /// <summary>
        /// Respresents a state when DAL asks user to input the Signature into the device
        /// </summary>
        StartSignatureMode,

        /// <summary>
        /// Respresents a state when DAL asks user to end the Signature mode in the device
        /// </summary>
        EndSignatureMode,
    }
}
