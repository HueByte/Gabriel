using System.Text;
using System.Text.Json;
using Gabriel.Core.Services;

namespace Gabriel.Engine.Tools.Projects;

public sealed class ReadProjectFileTool : ITool
{
    // Cap how many bytes of the file we hand to the model in one call. Larger
    // than this is paginated via the `offset` parameter so the model can pull
    // a long doc in pieces.
    private const int DefaultMaxBytes = 20_000;
    private const int HardCeiling = 80_000;

    private readonly IProjectFileService _files;
    private readonly IToolExecutionContext _context;

    public ReadProjectFileTool(IProjectFileService files, IToolExecutionContext context)
    {
        _files = files;
        _context = context;
    }

    public string Name => "read_project_file";

    public string Description =>
        "Read the contents of a file in THIS conversation's project, BY ID. " +
        "Get IDs from list_project_files first. " +
        "Only text-like files (code, markdown, JSON, plain text, etc.) can be read; " +
        "binary files (PDFs, DOCX, images) are refused — ask the user to convert. " +
        "Output is capped at ~20,000 chars by default; pass `max_bytes` up to 80,000 " +
        "for larger files.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "file_id": {
              "type": "string",
              "description": "The file's GUID. Use list_project_files to discover available IDs."
            },
            "max_bytes": {
              "type": "integer",
              "description": "Max bytes to return. Default 20000, hard ceiling 80000.",
              "default": 20000,
              "minimum": 1024,
              "maximum": 80000
            }
          },
          "required": ["file_id"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        if (_context.ProjectId is not { } projectId)
            return "Error: this conversation isn't attached to a project yet.";

        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("file_id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
            return "Error: 'file_id' is required and must be a string. Use list_project_files first.";
        if (!Guid.TryParse(idEl.GetString(), out var fileId))
            return "Error: 'file_id' is not a valid GUID.";

        var maxBytes = DefaultMaxBytes;
        if (root.TryGetProperty("max_bytes", out var maxEl) && maxEl.ValueKind == JsonValueKind.Number)
            maxBytes = Math.Clamp(maxEl.GetInt32(), 1024, HardCeiling);

        string? content;
        try { content = await _files.ReadTextAsync(projectId, fileId, maxBytes, ct); }
        catch (Exception ex) { return $"Error: could not read file — {ex.Message}"; }

        if (content is null)
            return "Error: this file is not text-like (binary content type). Refusing to dump bytes into the conversation.";

        var sb = new StringBuilder();
        sb.Append("=== BEGIN project file ===").AppendLine();
        sb.AppendLine(content);
        sb.Append("=== END project file ===");
        return sb.ToString();
    }
}
