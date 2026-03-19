using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;

public abstract class BaseBoxCreationAction
{
    protected readonly ITelegramBotClient _botClient;
    protected readonly GameManager _gameManager;
    protected readonly IFGStatsUnitOfWork _db;

    public BaseBoxCreationAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    {
        _db = dbConnection;
        _botClient = botClient;
        _gameManager = gameManager;
    }

    public abstract Task Execute(Chat chatInfo, CancellationToken cancellationToken = default, string data = "");
}
