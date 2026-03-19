using FinalGirlStatBot.Models;

namespace FinalGirlStatBot.Services;

public static class StateTransitionValidator
{
    private static readonly Dictionary<GameState, HashSet<GameState>> ValidTransitions = new()
    {
        [GameState.Init] = new() { GameState.CreatingGame, GameState.ViewingStats, GameState.DeleteGame, GameState.Collection },
        [GameState.CreatingGame] = new() { GameState.SelectGirl, GameState.SelectKiller, GameState.SelectLocation, GameState.GameInProgress, GameState.Init },
        [GameState.SelectGirl] = new() { GameState.CreatingGame },
        [GameState.SelectKiller] = new() { GameState.CreatingGame },
        [GameState.SelectLocation] = new() { GameState.CreatingGame },
        [GameState.GameInProgress] = new() { GameState.GameCompleted, GameState.Init },
        [GameState.GameCompleted] = new() { GameState.CreatingGame, GameState.ViewingStats, GameState.Init },
        [GameState.ViewingStats] = new() { GameState.CreatingGame, GameState.DeleteGame, GameState.Init, GameState.ViewingStats },
        [GameState.DeleteGame] = new() { GameState.Init, GameState.ViewingStats },
        [GameState.Collection] = new() { GameState.Collection, GameState.Init },
    };

    public static bool IsValidTransition(GameState from, GameState to)
    {
        return ValidTransitions.ContainsKey(from) && ValidTransitions[from].Contains(to);
    }

    public static void ValidateTransition(GameState from, GameState to)
    {
        if (!IsValidTransition(from, to))
        {
            throw new InvalidOperationException($"Invalid state transition from {from} to {to}");
        }
    }
}
