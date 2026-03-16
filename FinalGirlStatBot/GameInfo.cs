using FinalGirlStatBot.DB.DTOs;

namespace FinalGirlStatBot;

public class GameInfo
{
    public int? MessageId { get; set; }

    public long ChatId { get; set; }

    public GameState State { get; set; }

    public GameDto? Game { get; set; }

    public bool PendingDeleteGame { get; set; } = false;

    public bool ReadyToStart => Game is not null && Game.ReadyToStart;
}
