using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalGirlStatBot.DB.Domain;

/// <summary>
/// Table for storing info about user subscriptions
/// </summary>
[Table("Users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public long ChatId { get; set; }

    [Required]
    public required string UserId { get; set; }

    public ICollection<Game> Games { get; } = new List<Game>();
}
