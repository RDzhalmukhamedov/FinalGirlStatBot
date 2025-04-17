using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public abstract class GameStateActionBase(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
{
    protected readonly IFGStatsUnitOfWork _db = dbConnection;
    protected readonly ITelegramBotClient _botClient = botClient;
    protected readonly GameManager _gameManager = gameManager;

    public abstract Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken);

    protected async Task<Message> SendInitMessage(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        var keyboard = gameInfo.ReadyToStart ? Shared.Buttons.InitKeyboardReadyToStart : Shared.Buttons.InitKeyboard;

        if (gameInfo.MessageId is not null) await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);
        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: $"{Shared.Text.SelectionQuestionMessage}\n{gameInfo.Game}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        gameInfo.MessageId = message.Id;

        return message;
    }

    protected async Task<Message> SendSelectionMessage(GameInfo gameInfo, string text, InlineKeyboardButton[][] keyboard, CancellationToken cancellationToken)
    {
        if (gameInfo.MessageId is not null) await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);
        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: text,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        gameInfo.MessageId = message.Id;

        return message;
    }

    protected async Task<Message> SendGirlSelector(GameInfo gameInfo, CancellationToken cancellationToken, Season selectedSeason = Season.S1)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);

        gameInfo.State = GameState.SelectGirl;

        var girlsBySeason = allGirls.GroupBy(g => g.Season).ToDictionary(g => g.Key, g => g.ToList());
        var chunkedSeasonGirls = girlsBySeason[selectedSeason].Chunk(2);
        var keyboard = Array.Empty<InlineKeyboardButton[]>();
        foreach (var girls in chunkedSeasonGirls)
        {
            keyboard = keyboard.Append(girls.Select(go => new InlineKeyboardButton(go.Name, go.Id.ToString())).ToArray()).ToArray();
        }

        keyboard = keyboard.Append(GetSeasonButtons(girlsBySeason.Keys, selectedSeason)).ToArray();

        return await SendSelectionMessage(gameInfo, Shared.Text.SelectGirlMessage, keyboard, cancellationToken);
    }

    protected async Task<Message> SendKillerSelector(GameInfo gameInfo, CancellationToken cancellationToken, Season selectedSeason = Season.S1)
    {
        var allKillers = await _db.Killers.GetAll(cancellationToken);

        gameInfo.State = GameState.SelectKiller;

        var killersBySeason = allKillers.GroupBy(g => g.Season).ToDictionary(g => g.Key, g => g.ToList());
        var selectedSeasonKillers = killersBySeason[selectedSeason];
        var keyboard = Array.Empty<InlineKeyboardButton[]>();
        foreach (var killer in selectedSeasonKillers)
        {
            keyboard = keyboard.Append([(killer.Name, killer.Id.ToString())]).ToArray();
        }

        keyboard = keyboard.Append(GetSeasonButtons(killersBySeason.Keys, selectedSeason)).ToArray();

        return await SendSelectionMessage(gameInfo, Shared.Text.SelectKillerMessage, keyboard, cancellationToken);
    }

    protected async Task<Message> SendLocationSelector(GameInfo gameInfo, CancellationToken cancellationToken, Season selectedSeason = Season.S1)
    {
        var allLocations = await _db.Locations.GetAll(cancellationToken);

        gameInfo.State = GameState.SelectLocation;

        var locationsBySeason = allLocations.GroupBy(g => g.Season).ToDictionary(g => g.Key, g => g.ToList());
        var selectedSeasonLocations = locationsBySeason[selectedSeason];
        var keyboard = Array.Empty<InlineKeyboardButton[]>();
        foreach (var location in selectedSeasonLocations)
        {
            keyboard = keyboard.Append([(location.Name, location.Id.ToString())]).ToArray();
        }

        keyboard = keyboard.Append(GetSeasonButtons(locationsBySeason.Keys, selectedSeason)).ToArray();

        return await SendSelectionMessage(gameInfo, Shared.Text.SelectLocationMessage, keyboard, cancellationToken);
    }

    protected async Task<Message> Reset(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        _gameManager.DeleteGame(gameInfo.ChatId);

        await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);

        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: $"{gameInfo.Game}\n{Shared.Text.GameResetedMessage}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        return message;
    }

    private static InlineKeyboardButton[] GetSeasonButtons(IEnumerable<Season> seasons, Season selectedSeason)
    {
        return seasons
            .Select
            (
                season => new InlineKeyboardButton(season == selectedSeason ? $"•{season.ToString()}•" : season.ToString(), season.ToString())
            )
            .ToArray();
    }
}
