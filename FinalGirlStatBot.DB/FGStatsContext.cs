using FinalGirlStatBot.DB.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinalGirlStatBot.DB;

public sealed class FGStatsContext : DbContext
{
    private readonly DbConfiguration _config;

    public DbSet<Game> Games { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Girl> Girls { get; set; } = null!;

    public DbSet<Killer> Killers { get; set; } = null!;

    public DbSet<Location> Locations { get; set; } = null!;

    public FGStatsContext(IOptions<DbConfiguration> configuration)
    {
        _config = configuration.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql($"Host={_config.Host};Port={_config.Port};Database={_config.DbName};"
                                 + $"Username={_config.Username};Password={_config.Password}");
    }
}
