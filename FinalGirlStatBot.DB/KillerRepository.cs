using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class KillerRepository : IKillerRepository
{
    private readonly FGStatsContext _context;

    public KillerRepository(FGStatsContext fgStatsContext)
    {
        _context = fgStatsContext;
    }

    public async Task<Killer?> GetById(int id, CancellationToken stoppingToken)
    {
        return await _context.Killers.FindAsync(id, stoppingToken);
    }

    public async Task<List<Killer>> GetAll(CancellationToken stoppingToken)
    {
        return await _context.Killers.ToListAsync(stoppingToken);
    }

    public async Task Add(Killer killer, CancellationToken stoppingToken)
    {
        await _context.Killers.AddAsync(killer, stoppingToken);
    }

    public void Update(Killer killer)
    {
        _context.Killers.Update(killer);
    }

    public async Task Delete(int id, CancellationToken stoppingToken)
    {
        var killer = await GetById(id, stoppingToken);
        if (killer is not null)
        {
            _context.Killers.Remove(killer);
        }
    }
}