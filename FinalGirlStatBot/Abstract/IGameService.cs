using Telegram.Bot.Types;

namespace FinalGirlStatBot.Abstract;

public interface IGameService
{
    Task StartNewGame(Chat chatInfo, CancellationToken cancellationToken);

    Task ProcessUserInput(long chatId, string data, CancellationToken cancellationToken);

    Task GetStatistics(Chat chatInfo, CancellationToken cancellationToken);
}
