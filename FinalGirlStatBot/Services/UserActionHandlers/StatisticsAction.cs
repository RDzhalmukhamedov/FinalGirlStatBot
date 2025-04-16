using System.Text;
using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class StatisticsAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken)
    {
        var message = data switch
        {
            "statKiller" => StatsByKiller(gameInfo, cancellationToken),
            "statLocation" => StatsByLocation(gameInfo, cancellationToken),
            "reset" => Reset(gameInfo, cancellationToken),
        };
    }

    private async Task<Message> StatsByKiller(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        StringBuilder sb = new();
        var gamesByKiller = (await _db.Games.GetByUser(chatId: gameInfo.ChatId, cancellationToken)).GroupBy(g => g.Killer.Id).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var games in gamesByKiller)
        {
            if (games.Value.Count > 0)
            {
                var wins = games.Value.Where(g => g.Result == DB.Domain.ResultType.Win).Count();
                var loses = games.Value.Where(g => g.Result == DB.Domain.ResultType.Lose).Count();
                sb.AppendLine($"*{games.Value.First().Killer.Name}*: {games.Value.Count} игр ({Math.Round((double)wins * 100 / games.Value.Count)}%, {wins}/{loses})\n");
            }
        }

        var keyboard = new InlineKeyboardButton[][]
            {
                [("*·Статистика по 🔪·*", "statKiller"), ("Статистика по 🏫", "statLocation")]
            };

        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: sb.ToString(),
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        return message;
    }

    private async Task<Message> StatsByLocation(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        StringBuilder sb = new();
        var gamesByLocation = (await _db.Games.GetByUser(chatId: gameInfo.ChatId, cancellationToken)).GroupBy(g => g.Location.Id).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var games in gamesByLocation)
        {
            if (games.Value.Count > 0)
            {
                var wins = games.Value.Where(g => g.Result == DB.Domain.ResultType.Win).Count();
                var loses = games.Value.Where(g => g.Result == DB.Domain.ResultType.Lose).Count();
                sb.AppendLine($"*{games.Value.First().Location.Name}*: {games.Value.Count} игр ({Math.Round((double)wins * 100 / games.Value.Count)}%, {wins}/{loses})\n");
            }
        }

        var keyboard = new InlineKeyboardButton[][]
            {
                [("Статистика по 🔪", "statKiller"), ("*·Статистика по 🏫·*", "statLocation")]
            };

        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: sb.ToString(),
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        return message;
    }
}
