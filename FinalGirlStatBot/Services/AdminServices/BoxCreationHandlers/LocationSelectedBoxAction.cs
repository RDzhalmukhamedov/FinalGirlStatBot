using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;

public class LocationSelectedBoxAction : BaseBoxCreationAction
{
    public LocationSelectedBoxAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
        : base(dbConnection, botClient, gameManager)
    {
    }

    public override async Task Execute(Chat chatInfo, CancellationToken cancellationToken = default, string data = "")
    {
        if (int.TryParse(data, out var locationId))
        {
            if (_gameManager.BoxCreationStates.TryGetValue(chatInfo.Id, out var state))
            {
                state.LocationId = locationId;
                _gameManager.BoxCreationStates[chatInfo.Id] = state;

                await ShowKillerSelection(chatInfo, state, cancellationToken);
            }

            return;
        }

        if (data == Shared.Text.SkipLocationCallback)
        {
            if (_gameManager.BoxCreationStates.TryGetValue(chatInfo.Id, out var state))
            {
                await ShowKillerSelection(chatInfo, state, cancellationToken);
            }
        }
    }

    private async Task ShowKillerSelection(Chat chatInfo, BoxCreationState state, CancellationToken cancellationToken)
    {
        var allKillers = await _db.Killers.GetAll(cancellationToken);
        var killers = allKillers.Where(k => k.Season == state.Season).ToList();

        var keyboard = new List<InlineKeyboardButton[]>();
        var callbackPart = $"{Shared.Text.AddBoxCallback}{Shared.Text.Splitter}{Shared.Text.BoxKillerCallback}{Shared.Text.Splitter}";

        foreach (var chunk in killers.Chunk(2))
        {
            keyboard.Add(chunk.Select(k => new InlineKeyboardButton(k.BoxId.HasValue ? $"{k.Name} 📼" : k.Name, $"{callbackPart}{k.Id}")).ToArray());
        }

        keyboard.Add(
        [
            new InlineKeyboardButton(Shared.Text.SkipSelection, $"{callbackPart}{Shared.Text.SkipKillerCallback}"),
            new InlineKeyboardButton(Shared.Text.Reset, $"{Shared.Text.AddBoxCallback}{Shared.Text.Splitter}{Shared.Text.CancelCallback}")
        ]);

        var locationText = state.LocationId.HasValue
            ? $"{Shared.Text.Location}: {(await _db.Locations.GetById(state.LocationId.Value, cancellationToken))?.Name}"
            : $"{Shared.Text.LocationCheck}: —";

        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"{Shared.Text.AddBox} '{state.Name}' ({state.Season})\n{locationText}\n\n{Shared.Text.SelectBoxKillerMessage}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: keyboard.ToArray(),
            cancellationToken: cancellationToken);
    }

}
