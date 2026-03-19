using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.DTOs;

public record BoxDto(int Id, string Name, Season Season, LocationDto? Location, KillerDto? Killer, IEnumerable<GirlDto>? Girls = null) : IBaseDto
{
    public IEnumerable<GirlDto> Girls { get; init; } = Girls ?? new List<GirlDto>();
}