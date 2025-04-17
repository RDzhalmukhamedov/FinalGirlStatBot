using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot;

public class GameInfo
{
    public int? MessageId { get; set; }

    public long ChatId { get; set; }

    public GameState State { get; set; }

    public Game? Game { get; set; }

    public bool ReadyToStart => Game is not null && Game.ReadyToStart;
}
