using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.DTOs;

public record KillerDto(int Id, string Name, Season Season, int? BoxId) : IBaseDto;
