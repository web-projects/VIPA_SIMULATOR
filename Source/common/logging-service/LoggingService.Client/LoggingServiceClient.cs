using LoggingService.Client.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using XO.ProtoBuf;
using static XO.ProtoBuf.LogMessage.Types;

namespace LoggingService.Client
{
    internal sealed class LoggingServiceClient : ILoggingServiceClient, IDisposable
    {
        private const int MaximumBlockBatchBufferCount = 1000;

        private static readonly ConcurrentBag<long> frequencyTracker = new ConcurrentBag<long>();
        private static readonly string logBatchPath = "api/logging/logbatch";
        private static readonly long frequencyInterval = TimeSpan.FromSeconds(10).Ticks;

        //private readonly IProtoBufHttpClient protoBufHttpClient;
        private readonly Uri uri;
        private readonly string callingAssemblyName;
        private readonly string hostName;
        private readonly object lockObject = new object();
        private readonly IMultiTypeObjectPool objectPool;
        private volatile bool batchProcessing;
        private readonly ActionBlock<LogMessage> logActionBlock;
        private readonly BufferBlock<LogMessage> logBufferBlock;
        private int lastDelay;
        private int totalDelay;
        private volatile bool bypassDelay;
        private CancellationTokenSource bypassDelayCts = new CancellationTokenSource();
        private volatile bool isDisposed;

        public LoggingServiceClient(
            //IProtoBufHttpClient protoBufHttpClient,
            LoggingClientConfig loggingClientConfig,
            string callingAssemblyName,
            string hostName,
            //IMultiTypeObjectPool objectPool)
        {
            //this.protoBufHttpClient = protoBufHttpClient;
            uri = new Uri(loggingClientConfig.Url);
            this.callingAssemblyName = callingAssemblyName;
            this.hostName = hostName;
            logActionBlock = new ActionBlock<LogMessage>(async myLogMessage => await AddToBatchAsync(myLogMessage));
            logBufferBlock = new BufferBlock<LogMessage>();
            this.objectPool = objectPool;
        }

        public static int MinDelay { get; } = 2000;     // Minimum delay in milliseconds between batch submissions.

        public static int MaxDelay { get; } = 10000;    // Maximum delay..

        public async Task LogCriticalAsync(string message, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Critical, message, additionalData: additionalData);

        public async Task LogCriticalAsync(string message, Exception exception, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Critical, message, exception, additionalData);

        public async Task LogCriticalAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Critical, message, additionalData: additionalData, statusCode: statusCode, statusType: statusType);

        public async Task LogCriticalAsync(string message, int statusCode, StatusType statusType, Exception exception, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Critical, message, exception, additionalData: additionalData, statusCode, statusType);

        public async Task LogCriticalAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Critical, message, additionalData: additionalData, statusType: statusType);

        public async Task LogCriticalAsync(string message, StatusType statusType, Exception exception, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Critical, message, exception, additionalData: additionalData, statusType: statusType);

        public async Task LogCriticalAsync(string message, int statusCode, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Critical, message, additionalData: additionalData, statusCode: statusCode);

        public async Task LogCriticalAsync(string message, int statusCode, Exception exception, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Critical, message, exception, additionalData: additionalData, statusCode);

        public async Task LogDebugAsync(string message, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Debug, message, additionalData: additionalData);

        public async Task LogDebugAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Debug, message, additionalData: additionalData, statusCode: statusCode, statusType: statusType);

        public async Task LogDebugAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Debug, message, additionalData: additionalData, statusType: statusType);

        public async Task LogDebugAsync(string message, int statusCode, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Debug, message, additionalData: additionalData, statusCode: statusCode);

        public async Task LogErrorAsync(string message, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Error, message, additionalData: additionalData);

        public async Task LogErrorAsync(string message, Exception exception, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Error, message, exception, additionalData: additionalData);

        public async Task LogErrorAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Error, message, additionalData: additionalData, statusCode: statusCode, statusType: statusType);

        public async Task LogErrorAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Error, message, additionalData: additionalData, statusType: statusType);

        public async Task LogErrorAsync(string message, int statusCode, StatusType statusType, Exception exception, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Error, message, exception, additionalData: additionalData, statusCode, statusType);

        public async Task LogErrorAsync(string message, StatusType statusType, Exception exception, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Error, message, exception, additionalData: additionalData, statusType: statusType);

        public async Task LogErrorAsync(string message, int statusCode, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Error, message, additionalData: additionalData, statusCode: statusCode);

        public async Task LogErrorAsync(string message, int statusCode, Exception exception, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Error, message, exception, additionalData: additionalData, statusCode);

        public async Task LogInfoAsync(string message, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Info, message, additionalData: additionalData);

        public async Task LogInfoAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Info, message, additionalData: additionalData, statusCode: statusCode, statusType: statusType);

        public async Task LogInfoAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Info, message, additionalData: additionalData, statusType: statusType);

        public async Task LogInfoAsync(string message, int statusCode, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Info, message, additionalData: additionalData, statusCode: statusCode);

        public async Task LogTraceAsync(string message, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Trace, message, additionalData: additionalData);

        public async Task LogTraceAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Trace, message, additionalData: additionalData, statusCode: statusCode, statusType: statusType);

        public async Task LogTraceAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Trace, message, additionalData: additionalData, statusType: statusType);

        public async Task LogTraceAsync(string message, int statusCode, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Trace, message, additionalData: additionalData, statusCode: statusCode);

        public async Task LogWarnAsync(string message, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Warn, message, additionalData: additionalData);

        public async Task LogWarnAsync(string message, int statusCode, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Warn, message, additionalData: additionalData, statusCode: statusCode, statusType: statusType);

        public async Task LogWarnAsync(string message, StatusType statusType, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Warn, message, additionalData: additionalData, statusType: statusType);

        public async Task LogWarnAsync(string message, int statusCode, IDictionary<string, object> additionalData = null)
            => await LogAsync(LogLevel.Warn, message, additionalData: additionalData, statusCode: statusCode);

        public async Task LogAsync(
            LogLevel logLevel,
            string message,
            Exception ex = null,
            IDictionary<string, object> additionalData = null,
            int statusCode = 0,
            StatusType statusType = StatusType.NotInitialized)
        {
            if (!bypassDelay)
            {
                UpdateFrequencyTracker();
            }

            // Rent a logMessage from the objectPool to use thoughout the pipeline.
            LogMessage logMessage = objectPool.Rent<LogMessage>();
            logMessage.LogLevel = logLevel;
            logMessage.AssemblyName = callingAssemblyName;
            logMessage.TimeStamp = DateTimeOffset.Now.ToString(LoggingServiceConstants.DateTimeMilliSecondFormat);
            logMessage.HostName = hostName;

            // Do not inline these assignments. 
            // Construction of the LogMessage object will throw an ArgumentNullException if you try to assign null.
            if (!string.IsNullOrWhiteSpace(message))
            {
                logMessage.Message = message;
            }

            if (ex != null)
            {
                logMessage.Exception = ex.ToString();
            }

            if (additionalData != null)
            {
                foreach (KeyValuePair<string, object> param in additionalData)
                {
                    logMessage.Parameters.Add(param.Key, param.Value == null ? "[null]" : param.Value.ToString());
                }
            }

            logMessage.StatusCode = statusCode;
            logMessage.StatusType = (int)statusType;

            await logActionBlock.SendAsync(logMessage);
        }

        private void UpdateFrequencyTracker()
        {
            long now = DateTime.Now.Ticks;
            IEnumerable<long> expiredEntries = frequencyTracker.Where(x => now - x > frequencyInterval);

            foreach (long expiredEntry in expiredEntries)
            {
                frequencyTracker.TryTake(out _);
            }

            frequencyTracker.Add(now);
        }

        public async Task AddToBatchAsync(LogMessage logMessage)
        {
            if (logMessage == null)
            {
                return;
            }

            await logBufferBlock.SendAsync(logMessage);

            // Double-checked locking to prevent StartBatchAsync from getting called > 1 time concurrently.
            if (!batchProcessing)
            {
                lock (lockObject)
                {
                    if (!batchProcessing)
                    {
                        batchProcessing = true;
                        _ = StartBatchAsync();
                    }
                }
            }
        }

        public async Task StartBatchAsync()
        {
            int batchDelay;
            lastDelay = 0;
            totalDelay = 0;

            while (!bypassDelay && (batchDelay = CalculateDelay()) > 0)
            {
                try
                {
                    await Task.Delay(batchDelay, bypassDelayCts.Token);
                }
                catch (TaskCanceledException) { /* nothing to do here*/ }
            }

            if (logBufferBlock.Count == 0)
            {
                batchProcessing = false;
                return;
            }

            while (true)
            {
                LogBatch logBatch = objectPool.Rent<LogBatch>();

                try
                {
                    // We should also do so asynchronously to increase 
                    // application performance and reduce CPU waste.
                    while (logBufferBlock.Count > 0 && logBatch.LogMessages.Count <= MaximumBlockBatchBufferCount)
                    {
                        LogMessage msg = await logBufferBlock.ReceiveAsync();
                        logBatch.LogMessages.Add(msg);
                    }

                    if (logBatch.LogMessages.Count == 0)
                    {
                        break;
                    }

                    await protoBufHttpClient.PostAsync(uri, logBatchPath, logBatch)
                        .ConfigureAwait(false);

                    // On success, return objects to object pool.
                    if (protoBufHttpClient.LastHttpResponse.IsSuccessStatusCode)
                    {
                        foreach (LogMessage logMessage in logBatch.LogMessages)
                        {
                            objectPool.ReturnToObjectPool(logMessage);
                        }
                    }
                    // If post did not succeed, requeue all logMessages.
                    else
                    {
                        foreach (LogMessage logMessage in logBatch.LogMessages)
                        {
                            await AddToBatchAsync(logMessage);
                        }
                    }


                    break;
                }
                catch (Exception)
                {
                    // If an exception does occur we can at least ensure
                    // that it doesn't cause the "batchProcessing" flag
                    // to be left in a bad state that prevents any further
                    // processing from taking place.
                }
                finally
                {
                    objectPool.ReturnToObjectPool(logBatch);
                }
            }

            batchProcessing = false;

            // If we received logs between sending the batch to the logging service and now, we need to start a new batch.
            // The reason is because we could have received logs while processing a batch and logging has stopped.
            if (logBufferBlock.Count > 0)
            {
                _ = StartBatchAsync();
            }
        }

        // TODO: Decide if we should replace with genetic algorithm.
        public int CalculateDelay()
        {
            if (bypassDelay)
            {
                return 0;
            }

            double frequency = frequencyTracker.Count / 10.0;
            int delay = (int)(Math.Pow(2.0, frequency) * 1000.0);

            if (delay < MinDelay)
            {
                delay = MinDelay;
            }

            if (delay > lastDelay)
            {
                if (delay + totalDelay > MaxDelay)
                {
                    delay = MaxDelay - totalDelay;
                }

                lastDelay = delay;
                totalDelay += delay;
            }
            else
            {
                delay = 0;
            }

            return delay;
        }

        public async Task WaitForDrainAsync(CancellationToken waitCancellationToken = default)
        {
            bypassDelay = true;
            bypassDelayCts.Cancel();
            await Task.Delay(MinDelay);

            while (logBufferBlock.Count > 0 && !waitCancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50);
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                if (bypassDelayCts != null)
                {
                    bypassDelayCts.Dispose();
                    bypassDelayCts = null;
                }
            }
        }
    }
}
