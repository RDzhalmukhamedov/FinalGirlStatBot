using FinalGirlStatBot.DB.DTOs;

namespace FinalGirlStatBot.DB.Abstract;

public interface IUserRepository : IRepository<UserDto>
{
    Task<UserDto?> GetByChatId(long chatId, CancellationToken stoppingToken);

    Task<UserDto?> GetByUserId(string userId, CancellationToken stoppingToken);

    Task<UserDto> CreateIfNotExist(long chatId, string? userId, CancellationToken stoppingToken);

}
