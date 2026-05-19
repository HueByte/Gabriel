using Gabriel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gabriel.Infrastructure.Persistence.Configurations;

public class MemoryEntryConfiguration : IEntityTypeConfiguration<MemoryEntry>
{
    public void Configure(EntityTypeBuilder<MemoryEntry> builder)
    {
        builder.ToTable("MemoryEntries");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.ProjectId);     // nullable = user-scope
        builder.Property(m => m.Type).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(128).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(512).IsRequired();
        builder.Property(m => m.Body).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        // Hot lookup paths:
        //   * Listing a user's memories in a scope, sorted by UpdatedAt.
        builder.HasIndex(m => new { m.UserId, m.ProjectId, m.UpdatedAt });

        //   * Slug uniqueness within (UserId, ProjectId) — the memory_save tool
        //     uses Name as the upsert key. Project memories and user-scope
        //     memories can share a slug because ProjectId differs (and SQLite
        //     treats nulls as distinct for unique-index purposes by default).
        builder.HasIndex(m => new { m.UserId, m.ProjectId, m.Name }).IsUnique();
    }
}
