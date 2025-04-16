using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.Abstract;

public interface IGameRepository : IRepository<Game>
{
    Task<List<Game>> GetByUser(long chatId, CancellationToken stoppingToken);
}
