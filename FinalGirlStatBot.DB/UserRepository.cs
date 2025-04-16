﻿using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.DB.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public class UserRepository : IUserRepository
{
    private readonly FGStatsContext _context;

    public UserRepository(FGStatsContext fgStatsContext)
    {
        _context = fgStatsContext;
    }

    public async Task<User?> GetById(int id, CancellationToken stoppingToken)
    {
        return await _context.Users.FindAsync(id, stoppingToken);
    }

    public async Task<User?> GetByChatId(long chatId, CancellationToken stoppingToken)
    {
        return await _context.Users.AsQueryable().FirstOrDefaultAsync(u => u.ChatId == chatId, stoppingToken);
    }

    public async Task<User?> GetByUserId(string userId, CancellationToken stoppingToken)
    {
        return await _context.Users.AsQueryable().FirstOrDefaultAsync(u => u.UserId == userId, stoppingToken);
    }

    public async Task<List<User>> GetAll(CancellationToken stoppingToken)
    {
        return await _context.Users.ToListAsync(stoppingToken);
    }

    public async Task Add(User user, CancellationToken stoppingToken)
    {
        await _context.Users.AddAsync(user, stoppingToken);
    }

    public async Task<User> CreateIfNotExist(long chatId, string userId, CancellationToken stoppingToken)
    {
        var user = await GetByChatId(chatId, stoppingToken);
        if (user is null)
        {
            user = new User() { ChatId = chatId, UserId = userId };
            await Add(user, stoppingToken);
        }

        return user;
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task Delete(int id, CancellationToken stoppingToken)
    {
        var user = await GetById(id, stoppingToken);
        if (user is not null)
        {
            _context.Users.Remove(user);
        }
    }
}