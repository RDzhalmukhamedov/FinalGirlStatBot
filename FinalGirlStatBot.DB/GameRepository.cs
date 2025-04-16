using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class GameRepository : IGameRepository
{
    private readonly FGStatsContext _context;

    public GameRepository(FGStatsContext fgStatsContext)
    {
        _context = fgStatsContext;
    }

    public async Task<Game?> GetById(int id, CancellationToken stoppingToken)
    {
        return await _context.Games.Include(g => g.Girl).Include(g => g.Killer).Include(g => g.Location).FirstOrDefaultAsync(g => g.Id == id, stoppingToken);
    }

    public async Task<List<Game>> GetAll(CancellationToken stoppingToken)
    {
        return await _context.Games.ToListAsync(stoppingToken);
    }

    public async Task<List<Game>> GetByUser(long chatId, CancellationToken stoppingToken)
    {
        return (await _context.Users.Include(u => u.Games).FirstAsync(u => u.ChatId == chatId)).Games.ToList();
    }

    public async Task Add(Game game, CancellationToken stoppingToken)
    {
        game.ShortInfo = game.ToString();
        await _context.Games.AddAsync(game, stoppingToken);
    }

    public void Update(Game game)
    {
        game.ShortInfo = game.ToString();
        _context.Games.Update(game);
    }

    public async Task Delete(int id, CancellationToken stoppingToken)
    {
        var game = await GetById(id, stoppingToken);
        if (game is not null)
        {
            _context.Games.Remove(game);
        }
    }
}