using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class GirlRepository : IGirlRepository
{
    private readonly FGStatsContext _context;

    public GirlRepository(FGStatsContext fgStatsContext)
    {
        _context = fgStatsContext;
    }

    public async Task<Girl?> GetById(int id, CancellationToken stoppingToken)
    {
        return await _context.Girls.FindAsync(id, stoppingToken);
    }

    public async Task<List<Girl>> GetAll(CancellationToken stoppingToken)
    {
        return await _context.Girls.ToListAsync(stoppingToken);
    }

    public async Task Add(Girl girl, CancellationToken stoppingToken)
    {
        await _context.Girls.AddAsync(girl, stoppingToken);
    }

    public void Update(Girl girl)
    {
        _context.Girls.Update(girl);
    }

    public async Task Delete(int id, CancellationToken stoppingToken)
    {
        var girl = await GetById(id, stoppingToken);
        if (girl is not null)
        {
            _context.Girls.Remove(girl);
        }
    }
}