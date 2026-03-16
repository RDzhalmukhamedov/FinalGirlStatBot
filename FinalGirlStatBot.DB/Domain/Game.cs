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

    public string? ShortInfo { get; set; }

    [Required]
    public int GirlId { get; set; }

    [Required]
    public int KillerId { get; set; }

    [Required]
    public int LocationId { get; set; }

    [Required]
    public int UserId { get; set; }

    public virtual Girl? Girl { get; set; }

    public virtual Killer? Killer { get; set; }

    public virtual Location? Location { get; set; }

    public virtual User? User { get; set; }
}
