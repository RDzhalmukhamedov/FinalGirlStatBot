using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.DB.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class GameRepository : IGameRepository
{
    private readonly IDbContextFactory<FGStatsContext> _contextFactory;

    public GameRepository(IDbContextFactory<FGStatsContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<GameDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Games
            .AsNoTracking()
            .Include(g => g.Girl)
            .Include(g => g.Killer)
            .Include(g => g.Location)
            .ToDtos()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<GameDto>> GetAll(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.Games
            .AsNoTracking()
            .OrderByDescending(g => g.DatePlayed)
            .ToListAsync(cancellationToken))
            .ToDtos();
    }

    public async Task<IEnumerable<GameDto>> GetByUser(long chatId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.Games).ThenInclude(u => u.Girl)
            .Include(u => u.Games).ThenInclude(u => u.Killer)
            .Include(u => u.Games).ThenInclude(u => u.Location)
            .FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);

        return user?.Games.ToDtos().OrderByDescending(g => g.DatePlayed).ToList() ?? [];
    }

    public async Task<int> Add(GameDto game, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = game.ToEntity();
        await context.Games.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var game = await context.Games.FindAsync(id, cancellationToken);
        if (game is not null)
        {
            context.Games.Remove(game);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}