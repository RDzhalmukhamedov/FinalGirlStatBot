using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.DB.DTOs;

namespace FinalGirlStatBot.DB.Abstract;

public interface IBoxRepository : IRepository<BoxDto>
{
    Task<IEnumerable<BoxDto>> GetBySeason(Season season, CancellationToken stoppingToken);
    Task<BoxDto?> GetByGirl(int girlId, CancellationToken stoppingToken);
    Task<BoxDto?> GetByKiller(int killerId, CancellationToken stoppingToken);
    Task<BoxDto?> GetByLocation(int locationId, CancellationToken stoppingToken);
}