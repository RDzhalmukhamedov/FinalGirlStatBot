using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class GameInProgressAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        return await UpdateMessage(gameInfo, $"{Shared.Text.ShootInProgressMessage}\n{gameInfo.Game}\n{Shared.Text.WriteResultsMessage}",
                                   Shared.Buttons.ResultKeyboard, deletePrev, additionalMessage, cancellationToken);
    }

    public override async Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        var message = userAction switch
        {
            Shared.Text.WinCallback => await SetWin(gameInfo, cancellationToken),
            Shared.Text.LoseCallback => await SetLose(gameInfo, cancellationToken),
            Shared.Text.ResetCallback => await Reset(gameInfo, cancellationToken),
            _ => ActionResult.Error(),
        };

        return message;
    }

    private async Task<ActionResult> SetWin(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        return await SetGameResult(gameInfo, ResultType.Win, cancellationToken);
    }

    private async Task<ActionResult> SetLose(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        return await SetGameResult(gameInfo, ResultType.Lose, cancellationToken);
    }

    private async Task<ActionResult> SetGameResult(GameInfo gameInfo, ResultType result, CancellationToken cancellationToken)
    {
        var (success, gameDto) = _gameManager.SetGameResult(gameInfo.ChatId, result);

        if (success && gameDto is not null)
        {
            var id = await _db.Games.Add(gameDto, cancellationToken);
            _gameManager.UpdateGameId(gameInfo, id);
            return ActionResult.Ok(GameState.GameCompleted);
        }
        else
        {
            return ActionResult.Error(Shared.Text.SomethingWrongMessage);
        }
    }
}
