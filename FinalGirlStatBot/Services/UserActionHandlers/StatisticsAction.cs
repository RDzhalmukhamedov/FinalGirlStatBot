using FinalGirlStatBot.DB.Abstract;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class StatisticsAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager) : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> DoAction(GameInfo gameInfo, string data, CancellationToken cancellationToken, dynamic? payload = null)
    {
        var message = await (data switch
        {
            Shared.Text.GirlStatsCallback => StatsByGirl(gameInfo, cancellationToken),
            Shared.Text.KillerStatsCallback => StatsByKiller(gameInfo, cancellationToken),
            Shared.Text.LocationStatsCallback => StatsByLocation(gameInfo, cancellationToken),
            Shared.Text.HistoryStatsCallback => GamesHistory(gameInfo, cancellationToken, payload),
            Shared.Text.ResetCallback => Reset(gameInfo, cancellationToken),
        });

        gameInfo.MessageId = message.Id;

        return message;
    }

    private async Task<Message> StatsByGirl(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        StringBuilder sb = new();
        var gamesByGirl = (await _db.Games.GetByUser(gameInfo.ChatId, cancellationToken))
            .Where(g => g.Girl is not null)
            .GroupBy(g => g.Girl!.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var games in gamesByGirl.OrderByDescending(g => g.Value.Count))
        {
            if (games.Value.Count > 0)
            {
                var wins = games.Value.Where(g => g.Result == DB.Domain.ResultType.Win).Count();
                var loses = games.Value.Where(g => g.Result == DB.Domain.ResultType.Lose).Count();
                sb.AppendLine($"<b>{games.Value.First().Girl?.Name}</b>: {games.Value.Count} игр ({Math.Round((double)wins * 100 / games.Value.Count)}%, {wins}/{loses})\n");
            }
        }

        if (gameInfo.MessageId is not null) await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);
        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: sb.ToString(),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: Shared.Buttons.StatsKeyboardGirl,
            cancellationToken: cancellationToken);

        return message;
    }

    private async Task<Message> StatsByKiller(GameInfo gameInfo, CancellationToken cancellationToken)
    {
        StringBuilder sb = new();
        var gamesByKiller = (await _db.Games.GetByUser(gameInfo.ChatId, cancellationToken))
            .Where(g => g.Killer is not null)
            .GroupBy(g => g.Killer!.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var games in gamesByKiller.OrderByDescending(g => g.Value.Count))
        {
            if (games.Value.Count > 0)
            {
                var wins = games.Value.Where(g => g.Result == DB.Domain.ResultType.Win).Count();
                var loses = games.Value.Where(g => g.Result == DB.Domain.ResultType.Lose).Count();
                sb.AppendLine($"<b>{games.Value.First().Killer?.Name}</b>: {games.Value.Count} игр ({Math.Round((double)wins * 100 / games.Value.Count)}%, {wins}/{loses})\n");
            }
        }

        if (gameInfo.MessageId is not null) await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);
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
            .GroupBy(g => g.Location!.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var games in gamesByLocation.OrderByDescending(g => g.Value.Count))
        {
            if (games.Value.Count > 0)
            {
                var wins = games.Value.Where(g => g.Result == DB.Domain.ResultType.Win).Count();
                var loses = games.Value.Where(g => g.Result == DB.Domain.ResultType.Lose).Count();
                sb.AppendLine($"<b>{games.Value.First().Location?.Name}</b>: {games.Value.Count} игр ({Math.Round((double)wins * 100 / games.Value.Count)}%, {wins}/{loses})\n");
            }
        }

        if (gameInfo.MessageId is not null) await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);
        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: sb.ToString(),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: Shared.Buttons.StatsKeyboardLocation,
            cancellationToken: cancellationToken);

        return message;
    }

    private async Task<Message> GamesHistory(GameInfo gameInfo, CancellationToken cancellationToken, int? page = 1)
    {
        StringBuilder sb = new();
        var games = (await _db.Games.GetByUser(gameInfo.ChatId, cancellationToken)).OrderByDescending(g => g.DatePlayed);

        if (page is null) page = 1;

        var pagedGames = games.Chunk(10);
        foreach (var game in pagedGames.Skip(page.Value - 1).Take(1).First())
        {
            var dateString = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTimeFromUtc(game.DatePlayed, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")))
                .ToString();
            var resultString = game.Result switch
            {
                DB.Domain.ResultType.Unknown => Shared.Text.Unknown,
                DB.Domain.ResultType.Win => Shared.Text.Win,
                DB.Domain.ResultType.Lose => Shared.Text.Lose,
            };
            sb.AppendLine($"{game}\n      <b>{resultString} ({dateString})</b>\n");
        }

        var keyboard = new List<InlineKeyboardButton>();
        int i = 1;
        foreach (var chunk in pagedGames)
        {
            keyboard.Add(new InlineKeyboardButton(i == page ? $"•{i}•" : $"{i}", $"{Shared.Text.HistoryStatsCallback}-{i}"));
            i++;
        }

        if (gameInfo.MessageId is not null) await _botClient.DeleteMessage(gameInfo.ChatId, gameInfo.MessageId.Value, cancellationToken);
        var message = await _botClient.SendMessage(
            chatId: gameInfo.ChatId,
            text: sb.ToString(),
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: pagedGames.Count() > 1 ? keyboard.Chunk(10).ToArray() : null,
            cancellationToken: cancellationToken);

        return message;
    }
}
