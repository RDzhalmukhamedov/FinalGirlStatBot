using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public abstract class GameStateActionBase(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
{
    protected readonly IFGStatsUnitOfWork _db = dbConnection;
    protected readonly ITelegramBotClient _botClient = botClient;
    protected readonly GameManager _gameManager = gameManager;

    public abstract Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken);

    protected async Task<Message> BaseMessage(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        var message = await _botClient.EditMessageText(gameInfo.Game.User.ChatId, (int)gameInfo.MessageId, $"Какой фильм будем снимать?\n{gameInfo.Game}");

        var keyboard = new InlineKeyboardButton[][]
            {
                [("Выбрать 👩", "sGirl"), ("Выбрать 🔪", "sKiller"), ("Выбрать 🏫", "sLocation")],
                [("Случ. 👩", "rGirl"), ("Случ. 🔪", "rKiller"), ("Случ. 🏫", "rLocation")]
            };
        if (gameInfo.Game.ReadyToStart)
        {
            keyboard = keyboard.Append([("Начинаем съёмку!", "startGame")]).ToArray();
        }
        message = await _botClient.EditMessageReplyMarkup(gameInfo.Game.User.ChatId, message.Id, keyboard);
        gameInfo.MessageId = message.Id;

        return message;
    }

    protected async Task<Message> SelectionMessage(GameInfo gameInfo, string text, InlineKeyboardButton[][] keyboard, CancellationToken cancellationToken)
    {
        var message = await _botClient.EditMessageText(gameInfo.Game.User.ChatId, (int)gameInfo.MessageId, $"{text}");
        message = await _botClient.EditMessageReplyMarkup(gameInfo.Game.User.ChatId, message.Id, keyboard);
        gameInfo.MessageId = message.Id;

        return message;
    }

    protected async Task<Message> SelectedGirl(GameInfo gameInfo, CancellationToken cancellationToken, Season selectedSeason = Season.S1)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);

        gameInfo.State = GameState.SelectGirl;

        // TODO Возможно все это можно вынести
        var girlsBySeason = allGirls.GroupBy(g => g.Season).ToDictionary(g => g.Key, g => g.ToList());
        var chunkedGirls = girlsBySeason[selectedSeason].Chunk(2);
        var keyboard = new InlineKeyboardButton[chunkedGirls.Count() + 1][];
        foreach (var girls in chunkedGirls)
        {
            keyboard = keyboard.Append(girls.ToButtons()).ToArray();
        }

        var seasonsButtons = girlsBySeason.Select(gs =>
            new InlineKeyboardButton(gs.Key == selectedSeason ? $"*·{gs.Key.ToString()}·*" : gs.Key.ToString(), gs.Key.ToString())).ToArray();
        keyboard = keyboard.Append(seasonsButtons).ToArray();

        return await SelectionMessage(gameInfo, "👩 Выберите девушку:", keyboard, cancellationToken);
    }

    protected async Task<Message> SelectedKiller(GameInfo gameInfo, CancellationToken cancellationToken, Season selectedSeason = Season.S1)
    {
        var allKillers = await _db.Killers.GetAll(cancellationToken);

        gameInfo.State = GameState.SelectKiller;

        // TODO Возможно все это можно вынести
        var killersBySeason = allKillers.GroupBy(g => g.Season).ToDictionary(g => g.Key, g => g.ToList());
        var SelectedSeasonKillers = killersBySeason[selectedSeason];
        var keyboard = new InlineKeyboardButton[SelectedSeasonKillers.Count() + 1][];
        foreach (var killer in SelectedSeasonKillers)
        {
            keyboard = keyboard.Append([(killer.Name, killer.Id.ToString())]).ToArray();
        }

        var seasonsButtons = killersBySeason.Select(gs =>
            new InlineKeyboardButton(gs.Key == selectedSeason ? $"*·{gs.Key.ToString()}·*" : gs.Key.ToString(), gs.Key.ToString())).ToArray();
        keyboard = keyboard.Append(seasonsButtons).ToArray();

        return await SelectionMessage(gameInfo, "🔪 Выберите убийцу:", keyboard, cancellationToken);
    }

    protected async Task<Message> SelectedLocation(GameInfo gameInfo, CancellationToken cancellationToken, Season selectedSeason = Season.S1)
    {
        var allLocations = await _db.Locations.GetAll(cancellationToken);

        gameInfo.State = GameState.SelectLocation;

        // TODO Возможно все это можно вынести
        var locationsBySeason = allLocations.GroupBy(g => g.Season).ToDictionary(g => g.Key, g => g.ToList());
        var SelectedSeasonLocations = locationsBySeason[selectedSeason];
        var keyboard = new InlineKeyboardButton[SelectedSeasonLocations.Count() + 1][];
        foreach (var killer in SelectedSeasonLocations)
        {
            keyboard = keyboard.Append([(killer.Name, killer.Id.ToString())]).ToArray();
        }

        var seasonsButtons = locationsBySeason.Select(gs =>
            new InlineKeyboardButton(gs.Key == selectedSeason ? $"*·{gs.Key.ToString()}·*" : gs.Key.ToString(), gs.Key.ToString())).ToArray();
        keyboard = keyboard.Append(seasonsButtons).ToArray();

        return await SelectionMessage(gameInfo, "🏫 Выберите локацию:", keyboard, cancellationToken);
    }

    protected async Task<Message> Reset(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        _gameManager.DeleteGame(gameInfo.Game.User.ChatId);

        await _botClient.DeleteMessage(gameInfo.Game.User.ChatId, (int)gameInfo.MessageId, cancellationToken);

        var message = await _botClient.SendMessage(
            chatId: gameInfo.Game.User.ChatId,
            text: $"{gameInfo.Game}\nСъёмки прерваны, статистика записана не будет!",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        return message;
    }
}
