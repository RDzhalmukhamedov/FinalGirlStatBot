using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class InitStateAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var message = data switch
        {
            Shared.Text.SelectGirlCallback => SendGirlSelector(gameInfo, cancellationToken),
            Shared.Text.SelectKillerCallback => SendKillerSelector(gameInfo, cancellationToken),
            Shared.Text.SelectLocationCallback => SendLocationSelector(gameInfo, cancellationToken),
            Shared.Text.RandomGirlCallback => SelectRandomGirl(gameInfo, cancellationToken),
            Shared.Text.RandomKillerCallback => SelectRandomKiller(gameInfo, cancellationToken),
            Shared.Text.RandomLocationCallback => SelectRandomLocation(gameInfo, cancellationToken),
            Shared.Text.StartGameCallback => StartGame(gameInfo, cancellationToken),
            Shared.Text.ResetCallback => Reset(gameInfo, cancellationToken),
            Shared.Text.InitPrivateCallback => SendInitMessage(gameInfo, cancellationToken),
        };

        await message;
    }

    private async Task<Message> SelectRandomGirl(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);
        _gameManager.SetGirl(gameInfo.ChatId, GetRandomObject(allGirls));

        return await SendInitMessage(gameInfo, cancellationToken);
    }

    private async Task<Message> SelectRandomKiller(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        var allKillers = await _db.Killers.GetAll(cancellationToken);
        _gameManager.SetKiller(gameInfo.ChatId, GetRandomObject(allKillers));

        return await SendInitMessage(gameInfo, cancellationToken);
    }

    private async Task<Message> SelectRandomLocation(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        var allLocations = await _db.Locations.GetAll(cancellationToken);
        _gameManager.SetLocation(gameInfo.ChatId, GetRandomObject(allLocations));

        return await SendInitMessage(gameInfo, cancellationToken);
    }

    private TDto GetRandomObject<TDto>(List<TDto> dtos) where TDto : class
    {
        Random rand = new();
        int toSkip = rand.Next(0, dtos.Count);
        return dtos.Skip(toSkip).Take(1).First();
    }

    private async Task<Message> StartGame(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        gameInfo.State = GameState.GameInProgress;

        if (gameInfo.MessageId is not null) await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);
        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: Shared.Text.WriteResultsMessage,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: Shared.Buttons.ResultKeyboard,
            cancellationToken: cancellationToken);

        gameInfo.MessageId = message.Id;

        return message;
    }
}
