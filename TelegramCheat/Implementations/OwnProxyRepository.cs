using Microsoft.EntityFrameworkCore;
using TelegramCheat.Entity;
using TelegramCheat.Interfaces;

namespace TelegramCheat.Implementations;

public class OwnProxyRepository : IOwnProxyRepository
{
    private readonly ApplicationDatabaseContext _context;
    public OwnProxyRepository(ApplicationDatabaseContext context)
    {
        _context = context;
    }
    public async Task AddProxy(OwnProxy proxy)
    {
        await _context.OwnProxies.AddAsync(proxy);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OwnProxy>> GetAll()
    {
        return await _context.OwnProxies.ToListAsync();
    }

    public async Task UpdateProxy(OwnProxy proxy)
    {
        _context.OwnProxies.Update(proxy);
        await _context.SaveChangesAsync();
    }
}
