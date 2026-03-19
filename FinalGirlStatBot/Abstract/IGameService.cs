using Telegram.Bot.Types;

namespace FinalGirlStatBot.Abstract;

public interface IGameService
{
    Task StartNewGame(Chat chatInfo, CancellationToken cancellationToken);

    Task StartCollection(Chat chatInfo, CancellationToken cancellationToken);

    Task ProcessUserInput(long chatId, string data, CancellationToken cancellationToken);

    Task GetStatistics(Chat chatInfo, CancellationToken cancellationToken);

    Task SendUsage(Chat chatInfo, CancellationToken cancellationToken);

    Task DeleteGame(Chat chatInfo, string idStr, CancellationToken cancellationToken);

    Task RepeatGame(Chat chatInfo, string idStr, CancellationToken cancellationToken);
}
