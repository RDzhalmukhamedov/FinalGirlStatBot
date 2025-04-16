using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class LocationRepository : ILocationRepository
{
    private readonly FGStatsContext _context;

    public LocationRepository(FGStatsContext fgStatsContext)
    {
        _context = fgStatsContext;
    }

    public async Task<Location?> GetById(int id, CancellationToken stoppingToken)
    {
        return await _context.Locations.FindAsync(id, stoppingToken);
    }

    public async Task<List<Location>> GetAll(CancellationToken stoppingToken)
    {
        return await _context.Locations.ToListAsync(stoppingToken);
    }

    public async Task Add(Location location, CancellationToken stoppingToken)
    {
        await _context.Locations.AddAsync(location, stoppingToken);
    }

    public void Update(Location location)
    {
        _context.Locations.Update(location);
    }

    public async Task Delete(int id, CancellationToken stoppingToken)
    {
        var location = await GetById(id, stoppingToken);
        if (location is not null)
        {
            _context.Locations.Remove(location);
        }
    }
}