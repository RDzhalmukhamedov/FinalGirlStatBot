using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.DB.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class KillerRepository : IKillerRepository
{
    private readonly IDbContextFactory<FGStatsContext> _contextFactory;

    public KillerRepository(IDbContextFactory<FGStatsContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<KillerDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var killer = await context.Killers.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id, cancellationToken);

        return killer?.ToDto();
    }

    public async Task<IEnumerable<KillerDto>> GetAll(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.Killers.AsNoTracking().OrderBy(k => k.Id).ToListAsync(cancellationToken)).ToDtos();
    }

    public async Task<int> Add(KillerDto killer, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = killer.ToEntity();
        await context.Killers.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var killer = await context.Killers.FindAsync(id, cancellationToken);
        if (killer is not null)
        {
            context.Killers.Remove(killer);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}