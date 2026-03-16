using Telegram.Bot.Types;

namespace FinalGirlStatBot.Abstract;

/// <summary>
/// Interface for game service operations in the Final Girl Stat Bot.
/// Provides methods for managing game recording, statistics, and user interactions.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Starts a new game recording session for the specified chat.
    /// </summary>
    /// <param name="chatInfo">Information about the Telegram chat.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task StartNewGame(Chat chatInfo, CancellationToken cancellationToken);

    /// <summary>
    /// Processes user input (button callbacks) for the ongoing game session.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="data">The callback data from the user interaction.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task ProcessUserInput(long chatId, string data, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves and displays game statistics for the specified chat.
    /// </summary>
    /// <param name="chatInfo">Information about the Telegram chat.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task GetStatistics(Chat chatInfo, CancellationToken cancellationToken);

    /// <summary>
    /// Sends usage information and available commands to the user.
    /// </summary>
    /// <param name="chatInfo">Information about the Telegram chat.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task SendUsage(Chat chatInfo, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a specific game from the statistics by its ID.
    /// </summary>
    /// <param name="chatInfo">Information about the Telegram chat.</param>
    /// <param name="idStr">String representation of the game ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task DeleteGame(Chat chatInfo, string idStr, CancellationToken cancellationToken);

    /// <summary>
    /// Repeats a game with the same settings as a previously recorded game.
    /// </summary>
    /// <param name="chatInfo">Information about the Telegram chat.</param>
    /// <param name="idStr">String representation of the game ID to repeat.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task RepeatGame(Chat chatInfo, string idStr, CancellationToken cancellationToken);
}
