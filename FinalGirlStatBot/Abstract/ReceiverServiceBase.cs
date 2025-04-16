﻿using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace FinalGirlStatBot.Abstract;

public class ReceiverServiceBase<TUpdateHandler> : IReceiverService
    where TUpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandlers;
    private readonly ILogger<ReceiverServiceBase<TUpdateHandler>> _logger;
    private readonly BotConfiguration _config;

    internal ReceiverServiceBase(
        ITelegramBotClient botClient,
        TUpdateHandler updateHandler,
        ILogger<ReceiverServiceBase<TUpdateHandler>> logger,
        IOptions<BotConfiguration> config)
    {
        _botClient = botClient;
        _updateHandlers = updateHandler;
        _logger = logger;
        _config = config.Value;
    }

    public async Task Receive(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions()
        {
            // TODO Возможно null или конкретный список
            AllowedUpdates = Array.Empty<UpdateType>(),
            DropPendingUpdates = _config.ThrowPendingUpdates,
        };
        var me = await _botClient.GetMe(stoppingToken);

        _logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "Final Girl Statistic Bot");
        await _botClient.ReceiveAsync(
            updateHandler: _updateHandlers,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }
}
