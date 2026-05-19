namespace Gabriel.API.Contracts.Projects;

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    string? SystemPrompt,
    long AvatarSeed,
    bool IsDefault,
    string? PatternOverride,
    string? PaletteOverride,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ProjectFileResponse>? Files);

public record ProjectFileResponse(
    Guid Id,
    string Name,
    long SizeBytes,
    string ContentType,
    DateTimeOffset UploadedAt);

public record CreateProjectRequest(string Name, string? Description, string? SystemPrompt);

public record UpdateProjectRequest(string? Name, string? Description, string? SystemPrompt);

// Both fields are sent each call (PUT semantics): null clears that override
// and falls back to seed-derived behavior for that dimension. Catalog
// identifiers are validated against SequenceCatalog at the controller layer.
public record SetSkinRequest(string? Pattern, string? Palette);
