using Gabriel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gabriel.Infrastructure.Persistence.Configurations;

public class ProjectFileConfiguration : IEntityTypeConfiguration<ProjectFile>
{
    public void Configure(EntityTypeBuilder<ProjectFile> builder)
    {
        builder.ToTable("ProjectFiles");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.ProjectId).IsRequired();
        builder.Property(f => f.Name).HasMaxLength(256).IsRequired();
        builder.Property(f => f.RelativePath).HasMaxLength(512).IsRequired();
        builder.Property(f => f.SizeBytes).IsRequired();
        builder.Property(f => f.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(f => f.UploadedAt).IsRequired();

        // List path: files for a project sorted by upload time.
        builder.HasIndex(f => new { f.ProjectId, f.UploadedAt });

        // Same-name within a project is rejected via service-layer check + the
        // RelativePath uniqueness covers cross-renames cleanly.
        builder.HasIndex(f => new { f.ProjectId, f.RelativePath }).IsUnique();
    }
}
