namespace FinalGirlStatBot.DB.Abstract;

public interface IFGStatsUnitOfWork
{
    IUserRepository Users { get; }
    IGameRepository Games { get; }
    IGirlRepository Girls { get; }
    IKillerRepository Killers { get; }
    ILocationRepository Locations { get; }
    IBoxRepository Boxes { get; }
    IUserBoxRepository UserBoxes { get; }
}
