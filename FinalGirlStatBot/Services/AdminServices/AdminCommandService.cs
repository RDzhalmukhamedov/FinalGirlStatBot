using FinalGirlStatBot.Abstract;
using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.AdminServices;

public class AdminCommandService(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, IAdminService adminService, GameManager gameManager) : IAdminCommandService
{
    private readonly IFGStatsUnitOfWork _db = dbConnection;
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IAdminService _adminService = adminService;
    private readonly GameManager _gameManager = gameManager;

    public async Task HandleAddGirl(Chat chatInfo, string[] args, CancellationToken cancellationToken = default)
    {
        var season = await CheckAccessAndInputCorrectness(chatInfo, args, "/addgirl", cancellationToken);
        if (!season.HasValue) return;

        var name = string.Join(" ", args.Take(args.Length - 1));
        var existingGirl = await _db.Girls.GetAll(cancellationToken);
        if (existingGirl.Any(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && g.Season == season.Value))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: $"{Shared.Text.GirlWithNameMessage} '{name}' ({Shared.Text.Season} {season}) {Shared.Text.AlreadyExsistsMessage}",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            return;
        }

        var girl = new GirlDto(0, name, season.Value, null);
        await _db.Girls.Add(girl, cancellationToken);

        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"{Shared.Text.GirlCheck} '{name}' ({Shared.Text.Season} {season}) {Shared.Text.SuccessAddMessage}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    public async Task HandleAddKiller(Chat chatInfo, string[] args, CancellationToken cancellationToken = default)
    {
        var season = await CheckAccessAndInputCorrectness(chatInfo, args, "/addkiller", cancellationToken);
        if (!season.HasValue) return;

        var name = string.Join(" ", args.Take(args.Length - 1));
        var existingKiller = await _db.Killers.GetAll(cancellationToken);
        if (existingKiller.Any(k => k.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && k.Season == season.Value))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: $"{Shared.Text.KillerWithNameMessage} '{name}' ({Shared.Text.Season} {season}) {Shared.Text.AlreadyExsistsMessage}",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            return;
        }

        var killer = new KillerDto(0, name, season.Value, null);
        await _db.Killers.Add(killer, cancellationToken);

        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"{Shared.Text.KillerCheck} '{name}' ({Shared.Text.Season} {season}) {Shared.Text.SuccessAdd2Message}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    public async Task HandleAddLocation(Chat chatInfo, string[] args, CancellationToken cancellationToken = default)
    {
        var season = await CheckAccessAndInputCorrectness(chatInfo, args, "/addlocation", cancellationToken);
        if (!season.HasValue) return;

        var name = string.Join(" ", args.Take(args.Length - 1));
        var existingLocation = await _db.Locations.GetAll(cancellationToken);
        if (existingLocation.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && l.Season == season.Value))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: $"{Shared.Text.LocationWithNameMessage} '{name}' ({Shared.Text.Season} {season}) {Shared.Text.AlreadyExsistsMessage}",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            return;
        }

        var location = new LocationDto(0, name, season.Value, null);
        await _db.Locations.Add(location, cancellationToken);

        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"{Shared.Text.LocationCheck} '{name}' ({Shared.Text.Season} {season}) {Shared.Text.SuccessAddMessage}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    public async Task HandleAddBox(Chat chatInfo, string[] args, CancellationToken cancellationToken = default)
    {
        if (!await _adminService.IsAdmin(chatInfo, cancellationToken))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: Shared.Text.AccessDeniedMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            return;
        }

        await new InitBoxCreationAction(_db, _botClient, _gameManager).Execute(chatInfo, cancellationToken);
    }

    public async Task HandleAddBoxName(Chat chatInfo, string name, CancellationToken cancellationToken = default)
    {
        if (!await _adminService.IsAdmin(chatInfo, cancellationToken))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: Shared.Text.AccessDeniedMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            return;
        }

        await new NameEnteredBoxAction(_db, _botClient, _gameManager).Execute(chatInfo, cancellationToken, name);
    }

    public async Task ProcessAddBoxCallback(Chat chatInfo, string callbackData, CancellationToken cancellationToken = default)
    {
        if (!await _adminService.IsAdmin(chatInfo, cancellationToken))
        {
            await _botClient.AnswerCallbackQuery(callbackQueryId: "unauthorized", showAlert: true, cancellationToken: cancellationToken);
            return;
        }

        var parts = callbackData.Split(Shared.Text.Splitter);
        if (parts.Length < 2) return;

        var action = parts[1];
        BaseBoxCreationAction handler = action switch
        {
            Shared.Text.SeasonCallback => new SeasonSelectedBoxAction(_db, _botClient, _gameManager),
            Shared.Text.BoxLocationCallback => new LocationSelectedBoxAction(_db, _botClient, _gameManager),
            Shared.Text.BoxKillerCallback => new KillerSelectedBoxAction(_db, _botClient, _gameManager),
            Shared.Text.BoxGirlCallback => new GirlsSelectedBoxAction(_db, _botClient, _gameManager),
            Shared.Text.CancelCallback => new GirlsSelectedBoxAction(_db, _botClient, _gameManager),
            _ => new GirlsSelectedBoxAction(_db, _botClient, _gameManager),
        };
        await handler.Execute(chatInfo, cancellationToken, parts[2]);
    }

    public bool IsAwaitingBoxName(long chatId)
    {
        return _gameManager.BoxCreationStates.TryGetValue(chatId, out var state) && string.IsNullOrEmpty(state.Name);
    }

    private async Task<Season?> CheckAccessAndInputCorrectness(Chat chatInfo, string[] args, string command, CancellationToken cancellationToken = default)
    {

        if (!await _adminService.IsAdmin(chatInfo, cancellationToken))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: Shared.Text.AccessDeniedMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            return null;
        }

        if (args.Length < 2)
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: $"{Shared.Text.WrongCommandFormatMessage} {command} {Shared.Text.NameSeasonMessage}",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            return null;
        }

        var seasonStr = args.Last();

        if (!Enum.TryParse<Season>(seasonStr, true, out var season))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: $"{Shared.Text.WrongSeasonMessage} {string.Join(", ", Enum.GetNames<Season>())}",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            return null;
        }

        return season;
    }

}
