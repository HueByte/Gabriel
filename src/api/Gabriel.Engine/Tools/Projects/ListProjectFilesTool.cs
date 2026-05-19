using System.Text;
using Gabriel.Core.Services;

namespace Gabriel.Engine.Tools.Projects;

public sealed class ListProjectFilesTool : ITool
{
    private readonly IProjectFileService _files;
    private readonly IToolExecutionContext _context;

    public ListProjectFilesTool(IProjectFileService files, IToolExecutionContext context)
    {
        _files = files;
        _context = context;
    }

    public string Name => "list_project_files";

    public string Description =>
        "List every file uploaded to THIS conversation's project. " +
        "Each project is a folder the user has populated with reference material " +
        "(docs, notes, code, datasets, etc.). Use this to discover what's available " +
        "before reading a specific file with read_project_file. " +
        "Returns file names, sizes, content types, and upload timestamps.";

    public string ParametersJsonSchema => """{"type":"object","properties":{}}""";

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        if (_context.ProjectId is not { } projectId)
            return "Error: this conversation isn't attached to a project yet.";

        IReadOnlyList<Core.Entities.ProjectFile> files;
        try { files = await _files.ListAsync(projectId, ct); }
        catch (Exception ex) { return $"Error: could not list project files — {ex.Message}"; }

        if (files.Count == 0)
            return "No files uploaded to this project yet.";

        var sb = new StringBuilder();
        sb.Append("Project has ").Append(files.Count).AppendLine(" file(s):");
        foreach (var f in files)
        {
            sb.Append("- ").Append(f.Name)
              .Append("  (").Append(FormatSize(f.SizeBytes))
              .Append(", ").Append(f.ContentType)
              .Append(", uploaded ").Append(f.UploadedAt.ToString("u")).AppendLine(")");
        }
        return sb.ToString().TrimEnd();
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KiB",
        _ => $"{bytes / (1024.0 * 1024.0):F1} MiB",
    };
}
