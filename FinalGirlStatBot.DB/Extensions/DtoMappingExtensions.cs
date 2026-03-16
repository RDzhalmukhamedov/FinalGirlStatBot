using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.DB.DTOs;

namespace FinalGirlStatBot.DB.Extensions;

public static class DtoMappingExtensions
{
    // Girl mappings
    public static GirlDto ToDto(this Girl girl) =>
        new(girl.Id, girl.Name, girl.Season);

    public static Girl ToEntity(this GirlDto girlDto) =>
        new() { Name = girlDto.Name, Season = girlDto.Season };

    // Killer mappings
    public static KillerDto ToDto(this Killer killer) =>
        new(killer.Id, killer.Name, killer.Season);

    public static Killer ToEntity(this KillerDto killerDto) =>
        new() { Name = killerDto.Name, Season = killerDto.Season };

    // Location mappings
    public static LocationDto ToDto(this Location location) =>
        new(location.Id, location.Name, location.Season);

    public static Location ToEntity(this LocationDto locationDto) =>
        new() { Name = locationDto.Name, Season = locationDto.Season };

    // User mappings
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.ChatId, null);

    public static User ToEntity(this UserDto userDto) =>
        new() { ChatId = userDto.ChatId, UserId = null };

    // Game mappings
    public static GameDto ToDto(this Game game) =>
        new()
        {
            Id = game.Id,
            Result = game.Result,
            DatePlayed = game.DatePlayed,
            Girl = game.Girl?.ToDto(),
            Killer = game.Killer?.ToDto(),
            Location = game.Location?.ToDto(),
            User = game.User?.ToDto()
        };

    public static Game ToEntity(this GameDto gameDto) =>
        new()
        {
            Result = gameDto.Result,
            DatePlayed = gameDto.DatePlayed,
            ShortInfo = gameDto.ToString(),
            GirlId = gameDto.Girl?.Id ?? 0,
            KillerId = gameDto.Killer?.Id ?? 0,
            LocationId = gameDto.Location?.Id ?? 0,
            UserId = gameDto.User?.Id ?? 0
        };

    // Collection mappings
    public static IEnumerable<GirlDto> ToDtos(this IEnumerable<Girl> girls) =>
        girls.Select(g => g.ToDto());
    public static IQueryable<GirlDto> ToDtos(this IQueryable<Girl> girls) =>
        girls.Select(g => g.ToDto());

    public static IEnumerable<KillerDto> ToDtos(this IEnumerable<Killer> killers) =>
        killers.Select(k => k.ToDto());
    public static IQueryable<KillerDto> ToDtos(this IQueryable<Killer> killers) =>
        killers.Select(k => k.ToDto());

    public static IEnumerable<LocationDto> ToDtos(this IEnumerable<Location> locations) =>
        locations.Select(l => l.ToDto());
    public static IQueryable<LocationDto> ToDtos(this IQueryable<Location> locations) =>
        locations.Select(l => l.ToDto());

    public static IEnumerable<UserDto> ToDtos(this IEnumerable<User> users) =>
        users.Select(u => u.ToDto());
    public static IQueryable<UserDto> ToDtos(this IQueryable<User> users) =>
        users.Select(u => u.ToDto());

    public static IEnumerable<GameDto> ToDtos(this IEnumerable<Game> games) =>
        games.Select(g => g.ToDto());
    public static IQueryable<GameDto> ToDtos(this IQueryable<Game> games) =>
        games.Select(g => g.ToDto());
}