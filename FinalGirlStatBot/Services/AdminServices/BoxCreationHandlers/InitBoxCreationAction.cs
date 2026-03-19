using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;

public class InitBoxCreationAction : BaseBoxCreationAction
{
    public InitBoxCreationAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
        : base(dbConnection, botClient, gameManager)
    {
    }

    public override async Task Execute(Chat chatInfo, CancellationToken cancellationToken = default, string data = "")
    {
        var seasons = Enum.GetValues<Season>();
        var keyboard = seasons
            .Select(s => new InlineKeyboardButton(s.ToString(), $"{Shared.Text.AddBoxCallback}{Shared.Text.Splitter}season{Shared.Text.Splitter}{s}"))
            .Chunk(3)
            .Select(row => row.ToArray())
            .ToArray();

        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: Shared.Text.SelectSeasonMessage,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}
