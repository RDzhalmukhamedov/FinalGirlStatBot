using FinalGirlStatBot.DB.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;

public class CancelBoxCreationAction : BaseBoxCreationAction
{
    public CancelBoxCreationAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
        : base(dbConnection, botClient, gameManager)
    {
    }

    public override async Task Execute(Chat chatInfo, CancellationToken cancellationToken = default, string data = "")
    {
        _gameManager.BoxCreationStates.TryRemove(chatInfo.Id, out var _);

        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: Shared.Text.BoxCreationCancelledMessage,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}
