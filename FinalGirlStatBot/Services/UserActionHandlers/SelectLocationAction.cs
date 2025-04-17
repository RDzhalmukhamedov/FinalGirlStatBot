using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectLocationAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var success = int.TryParse(data, out var locationId);

        if (!success)
        {
            success = Enum.TryParse(data, out Season season);
            if (success)
            {
                await SendLocationSelector(gameInfo, cancellationToken, season);
            }
            else if (data.Equals(Shared.Text.ResetCallback))
            {

                await Reset(gameInfo, cancellationToken);
            }

            return;
        }

        var location = await _db.Locations.GetById(locationId, cancellationToken);

        if (location is not null)
        {
            _gameManager.SetLocation(gameInfo.ChatId, location);
            gameInfo.State = GameState.Init;

            await SendInitMessage(gameInfo, cancellationToken);
        }
    }
}
