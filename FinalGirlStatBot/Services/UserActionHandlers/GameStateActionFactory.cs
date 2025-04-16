using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class GameStateActionFactory(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
{
    protected readonly IFGStatsUnitOfWork _db = dbConnection;
    protected readonly ITelegramBotClient _botClient = botClient;
    protected readonly GameManager _gameManager = gameManager;

    public GameStateActionBase GetStateAction(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Init:
                return new InitStateAction(_db, _botClient, _gameManager);
            case GameState.SelectGirl:
                return new SelectGirlAction(_db, _botClient, _gameManager);
            case GameState.SelectKiller:
                return new SelectKillerAction(_db, _botClient, _gameManager);
            case GameState.SelectLocation:
                return new SelectLocationAction(_db, _botClient, _gameManager);
            case GameState.GameInProgress:
                return new GameInProgressAction(_db, _botClient, _gameManager);
            case GameState.Stats:
                return new StatisticsAction(_db, _botClient, _gameManager);
            default:
                return new InitStateAction(_db, _botClient, _gameManager);
        }
    }
}
