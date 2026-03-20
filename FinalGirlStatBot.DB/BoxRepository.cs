using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.DB.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class BoxRepository : IBoxRepository
{
    private readonly IDbContextFactory<FGStatsContext> _contextFactory;

    public BoxRepository(IDbContextFactory<FGStatsContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<BoxDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Boxes
            .AsNoTracking()
            .Include(b => b.Location)
            .Include(b => b.Killer)
            .Include(b => b.Girls)
            .ToDtos()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<BoxDto>> GetAll(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.Boxes
            .AsNoTracking()
            .Include(b => b.Location)
            .Include(b => b.Killer)
            .Include(b => b.Girls)
            .OrderByDescending(b => b.Season)
            .ToListAsync(cancellationToken))
            .ToDtos();
    }

    public async Task<IEnumerable<BoxDto>> GetBySeason(Season season, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.Boxes
            .AsNoTracking()
            .Include(b => b.Location)
            .Include(b => b.Killer)
            .Include(b => b.Girls)
            .Where(b => b.Season == season)
            .OrderByDescending(b => b.Id)
            .ToListAsync(cancellationToken))
            .ToDtos();
    }

    public async Task<BoxDto?> GetByGirl(int girlId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Boxes
            .AsNoTracking()
            .Include(b => b.Location)
            .Include(b => b.Killer)
            .Include(b => b.Girls)
            .Where(b => b.Girls.Any(g => g.Id == girlId))
            .ToDtos()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BoxDto?> GetByKiller(int killerId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Boxes
            .AsNoTracking()
            .Include(b => b.Location)
            .Include(b => b.Killer)
            .Include(b => b.Girls)
            .Where(b => b.Killer != null && b.Killer.Id == killerId)
            .ToDtos()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BoxDto?> GetByLocation(int locationId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Boxes
            .AsNoTracking()
            .Include(b => b.Location)
            .Include(b => b.Killer)
            .Include(b => b.Girls)
            .Where(b => b.Location != null && b.Location.Id == locationId)
            .ToDtos()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> Add(BoxDto box, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = new Box
        {
            Name = box.Name,
            Season = box.Season,
            LocationId = box.Location?.Id,
            KillerId = box.Killer?.Id,
        };

        await context.Boxes.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var girlIds = box.Girls.Select(g => g.Id).ToList();
        if (girlIds.Any())
        {
            var girls = await context.Girls.Where(g => girlIds.Contains(g.Id)).ToListAsync(cancellationToken);

            foreach (var girl in girls)
            {
                girl.BoxId = entity.Id;
            }
        }

        if (box.Location is not null)
        {
            var location = await context.Locations.FirstOrDefaultAsync(l => l.Id == box.Location.Id, cancellationToken);
            if (location is not null) location.BoxId = entity.Id;
        }

        if (box.Killer is not null)
        {
            var killer = await context.Locations.FirstOrDefaultAsync(l => l.Id == box.Killer.Id, cancellationToken);
            if (killer is not null) killer.BoxId = entity.Id;
        }

        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var box = await context.Boxes.FindAsync(id, cancellationToken);
        if (box is not null)
        {
            context.Boxes.Remove(box);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}