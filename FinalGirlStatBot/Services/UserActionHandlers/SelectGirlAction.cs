using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class SelectGirlAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        return await SendGirlSelector(gameInfo, deletePrev, additionalMessage, cancellationToken: cancellationToken);
    }

    public override async Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        var girlSelected = int.TryParse(userAction, out var girlId);
        if (girlSelected)
        {
            var girl = await _db.Girls.GetById(girlId, cancellationToken);
            if (girl is not null)
            {
                _gameManager.SetGirl(gameInfo, girl);
                return ActionResult.Ok(GameState.CreatingGame);
            }
        }

        var seasonSelected = Enum.TryParse(userAction, out Season season);

        if (seasonSelected)
        {
            await SendGirlSelector(gameInfo, selectedSeason: season, cancellationToken: cancellationToken);

            return ActionResult.Ok();
        }

        return ActionResult.Error(Shared.Text.SomethingWrongMessage);
    }

    private async Task<Message> SendGirlSelector(GameInfo gameInfo, bool deletePrev = false,
        string additionalMessage = "", Season selectedSeason = Season.S1, CancellationToken cancellationToken = default)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);

        var keyboard = GetSelectionButtons(allGirls, selectedSeason);

        return await UpdateMessage(gameInfo, Shared.Text.SelectGirlMessage, keyboard, deletePrev, additionalMessage, cancellationToken);
    }
}
