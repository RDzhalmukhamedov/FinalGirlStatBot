using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.DTOs;

public record GirlDto(int Id, string Name, Season Season) : IBaseDto;
