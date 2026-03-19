using FinalGirlStatBot.DB.Abstract;

namespace FinalGirlStatBot.DB;

public class FGStatsUnitOfWork : IFGStatsUnitOfWork
{
    public IUserRepository Users { get; }
    public IGameRepository Games { get; }
    public IGirlRepository Girls { get; }
    public IKillerRepository Killers { get; }
    public ILocationRepository Locations { get; }
    public IBoxRepository Boxes { get; }
    public IUserBoxRepository UserBoxes { get; }

    public FGStatsUnitOfWork(
        IUserRepository userRepository,
        IGameRepository gameRepository,
        IGirlRepository girlRepository,
        IKillerRepository killerRepository,
        ILocationRepository locationRepository,
        IBoxRepository boxRepository,
        IUserBoxRepository userBoxRepository)
    {
        Users = userRepository;
        Games = gameRepository;
        Girls = girlRepository;
        Killers = killerRepository;
        Locations = locationRepository;
        Boxes = boxRepository;
        UserBoxes = userBoxRepository;
    }
}
