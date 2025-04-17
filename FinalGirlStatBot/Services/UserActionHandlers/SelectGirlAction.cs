using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectGirlAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var success = int.TryParse(data, out var girlId);

        if (!success)
        {
            success = Enum.TryParse(data, out Season season);
            if (success)
            {
                await SendGirlSelector(gameInfo, cancellationToken, season);
            }
            else if (data.Equals(Shared.Text.ResetCallback))
            {

                await Reset(gameInfo, cancellationToken);
            }

            return;
        }

        var girl = await _db.Girls.GetById(girlId, cancellationToken);

        if (girl is not null)
        {
            _gameManager.SetGirl(gameInfo.ChatId, girl);
            gameInfo.State = GameState.Init;

            await SendInitMessage(gameInfo, cancellationToken);
        }
    }
}
