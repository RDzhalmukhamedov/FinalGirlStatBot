using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot;

public class GameInfo
{
    public long MessageId { get; set; }

    public long ChatId { get; set; }

    public GameState State { get; set; }

    public Game? Game { get; set; }
}
