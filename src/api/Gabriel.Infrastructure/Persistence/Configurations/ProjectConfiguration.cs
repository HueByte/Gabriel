using Gabriel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gabriel.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.OwnerUserId).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2048);
        builder.Property(p => p.SystemPrompt);  // nullable, long-text by default
        builder.Property(p => p.AvatarSeed).IsRequired();
        builder.Property(p => p.IsDefault).IsRequired();
        builder.Property(p => p.PatternOverride).HasMaxLength(32);  // catalog ids are short
        builder.Property(p => p.PaletteOverride).HasMaxLength(32);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        // List path: a user's own projects sorted by UpdatedAt.
        builder.HasIndex(p => new { p.OwnerUserId, p.UpdatedAt });

        // Identity-style lookup: there is at most one Default per user.
        // Filtered index keeps it cheap. SQLite supports filtered indexes.
        builder.HasIndex(p => new { p.OwnerUserId, p.IsDefault })
            .HasFilter("\"IsDefault\" = 1");

        // Map the private _files backing field so callers mutate via the aggregate.
        var filesNav = builder.Metadata.FindNavigation(nameof(Project.Files))!;
        filesNav.SetPropertyAccessMode(PropertyAccessMode.Field);
        filesNav.SetField("_files");

        builder.HasMany(p => p.Files)
            .WithOne()
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
