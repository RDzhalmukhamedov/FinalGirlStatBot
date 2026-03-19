using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.Models;

public class BoxCreationState()
{
    public Season Season { get; set; }
    public string Name { get; set; } = "";
    public int? LocationId { get; set; }
    public int? KillerId { get; set; }
    public HashSet<int> GirlIds { get; set; } = new();
    public int? MessageId { get; set; }
}