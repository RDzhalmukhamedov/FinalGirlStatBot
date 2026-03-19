using System.Collections.Concurrent;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.Models;

namespace FinalGirlStatBot.Services;

public class GameManager
{
    public readonly ConcurrentDictionary<long, BoxCreationState> BoxCreationStates = new();
    public readonly ConcurrentDictionary<long, UserCollectionState> UserCollections = new();
    private readonly ConcurrentDictionary<long, GameInfo> ActiveGames = new();

    public UserCollectionState GetCollectionState(long chatId)
    {
        if (!UserCollections.TryGetValue(chatId, out var state))
        {
            state = new UserCollectionState();
        }
        UserCollections[chatId] = state;

        return state;
    }

    public GameInfo StartNewGame(UserDto user)
    {
        var gameInfo = CreateGameInfoIfNotExsist(user.ChatId);

        var newGame = new GameDto() { Result = ResultType.Unknown, DatePlayed = DateTime.UtcNow, User = user };
        gameInfo.Game = newGame;

        return gameInfo;
    }

    public (bool, GameDto?) SetGameResult(long chatId, ResultType result)
    {
        var success = ActiveGames.TryGetValue(chatId, out var currentGame);

        if (success && currentGame?.Game is not null)
        {
            currentGame.Game.Result = result;
        }

        return (success, currentGame?.Game);
    }

    public bool UpdateGameId(GameInfo gameInfo, int id)
    {
        if (gameInfo.Game is not null)
        {
            gameInfo.Game.Id = id;
            return true;
        }

        return false;
    }

    public bool ResetGame(long chatId)
    {
        var gameInfo = CreateGameInfoIfNotExsist(chatId);
        gameInfo.Game = null;
        gameInfo.PendingDeleteGame = false;
        return true;
    }

    public bool SetGirl(GameInfo gameInfo, GirlDto? girl)
    {
        if (gameInfo.Game is not null)
        {
            gameInfo.Game.Girl = girl;

            return true;
        }

        return false;
    }

    public bool SetKiller(GameInfo gameInfo, KillerDto? killer)
    {
        if (gameInfo.Game is not null)
        {
            gameInfo.Game.Killer = killer;

            return true;
        }

        return false;
    }

    public bool SetLocation(GameInfo gameInfo, LocationDto? location)
    {
        if (gameInfo.Game is not null)
        {
            gameInfo.Game.Location = location;

            return true;
        }

        return false;
    }

    public bool SetFromOtherGame(GameInfo currentGameInfo, GameDto? gameFrom)
    {
        if (gameFrom is null) return false;

        var success = SetGirl(currentGameInfo, gameFrom.Girl);
        success &= SetKiller(currentGameInfo, gameFrom.Killer);
        success &= SetLocation(currentGameInfo, gameFrom.Location);

        return success;
    }

    public void MarkGameToDelete(GameInfo gameInfo, GameDto gameToDelete)
    {
        gameInfo.Game = gameToDelete;
        gameInfo.PendingDeleteGame = true;
    }

    public GameInfo GetGameInfo(long chatId)
    {
        return CreateGameInfoIfNotExsist(chatId);
    }

    public bool TransitionToState(long chatId, GameState newState)
    {
        var gameInfo = GetGameInfo(chatId);
        if (gameInfo == null)
            return false;

        TransitionToState(gameInfo, newState);
        return true;
    }

    public bool TransitionToState(GameInfo gameInfo, GameState newState)
    {
        StateTransitionValidator.ValidateTransition(gameInfo.State, newState);
        gameInfo.State = newState;
        return true;
    }

    private GameInfo CreateGameInfoIfNotExsist(long chatId)
    {
        var exsist = ActiveGames.TryGetValue(chatId, out var gameInfo);
        if (gameInfo is null)
        {
            exsist = false;
            ActiveGames.TryRemove(chatId, out var _);
        }
        if (!exsist)
        {
            gameInfo = new GameInfo() { State = GameState.Init, ChatId = chatId };

            ActiveGames.TryAdd(chatId, gameInfo);
        }

        return gameInfo!;
    }
}
