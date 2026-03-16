namespace FinalGirlStatBot;

public record AdminConfiguration
{
    public static readonly string Configuration = "AdminConfiguration";

    public List<long> AdminChatIds { get; set; } = [];
    public long BotOwnerId { get; set; } = 0;
}