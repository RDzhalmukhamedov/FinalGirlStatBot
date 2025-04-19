using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectGirlAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken, dynamic? payload = null)
    {
        var success = int.TryParse(data, out var girlId);
        var message = new Message();

        if (!success)
        {
            success = Enum.TryParse(data, out Season season);
            if (success)
            {
                message = await SendGirlSelector(gameInfo, cancellationToken, season);
            }
            else if (data.Equals(Shared.Text.ResetCallback))
            {

                message = await Reset(gameInfo, cancellationToken);
            }

            return message;
        }

        var girl = await _db.Girls.GetById(girlId, cancellationToken);

        if (girl is not null)
        {
            _gameManager.SetGirl(gameInfo.ChatId, girl);
            gameInfo.State = GameState.Init;

            message = await SendInitMessage(gameInfo, cancellationToken);
        }

        return message;
    }
}
