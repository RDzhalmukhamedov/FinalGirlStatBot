namespace FinalGirlStatBot.DB.Abstract;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll(CancellationToken stoppingToken);
    Task<T?> GetById(int id, CancellationToken stoppingToken);
    Task<int> Add(T entity, CancellationToken stoppingToken);
    Task Delete(int id, CancellationToken stoppingToken);
}