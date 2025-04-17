using System.Text;
using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class StatisticsAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var message = data switch
        {
            Shared.Text.KillerStatsCallback => StatsByKiller(gameInfo, cancellationToken),
            Shared.Text.LocationStatsCallback => StatsByLocation(gameInfo, cancellationToken),
            Shared.Text.ResetCallback => Reset(gameInfo, cancellationToken),
        };

        await message;
        gameInfo.MessageId = message.Id;
    }

    private async Task<Message> StatsByKiller(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        StringBuilder sb = new();
        var gamesByKiller = (await _db.Games.GetByUser(gameInfo.ChatId, cancellationToken))
            .Where(g => g.Killer is not null)
            .GroupBy(g => g.Killer.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var games in gamesByKiller)
        {
            if (games.Value.Count > 0)
            {
                var wins = games.Value.Where(g => g.Result == DB.Domain.ResultType.Win).Count();
                var loses = games.Value.Where(g => g.Result == DB.Domain.ResultType.Lose).Count();
                sb.AppendLine($"<b>{games.Value.First().Killer.Name}</b>: {games.Value.Count} игр ({Math.Round((double)wins * 100 / games.Value.Count)}%, {wins}/{loses})\n");
            }
        }

        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: sb.ToString(),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: Shared.Buttons.StatsKeyboardKiller,
            cancellationToken: cancellationToken);

        return message;
    }

    private async Task<Message> StatsByLocation(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        StringBuilder sb = new();
        var gamesByLocation = (await _db.Games.GetByUser(chatId: gameInfo.ChatId, cancellationToken))
            .Where(g => g.Location is not null)
            .GroupBy(g => g.Location.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var games in gamesByLocation)
        {
            if (games.Value.Count > 0)
            {
                var wins = games.Value.Where(g => g.Result == DB.Domain.ResultType.Win).Count();
                var loses = games.Value.Where(g => g.Result == DB.Domain.ResultType.Lose).Count();
                sb.AppendLine($"<b>{games.Value.First().Location.Name}</b>: {games.Value.Count} игр ({Math.Round((double)wins * 100 / games.Value.Count)}%, {wins}/{loses})\n");
            }
        }

        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: sb.ToString(),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: Shared.Buttons.StatsKeyboardLocation,
            cancellationToken: cancellationToken);

        return message;
    }
}
