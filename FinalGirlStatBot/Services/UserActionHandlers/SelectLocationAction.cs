using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectLocationAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        return await SendLocationSelector(gameInfo, deletePrev, additionalMessage, cancellationToken: cancellationToken);
    }

    public override async Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        var locationSelected = int.TryParse(userAction, out var locationId);
        if (locationSelected)
        {
            var location = await _db.Locations.GetById(locationId, cancellationToken);
            if (location is not null)
            {
                _gameManager.SetLocation(gameInfo, location);
                return ActionResult.Ok(GameState.CreatingGame);
            }
        }

        var seasonSelected = Enum.TryParse(userAction, out Season season);

        if (seasonSelected)
        {
            await SendLocationSelector(gameInfo, selectedSeason: season, cancellationToken: cancellationToken);

            return ActionResult.Ok();
        }

        return ActionResult.Error(Shared.Text.SomethingWrongMessage);
    }

    private async Task<Message> SendLocationSelector(GameInfo gameInfo, bool deletePrev = false,
        string additionalMessage = "", Season selectedSeason = Season.S1, CancellationToken cancellationToken = default)
    {
        var allLocations = await _db.Locations.GetAll(cancellationToken);

        var keyboard = GetSelectionButtons(allLocations, selectedSeason);

        return await UpdateMessage(gameInfo, Shared.Text.SelectLocationMessage, keyboard, deletePrev, additionalMessage, cancellationToken);
    }
}
