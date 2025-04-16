using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectGirlAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var girlId = Convert.ToInt32(data);

        if (girlId == 0)
        {
            var success = Enum.TryParse(data, out Season season);
            if (success)
            {
                await SelectedGirl(gameInfo, cancellationToken, season);
            }
            else if (data.Equals("reset"))
            {

                await Reset(gameInfo, cancellationToken);
            }

            return;
        }

        var girl = await _db.Girls.GetById(girlId, cancellationToken);

        if (girl is not null)
        {
            gameInfo.Game.Girl = girl;
            gameInfo.State = GameState.Init;

            await BaseMessage(gameInfo, cancellationToken);
        }
    }
}
