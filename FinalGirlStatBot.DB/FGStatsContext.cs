using FinalGirlStatBot.DB.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalGirlStatBot.DB;

public sealed class FGStatsContext : DbContext
{
    public DbSet<Game> Games { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Girl> Girls { get; set; } = null!;

    public DbSet<Killer> Killers { get; set; } = null!;

    public DbSet<Location> Locations { get; set; } = null!;

    public FGStatsContext(DbContextOptions<FGStatsContext> options) : base(options)
    {
        // Явно отключаем lazy loading и change tracking proxy
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
}
