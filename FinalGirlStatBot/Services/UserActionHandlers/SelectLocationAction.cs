using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;

namespace FinalGirlStatBot.Services.UserActionHandlers;

// TODO Возможно сделать Generic
public class SelectLocationAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var locationId = Convert.ToInt32(data);

        if (locationId == 0)
        {
            var success = Enum.TryParse(data, out Season season);
            if (success)
            {
                await SelectedLocation(gameInfo, cancellationToken, season);
            }
            else if (data.Equals("reset"))
            {

                await Reset(gameInfo, cancellationToken);
            }

            return;
        }

        var location = await _db.Locations.GetById(locationId, cancellationToken);

        if (location is not null)
        {
            gameInfo.Game.Location = location;
            gameInfo.State = GameState.Init;

            await BaseMessage(gameInfo, cancellationToken);
        }
    }
}
