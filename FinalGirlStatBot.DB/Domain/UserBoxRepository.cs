using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class UserBoxRepository : IUserBoxRepository
{
    private readonly IDbContextFactory<FGStatsContext> _contextFactory;

    public UserBoxRepository(IDbContextFactory<FGStatsContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<int>> GetBoxIdsForUser(long chatId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users.FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);
        if (user is null) return Enumerable.Empty<int>();

        return await context.UserBoxes
            .AsNoTracking()
            .Where(ub => ub.UserId == user.Id)
            .Select(ub => ub.BoxId)
            .ToListAsync(cancellationToken);
    }

    public async Task SetBoxesForUser(long chatId, IEnumerable<int> boxIds, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users.FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);
        if (user is null) return;

        var existing = await context.UserBoxes.Where(ub => ub.UserId == user.Id).ToListAsync(cancellationToken);
        var newIds = new HashSet<int>(boxIds);
        foreach (var ub in existing)
        {
            if (!newIds.Contains(ub.BoxId))
            {
                context.UserBoxes.Remove(ub);
            }
        }

        var existingBoxIds = new HashSet<int>(existing.Select(e => e.BoxId));
        foreach (var boxId in newIds)
        {
            if (!existingBoxIds.Contains(boxId))
            {
                context.UserBoxes.Add(new UserBox { UserId = user.Id, BoxId = boxId });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
