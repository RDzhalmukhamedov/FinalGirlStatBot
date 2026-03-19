using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.DB.Domain;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using FinalGirlStatBot.Models;

namespace FinalGirlStatBot.Services.UserActionHandlers;

public class StatisticsAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
    : GameStateActionBase(dbConnection, botClient, gameManager)
{
    public override async Task<Message> SendActionMessage
        (GameInfo gameInfo, bool deletePrev = false, string additionalMessage = "", CancellationToken cancellationToken = default)
    {
        var games = (await _db.Games.GetByUser(chatId: gameInfo.ChatId, cancellationToken)).ToList();
        var text = BuildMainStatsString(games);
        var keyboard = games.Count > 0 ? Shared.Buttons.StatsKeyboard : null;

        return await UpdateMessage(gameInfo, text, keyboard, deletePrev, additionalMessage, cancellationToken);
    }

    public override async Task<ActionResult> ProcessCallback
        (GameInfo gameInfo, string userAction, CancellationToken cancellationToken = default, dynamic? payload = null)
    {
        var message = await (userAction switch
        {
            Shared.Text.GirlStatsCallback => StatsByGirl(gameInfo, cancellationToken),
            Shared.Text.KillerStatsCallback => StatsByKiller(gameInfo, cancellationToken),
            Shared.Text.LocationStatsCallback => StatsByLocation(gameInfo, cancellationToken),
            Shared.Text.HistoryStatsCallback => GamesHistory(gameInfo, payload, cancellationToken),
            Shared.Text.ResetCallback => Reset(gameInfo, cancellationToken),
            _ => ActionResult.Error(),
        });

        return message;
    }

    private string BuildMainStatsString(List<GameDto> games)
    {
        var wins = games.Count(g => g.Result == ResultType.Win);
        var loses = games.Count(g => g.Result == ResultType.Lose);
        var unknown = games.Count - (wins + loses);
        var unknownString = unknown > 0 ? $" ({unknown} {Shared.Text.NoResultsCountMessage})" : "";
        var winPercentage = Math.Round((double)wins * 100 / (games.Count - unknown), 2);
        var percentageString = games.Count > 0 ? $"\n{Shared.Text.WinPercentageMessage} {winPercentage}% ({wins}/{loses})" : "";

        return $"{Shared.Text.TotalGamesMessage} {games.Count}{unknownString}.{percentageString}";
    }

    private async Task<ActionResult> StatsByGirl(GameInfo gameInfo, CancellationToken cancellationToken = default)
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
                var wins = games.Value.Count(g => g.Result == ResultType.Win);
                var loses = games.Value.Count(g => g.Result == ResultType.Lose);
                var winPercetage = Math.Round((double)wins * 100 / games.Value.Count);
                sb.AppendLine($"<b>{games.Value.First().Girl?.Name}</b>: {GetGameCountString(games.Value.Count)} ({winPercetage}%, {wins}/{loses})\n");
            }
        }

        await UpdateMessage(gameInfo, sb.ToString(), Shared.Buttons.StatsKeyboardGirl, cancellationToken: cancellationToken);

        return ActionResult.Ok();
    }

    private async Task<ActionResult> StatsByKiller(GameInfo gameInfo, CancellationToken cancellationToken = default)
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
                var wins = games.Value.Count(g => g.Result == ResultType.Win);
                var loses = games.Value.Count(g => g.Result == ResultType.Lose);
                var winPercetage = Math.Round((double)wins * 100 / games.Value.Count);
                sb.AppendLine($"<b>{games.Value.First().Killer?.Name}</b>: {GetGameCountString(games.Value.Count)} ({winPercetage}%, {wins}/{loses})\n");
            }
        }

        await UpdateMessage(gameInfo, sb.ToString(), Shared.Buttons.StatsKeyboardKiller, cancellationToken: cancellationToken);

        return ActionResult.Ok();
    }

    private async Task<ActionResult> StatsByLocation(GameInfo gameInfo, CancellationToken cancellationToken = default)
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
                var wins = games.Value.Count(g => g.Result == ResultType.Win);
                var loses = games.Value.Count(g => g.Result == ResultType.Lose);
                var winPercetage = Math.Round((double)wins * 100 / games.Value.Count);
                sb.AppendLine($"<b>{games.Value.First().Location?.Name}</b>: {GetGameCountString(games.Value.Count)} ({winPercetage}%, {wins}/{loses})\n");
            }
        }

        await UpdateMessage(gameInfo, sb.ToString(), Shared.Buttons.StatsKeyboardLocation, cancellationToken: cancellationToken);

        return ActionResult.Ok(null);
    }

    private async Task<ActionResult> GamesHistory(GameInfo gameInfo, IEnumerable<string> pageStr, CancellationToken cancellationToken = default)
    {
        var page = 1;
        var havePage = pageStr is not null && pageStr.Count() > 0 && int.TryParse(pageStr.First(), out page);
        StringBuilder sb = new();
        var gamesList = (await _db.Games.GetByUser(gameInfo.ChatId, cancellationToken)).OrderByDescending(g => g.DatePlayed).ToList();
        var totalPages = Math.Max(1, (gamesList.Count + 9) / 10);
        var safePage = Math.Clamp(page, 1, totalPages);
        var chunk = gamesList.Skip((safePage - 1) * 10).Take(10);

        foreach (var game in chunk)
        {
            sb.AppendLine(GetFullGameInfoString(game));
            sb.AppendLine($"            {Shared.Text.DeleteGame}: /delete{Shared.Text.Splitter}{game.Id}");
            sb.AppendLine($"            {Shared.Text.RepeatGame}: /repeat{Shared.Text.Splitter}{game.Id}\n");
        }

        var keyboard = new List<InlineKeyboardButton>();
        for (int i = 1; i <= totalPages; i++)
        {
            keyboard.Add(new InlineKeyboardButton(i == safePage ? $"•{i}•" : $"{i}", $"{Shared.Text.HistoryStatsCallback}{Shared.Text.Splitter}{i}"));
        }

        await UpdateMessage(gameInfo, sb.Length > 0 ? sb.ToString() : "", totalPages > 1 ? keyboard.Chunk(10).ToArray() : null, cancellationToken: cancellationToken);

        return ActionResult.Ok();
    }

    private string GetGameCountString(int count)
    {
        // Получаем последнюю цифру и две последние цифры числа
        int lastDigit = count % 10;
        int lastTwoDigits = count % 100;

        // Исключение для чисел от 11 до 14 (всегда "игр")
        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return $"{count} игр";

        // Для чисел, оканчивающихся на 1 (кроме 11)
        if (lastDigit == 1)
            return $"{count} игра";

        // Для чисел, оканчивающихся на 2, 3, 4 (кроме 12-14)
        if (lastDigit >= 2 && lastDigit <= 4)
            return $"{count} игры";

        // Во всех остальных случаях
        return $"{count} игр";
    }
}
