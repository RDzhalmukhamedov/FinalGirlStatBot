using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalGirlStatBot.DB.Domain;

[Table("UserBoxes")]
public class UserBox
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int BoxId { get; set; }

    public virtual User? User { get; set; }
    public virtual Box? Box { get; set; }
}
