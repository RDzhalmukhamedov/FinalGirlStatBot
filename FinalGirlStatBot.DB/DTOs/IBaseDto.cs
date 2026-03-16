using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.DTOs;

public interface IBaseDto
{
    int Id { get; }
    string Name { get; }
    public Season Season { get; }
}
