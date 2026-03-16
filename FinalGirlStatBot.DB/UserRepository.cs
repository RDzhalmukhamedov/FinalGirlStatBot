using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using FinalGirlStatBot.DB.DTOs;
using FinalGirlStatBot.DB.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<FGStatsContext> _contextFactory;

    public UserRepository(IDbContextFactory<FGStatsContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<UserDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return user?.ToDto();
    }

    public async Task<UserDto?> GetByChatId(long chatId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);
        return user?.ToDto();
    }

    public async Task<UserDto?> GetByUserId(string userId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        return user?.ToDto();
    }

    public async Task<IEnumerable<UserDto>> GetAll(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.Users.AsNoTracking().ToListAsync(cancellationToken)).ToDtos();
    }

    public async Task<int> Add(UserDto user, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = user.ToEntity();
        await context.Users.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<UserDto> CreateIfNotExist(long chatId, string? userId, CancellationToken cancellationToken = default)
    {
        // TODO обновление userId добавить
        // if ((user.UserId is null || !user.UserId.Equals(username)) && username is not null)
        // {
        //     user.UserId = username;
        //     await _db.Users.Update(user, cancellationToken);
        // }
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);
        if (user is null)
        {
            user = new User() { ChatId = chatId, UserId = userId };
            await context.Users.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        return user.ToDto();
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users.FindAsync(id, cancellationToken);
        if (user is not null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}