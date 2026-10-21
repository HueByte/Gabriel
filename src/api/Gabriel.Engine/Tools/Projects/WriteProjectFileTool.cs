using System.Text.Json;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Services;

namespace Gabriel.Engine.Tools.Projects;

// Creates (or, explicitly, replaces) a text file in THIS conversation's
// project storage. The blast radius is deliberately the project's own file
// area - same sanitizing, extension allowlist, and size cap as user uploads -
// which is why it needs no approval flow while host-filesystem writes would.
// Overwrite is opt-in per call so an unattended task can't silently clobber a
// file the user uploaded.
public sealed class WriteProjectFileTool : ITool
{
    private readonly IProjectFileService _files;
    private readonly IToolExecutionContext _context;

    public WriteProjectFileTool(IProjectFileService files, IToolExecutionContext context)
    {
        _files = files;
        _context = context;
    }

    public string Name => "write_project_file";

    public string Description =>
        "Create a text file (markdown, code, JSON, CSV, plain text, ...) in THIS conversation's " +
        "project files, where the user can see and download it. Use it to deliver real artifacts " +
        "instead of pasting long content into chat. Fails if the file already exists unless " +
        "`overwrite` is true - prefer edit_project_file for changing part of an existing file.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "name":      { "type": "string", "description": "Filename with extension, e.g. 'report.md'. No folders." },
            "content":   { "type": "string", "description": "The complete file content (UTF-8 text)." },
            "overwrite": { "type": "boolean", "description": "Replace the file if it exists. Default false.", "default": false }
          },
          "required": ["name", "content"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        if (_context.ProjectId is not { } projectId)
            return "Error: this conversation isn't attached to a project.";

        Args args;
        try
        {
            args = JsonSerializer.Deserialize<Args>(argumentsJson, JsonOpts) ?? throw new JsonException("null args");
        }
        catch (JsonException ex)
        {
            return $"Error: invalid arguments JSON - {ex.Message}";
        }
        if (string.IsNullOrWhiteSpace(args.Name))
            return "Error: 'name' is required.";
        if (args.Content is null)
            return "Error: 'content' is required.";

        try
        {
            var file = await _files.WriteTextAsync(projectId, args.Name, args.Content, args.Overwrite ?? false, ct);
            return $"Wrote {file.Name} ({file.SizeBytes:N0} bytes) [{file.Id}].";
        }
        catch (Exception ex) when (ex is DomainException or ArgumentException or UnauthorizedAccessException)
        {
            return $"Error: {ex.Message}";
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record Args(string? Name, string? Content, bool? Overwrite);
}
