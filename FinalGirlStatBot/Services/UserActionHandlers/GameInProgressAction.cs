using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class GameInProgressAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var message = data switch
        {
            Shared.Text.WinCallback => SetWin(gameInfo, cancellationToken),
            Shared.Text.LoseCallback => SetLose(gameInfo, cancellationToken),
            Shared.Text.ResetCallback => Reset(gameInfo, cancellationToken),
        };

        await message;
    }

    private async Task<Message> SetWin(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        return await SetGameResult(gameInfo, ResultType.Win,
            $"{Shared.Text.ShootEndedMessage}\n{gameInfo.Game}\n{Shared.Text.WinCongratsMessage}", cancellationToken);
    }

    private async Task<Message> SetLose(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        return await SetGameResult(gameInfo, ResultType.Lose,
            $"{Shared.Text.ShootEndedMessage} {Shared.Text.KillerWinsMessage}\n{gameInfo.Game}\n{Shared.Text.LoseCongratsMessage}", cancellationToken);
    }

    private async Task<Message> SetGameResult(GameInfo gameInfo, ResultType result, string messageText, CancellationToken cancellationToken)
    {
        var (success, game) = _gameManager.FinishGame(gameInfo.ChatId, result);
        var text = messageText;

        if (success)
        {
            await _db.Games.Add(game, cancellationToken);
            await _db.Commit(cancellationToken);
        }
        else
        {
            text = Shared.Text.SomethingWrongMessage;
        }

        await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);

        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: text,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        return message;
    }
}
