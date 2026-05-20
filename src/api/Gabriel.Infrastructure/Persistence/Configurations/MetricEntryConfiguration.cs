using Gabriel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gabriel.Infrastructure.Persistence.Configurations;

public class MetricEntryConfiguration : IEntityTypeConfiguration<MetricEntry>
{
    public void Configure(EntityTypeBuilder<MetricEntry> builder)
    {
        builder.ToTable("MetricEntries");
        builder.HasKey(m => m.Id);

        // 128 is plenty for dotted subsystem names like "web_search.tavily" /
        // "agent_loop.iteration.compact". Constrained so a buggy caller can't
        // explode the index size with megabyte strings.
        builder.Property(m => m.System).HasMaxLength(128).IsRequired();

        // Metric is JSON. No max length - some subsystems may want to record
        // failure messages or full request payloads. SQLite has no native
        // JSON type; TEXT works and SQLite's json_extract / json_each are
        // available if anyone wants SQL-side queries against the payload.
        builder.Property(m => m.Metric).IsRequired();

        builder.Property(m => m.CreatedAt).IsRequired();

        // The primary read patterns - "recent for system X" and "recent for
        // any system matching prefix Y" - both want (System, CreatedAt DESC).
        // SQLite respects ASC index for DESC ORDER BY via backward scan, so
        // this single index covers both directions.
        builder.HasIndex(m => new { m.System, m.CreatedAt });

        // Standalone CreatedAt index for any "delete everything older than X"
        // cleanup queries that don't care about System.
        builder.HasIndex(m => m.CreatedAt);
    }
}
