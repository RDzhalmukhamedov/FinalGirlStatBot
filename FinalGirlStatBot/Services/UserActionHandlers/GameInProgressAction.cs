using FinalGirlStatBot.DB.Abstract;
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
            "win" => SetWin(gameInfo, cancellationToken),
            "lose" => SetLose(gameInfo, cancellationToken),
            "reset" => Reset(gameInfo, cancellationToken),
        };
    }

    private async Task<Message> SetWin(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        gameInfo.Game.Result = DB.Domain.ResultType.Win;

        await _db.Games.Add(gameInfo.Game, cancellationToken);
        await _db.Commit(cancellationToken);

        _gameManager.DeleteGame(gameInfo.Game.User.ChatId);

        await _botClient.DeleteMessage(gameInfo.Game.User.ChatId, (int)gameInfo.MessageId, cancellationToken);

        var message = await _botClient.SendMessage(
            chatId: gameInfo.Game.User.ChatId,
            text: $"Снято!\n{gameInfo.Game}\nПоздравляем с победой!",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        return message;
    }

    private async Task<Message> SetLose(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        gameInfo.Game.Result = DB.Domain.ResultType.Lose;

        await _db.Games.Add(gameInfo.Game, cancellationToken);
        await _db.Commit(cancellationToken);

        _gameManager.DeleteGame(gameInfo.Game.User.ChatId);

        await _botClient.DeleteMessage(gameInfo.Game.User.ChatId, (int)gameInfo.MessageId, cancellationToken);

        var message = await _botClient.SendMessage(
            chatId: gameInfo.Game.User.ChatId,
            text: $"Снято!\n{gameInfo.Game}\nПовезёт в следующий раз!",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        return message;
    }
}
