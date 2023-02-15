namespace LoggingService.Client.Providers
{
    public interface ILoggingServiceClientProvider
    {
        ILoggingServiceClient GetLoggingServiceClient();
    }
}
