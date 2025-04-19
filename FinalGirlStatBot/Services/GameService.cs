using FinalGirlStatBot.Abstract;
using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.Services.UserActionHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Game = FinalGirlStatBot.DB.Domain.Game;
using User = FinalGirlStatBot.DB.Domain.User;

namespace FinalGirlStatBot.Services;

public class GameService(GameManager gameManager, IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameStateActionFactory stateActionFactory) : IGameService
{
    private readonly GameManager _gameManager = gameManager;
    private readonly IFGStatsUnitOfWork _db = dbConnection;
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly GameStateActionFactory _stateActionFactory = stateActionFactory;

    public async Task StartNewGame(Chat chatInfo, CancellationToken cancellationToken)
    {
        var user = await _db.Users.CreateIfNotExist(chatInfo.Id, chatInfo.Username, cancellationToken);
        await _db.Commit(cancellationToken);

        var gameInfo = _gameManager.GetGameInfo(chatInfo.Id);

        if (gameInfo is not null)
        {
            await ReprocessCurrentGameState(gameInfo, user, cancellationToken);

            return;
        }

        await CreateNewGame(user, cancellationToken);
    }

    public async Task GetStatistics(Chat chatInfo, CancellationToken cancellationToken)
    {
        var user = await _db.Users.CreateIfNotExist(chatInfo.Id, chatInfo.Username, cancellationToken);
        await _db.Commit(cancellationToken);

        var gameInfo = _gameManager.GetGameInfo(chatInfo.Id);

        if (gameInfo is not null)
        {
            await ReprocessCurrentGameState(gameInfo, user, cancellationToken, true);

            if (gameInfo.State != GameState.Stats) return;
        }

        var (success, statInfo) = _gameManager.CreateStatsDummy(user);

        var games = await _db.Games.GetByUser(chatId: chatInfo.Id, cancellationToken);

        var message = await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: BuildMainStatsString(games),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: games.Count > 0 ? Shared.Buttons.StatsKeyboard : null,
            cancellationToken: cancellationToken);

        statInfo.MessageId = message.Id;
    }

    public async Task SendUsage(Chat chatInfo, CancellationToken cancellationToken)
    {
        var message = await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: "Команды для бота:\n/ng - записать информацию о сыгранной партии,\n/stat - статистика по сыгранным партиям",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    public async Task ProcessUserInput(long chatId, string data, CancellationToken cancellationToken)
    {
        var gameInfo = _gameManager.GetGameInfo(chatId);
        if (gameInfo is not null)
        {
            await _stateActionFactory.GetStateAction(gameInfo.State).DoAction(gameInfo, data, cancellationToken);
        }
        else
        {
            var parts = data.Split('-');
            if (parts.Length > 0 && parts[0].Equals(Shared.Text.RepeatGameCallback))
            {
                Game? lastGame = null;
                int gameId = 0;
                var hasGameId = parts.Length > 1 && int.TryParse(parts[1], out gameId);
                if (hasGameId)
                {
                    lastGame = await _db.Games.GetById(gameId, cancellationToken);
                }
                else
                {
                    lastGame = await _db.Games.GetLastForUser(chatId, cancellationToken);
                }

                var user = await _db.Users.GetByChatId(chatId, cancellationToken);
                await CreateNewGame(user, cancellationToken, lastGame);
            }
        }
    }

    private async Task ReprocessCurrentGameState(GameInfo gameInfo, User user, CancellationToken cancellationToken, bool skipNewGameCreation = false)
    {
        switch (gameInfo.State)
        {
            case GameState.Init:
                await _stateActionFactory.GetStateAction(GameState.Init).DoAction(gameInfo, Shared.Text.InitPrivateCallback, cancellationToken, $"{Shared.Text.UHaveUnfinishedGameMessage}:\n{gameInfo.Game}");
                break;
            case GameState.SelectGirl:
                await ResendSelectionMessage(gameInfo, Shared.Text.SelectGirlCallback, cancellationToken);
                break;
            case GameState.SelectKiller:
                await ResendSelectionMessage(gameInfo, Shared.Text.SelectKillerCallback, cancellationToken);
                break;
            case GameState.SelectLocation:
                await ResendSelectionMessage(gameInfo, Shared.Text.SelectLocationCallback, cancellationToken);
                break;
            case GameState.GameInProgress:
                await ResendSelectionMessage(gameInfo, Shared.Text.StartGameCallback, cancellationToken);
                break;
            case GameState.Stats:
                if (!skipNewGameCreation)
                {
                    _gameManager.DeleteGame(gameInfo.ChatId);
                    var (_, _) = _gameManager.StartNewGame(user);
                    var newGameInfo = _gameManager.GetGameInfo(gameInfo.ChatId);
                    await _stateActionFactory.GetStateAction(GameState.Init).DoAction(newGameInfo, Shared.Text.InitPrivateCallback, cancellationToken);
                }
                break;
        }
    }

    private async Task ResendSelectionMessage(GameInfo gameInfo, string callbackData, CancellationToken cancellationToken)
    {
        await _botClient.SendMessage(
                    chatId: gameInfo.ChatId,
                    text: $"{Shared.Text.UHaveUnfinishedGameMessage}:\n{gameInfo.Game}",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    cancellationToken: cancellationToken);
        await _stateActionFactory.GetStateAction(GameState.Init).DoAction(gameInfo, callbackData, cancellationToken);
    }

    private string BuildMainStatsString(List<Game> games)
    {
        var wins = games.Where(g => g.Result == ResultType.Win).Count();
        var loses = games.Where(g => g.Result == ResultType.Lose).Count();
        var unknown = games.Count - (wins + loses);
        var unknownString = unknown > 0 ? $" (из них {unknown} без результата)" : "";
        var percentageString = games.Count > 0 ? $"\n{Shared.Text.WinPercentageMessage} {Math.Round((double)wins * 100 / (games.Count - unknown), 2)}% ({wins}/{loses})" : "";

        return $"{Shared.Text.TotalGamesMessage} {games.Count}{unknownString}.{percentageString}";
    }

    private async Task CreateNewGame(User user, CancellationToken cancellationToken, Game gameFrom = null)
    {
        var (success, game) = _gameManager.StartNewGame(user);

        var gameInfo = _gameManager.GetGameInfo(user.ChatId);

        if (gameFrom is not null)
        {
            _gameManager.SetGirl(user.ChatId, gameFrom.Girl);
            _gameManager.SetKiller(user.ChatId, gameFrom.Killer);
            _gameManager.SetLocation(user.ChatId, gameFrom.Location);
        }

        if (success)
        {
            await _stateActionFactory.GetStateAction(GameState.Init).DoAction(gameInfo, Shared.Text.InitPrivateCallback, cancellationToken);
        }
        else
        {
            _gameManager.DeleteGame(user.ChatId);

            await _botClient.SendMessage(
                chatId: user.ChatId,
                text: Shared.Text.SomethingWrongMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
        }
    }
}
