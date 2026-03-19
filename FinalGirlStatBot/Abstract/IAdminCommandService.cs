using Telegram.Bot.Types;

namespace FinalGirlStatBot.Abstract;

public interface IAdminCommandService
{
    Task HandleAddBox(Chat chatInfo, string[] args, CancellationToken cancellationToken);
    Task ProcessAddBoxCallback(Chat chatInfo, string callbackData, CancellationToken cancellationToken);
    Task HandleAddBoxName(Chat chatInfo, string name, CancellationToken cancellationToken);
    bool IsAwaitingBoxName(long chatId);
    Task HandleAddGirl(Chat chatInfo, string[] args, CancellationToken cancellationToken);
    Task HandleAddKiller(Chat chatInfo, string[] args, CancellationToken cancellationToken);
    Task HandleAddLocation(Chat chatInfo, string[] args, CancellationToken cancellationToken);
}