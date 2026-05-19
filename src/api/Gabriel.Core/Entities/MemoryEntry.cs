namespace Gabriel.Core.Entities;

// A single thing Gabriel has remembered. Scoped either to the user globally
// (ProjectId == null — applies to every conversation that user owns) or to
// a specific project (ProjectId set — only loaded when chatting in that
// project). Mirrors Claude Code's auto-memory file shape: a short name
// (kebab-case slug, unique within scope), a one-line description used at
// retrieval time, the body with the actual content, and a type that tells
// the model what kind of thing this is.
//
// Storage is in the DB rather than markdown files because everything else
// in this app already lives in EF/SQLite and the UI editor wants atomic
// CRUD. The on-disk-CLAUDE.md ergonomics are preserved by exposing the
// entries through a settings page where the user can review and edit them
// as plain text.
public class MemoryEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Always set. Memories never leak across users.
    public Guid UserId { get; private set; }

    // null = user-scope (applies everywhere); set = project-scope (only
    // surfaces inside conversations in this project).
    public Guid? ProjectId { get; private set; }

    public MemoryEntryType Type { get; private set; }

    // Kebab-case identifier used by the agent's memory_remove tool and by
    // links between entries. Unique within (UserId, ProjectId) — the same
    // slug can exist at user scope and inside a project at the same time
    // without collision.
    public string Name { get; private set; } = string.Empty;

    // One-line summary used to decide relevance when the agent scans the
    // memory list before deciding whether to read a specific entry.
    public string Description { get; private set; } = string.Empty;

    // The actual content. For Feedback and Project entries the convention
    // is "rule/fact, then **Why:** and **How to apply:** lines" so future
    // reads have enough context to judge edge cases — but this is just a
    // string; the agent enforces the convention via its system prompt.
    public string Body { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private MemoryEntry() { }

    public static MemoryEntry Create(
        Guid userId,
        Guid? projectId,
        MemoryEntryType type,
        string name,
        string description,
        string body)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        Validate(name, description, body);

        return new MemoryEntry
        {
            UserId = userId,
            ProjectId = projectId,
            Type = type,
            Name = name.Trim(),
            Description = description.Trim(),
            Body = body.Trim(),
        };
    }

    // Idempotent update used by the agent's memory_save tool when an entry
    // with the same Name already exists in the same scope. Bumps UpdatedAt;
    // CreatedAt stays put so the UI can show "first remembered on …".
    public void Update(MemoryEntryType type, string description, string body)
    {
        Validate(Name, description, body);
        Type = type;
        Description = description.Trim();
        Body = body.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void Validate(string name, string description, string body)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Memory entry name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Memory entry description is required.", nameof(description));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Memory entry body is required.", nameof(body));
    }
}
