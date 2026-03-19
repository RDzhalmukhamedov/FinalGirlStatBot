using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;

public class GirlsSelectedBoxAction : BaseBoxCreationAction
{
    public GirlsSelectedBoxAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
        : base(dbConnection, botClient, gameManager)
    {
    }

    public override async Task Execute(Chat chatInfo, CancellationToken cancellationToken = default, string data = "")
    {
        if (int.TryParse(data, out var girlId))
        {
            if (_gameManager.BoxCreationStates.TryGetValue(chatInfo.Id, out var state))
            {
                if (state.GirlIds.Contains(girlId))
                    state.GirlIds.Remove(girlId);
                else
                    state.GirlIds.Add(girlId);

                _gameManager.BoxCreationStates[chatInfo.Id] = state;

                await UpdateGirlsSelection(chatInfo, state, cancellationToken);
            }
            return;
        }

        if (data == Shared.Text.DoneGirlCallback)
        {
            if (_gameManager.BoxCreationStates.TryGetValue(chatInfo.Id, out var state))
            {
                await SaveBox(chatInfo, state, cancellationToken);
            }
            return;
        }

        if (data == Shared.Text.SkipGirlCallback)
        {
            if (_gameManager.BoxCreationStates.TryGetValue(chatInfo.Id, out var state))
            {
                await SaveBox(chatInfo, state, cancellationToken);
            }
            return;
        }
    }

    private async Task UpdateGirlsSelection(Chat chatInfo, BoxCreationState state, CancellationToken cancellationToken)
    {
        if (state.MessageId is null) return;

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

        await _botClient.EditMessageText(
            chatId: chatInfo.Id,
            messageId: state.MessageId.Value,
            text: $"{Shared.Text.AddBox} '{state.Name}' ({state.Season})\n{locationText}\n{killerText}\n{girlsCountText}\n\n{Shared.Text.SelectBoxGirlsMessage}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: keyboard.ToArray(),
            cancellationToken: cancellationToken);
    }

    private async Task SaveBox(Chat chatInfo, BoxCreationState state, CancellationToken cancellationToken)
    {
        LocationDto? location = null;
        KillerDto? killer = null;
        List<GirlDto> girls = new();

        if (state.LocationId.HasValue)
            location = await _db.Locations.GetById(state.LocationId.Value, cancellationToken);

        if (state.KillerId.HasValue)
            killer = await _db.Killers.GetById(state.KillerId.Value, cancellationToken);

        foreach (var girlId in state.GirlIds)
        {
            var girl = await _db.Girls.GetById(girlId, cancellationToken);
            if (girl is not null)
                girls.Add(girl);
        }

        var boxDto = new BoxDto(0, state.Name, state.Season, location, killer, girls);
        await _db.Boxes.Add(boxDto, cancellationToken);

        var locationText = location is not null ? $"\n{Shared.Text.Location}: {location.Name}" : "";
        var killerText = killer is not null ? $"\n{Shared.Text.Killer}: {killer.Name}" : "";
        var girlsText = girls.Any() ? $"\n{Shared.Text.Girls}: {string.Join(", ", girls.Select(g => g.Name))}" : "";

        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: $"{Shared.Text.AddBox} '{state.Name}' ({Shared.Text.Season} {state.Season}){locationText}{killerText}{girlsText}\n\n{Shared.Text.BoxCreatedMessage}",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: cancellationToken);

        _gameManager.BoxCreationStates.TryRemove(chatInfo.Id, out var _);
    }
}
