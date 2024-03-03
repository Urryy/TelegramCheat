using Microsoft.EntityFrameworkCore;
using System.Data;
using TelegramCheat.Entity;

namespace TelegramCheat;

public class ApplicationDatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<OwnProxy> OwnProxies { get; set; }
    public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
            : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(x => x.Id);
        modelBuilder.Entity<OwnProxy>().HasKey(x => x.Id);
    }
}
