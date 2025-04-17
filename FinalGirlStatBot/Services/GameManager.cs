using System.Collections.Concurrent;
using FinalGirlStatBot.DB.Domain;
using Game = FinalGirlStatBot.DB.Domain.Game;
using Location = FinalGirlStatBot.DB.Domain.Location;

namespace FinalGirlStatBot.Services;

public class GameManager
{
    private readonly ConcurrentDictionary<long, GameInfo> ActiveGames = new();

    public (bool, Game?) StartNewGame(User user)
    {
        var newGame = new Game() { User = user, DatePlayed = DateTime.UtcNow };
        var newInfo = new GameInfo() { Game = newGame, State = GameState.Init, ChatId = user.ChatId };
        var success = ActiveGames.TryAdd(user.ChatId, newInfo);
        ActiveGames.TryGetValue(user.ChatId, out newInfo);

        return (success, newInfo?.Game);
    }

    public (bool, Game?) FinishGame(long chatId, ResultType result)
    {
        var success = ActiveGames.TryRemove(chatId, out var currentGame);

        if (success && currentGame?.Game is not null)
        {
            currentGame.Game.Result = result;
        }

        return (success, currentGame?.Game);
    }

    public bool SetGirl(long chatId, Girl girl)
    {
        var success = ActiveGames.TryGetValue(chatId, out var currentGame);

        if (success && currentGame?.Game is not null)
        {
            currentGame.Game.Girl = girl;

            return true;
        }

        return false;
    }

    public bool SetKiller(long chatId, Killer killer)
    {
        var success = ActiveGames.TryGetValue(chatId, out var currentGame);

        if (success && currentGame?.Game is not null)
        {
            currentGame.Game.Killer = killer;

            return true;
        }

        return false;
    }

    public bool SetLocation(long chatId, Location location)
    {
        var success = ActiveGames.TryGetValue(chatId, out var currentGame);

        if (success && currentGame?.Game is not null)
        {
            currentGame.Game.Location = location;

            return true;
        }

        return false;
    }

    public GameInfo? GetGameInfo(long chatId)
    {
        var success = ActiveGames.TryGetValue(chatId, out var currentGame);

        return currentGame;
    }

    public void DeleteGame(long chatId)
    {
        ActiveGames.TryRemove(chatId, out var _);
    }

    public (bool, GameInfo?) CreateStatsDummy(User user)
    {
        var newInfo = new GameInfo() { State = GameState.Stats, ChatId = user.ChatId };
        var success = ActiveGames.TryAdd(user.ChatId, newInfo);
        ActiveGames.TryGetValue(user.ChatId, out newInfo);

        return (success, newInfo);
    }
}
