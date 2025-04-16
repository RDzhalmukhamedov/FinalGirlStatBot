using FinalGirlStatBot.Abstract;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace FinalGirlStatBot.Services;

public class TelegramReceiverService : ReceiverServiceBase<TelegramUpdateHandler>
{
    public TelegramReceiverService(
        ITelegramBotClient botClient,
        TelegramUpdateHandler updateHandler,
        ILogger<ReceiverServiceBase<TelegramUpdateHandler>> logger,
        IOptions<BotConfiguration> config)
        : base(botClient, updateHandler, logger, config)
    {
    }
}
