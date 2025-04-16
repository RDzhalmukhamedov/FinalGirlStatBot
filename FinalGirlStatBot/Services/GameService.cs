using FinalGirlStatBot.Abstract;
using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.Services.UserActionHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
        var additionalButtons = new InlineKeyboardButton[1];

        var (success, game) = _gameManager.StartNewGame(user);

        var gameInfo = _gameManager.GetGameInfo(chatInfo.Id);

        if (!success)
        {
            if (game is not null)
            {
                additionalButtons = [("🗘 Сброс", "reset")];

                await _botClient.SendMessage(
                    chatId: chatInfo.Id,
                    text: $"У вас есть другой незавершенный фильм: {game.ToString()}",
                    cancellationToken: cancellationToken);
            }
            else
            {
                if (gameInfo.State != GameState.Stats)
                {
                    _gameManager.DeleteGame(chatInfo.Id);

                    await _botClient.SendMessage(
                        chatId: chatInfo.Id,
                        text: $"Что-то пошло не так, попробуйте ещё раз",
                        cancellationToken: cancellationToken);

                    return;
                }

                _gameManager.DeleteGame(chatInfo.Id);
                _gameManager.StartNewGame(user);
            }
        }

        gameInfo = _gameManager.GetGameInfo(chatInfo.Id);

        if (gameInfo.Game.ReadyToStart)
        {
            additionalButtons = additionalButtons.Append(("Начинаем съёмку!", "startGame")).ToArray();
        }
        var keyboard = new InlineKeyboardButton[][]
            {
                [("Выбрать 👩", "sGirl"), ("Выбрать 🔪", "sKiller"), ("Выбрать 🏫", "sLocation")],
                [("Случ. 👩", "rGirl"), ("Случ. 🔪", "rKiller"), ("Случ. 🏫", "rLocation")]
            };
        keyboard = keyboard.Append(additionalButtons).ToArray();

        var message = await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"Какой фильм будем снимать?\n{gameInfo.Game}",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        gameInfo.MessageId = message.Id;
    }

    public async Task GetStatistics(Chat chatInfo, CancellationToken cancellationToken)
    {
        var user = await _db.Users.CreateIfNotExist(chatInfo.Id, chatInfo.Username, cancellationToken);
        _gameManager.CreateStatsDummy(user);

        var games = await _db.Games.GetByUser(chatId: chatInfo.Id, cancellationToken);
        var wins = games.Where(g => g.Result == DB.Domain.ResultType.Win).Count();
        var loses = games.Where(g => g.Result == DB.Domain.ResultType.Lose).Count();
        var unknown = games.Count - (wins + loses);
        var unknownString = unknown > 0 ? $" (из них {unknown} без результата)" : "";

        var keyboard = new InlineKeyboardButton[][]
            {
                [("Статистика по 🔪", "statKiller"), ("Статистика по 🏫", "statLocation")]
            };

        var message = await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"Общее количество игр: {games.Count}{unknownString}.\nПроцент побед: {Math.Round((double) wins * 100 / (games.Count - unknown), 2)}% ({wins}/{loses})",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

    }

    public async Task ProcessUserInput(long chatId, string data, CancellationToken cancellationToken)
    {
        var gameInfo = _gameManager.GetGameInfo(chatId);
        await _stateActionFactory.GetStateAction(gameInfo.State).DoAction(gameInfo, data, cancellationToken);
    }
}
