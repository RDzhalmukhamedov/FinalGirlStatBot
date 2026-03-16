using FinalGirlStatBot.Abstract;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services;

public class TelegramUpdateHandler : IUpdateHandler
{
    private readonly ILogger<TelegramUpdateHandler> _logger;
    private readonly IGameService _gameService;
    private readonly IAdminCommandService _adminCommandService;

    public TelegramUpdateHandler(IGameService gameService, IAdminCommandService adminCommandService, ILogger<TelegramUpdateHandler> logger)
    {
        _gameService = gameService;
        _adminCommandService = adminCommandService;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken stoppingToken)
    {
        var handler = update switch
        {
            { Message: { } message } => BotOnMessageReceived(message, stoppingToken),
            { EditedMessage: { } message } => BotOnMessageReceived(message, stoppingToken),
            { CallbackQuery: { } cbQuery } => BotOnCallbackReceived(cbQuery, stoppingToken),
            _ => UnknownUpdateHandler(update)
        };

        await handler;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken stoppingToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError("HandleError: {ErrorMessage}", errorMessage);

        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Receive message type: {MessageType} from {Username}", message.Type, message.Chat.Username);
        if (message.Text is not { } messageText)
            return;

        var words = messageText.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var args = words.Length > 1 ? words.Skip(1).ToArray() : [];

        var commandParts = words[0].Split(Shared.Text.Splitter, StringSplitOptions.RemoveEmptyEntries);
        var command = commandParts[0];
        var idStr = commandParts.Length > 1 ? commandParts[1] : string.Empty;

        var action = command switch
        {
            "/newgame" or "/ng" => _gameService.StartNewGame(message.Chat, stoppingToken),
            "/stat" => _gameService.GetStatistics(message.Chat, stoppingToken),
            "/delete" => _gameService.DeleteGame(message.Chat, idStr, stoppingToken),
            "/repeat" => _gameService.RepeatGame(message.Chat, idStr, stoppingToken),

            "/addgirl" => _adminCommandService.HandleAddGirl(message.Chat, args, stoppingToken),
            "/addkiller" => _adminCommandService.HandleAddKiller(message.Chat, args, stoppingToken),
            "/addlocation" => _adminCommandService.HandleAddLocation(message.Chat, args, stoppingToken),

            _ => _gameService.SendUsage(message.Chat, stoppingToken),
        };

        await action;
    }

    private async Task BotOnCallbackReceived(CallbackQuery query, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Receive callback query from {Username}", query.From.Username);
        await _gameService.ProcessUserInput(query.Message!.Chat.Id, query.Data!, stoppingToken);
    }

    private Task UnknownUpdateHandler(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        _logger.LogError("Telegram error handled: {Exception} with source {Source}", exception, source);
        return Task.CompletedTask;
    }
}
