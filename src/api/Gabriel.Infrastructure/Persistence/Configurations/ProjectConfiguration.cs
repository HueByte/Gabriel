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
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        // List path: a user's own projects sorted by UpdatedAt.
        builder.HasIndex(p => new { p.OwnerUserId, p.UpdatedAt });

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
