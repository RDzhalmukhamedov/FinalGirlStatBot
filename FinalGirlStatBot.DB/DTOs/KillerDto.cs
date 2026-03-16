using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.DTOs;

public record KillerDto(int Id, string Name, Season Season) : IBaseDto;