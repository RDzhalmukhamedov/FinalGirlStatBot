using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;

public class KillerSelectedBoxAction : BaseBoxCreationAction
{
    public KillerSelectedBoxAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
        : base(dbConnection, botClient, gameManager)
    {
    }

    public override async Task Execute(Chat chatInfo, CancellationToken cancellationToken = default, string data = "")
    {
        if (int.TryParse(data, out var killerId))
        {
            if (_gameManager.BoxCreationStates.TryGetValue(chatInfo.Id, out var state))
            {
                state.KillerId = killerId;
                _gameManager.BoxCreationStates[chatInfo.Id] = state;

                await ShowGirlsSelection(chatInfo, state, cancellationToken);
            }

            return;
        }

        if (data == Shared.Text.SkipKillerCallback)
        {
            if (_gameManager.BoxCreationStates.TryGetValue(chatInfo.Id, out var state))
            {
                await ShowGirlsSelection(chatInfo, state, cancellationToken);
            }
        }
    }

    private async Task ShowGirlsSelection(Chat chatInfo, BoxCreationState state, CancellationToken cancellationToken)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);
        var girls = allGirls.Where(g => g.Season == state.Season).ToList();

        var keyboard = new List<InlineKeyboardButton[]>();
        var callbackPart = $"{Shared.Text.AddBoxCallback}{Shared.Text.Splitter}{Shared.Text.BoxGirlCallback}{Shared.Text.Splitter}";

        foreach (var chunk in girls.Chunk(2))
        {
            keyboard.Add(chunk.Select(g =>
            {
                var isSelected = state.GirlIds.Contains(g.Id);
                var buttonText = isSelected ? $"✓ {g.Name}" : g.Name;
                return new InlineKeyboardButton(buttonText, $"{callbackPart}{g.Id}");
            }).ToArray());
        }

        keyboard.Add(
        [
            new InlineKeyboardButton(Shared.Text.DoneSelection, $"{callbackPart}{Shared.Text.DoneGirlCallback}"),
            new InlineKeyboardButton(Shared.Text.SkipSelection, $"{callbackPart}{Shared.Text.SkipGirlCallback}")
        ]);
        keyboard.Add([new InlineKeyboardButton(Shared.Text.Reset, $"{Shared.Text.AddBoxCallback}{Shared.Text.Splitter}{Shared.Text.CancelCallback}")]);

        var locationText = state.LocationId.HasValue
            ? $"{Shared.Text.Location}: {(await _db.Locations.GetById(state.LocationId.Value, cancellationToken))?.Name}"
            : $"{Shared.Text.LocationCheck}: —";
        var killerText = state.KillerId.HasValue
            ? $"{Shared.Text.Killer}: {(await _db.Killers.GetById(state.KillerId.Value, cancellationToken))?.Name}"
            : $"{Shared.Text.KillerCheck}: —";

        var girlsCountText = string.Format(Shared.Text.GirlsSelectedMessage, state.GirlIds.Count);

        var message = await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"{Shared.Text.AddBox} '{state.Name}' ({state.Season})\n{locationText}\n{killerText}\n{girlsCountText}\n\n{Shared.Text.SelectBoxGirlsMessage}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: keyboard.ToArray(),
            cancellationToken: cancellationToken);

        _gameManager.BoxCreationStates[chatInfo.Id].MessageId = message.MessageId;
    }
}
