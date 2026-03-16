namespace FinalGirlStatBot.DB.Domain;

public interface IBaseDomain
{
    int Id { get; set; }
    string Name { get; set; }
    public Season Season { get; set; }
}
