using FinalGirlStatBot.Abstract;
using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.Models;
using FinalGirlStatBot.Services.UserActionHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinalGirlStatBot.Services;

public class GameService(GameManager gameManager, IFGStatsUnitOfWork dbConnection, ITelegramBotClient botClient, GameStateActionFactory stateActionFactory) : IGameService
{
    private readonly GameManager _gameManager = gameManager;
    private readonly IFGStatsUnitOfWork _db = dbConnection;
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly GameStateActionFactory _stateActionFactory = stateActionFactory;

    public async Task StartNewGame(Chat chatInfo, CancellationToken cancellationToken = default)
    {
        var user = await GetOrCreateUser(chatInfo.Id, chatInfo.Username, cancellationToken);

        var gameInfo = await CheckExsistingGame(chatInfo.Id, cancellationToken);
        if (gameInfo is null) return;

        await CreateNewGame(user, cancellationToken);
    }

    public async Task StartCollection(Chat chatInfo, CancellationToken cancellationToken = default)
    {
        await GetOrCreateUser(chatInfo.Id, chatInfo.Username, cancellationToken);

        var gameInfo = _gameManager.GetGameInfo(chatInfo.Id);
        if (_gameManager.TransitionToState(chatInfo.Id, GameState.Collection))
        {
            await _stateActionFactory.GetStateAction(GameState.Collection)
                .SendActionMessage(gameInfo, true, cancellationToken: cancellationToken);
        }
    }

    public async Task GetStatistics(Chat chatInfo, CancellationToken cancellationToken = default)
    {
        await GetOrCreateUser(chatInfo.Id, chatInfo.Username, cancellationToken);

        var gameInfo = await CheckExsistingGame(chatInfo.Id, cancellationToken);
        if (gameInfo is null) return;

        if (_gameManager.TransitionToState(gameInfo, GameState.ViewingStats))
        {
            await _stateActionFactory.GetStateAction(gameInfo.State)
                .SendActionMessage(gameInfo, true, cancellationToken: cancellationToken);
        }
    }

    public async Task SendUsage(Chat chatInfo, CancellationToken cancellationToken = default)
    {
        await _botClient.SendMessage(
            chatId: chatInfo.Id,
            text: Shared.Text.UsageInfoMessage,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    public async Task ProcessUserInput(long chatId, string userInput, CancellationToken cancellationToken = default)
    {
        var gameInfo = _gameManager.GetGameInfo(chatId);
        var userInputParts = userInput.Split(Shared.Text.Splitter);
        var additionalInput = userInputParts.Length > 1 ? userInputParts.Skip(1) : null;

        var result = await _stateActionFactory.GetStateAction(gameInfo.State)
            .ProcessCallback(gameInfo, userInputParts[0], cancellationToken, additionalInput);

        if (result.Success && result.TargetState.HasValue)
        {
            var transitioned = _gameManager.TransitionToState(gameInfo, result.TargetState.Value);
            var message = transitioned ? result.AdditionalMessage : Shared.Text.SomethingWrongMessage;

            await _stateActionFactory.GetStateAction(gameInfo.State)
                .SendActionMessage(gameInfo, additionalMessage: message, cancellationToken: cancellationToken);
        }
        else if (!result.Success)
        {
            await _stateActionFactory.GetStateAction(gameInfo.State)
                .SendActionMessage(gameInfo, additionalMessage: result.AdditionalMessage, cancellationToken: cancellationToken);
        }
    }

    public async Task DeleteGame(Chat chatInfo, string idStr, CancellationToken cancellationToken = default)
    {
        var gameInfo = await CheckExsistingGame(chatInfo.Id, cancellationToken);
        if (gameInfo is null) return;

        if (int.TryParse(idStr, out var gameId))
        {
            var user = await GetOrCreateUser(chatInfo.Id, chatInfo.Username, cancellationToken);
            var gameToDelete = (await _db.Games.GetByUser(chatInfo.Id, cancellationToken)).FirstOrDefault(g => g?.Id == gameId, null);
            if (gameToDelete is not null)
            {
                _gameManager.MarkGameToDelete(gameInfo, gameToDelete);

                if (_gameManager.TransitionToState(gameInfo, GameState.DeleteGame))
                {
                    await _stateActionFactory.GetStateAction(gameInfo.State)
                        .SendActionMessage(gameInfo, true, cancellationToken: cancellationToken);
                }
            }
        }
        else
        {
            await _botClient.SendMessage(
                            chatId: chatInfo.Id,
                            text: Shared.Text.RecordNotFoundMessage,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            cancellationToken: cancellationToken);
        }
    }

    public async Task RepeatGame(Chat chatInfo, string idStr, CancellationToken cancellationToken = default)
    {
        var gameInfo = await CheckExsistingGame(chatInfo.Id, cancellationToken);
        if (gameInfo is null) return;

        if (int.TryParse(idStr, out var gameId))
        {
            var user = await GetOrCreateUser(chatInfo.Id, chatInfo.Username, cancellationToken);
            var gameToRepeat = (await _db.Games.GetByUser(chatInfo.Id, cancellationToken)).FirstOrDefault(g => g?.Id == gameId, null);
            if (gameToRepeat is not null)
            {
                await CreateNewGame(user, cancellationToken, gameToRepeat);
                return;
            }
        }

        await _botClient.SendMessage(
                        chatId: chatInfo.Id,
                        text: Shared.Text.RecordNotFoundMessage,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        cancellationToken: cancellationToken);
    }

    private async Task<GameInfo?> CheckExsistingGame(long id, CancellationToken cancellationToken = default)
    {
        var gameInfo = _gameManager.GetGameInfo(id);

        if (gameInfo.Game is not null)
        {
            await _stateActionFactory.GetStateAction(gameInfo.State)
                .SendActionMessage(gameInfo, true, Shared.Text.UHaveUnfinishedGameMessage, cancellationToken);
            return null;
        }

        return gameInfo;
    }

    private async Task CreateNewGame(UserDto user, CancellationToken cancellationToken = default, GameDto? gameFrom = null)
    {
        var gameInfo = _gameManager.StartNewGame(user);

        _gameManager.SetFromOtherGame(gameInfo, gameFrom);
        _gameManager.TransitionToState(user.ChatId, GameState.CreatingGame);

        await _stateActionFactory.GetStateAction(gameInfo.State)
            .SendActionMessage(gameInfo, true, cancellationToken: cancellationToken);
    }

    private async Task<UserDto> GetOrCreateUser(long chatId, string? username, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.CreateIfNotExist(chatId, username, cancellationToken);
        return user;
    }
}
