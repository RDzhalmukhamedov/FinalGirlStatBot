using Telegram.Bot.Types;

namespace FinalGirlStatBot.Abstract;

public interface IAdminService
{
    Task<bool> IsAdmin(Chat chatInfo, CancellationToken cancellationToken);
    Task<bool> IsAdmin(long chatId, CancellationToken cancellationToken);
    Task<bool> IsBotOwner(Chat chatInfo, CancellationToken cancellationToken);
    Task<bool> IsBotOwner(long chatId, CancellationToken cancellationToken);
}