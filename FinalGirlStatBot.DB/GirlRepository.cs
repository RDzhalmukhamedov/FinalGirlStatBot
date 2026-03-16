using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.DB.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class GirlRepository : IGirlRepository
{
    private readonly IDbContextFactory<FGStatsContext> _contextFactory;

    public GirlRepository(IDbContextFactory<FGStatsContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<GirlDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var girl = await context.Girls.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        return girl?.ToDto();
    }

    public async Task<IEnumerable<GirlDto>> GetAll(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.Girls.AsNoTracking().OrderBy(g => g.Id).ToListAsync(cancellationToken)).ToDtos();
    }

    public async Task<int> Add(GirlDto girl, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = girl.ToEntity();
        await context.Girls.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var girl = await context.Girls.FindAsync(id, cancellationToken);
        if (girl is not null)
        {
            context.Girls.Remove(girl);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}