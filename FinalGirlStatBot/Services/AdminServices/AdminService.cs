using FinalGirlStatBot.Abstract;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.AdminServices;

public class AdminService(IOptions<AdminConfiguration> adminConfigOptions) : IAdminService
{
    private readonly AdminConfiguration _adminConfig = adminConfigOptions.Value;

    public Task<bool> IsAdmin(Chat chatInfo, CancellationToken cancellationToken)
    {
        return Task.FromResult(_adminConfig.AdminChatIds.Contains(chatInfo.Id) || _adminConfig.BotOwnerId == chatInfo.Id);
    }

    public Task<bool> IsAdmin(long chatId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_adminConfig.AdminChatIds.Contains(chatId) || _adminConfig.BotOwnerId == chatId);
    }

    public Task<bool> IsBotOwner(Chat chatInfo, CancellationToken cancellationToken)
    {
        return Task.FromResult(_adminConfig.BotOwnerId == chatInfo.Id);
    }

    public Task<bool> IsBotOwner(long chatId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_adminConfig.BotOwnerId == chatId);
    }
}