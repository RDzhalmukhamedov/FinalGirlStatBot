using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalGirlStatBot.DB.Domain;

[Table("Killers")]
public class Killer : IBaseDomain
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public Season Season { get; set; }

    public ICollection<Game> Games { get; } = new List<Game>();
}
