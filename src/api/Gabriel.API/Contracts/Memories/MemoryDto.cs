namespace Gabriel.API.Contracts.Memories;

// Wire shape of one MemoryEntry. ProjectId null means user-scope; set means
// it's tied to that project (and only visible inside it).
public sealed record MemoryDto(
    Guid Id,
    Guid? ProjectId,
    string Type,
    string Name,
    string Description,
    string Body,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

// POST /api/memories body. Idempotent upsert keyed on (UserId, ProjectId, Name).
// Body / Description / Name required; Type is one of: user, feedback, project, reference.
public sealed record SaveMemoryRequest(
    Guid? ProjectId,
    string Type,
    string Name,
    string Description,
    string Body);
