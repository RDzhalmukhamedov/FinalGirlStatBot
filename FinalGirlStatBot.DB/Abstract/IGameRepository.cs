using FinalGirlStatBot.DB.DTOs;

namespace FinalGirlStatBot.DB.Abstract;

public interface IGameRepository : IRepository<GameDto>
{
    Task<IEnumerable<GameDto>> GetByUser(long chatId, CancellationToken stoppingToken);
}
