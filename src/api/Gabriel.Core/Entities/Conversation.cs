using System.Text.Json;
using Gabriel.Core.Personality;

namespace Gabriel.Core.Entities;

public class Conversation
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Owner — every conversation is scoped to a user. Repository queries always
    // filter by this so users only see their own threads. Required since auth
    // landed; pre-auth dev data was wiped on migration.
    public Guid UserId { get; private set; }

    // Project containment (Phase 8). Nullable on the entity to keep the
    // migration backwards-compatible: existing conversations get assigned to
    // each user's lazy-created "Default" project on first interaction. New
    // conversations always have a non-null ProjectId.
    public Guid? ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Drives the avatar's pixel pattern + palette on the client. Each conversation
    // gets its own seed at creation so the visual identity stays stable across loads.
    // Stored as long (DB) but always within JS-safe uint32 range so it round-trips
    // cleanly through JSON.
    public long AvatarSeed { get; private set; }

    // Optional "skin" pins for standalone (Default-project) chats — when set,
    // they override the seed-derived pattern / palette picks. Mirrors the
    // matching fields on Project; the sequence service prefers the project's
    // overrides for project-shared sequences and falls back to the
    // conversation's overrides for standalone chats. See SequenceCatalog.
    public string? PatternOverride { get; private set; }
    public string? PaletteOverride { get; private set; }

    // Rolling summary of everything up to and including SummarizedThroughMessageId.
    // History assembly prepends this as a system message and drops the messages it covers
    // so the provider context stays bounded.
    public string? Summary { get; private set; }
    public Guid? SummarizedThroughMessageId { get; private set; }

    // Serialized ConversationState — turn count, mood, length tracking, user-style
    // flags. Read via GetState(), written via SetState(). JSON column instead of a
    // separate table because the shape evolves and we never query its fields.
    public string? StateJson { get; private set; }

    private readonly List<Message> _messages = new();
    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();

    // EF Core requires a parameterless constructor.
    private Conversation() { }

    public static Conversation Create(Guid userId, Guid projectId, string? title = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId is required.", nameof(projectId));

        // Construct first so we can use the freshly-allocated Id as the
        // default title when the caller didn't supply one. Each new chat
        // therefore gets a unique, distinguishable name out of the box — the
        // user (or a future auto-titler) can still rename via PATCH.
        var conv = new Conversation
        {
            UserId = userId,
            ProjectId = projectId,
            AvatarSeed = GenerateAvatarSeed(),
        };
        conv.Title = string.IsNullOrWhiteSpace(title) ? conv.Id.ToString() : title.Trim();
        return conv;
    }

    // Used by the lazy backfill when a Default project is created for a user
    // who has pre-existing (project-less) conversations.
    public void AssignToProject(Guid projectId)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId is required.", nameof(projectId));
        ProjectId = projectId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RerollAvatar()
    {
        AvatarSeed = GenerateAvatarSeed();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Mirror of Project.SetSkin — null/empty clears the override and lets the
    // seed drive that dimension again. Catalog validation lives in the API
    // layer (Engine has the catalog, Core can't reference Engine).
    public void SetSkin(string? pattern, string? palette)
    {
        PatternOverride = string.IsNullOrWhiteSpace(pattern) ? null : pattern.Trim();
        PaletteOverride = string.IsNullOrWhiteSpace(palette) ? null : palette.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // 1..2^32-1 — positive, fits JS Number safely, matches the client RNG's expected range.
    private static long GenerateAvatarSeed() => Random.Shared.NextInt64(1L, 1L << 32);

    public Message AppendMessage(
        MessageRole role,
        string? content,
        string? toolCallId = null,
        string? toolCallsJson = null,
        Guid? variantGroupId = null)
    {
        var message = Message.Create(Id, role, content, toolCallId, toolCallsJson, variantGroupId);
        _messages.Add(message);
        UpdatedAt = DateTimeOffset.UtcNow;
        return message;
    }

    public Message AppendUserMessage(string content) => AppendMessage(MessageRole.User, content);
    public Message AppendAssistantText(string content, Guid? variantGroupId = null, string? reasoningContent = null)
    {
        var msg = AppendMessage(MessageRole.Assistant, content, variantGroupId: variantGroupId);
        if (!string.IsNullOrEmpty(reasoningContent)) msg.SetReasoningContent(reasoningContent);
        return msg;
    }
    public Message AppendAssistantToolCalls(string toolCallsJson, string? content = null, Guid? variantGroupId = null, string? reasoningContent = null)
    {
        var msg = AppendMessage(MessageRole.Assistant, content, toolCallsJson: toolCallsJson, variantGroupId: variantGroupId);
        if (!string.IsNullOrEmpty(reasoningContent)) msg.SetReasoningContent(reasoningContent);
        return msg;
    }
    public Message AppendToolResult(string toolCallId, string content, Guid? variantGroupId = null)
        => AppendMessage(MessageRole.Tool, content, toolCallId: toolCallId, variantGroupId: variantGroupId);

    // Removes the given message and every message after it (by CreatedAt). For
    // assistant messages with regen siblings, we anchor on the earliest message
    // in the variant group so the whole turn is wiped (otherwise an inactive
    // sibling would be orphaned with no active partner).
    //
    // Returns the removed messages so the repository can detach them from the
    // change tracker.
    public IReadOnlyList<Message> TruncateFrom(Guid messageId)
    {
        var target = _messages.FirstOrDefault(m => m.Id == messageId)
            ?? throw new ArgumentException("Message not found in conversation.", nameof(messageId));

        // Anchor on the earliest sibling in the same variant group.
        var anchor = _messages
            .Where(m => m.VariantGroupId == target.VariantGroupId)
            .OrderBy(m => m.CreatedAt)
            .First();

        var toRemove = _messages.Where(m => m.CreatedAt >= anchor.CreatedAt).ToList();
        foreach (var m in toRemove) _messages.Remove(m);
        UpdatedAt = DateTimeOffset.UtcNow;
        return toRemove;
    }

    // Marks every assistant message in the variant group inactive — used by
    // regenerate to deactivate the prior reply before streaming the new variant.
    // The tool aftermath of the inactive variants is already excluded from
    // provider history by tool_call_id matching (see AgentService.ToProviderHistory).
    public void DeactivateVariantGroup(Guid variantGroupId)
    {
        var changed = false;
        foreach (var m in _messages.Where(m => m.VariantGroupId == variantGroupId && m.IsActiveVariant))
        {
            m.MarkInactiveVariant();
            changed = true;
        }
        if (changed) UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Switches which variant in a group is active. The target's siblings get
    // flipped off, the target gets flipped on. No-op if already the active one.
    public void SetActiveVariant(Guid messageId)
    {
        var target = _messages.FirstOrDefault(m => m.Id == messageId)
            ?? throw new ArgumentException("Message not found in conversation.", nameof(messageId));

        var changed = false;
        foreach (var m in _messages.Where(m => m.VariantGroupId == target.VariantGroupId))
        {
            var shouldBeActive = m.Id == target.Id;
            if (m.IsActiveVariant != shouldBeActive)
            {
                if (shouldBeActive) m.MarkActiveVariant();
                else m.MarkInactiveVariant();
                changed = true;
            }
        }
        if (changed) UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        Title = title.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateSummary(string summary, Guid throughMessageId)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));
        Summary = summary;
        SummarizedThroughMessageId = throughMessageId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public ConversationState? GetState()
        => string.IsNullOrEmpty(StateJson) ? null : JsonSerializer.Deserialize<ConversationState>(StateJson);

    public void SetState(ConversationState state)
    {
        StateJson = JsonSerializer.Serialize(state);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
