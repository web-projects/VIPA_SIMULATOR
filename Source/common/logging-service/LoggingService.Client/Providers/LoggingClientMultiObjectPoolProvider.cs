using IPA5.Core.Patterns.Pools;
using IPA5.Core.Providers;
using IPA5.XO.ProtoBuf;
using IPA5.Core.Extensions;

namespace LoggingService.Client.Providers
{
    internal sealed class LoggingClientMultiObjectPoolProvider : IMultiObjectPoolProvider
    {
        private static readonly object LockObject = new object();
        private static IMultiTypeObjectPool Instance;

        public IMultiTypeObjectPool GetMultiTypeObjectPool(int maxTypePoolSize = 0)
        {
            if (Instance is null)
            {
                lock (LockObject)
                {
                    if (Instance is null)
                    {
                        Instance = new MultiTypeObjectPool(maxTypePoolSize);
                        Instance.RegisterType<LogMessage>(deactivator: LogMessageExtensions.Deactivate);
                        Instance.RegisterType<LogBatch>(deactivator: LogBatchExtensions.Deactivate);
                    }
                }
            }

            return Instance;
        }
    }
}
