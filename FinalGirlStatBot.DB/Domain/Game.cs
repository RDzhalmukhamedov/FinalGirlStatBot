using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalGirlStatBot.DB.Domain;

[Table("Games")]
public class Game
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public ResultType Result { get; set; }

    public DateTime DatePlayed { get; set; }

    public string? ShortInfo {  get; set; }

    [Required]
    public Girl? Girl { get; set; }

    [Required]
    public Killer? Killer { get; set; }

    [Required]
    public Location? Location { get; set; }

    [Required]
    public User? User { get; set; }

    public bool ReadyToStart => Girl is not null && Killer is not null && Location is not null;

    public override string ToString()
    {
        return $"👩<b>{ Girl?.Name ?? "???" }</b> vs 🔪<b>{ Killer?.Name ?? "???"}</b> (🏠{ Location?.Name ?? "???"})";
    }
}
