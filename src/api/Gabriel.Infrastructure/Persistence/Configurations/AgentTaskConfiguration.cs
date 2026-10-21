using Gabriel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gabriel.Infrastructure.Persistence.Configurations;

public class AgentTaskConfiguration : IEntityTypeConfiguration<AgentTask>
{
    public void Configure(EntityTypeBuilder<AgentTask> builder)
    {
        builder.ToTable("AgentTasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.ProjectId).IsRequired();
        builder.Property(t => t.ConversationId).IsRequired();
        builder.Property(t => t.Goal).IsRequired();
        builder.Property(t => t.Objective).IsRequired();
        // Short ordered lists the user edits as a whole - JSON columns, not tables.
        builder.PrimitiveCollection(t => t.DoneCriteria).IsRequired();
        builder.PrimitiveCollection(t => t.Constraints).IsRequired();
        builder.Property(t => t.Status).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.Ignore(t => t.IsTerminal);
        builder.Ignore(t => t.CurrentStepCheckpoint);

        // The conversation exists for the task; deleting it takes the task along.
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(t => t.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Log)
            .WithOne()
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Log).HasField("_log").UsePropertyAccessMode(PropertyAccessMode.Field);

        // Hot paths: a user's task list, the worker's runnable scan, and the
        // "is this conversation driven by a task?" guard on every chat send.
        builder.HasIndex(t => new { t.UserId, t.ProjectId, t.UpdatedAt });
        builder.HasIndex(t => new { t.Status, t.UpdatedAt });
        builder.HasIndex(t => t.ConversationId).IsUnique();
    }
}

public class AgentTaskLogEntryConfiguration : IEntityTypeConfiguration<AgentTaskLogEntry>
{
    public void Configure(EntityTypeBuilder<AgentTaskLogEntry> builder)
    {
        builder.ToTable("AgentTaskLogEntries");
        builder.HasKey(e => e.Id);
        // The entity assigns its own Guid. Telling EF the key is never
        // store-generated makes entries appended to AgentTask.Log track as
        // Added; otherwise EF reads the preset key as "existing row" and
        // issues an UPDATE that matches nothing.
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Kind).IsRequired();
        builder.Property(e => e.Text).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.HasIndex(e => new { e.TaskId, e.CreatedAt });
    }
}
