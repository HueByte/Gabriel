using Gabriel.Core.Entities;
using Gabriel.Core.Identity;
using Gabriel.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Gabriel.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectFile> ProjectFiles => Set<ProjectFile>();
    public DbSet<MemoryEntry> MemoryEntries => Set<MemoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IdentityDbContext.OnModelCreating wires up the AspNet* tables;
        // call it first, THEN apply our own configurations on top.
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // SQLite has no native DateTimeOffset type and can't ORDER BY it.
        // Persist as a long so comparisons/sorts work in SQL.
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToBinaryConverter>();
    }
}
