using IPA5.XO.Enums.Legacy;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static IPA5.XO.ProtoBuf.LogMessage.Types;

namespace LoggingService.Client
{
    public interface ILoggingServiceClient
    {
        Task LogCriticalAsync(string message, IDictionary<string, object> additionalData = null);
        Task LogCriticalAsync(string message, Exception exception, IDictionary<string, object> additionalData = null);
        Task LogCriticalAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogCriticalAsync(string message, int statusCode, StatusType statusType, Exception exception, IDictionary<string, object> additionalData = null);
        Task LogCriticalAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogCriticalAsync(string message, StatusType statusType, Exception exception, IDictionary<string, object> additionalData = null);
        Task LogCriticalAsync(string message, int statusCode, IDictionary<string, object> additionalData = null);
        Task LogCriticalAsync(string message, int statusCode, Exception exception, IDictionary<string, object> additionalData = null);
        Task LogDebugAsync(string message, IDictionary<string, object> additionalData = null);
        Task LogDebugAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogDebugAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogDebugAsync(string message, int statusCode, IDictionary<string, object> additionalData = null);
        Task LogErrorAsync(string message, IDictionary<string, object> additionalData = null);
        Task LogErrorAsync(string message, Exception exception, IDictionary<string, object> additionalData = null);
        Task LogErrorAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogErrorAsync(string message, int statusCode, StatusType statusType, Exception exception, IDictionary<string, object> additionalData = null);
        Task LogErrorAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogErrorAsync(string message, StatusType statusType, Exception exception, IDictionary<string, object> additionalData = null);
        Task LogErrorAsync(string message, int statusCode, IDictionary<string, object> additionalData = null);
        Task LogErrorAsync(string message, int statusCode, Exception exception, IDictionary<string, object> additionalData = null);
        Task LogInfoAsync(string message, IDictionary<string, object> additionalData = null);       
        Task LogInfoAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogInfoAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogInfoAsync(string message, int statusCode, IDictionary<string, object> additionalData = null);
        Task LogTraceAsync(string message, IDictionary<string, object> additionalData = null);
        Task LogTraceAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogTraceAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogTraceAsync(string message, int statusCode, IDictionary<string, object> additionalData = null);
        Task LogWarnAsync(string message, IDictionary<string, object> additionalData = null);
        Task LogWarnAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null);  
        Task LogWarnAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null);
        Task LogWarnAsync(string message, int statusCode, IDictionary<string, object> additionalData = null);
        Task LogAsync(LogLevel logLevel, string message, Exception ex = null, IDictionary<string, object> additionalData = null, int statusCode = 0, StatusType statusType = StatusType.NotInitialized);
        Task WaitForDrainAsync(CancellationToken waitCancellationToken = default);
    }
}
