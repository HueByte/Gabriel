using Gabriel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gabriel.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.ConversationId).IsRequired();
        builder.Property(m => m.Role).HasConversion<int>().IsRequired();
        builder.Property(m => m.Content);                         // nullable: assistant-with-only-tool-calls
        builder.Property(m => m.ToolCallId).HasMaxLength(64);    // nullable: set only on Tool-role messages
        builder.Property(m => m.ToolCallsJson);                  // nullable: set only on Assistant-with-tool-calls
        builder.Property(m => m.ReasoningContent);                // nullable: model "thinking" stream, when the provider supplies one
        builder.Property(m => m.CreatedAt).IsRequired();

        // Variant grouping - same value for all regen siblings; equals Id for singletons.
        builder.Property(m => m.VariantGroupId).IsRequired();
        builder.Property(m => m.IsActiveVariant).IsRequired();

        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt });

        // Used by SetActiveVariant + provider-history filtering when there are
        // many variants on a turn. Indexed on the conversation too because
        // VariantGroupId is otherwise globally arbitrary.
        builder.HasIndex(m => new { m.ConversationId, m.VariantGroupId });
    }
}
