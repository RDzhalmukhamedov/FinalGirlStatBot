using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.DTOs;

public class GameDto
{
    public int Id { get; set; }

    public ResultType Result { get; set; }

    public DateTime DatePlayed { get; set; }

    public virtual GirlDto? Girl { get; set; }

    public virtual KillerDto? Killer { get; set; }

    public virtual LocationDto? Location { get; set; }

    public virtual UserDto? User { get; set; }

    public bool ReadyToStart => Girl is not null && Killer is not null && Location is not null;

    public override string ToString()
    {
        return $"👩<b>{Girl?.Name ?? "???"}</b> vs 🔪<b>{Killer?.Name ?? "???"}</b> (🏠{Location?.Name ?? "???"})";
    }
}