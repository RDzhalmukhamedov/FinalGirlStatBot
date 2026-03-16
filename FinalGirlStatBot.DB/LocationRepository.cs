using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.DB.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class LocationRepository : ILocationRepository
{
    private readonly IDbContextFactory<FGStatsContext> _contextFactory;

    public LocationRepository(IDbContextFactory<FGStatsContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<LocationDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var location = await context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        return location?.ToDto();
    }

    public async Task<IEnumerable<LocationDto>> GetAll(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.Locations.AsNoTracking().OrderBy(l => l.Id).ToListAsync(cancellationToken)).ToDtos();
    }

    public async Task<int> Add(LocationDto location, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = location.ToEntity();
        await context.Locations.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var location = await context.Locations.FindAsync([id], cancellationToken);
        if (location is not null)
        {
            context.Locations.Remove(location);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}