using System.Text.Json;
using System.Text.Json.Serialization;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Services;

namespace Gabriel.Engine.Tools.Projects;

// Exact find-and-replace inside an existing text file in THIS conversation's
// project storage. Exact matching (not regex, not line numbers) is the
// contract that holds up across long runs: the model must quote what is
// actually there, and an ambiguous match fails loudly instead of editing the
// wrong occurrence.
public sealed class EditProjectFileTool : ITool
{
    // Edits read the whole file; cap it well above typical docs/code but far
    // below "someone uploaded a dataset".
    private const int MaxEditableBytes = 2 * 1024 * 1024;

    private readonly IProjectFileService _files;
    private readonly IToolExecutionContext _context;

    public EditProjectFileTool(IProjectFileService files, IToolExecutionContext context)
    {
        _files = files;
        _context = context;
    }

    public string Name => "edit_project_file";

    public string Description =>
        "Replace an exact piece of text in an existing text file in THIS conversation's project " +
        "files. `old_text` must match the file exactly (including whitespace) and be unique unless " +
        "`replace_all` is true. Read the file first with read_project_file so you quote it exactly.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "file":        { "type": "string", "description": "Filename (case-insensitive) or file GUID from list_project_files." },
            "old_text":    { "type": "string", "description": "Exact text to replace." },
            "new_text":    { "type": "string", "description": "Replacement text (may be empty to delete)." },
            "replace_all": { "type": "boolean", "description": "Replace every occurrence. Default false.", "default": false }
          },
          "required": ["file", "old_text", "new_text"]
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
        if (string.IsNullOrWhiteSpace(args.File)) return "Error: 'file' is required.";
        if (string.IsNullOrEmpty(args.OldText)) return "Error: 'old_text' must be non-empty.";
        if (args.NewText is null) return "Error: 'new_text' is required (use an empty string to delete).";

        try
        {
            var files = await _files.ListAsync(projectId, ct);
            var target = Guid.TryParse(args.File, out var id)
                ? files.FirstOrDefault(f => f.Id == id)
                : files.FirstOrDefault(f => string.Equals(f.Name, args.File.Trim(), StringComparison.OrdinalIgnoreCase));
            if (target is null)
                return $"Error: no file matches '{args.File}'. Call list_project_files to see available files.";
            if (target.SizeBytes > MaxEditableBytes)
                return $"Error: {target.Name} is too large to edit in place ({target.SizeBytes:N0} bytes).";

            var text = await _files.ReadTextAsync(projectId, target.Id, MaxEditableBytes, ct);
            if (text is null)
                return $"Error: {target.Name} is not a text file.";

            var count = CountOccurrences(text, args.OldText);
            if (count == 0)
                return "Error: old_text was not found. Re-read the file and quote it exactly, including whitespace.";
            if (count > 1 && args.ReplaceAll != true)
                return $"Error: old_text occurs {count} times. Include more surrounding text to make it unique, or set replace_all=true.";

            var updated = text.Replace(args.OldText, args.NewText, StringComparison.Ordinal);
            var written = await _files.WriteTextAsync(projectId, target.Name, updated, overwrite: true, ct);
            return $"Edited {written.Name}: replaced {count} occurrence{(count == 1 ? "" : "s")} ({written.SizeBytes:N0} bytes).";
        }
        catch (Exception ex) when (ex is DomainException or ArgumentException or UnauthorizedAccessException or NotFoundException)
        {
            return $"Error: {ex.Message}";
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var i = text.IndexOf(value, StringComparison.Ordinal); i >= 0;
             i = text.IndexOf(value, i + value.Length, StringComparison.Ordinal))
        {
            count++;
        }
        return count;
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // The schema is snake_case (what models emit most reliably); Web defaults are camelCase.
    private sealed record Args(
        string? File,
        [property: JsonPropertyName("old_text")] string? OldText,
        [property: JsonPropertyName("new_text")] string? NewText,
        [property: JsonPropertyName("replace_all")] bool? ReplaceAll);
}
