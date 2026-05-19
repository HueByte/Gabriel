namespace Gabriel.API.Contracts.Projects;

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    string? SystemPrompt,
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
