using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;

public class NameEnteredBoxAction : BaseBoxCreationAction
{
    public NameEnteredBoxAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
        : base(dbConnection, botClient, gameManager)
    {
    }

    public override async Task Execute(Chat chatInfo, CancellationToken cancellationToken = default, string data = "")
    {
        if (!_gameManager.BoxCreationStates.TryGetValue(chatInfo.Id, out var state))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: Shared.Text.SomethingWrongMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);

            return;
        }

        if (string.IsNullOrWhiteSpace(data))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: Shared.Text.EnterBoxNameMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);

            return;
        }

        var existingBoxes = await _db.Boxes.GetBySeason(state.Season, cancellationToken);
        if (existingBoxes.Any(b => b.Name.Equals(data, StringComparison.OrdinalIgnoreCase)))
        {
            await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: $"{Shared.Text.BoxWithNameMessage} '{data}' ({Shared.Text.Season} {state.Season}) {Shared.Text.AlreadyExsistsMessage}",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            _gameManager.BoxCreationStates.TryRemove(chatInfo.Id, out var _);

            return;
        }

        state.Name = data;
        _gameManager.BoxCreationStates[chatInfo.Id] = state;

        await ShowLocationSelection(chatInfo, state, cancellationToken);
    }

    private async Task ShowLocationSelection(Chat chatInfo, BoxCreationState state, CancellationToken cancellationToken)
    {
        var allLocations = await _db.Locations.GetAll(cancellationToken);
        var locations = allLocations.Where(l => l.Season == state.Season).ToList();

        var keyboard = new List<InlineKeyboardButton[]>();
        var callbackPart = $"{Shared.Text.AddBoxCallback}{Shared.Text.Splitter}{Shared.Text.BoxLocationCallback}{Shared.Text.Splitter}";

        foreach (var chunk in locations.Chunk(2))
        {
            keyboard.Add(chunk.Select(l => new InlineKeyboardButton(l.Name, $"{callbackPart}{l.Id}")).ToArray());
        }

        keyboard.Add(
        [
            new InlineKeyboardButton(Shared.Text.SkipSelection, $"{callbackPart}{Shared.Text.SkipLocationCallback}"),
            new InlineKeyboardButton(Shared.Text.Reset, $"{Shared.Text.AddBoxCallback}{Shared.Text.Splitter}{Shared.Text.CancelCallback}")
        ]);

        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"{Shared.Text.AddBox} '{state.Name}' ({state.Season})\n\n{Shared.Text.SelectBoxLocationMessage}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: keyboard.ToArray(),
            cancellationToken: cancellationToken);
    }
}
