using FinalGirlStatBot.Abstract;

namespace FinalGirlStatBot.Services.TelegramServices;

public class TelegramPollingService : PollingServiceBase<TelegramReceiverService>
{
    public TelegramPollingService(IServiceProvider serviceProvider, ILogger<TelegramPollingService> logger)
        : base(serviceProvider, logger)
    {
    }
}
