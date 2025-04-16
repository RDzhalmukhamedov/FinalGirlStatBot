using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;

namespace FinalGirlStatBot.Services.UserActionHandlers;

// TODO Возможно сделать Generic
public class SelectKillerAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var killerId = Convert.ToInt32(data);

        if (killerId == 0)
        {
            var success = Enum.TryParse(data, out Season season);
            if (success)
            {
                await SelectedKiller(gameInfo, cancellationToken, season);
            }
            else if (data.Equals("reset"))
            {

                await Reset(gameInfo, cancellationToken);
            }

            return;
        }

        var killer = await _db.Killers.GetById(killerId, cancellationToken);

        if (killer is not null)
        {
            gameInfo.Game.Killer = killer;
            gameInfo.State = GameState.Init;

            await BaseMessage(gameInfo, cancellationToken);
        }
    }
}
