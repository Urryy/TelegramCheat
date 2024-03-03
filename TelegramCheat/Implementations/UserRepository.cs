using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using TelegramCheat.Entity;
using TelegramCheat.Interfaces;

namespace TelegramCheat.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDatabaseContext _context;
    public UserRepository(ApplicationDatabaseContext context)
    {
        _context = context;
    }
    public async Task AddRangeUser(List<Entity.User> users)
    {
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }

    public async Task AddUser(Entity.User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Entity.User>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task RemoveRangeUsers(List<Entity.User> users)
    {
        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUser(Entity.User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUser(Entity.User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
