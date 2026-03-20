using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class InitStateAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        var message = await UpdateMessage(gameInfo, additionalMessage, null, deletePrev, cancellationToken: cancellationToken);
        gameInfo.MessageId = null;

        return message;
    }

    public override async Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        return userAction switch
        {
            Shared.Text.RepeatGameCallback => await FillRepeatGameInfo(gameInfo, payload, cancellationToken),
            _ => ActionResult.Error(),
        };
    }

}
