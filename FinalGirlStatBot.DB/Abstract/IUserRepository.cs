using FinalGirlStatBot.DB.Domain;

namespace FinalGirlStatBot.DB.Abstract;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByChatId(long chatId, CancellationToken stoppingToken);

    Task<User?> GetByUserId(string userId, CancellationToken stoppingToken);

    Task<User> CreateIfNotExist(long chatId, string userId, CancellationToken stoppingToken);

    //Task<List<User>> GetUsersWithSubscriptions(CancellationToken stoppingToken);
}
