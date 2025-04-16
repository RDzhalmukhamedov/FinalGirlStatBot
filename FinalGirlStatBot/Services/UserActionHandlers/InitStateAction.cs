using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class InitStateAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var message = data switch
        {
            "sGirl" => SelectedGirl(gameInfo, cancellationToken),
            "sKiller" => SelectedKiller(gameInfo, cancellationToken),
            "sLocation" => SelectedLocation(gameInfo, cancellationToken),
            "rGirl" => RandomGirl(gameInfo, cancellationToken),
            "rKiller" => RandomKiller(gameInfo, cancellationToken),
            "rLocation" => RandomLocation(gameInfo, cancellationToken),
            "startGame" => StartGame(gameInfo, cancellationToken),
            "reset" => Reset(gameInfo, cancellationToken),
        };
    }

    private async Task<Message> RandomGirl(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        var allGirls = await _db.Girls.GetAll(cancellationToken);
        Random rand = new();
        int toSkip = rand.Next(0, allGirls.Count);
        var girl = allGirls.Skip(toSkip).Take(1).First();

        gameInfo.Game.Girl = girl;

        return await BaseMessage(gameInfo, cancellationToken);
    }

    private async Task<Message> RandomKiller(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        var allKillers = await _db.Killers.GetAll(cancellationToken);
        Random rand = new();
        int toSkip = rand.Next(0, allKillers.Count);
        var killer = allKillers.Skip(toSkip).Take(1).First();

        gameInfo.Game.Killer = killer;

        return await BaseMessage(gameInfo, cancellationToken);
    }

    private async Task<Message> RandomLocation(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        var allLocations = await _db.Locations.GetAll(cancellationToken);
        Random rand = new();
        int toSkip = rand.Next(0, allLocations.Count);
        var location = allLocations.Skip(toSkip).Take(1).First();

        gameInfo.Game.Location = location;

        return await BaseMessage(gameInfo, cancellationToken);
    }

    private async Task<Message> StartGame(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        gameInfo.State = GameState.GameInProgress;

        var keyboard = new InlineKeyboardButton[][]
            {
                [("❌ Поражение", "lose"), ("🗘 Сброс", "reset"), ("✔️ Победа", "win")]
            };
        var message = await _botClient.EditMessageText(gameInfo.Game.User.ChatId, (int)gameInfo.MessageId, $"Отметьте результат партии:");
        message = await _botClient.EditMessageReplyMarkup(gameInfo.Game.User.ChatId, message.Id, keyboard);
        gameInfo.MessageId = message.Id;

        return message;
    }
}
