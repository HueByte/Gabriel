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

    private readonly List<ProjectFile> _files = new();
    public IReadOnlyList<ProjectFile> Files => _files.AsReadOnly();

    private Project() { }

    public static Project Create(Guid ownerUserId, string name, string? description = null, string? systemPrompt = null)
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
        };
    }

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
