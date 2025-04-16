using FinalGirlStatBot.DB.Abstract;

namespace FinalGirlStatBot.DB;

public class FGStatsUnitOfWork : IFGStatsUnitOfWork
{
    private bool _isDisposed;
    private readonly FGStatsContext _context;

    public IUserRepository Users { get; }
    public IGameRepository Games { get; }
    public IGirlRepository Girls { get; }
    public IKillerRepository Killers { get; }
    public ILocationRepository Locations { get; }

    public FGStatsUnitOfWork(FGStatsContext fgStatsContext,
        IUserRepository userRepository,
        IGameRepository gameRepository,
        IGirlRepository girlRepository,
        IKillerRepository killerRepository,
        ILocationRepository locationRepository)
    {
        _context = fgStatsContext;

        Users = userRepository;
        Games = gameRepository;
        Girls = girlRepository;
        Killers = killerRepository;
        Locations = locationRepository;
    }

    public Task<int> Commit(CancellationToken stoppingToken)
    {
        return _context.SaveChangesAsync(stoppingToken);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _context.Dispose();
    }
}