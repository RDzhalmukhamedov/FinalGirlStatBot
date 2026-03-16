using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.DTOs;

public record LocationDto(int Id, string Name, Season Season) : IBaseDto;