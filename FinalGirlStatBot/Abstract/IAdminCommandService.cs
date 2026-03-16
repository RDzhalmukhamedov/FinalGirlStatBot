using Telegram.Bot.Types;

namespace FinalGirlStatBot.Abstract;

public interface IAdminCommandService
{
    Task HandleAddGirl(Chat chatInfo, string[] args, CancellationToken cancellationToken);
    Task HandleAddKiller(Chat chatInfo, string[] args, CancellationToken cancellationToken);
    Task HandleAddLocation(Chat chatInfo, string[] args, CancellationToken cancellationToken);
}