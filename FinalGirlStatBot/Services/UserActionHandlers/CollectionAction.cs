using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.Models;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class CollectionAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        var boxIds = new HashSet<int>(await _db.UserBoxes.GetBoxIdsForUser(gameInfo.ChatId, cancellationToken));

        var state = _gameManager.GetCollectionState(gameInfo.ChatId);
        state.BoxIds = boxIds;

        return await SendBoxSelector(gameInfo, boxIds, deletePrev, additionalMessage, Season.S1, cancellationToken);
    }

    public override async Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        var state = _gameManager.GetCollectionState(gameInfo.ChatId);

        var boxSelected = int.TryParse(userAction, out var boxId);
        if (boxSelected)
        {
            if (state.BoxIds.Contains(boxId)) state.BoxIds.Remove(boxId);
            else state.BoxIds.Add(boxId);

            return ActionResult.Ok();
        }

        var seasonSelected = Enum.TryParse(userAction, out Season season);
        if (seasonSelected)
        {
            state.SelectedSeason = season;
            var boxIds = _gameManager.UserCollections[gameInfo.ChatId].BoxIds;
            await SendBoxSelector(gameInfo, boxIds, selectedSeason: season, cancellationToken: cancellationToken);

            return ActionResult.Ok();
        }

        if (userAction == Shared.Text.CollectionDoneCallback)
        {
            await _db.UserBoxes.SetBoxesForUser(gameInfo.ChatId, state.BoxIds, cancellationToken);
            _gameManager.UserCollections.TryRemove(gameInfo.ChatId, out var _);

            return ActionResult.Ok(GameState.Init);
        }

        if (userAction == Shared.Text.CancelCallback)
        {
            _gameManager.UserCollections.TryRemove(gameInfo.ChatId, out var _);

            return ActionResult.Ok(GameState.Init);
        }

        return ActionResult.Error(Shared.Text.SomethingWrongMessage);
    }

    private async Task<Message> SendBoxSelector(GameInfo gameInfo, HashSet<int> boxIds, bool deletePrev = false,
        string additionalMessage = "", Season selectedSeason = Season.S1, CancellationToken cancellationToken = default)
    {
        var allBoxes = await _db.Boxes.GetAll(cancellationToken);

        var keyboard = GetSelectionButtons(allBoxes, selectedSeason, boxIds);

        keyboard = keyboard.Append([
            new InlineKeyboardButton(Shared.Text.DoneSelection, $"{Shared.Text.CollectionDoneCallback}"),
            new InlineKeyboardButton(Shared.Text.Reset, $"{Shared.Text.CancelCallback}")
        ]).ToArray();

        return await UpdateMessage(gameInfo, Shared.Text.CollectionMessage, keyboard, deletePrev, additionalMessage, cancellationToken);
    }
}
