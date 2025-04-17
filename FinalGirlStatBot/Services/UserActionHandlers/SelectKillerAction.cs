using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectKillerAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var success = int.TryParse(data, out var killerId);

        if (!success)
        {
            success = Enum.TryParse(data, out Season season);
            if (success)
            {
                await SendKillerSelector(gameInfo, cancellationToken, season);
            }
            else if (data.Equals(Shared.Text.ResetCallback))
            {

                await Reset(gameInfo, cancellationToken);
            }

            return;
        }

        var killer = await _db.Killers.GetById(killerId, cancellationToken);

        if (killer is not null)
        {
            _gameManager.SetKiller(gameInfo.ChatId, killer);
            gameInfo.State = GameState.Init;

            await SendInitMessage(gameInfo, cancellationToken);
        }
    }
}
