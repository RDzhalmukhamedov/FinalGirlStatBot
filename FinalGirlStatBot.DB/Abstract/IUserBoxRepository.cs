namespace FinalGirlStatBot.DB.Abstract;

public interface IUserBoxRepository
{
    Task<IEnumerable<int>> GetBoxIdsForUser(long chatId, CancellationToken cancellationToken = default);
    Task SetBoxesForUser(long chatId, IEnumerable<int> boxIds, CancellationToken cancellationToken = default);
}
