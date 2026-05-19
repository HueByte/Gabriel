using Gabriel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gabriel.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.UserId).IsRequired();
        // Project containment (Phase 8). Nullable so pre-existing conversations
        // survive the migration; backfilled lazily when the user first creates
        // or visits a project.
        builder.Property(c => c.ProjectId);
        builder.Property(c => c.Title).HasMaxLength(256).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();
        builder.Property(c => c.AvatarSeed).IsRequired();

        // Sidebar list (UserId + UpdatedAt desc) — the hot query for the dashboard.
        builder.HasIndex(c => new { c.UserId, c.UpdatedAt });
        // Project filter — list conversations within a single project.
        builder.HasIndex(c => new { c.ProjectId, c.UpdatedAt });

        // Rolling-summary columns — both nullable until the conversation crosses the compact threshold.
        builder.Property(c => c.Summary);
        builder.Property(c => c.SummarizedThroughMessageId);

        // Conversation behavioral state (ConversationState serialized to JSON).
        // Nullable — populated lazily on the first user turn.
        builder.Property(c => c.StateJson);

        // Map the private _messages backing field so callers can only mutate via the aggregate.
        var messagesNav = builder.Metadata.FindNavigation(nameof(Conversation.Messages))!;
        messagesNav.SetPropertyAccessMode(PropertyAccessMode.Field);
        messagesNav.SetField("_messages");

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
