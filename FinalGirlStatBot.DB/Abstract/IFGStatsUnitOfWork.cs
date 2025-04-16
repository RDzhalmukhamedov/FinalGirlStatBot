namespace FinalGirlStatBot.DB.Abstract;

public interface IFGStatsUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IGameRepository Games { get; }
    IGirlRepository Girls { get; }
    IKillerRepository Killers { get; }
    ILocationRepository Locations { get; }

    Task<int> Commit(CancellationToken stoppingToken);
}
