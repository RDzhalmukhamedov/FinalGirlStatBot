using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot.Services.AdminServices.BoxCreationHandlers;

public class SeasonSelectedBoxAction : BaseBoxCreationAction
{
    public SeasonSelectedBoxAction(IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameManager gameManager)
        : base(dbConnection, botClient, gameManager)
    {
    }

    public override async Task Execute(Chat chatInfo, CancellationToken cancellationToken = default, string data = "")
    {
        if (Enum.TryParse<Season>(data, out var season))
        {
            _gameManager.BoxCreationStates[chatInfo.Id] = new BoxCreationState()
            {
                Season = season,
            };

            var message = await _botClient.SendMessage(
                chatId: chatInfo.Id,
                text: string.Format(Shared.Text.BoxCreationStepMessage, season.ToString()) + "\n\n" + Shared.Text.EnterBoxNameMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: GetCancelKeyboard(),
                cancellationToken: cancellationToken);

            _gameManager.BoxCreationStates[chatInfo.Id].MessageId = message.MessageId;
        }
    }

    private InlineKeyboardButton[][] GetCancelKeyboard()
    {
        return [[new InlineKeyboardButton(Shared.Text.Reset, $"{Shared.Text.AddBoxCallback}{Shared.Text.Splitter}cancel")]];
    }

}
