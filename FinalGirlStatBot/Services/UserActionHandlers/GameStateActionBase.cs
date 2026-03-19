using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public abstract class GameStateActionBase(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
{
    protected readonly IFGStatsUnitOfWork _db = dbConnection;
    protected readonly ITelegramBotClient _botClient = botClient;
    protected readonly GameManager _gameManager = gameManager;

    public abstract Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null);

    public abstract Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default);

    protected async Task<Message> UpdateMessage(GameInfo gameInfo, string text, InlineKeyboardButton[][]? keyboard,
        bool deletePrev = false, string additionalText = "", CancellationToken cancellationToken = default)
    {
        if (deletePrev && gameInfo.MessageId is not null)
        {
            await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);
            gameInfo.MessageId = null;
        }

        if (!string.IsNullOrEmpty(additionalText))
        {
            await _botClient.SendMessage(
                chatId: gameInfo.ChatId,
                text: additionalText,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        if (gameInfo.MessageId is not null)
        {
            var message = await _botClient.EditMessageText(
                chatId: gameInfo.ChatId,
                messageId: gameInfo.MessageId.Value,
                text: text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            return message;
        }
        else
        {
            var message = await _botClient.SendMessage(
                chatId: gameInfo.ChatId,
                text: text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            gameInfo.MessageId = message.Id;
            return message;
        }
    }

    protected async Task<ActionResult> Reset(GameInfo gameInfo, CancellationToken cancellationToken = default, string additionalMessage = "")
    {
        if (string.IsNullOrEmpty(additionalMessage))
        {
            var gameString = gameInfo.Game?.ToString();
            additionalMessage = $"{gameString}\n{Shared.Text.GameResetedMessage}";
        }
        _gameManager.ResetGame(gameInfo.ChatId);

        if (gameInfo.MessageId is not null)
            await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);

        gameInfo.MessageId = null;

        return ActionResult.Ok(GameState.Init, $"{additionalMessage}");
    }


    protected async Task<ActionResult> FillRepeatGameInfo(GameInfo gameInfo, IEnumerable<string> idStr, CancellationToken cancellationToken = default)
    {
        if (int.TryParse(idStr.First(), out var gameId))
        {
            var gameToRepeat = (await _db.Games.GetByUser(gameInfo.ChatId, cancellationToken)).FirstOrDefault(g => g?.Id == gameId, null);
            if (gameToRepeat is not null)
            {
                var user = await _db.Users.CreateIfNotExist(gameInfo.ChatId, null, cancellationToken);
                gameInfo = _gameManager.StartNewGame(user);
                _gameManager.SetFromOtherGame(gameInfo, gameToRepeat);

                return ActionResult.Ok(GameState.CreatingGame);
            }
        }

        return ActionResult.Error();
    }

    protected async Task<IEnumerable<GirlDto>> GetGirlsForUser(GameInfo gameInfo, CancellationToken cancellationToken = default)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);
        var userBoxIds = await _db.UserBoxes.GetBoxIdsForUser(gameInfo.ChatId, cancellationToken);
        var filteredGirls = allGirls.Where(g => g.BoxId.HasValue && userBoxIds.Contains(g.BoxId.Value)).ToList();

        return filteredGirls.Count > 0 ? filteredGirls : allGirls;
    }

    protected async Task<IEnumerable<KillerDto>> GetKillersForUser(GameInfo gameInfo, CancellationToken cancellationToken = default)
    {
        var allKillers = await _db.Killers.GetAll(cancellationToken);
        var userBoxIds = await _db.UserBoxes.GetBoxIdsForUser(gameInfo.ChatId, cancellationToken);
        var filteredKillers = allKillers.Where(g => g.BoxId.HasValue && userBoxIds.Contains(g.BoxId.Value)).ToList();

        return filteredKillers.Count > 0 ? filteredKillers : allKillers;
    }

    protected async Task<IEnumerable<LocationDto>> GetLocationsForUser(GameInfo gameInfo, CancellationToken cancellationToken = default)
    {
        var allLocations = await _db.Locations.GetAll(cancellationToken);
        var userBoxIds = await _db.UserBoxes.GetBoxIdsForUser(gameInfo.ChatId, cancellationToken);
        var filteredLocations = allLocations.Where(g => g.BoxId.HasValue && userBoxIds.Contains(g.BoxId.Value)).ToList();

        return filteredLocations.Count > 0 ? filteredLocations : allLocations;
    }

    protected static InlineKeyboardButton[][] GetSelectionButtons(IEnumerable<IBaseDto> entities, Season selectedSeason, HashSet<int>? checkedButtons = null)
    {
        var seasons = entities.Select(g => g.Season).Distinct().Order();
        var keyboard = GetEntitiesBySeasonButtons(entities, selectedSeason, checkedButtons);
        keyboard = keyboard.Append(GetSeasonButtons(seasons, selectedSeason)).ToArray();

        return keyboard;
    }

    private static InlineKeyboardButton[][] GetEntitiesBySeasonButtons(IEnumerable<IBaseDto> entities, Season selectedSeason, HashSet<int>? checkedButtons = null)
    {
        var needCheckmark = checkedButtons is not null;
        var bySeason = entities.GroupBy(g => g.Season).ToDictionary(g => g.Key, g => g.ToList());
        var chunked = bySeason[selectedSeason].Chunk(2);
        var keyboard = Array.Empty<InlineKeyboardButton[]>();
        foreach (var chunk in chunked)
        {
            keyboard = keyboard.Append(chunk
                .Select(e => new InlineKeyboardButton(needCheckmark && checkedButtons!.Contains(e.Id) ? $"✓ {e.Name}" : e.Name, e.Id.ToString()))
                .ToArray())
            .ToArray();
        }

        return keyboard;
    }

    private static InlineKeyboardButton[] GetSeasonButtons(IEnumerable<Season> seasons, Season selectedSeason)
    {
        return seasons
            .Select
            (
                season => new InlineKeyboardButton(season == selectedSeason ? $"•{season}•" : season.ToString(), season.ToString())
            )
            .ToArray();
    }

    protected string GetFullGameInfoString(GameDto? game)
    {
        if (game is null) return string.Empty;

        var dateString = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(game.DatePlayed, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")))
            .ToString("dd/MM/yyyy");
        var resultString = game.Result switch
        {
            ResultType.Win => Shared.Text.Win,
            ResultType.Lose => Shared.Text.Lose,
            _ => Shared.Text.Unknown,
        };

        return $"{game}\n      <b>{resultString} ({dateString})</b>";
    }
}
