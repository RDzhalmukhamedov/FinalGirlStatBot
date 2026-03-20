using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.Models;

public class UserCollectionState
{
    public HashSet<int>? BoxIds { get; set; } = null;
    public Season SelectedSeason { get; set; } = Season.S1;
}
