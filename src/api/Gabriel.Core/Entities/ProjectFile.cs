namespace Gabriel.Core.Entities;

// Metadata for a file uploaded to a project. The bytes themselves live on disk
// under {ProjectsRoot}/{ProjectId:N}/{RelativePath} — this entity only records
// the descriptive shape.
//
// `Name` is the user-facing filename. `RelativePath` is the on-disk path
// relative to the project's directory; path traversal is enforced at the
// service layer (no `..` segments, must resolve under the project root).
public class ProjectFile
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string RelativePath { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string ContentType { get; private set; } = "application/octet-stream";
    public DateTimeOffset UploadedAt { get; private set; } = DateTimeOffset.UtcNow;

    private ProjectFile() { }

    public static ProjectFile Create(Guid projectId, string name, string relativePath, long sizeBytes, string contentType)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId is required.", nameof(projectId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("RelativePath is required.", nameof(relativePath));
        if (sizeBytes < 0)
            throw new ArgumentException("SizeBytes must be non-negative.", nameof(sizeBytes));

        return new ProjectFile
        {
            ProjectId = projectId,
            Name = name.Trim(),
            RelativePath = relativePath.Replace('\\', '/'),
            SizeBytes = sizeBytes,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
        };
    }
}
