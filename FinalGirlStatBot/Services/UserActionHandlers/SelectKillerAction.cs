using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectKillerAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        return await SendKillerSelector(gameInfo, deletePrev, additionalMessage, cancellationToken: cancellationToken);
    }

    public override async Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        var killerSelected = int.TryParse(userAction, out var killerId);
        if (killerSelected)
        {
            var killer = await _db.Killers.GetById(killerId, cancellationToken);
            if (killer is not null)
            {
                _gameManager.SetKiller(gameInfo, killer);
                return ActionResult.Ok(GameState.CreatingGame);
            }
        }

        var seasonSelected = Enum.TryParse(userAction, out Season season);

        if (seasonSelected)
        {
            await SendKillerSelector(gameInfo, selectedSeason: season, cancellationToken: cancellationToken);

            return ActionResult.Ok();
        }

        return ActionResult.Error(Shared.Text.SomethingWrongMessage);
    }

    private async Task<Message> SendKillerSelector(GameInfo gameInfo, bool deletePrev = false,
        string additionalMessage = "", Season selectedSeason = Season.S1, CancellationToken cancellationToken = default)
    {
        var killers = await GetKillersForUser(gameInfo, cancellationToken);

        var keyboard = GetSelectionButtons(killers, selectedSeason);

        return await UpdateMessage(gameInfo, Shared.Text.SelectKillerMessage, keyboard, deletePrev, additionalMessage, cancellationToken);
    }
}
