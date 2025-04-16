using FinalGirlStatBot.Abstract;

namespace FinalGirlStatBot.Services;

public class TelegramPollingService : PollingServiceBase<TelegramReceiverService>
{
    public TelegramPollingService(IServiceProvider serviceProvider, ILogger<TelegramPollingService> logger)
        : base(serviceProvider, logger)
    {
    }
}
