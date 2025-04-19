using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectLocationAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken, dynamic? payload = null)
    {
        var success = int.TryParse(data, out var locationId);
        var message = new Message();

        if (!success)
        {
            success = Enum.TryParse(data, out Season season);
            if (success)
            {
                message = await SendLocationSelector(gameInfo, cancellationToken, season);
            }
            else if (data.Equals(Shared.Text.ResetCallback))
            {

                message = await Reset(gameInfo, cancellationToken);
            }

            return message;
        }

        var location = await _db.Locations.GetById(locationId, cancellationToken);

        if (location is not null)
        {
            _gameManager.SetLocation(gameInfo.ChatId, location);
            gameInfo.State = GameState.Init;

            message = await SendInitMessage(gameInfo, cancellationToken);
        }

        return message;
    }
}
