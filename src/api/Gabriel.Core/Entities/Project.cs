namespace Gabriel.Core.Entities;

// User-owned container that groups conversations + files + a personality
// override. Phase 8 feature.
//
// `SystemPrompt` is prepended to the agent's per-turn history (after the
// global persona, before the rolling summary). Each user has at minimum a
// "Default" project, created lazily on first interaction.
public class Project
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? SystemPrompt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Drives the *shared* Gabriel Sequence rendered for this project. Every
    // user-created project has a stable seed so all of its conversations show
    // the same avatar — the project itself has an identity. The auto-created
    // "Default" project (see IsDefault) still carries a seed but the client
    // ignores it: conversations in the Default bucket each render their own
    // per-conversation sequence ("standalone" behavior).
    public long AvatarSeed { get; private set; }

    // True for the lazy-created Default project; false for user-created ones.
    // Used by the client to switch between project-shared and per-conversation
    // sequence rendering, and to suppress project-level affordances (file UI,
    // shared diagnostics) that don't make sense for the "standalone bucket".
    public bool IsDefault { get; private set; }

    // Optional "skin" pins that override the seed-derived Gabriel Sequence
    // pattern / palette picks. Both are catalog identifiers (lowercase, e.g.
    // "plasma" / "heat"). Null means "use whatever the seed chooses" — the
    // default behavior. See SequenceCatalog for the known values.
    public string? PatternOverride { get; private set; }
    public string? PaletteOverride { get; private set; }

    private readonly List<ProjectFile> _files = new();
    public IReadOnlyList<ProjectFile> Files => _files.AsReadOnly();

    private Project() { }

    public static Project Create(
        Guid ownerUserId,
        string name,
        string? description = null,
        string? systemPrompt = null,
        bool isDefault = false)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("OwnerUserId is required.", nameof(ownerUserId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        return new Project
        {
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            Description = description?.Trim(),
            SystemPrompt = systemPrompt?.Trim(),
            AvatarSeed = GenerateAvatarSeed(),
            IsDefault = isDefault,
        };
    }

    public void RerollAvatar()
    {
        AvatarSeed = GenerateAvatarSeed();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Pin / clear the avatar's pattern + palette. Callers should pass already-
    // validated catalog identifiers (lower-case, known names) or null to
    // clear; the entity doesn't validate against the catalog because the
    // catalog lives in Engine and Core can't reference it. Empty strings are
    // treated as nulls to keep the API caller's "PATCH with empty" friendly.
    public void SetSkin(string? pattern, string? palette)
    {
        PatternOverride = string.IsNullOrWhiteSpace(pattern) ? null : pattern.Trim();
        PaletteOverride = string.IsNullOrWhiteSpace(palette) ? null : palette.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Matches Conversation.GenerateAvatarSeed — 1..2^32-1 so the value
    // round-trips through JSON Number safely.
    private static long GenerateAvatarSeed() => Random.Shared.NextInt64(1L, 1L << 32);

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.", nameof(name));
        Name = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateSystemPrompt(string? systemPrompt)
    {
        SystemPrompt = systemPrompt?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void AddFile(ProjectFile file)
    {
        if (file.ProjectId != Id)
            throw new ArgumentException("File does not belong to this project.", nameof(file));
        _files.Add(file);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void RemoveFile(ProjectFile file)
    {
        _files.Remove(file);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
