using System.Text;
using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class DeleteGameAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        StringBuilder sb = new();
        sb.AppendLine($"{Shared.Text.YouGonnaDeleteMessage}\n");
        sb.AppendLine(GetFullGameInfoString(gameInfo.Game));
        sb.AppendLine(Shared.Text.DeleteConfirmQuestion);

        return await UpdateMessage(gameInfo, sb.ToString(), Shared.Buttons.DeleteActionKeyboard, deletePrev, additionalMessage, cancellationToken);
    }

    public override async Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        return userAction switch
        {
            Shared.Text.DeleteGameCallback => await DeleteGame(gameInfo, cancellationToken),
            Shared.Text.ResetCallback => await Reset(gameInfo, cancellationToken, $"{GetFullGameInfoString(gameInfo.Game)}\n{Shared.Text.DeletionCancelledMessage}"),
            _ => ActionResult.Error(),
        };
    }

    private async Task<ActionResult> DeleteGame(GameInfo gameInfo, CancellationToken cancellationToken = default)
    {
        if (gameInfo.PendingDeleteGame && gameInfo.Game is not null)
        {
            var gameInfoString = GetFullGameInfoString(gameInfo.Game);
            await _db.Games.Delete(gameInfo.Game.Id, cancellationToken);
            _gameManager.ResetGame(gameInfo.ChatId);

            return ActionResult.Ok(GameState.Init, $"{Shared.Text.DeleteGameMessage}\n{gameInfoString}");
        }

        return ActionResult.Error(Shared.Text.SomethingWrongMessage);

    }
}
