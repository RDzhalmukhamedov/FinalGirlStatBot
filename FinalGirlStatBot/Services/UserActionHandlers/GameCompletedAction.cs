using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class GameCompletedAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        var text = $"{Shared.Text.ShootEndedMessage}{(gameInfo.Game?.Result is ResultType.Lose ? Shared.Text.KillerWinsMessage : " ")}"
                 + $"\n{gameInfo.Game}\n{(gameInfo.Game?.Result is ResultType.Lose ? Shared.Text.LoseCongratsMessage : Shared.Text.WinCongratsMessage)}";

        var repeatButton = new InlineKeyboardButton(Shared.Text.RepeatGame, $"{Shared.Text.RepeatGameCallback}{Shared.Text.Splitter}{gameInfo.Game?.Id}");
        InlineKeyboardButton[][] keyboard = [[repeatButton]];

        var message = await UpdateMessage(gameInfo, text, keyboard, deletePrev, additionalMessage, cancellationToken);
        gameInfo.MessageId = null;
        _gameManager.ResetGame(gameInfo.ChatId);

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
