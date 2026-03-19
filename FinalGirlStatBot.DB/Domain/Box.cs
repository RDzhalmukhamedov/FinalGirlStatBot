using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalGirlStatBot.DB.Domain;

[Table("Boxes")]
public class Box : IBaseDomain
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public Season Season { get; set; }

    public int? LocationId { get; set; }

    public int? KillerId { get; set; }

    public virtual Location? Location { get; set; }

    public virtual Killer? Killer { get; set; }

    public virtual ICollection<Girl> Girls { get; } = new List<Girl>();
}