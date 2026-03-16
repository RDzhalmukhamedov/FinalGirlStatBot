using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class CreatingGameAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
            : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        var keyboard = gameInfo.ReadyToStart ? Shared.Buttons.InitKeyboardReadyToStart : Shared.Buttons.InitKeyboard;

        return await UpdateMessage(gameInfo, $"{Shared.Text.SelectionQuestionMessage}\n{gameInfo.Game}", keyboard, deletePrev, additionalMessage, cancellationToken);
    }

    public override async Task<ActionResult> ProcessCallback
    (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        var message = userAction switch
        {
            Shared.Text.SelectGirlCallback => ActionResult.Ok(GameState.SelectGirl),
            Shared.Text.SelectKillerCallback => ActionResult.Ok(GameState.SelectKiller),
            Shared.Text.SelectLocationCallback => ActionResult.Ok(GameState.SelectLocation),

            Shared.Text.RandomGirlCallback => await SelectRandomGirl(gameInfo, cancellationToken),
            Shared.Text.RandomKillerCallback => await SelectRandomKiller(gameInfo, cancellationToken),
            Shared.Text.RandomLocationCallback => await SelectRandomLocation(gameInfo, cancellationToken),
            Shared.Text.RandomUnplayedCallback => await SelectRandomUnplayedCombination(gameInfo, cancellationToken),

            Shared.Text.StartGameCallback => ActionResult.Ok(GameState.GameInProgress),
            Shared.Text.ResetCallback => await Reset(gameInfo, cancellationToken),
            Shared.Text.CreatingGameCallback => ActionResult.Error(Shared.Text.UHaveUnfinishedGameMessage),
            _ => ActionResult.Error(),
        };

        return message;
    }

    private async Task<ActionResult> SelectRandomGirl(GameInfo gameInfo, CancellationToken cancellationToken = default)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);
        _gameManager.SetGirl(gameInfo, GetRandomObject(allGirls));
        await SendActionMessage(gameInfo, cancellationToken: cancellationToken);

        return ActionResult.Ok();
    }

    private async Task<ActionResult> SelectRandomKiller(GameInfo gameInfo, CancellationToken cancellationToken = default)
    {
        var allKillers = await _db.Killers.GetAll(cancellationToken);
        _gameManager.SetKiller(gameInfo, GetRandomObject(allKillers));
        await SendActionMessage(gameInfo, cancellationToken: cancellationToken);

        return ActionResult.Ok();
    }

    private async Task<ActionResult> SelectRandomLocation(GameInfo gameInfo, CancellationToken cancellationToken = default)
    {
        var allLocations = await _db.Locations.GetAll(cancellationToken);
        _gameManager.SetLocation(gameInfo, GetRandomObject(allLocations));
        await SendActionMessage(gameInfo, cancellationToken: cancellationToken);

        return ActionResult.Ok();
    }

    private TDto GetRandomObject<TDto>(IEnumerable<TDto> dtos) where TDto : class
    {
        var dtoList = dtos.ToList();
        Random rand = new();
        int toSkip = rand.Next(0, dtoList.Count);
        return dtoList.Skip(toSkip).Take(1).First();
    }

    private async Task<ActionResult> SelectRandomUnplayedCombination(GameInfo gameInfo, CancellationToken cancellationToken = default)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);
        var allKillers = await _db.Killers.GetAll(cancellationToken);
        var allLocations = await _db.Locations.GetAll(cancellationToken);

        var userGames = await _db.Games.GetByUser(gameInfo.ChatId, cancellationToken);

        var playedCombinations = userGames
            .Where(g => g.Girl != null && g.Killer != null && g.Location != null)
            .Select(g => new { GirlId = g.Girl!.Id, KillerId = g.Killer!.Id, LocationId = g.Location!.Id })
            .ToHashSet();

        var unplayedCombinations = allGirls
            .SelectMany(girl => allKillers, (girl, killer) => new { Girl = girl, Killer = killer })
            .SelectMany(gk => allLocations, (gk, location) => new { gk.Girl, gk.Killer, Location = location })
            .Where(comb => !playedCombinations.Contains(new { GirlId = comb.Girl.Id, KillerId = comb.Killer.Id, LocationId = comb.Location.Id }))
            .ToList();

        if (unplayedCombinations.Count == 0)
        {
            // Если все комбинации уже сыграны, выбираем случайную комбинацию
            _gameManager.SetGirl(gameInfo, GetRandomObject(allGirls));
            _gameManager.SetKiller(gameInfo, GetRandomObject(allKillers));
            _gameManager.SetLocation(gameInfo, GetRandomObject(allLocations));
        }
        else
        {
            var randomCombination = GetRandomObject(unplayedCombinations);
            _gameManager.SetGirl(gameInfo, randomCombination.Girl);
            _gameManager.SetKiller(gameInfo, randomCombination.Killer);
            _gameManager.SetLocation(gameInfo, randomCombination.Location);
        }

        await SendActionMessage(gameInfo, cancellationToken: cancellationToken);

        return ActionResult.Ok();
    }
}
