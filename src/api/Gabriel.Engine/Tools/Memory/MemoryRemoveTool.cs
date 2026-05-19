using System.Text.Json;
using Gabriel.Core.Services;

namespace Gabriel.Engine.Tools.Memory;

// Inverse of memory_save. The agent uses this when the user asks to forget
// something, or when a previously-saved entry turns out to be stale/wrong.
// Lookup is by (scope, name) — the slug is what the model can reliably echo
// back from a memory_list result.
public sealed class MemoryRemoveTool : ITool
{
    private readonly IMemoryService _memories;
    private readonly IToolExecutionContext _context;

    public MemoryRemoveTool(IMemoryService memories, IToolExecutionContext context)
    {
        _memories = memories;
        _context = context;
    }

    public string Name => "memory_remove";

    public string Description =>
        "Delete a saved memory entry by its kebab-case name. Use 'user' scope for " +
        "memories that apply across every project; 'project' scope only acts on " +
        "memories saved for THIS project. Returns whether anything was deleted.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "scope": { "type": "string", "enum": ["user", "project"] },
            "name":  { "type": "string", "description": "kebab-case slug of the entry to remove" }
          },
          "required": ["scope", "name"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        MemoryRemoveArgs args;
        try
        {
            args = JsonSerializer.Deserialize<MemoryRemoveArgs>(argumentsJson, JsonOpts)
                ?? throw new InvalidOperationException("null args");
        }
        catch (Exception ex)
        {
            return $"Error: invalid arguments JSON — {ex.Message}";
        }

        if (string.IsNullOrWhiteSpace(args.Name))
        {
            return "Error: name is required.";
        }

        Guid? projectId;
        if (string.Equals(args.Scope, "project", StringComparison.OrdinalIgnoreCase))
        {
            if (_context.ProjectId is not { } pid)
            {
                return "Error: scope='project' but this conversation isn't attached to a project.";
            }
            projectId = pid;
        }
        else
        {
            projectId = null;
        }

        var removed = await _memories.RemoveByNameAsync(projectId, args.Name, ct);
        return removed
            ? $"Removed {(projectId is null ? "user-scope" : "project-scope")} memory '{args.Name}'."
            : $"No {(projectId is null ? "user-scope" : "project-scope")} memory found named '{args.Name}'.";
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record MemoryRemoveArgs(string Scope, string Name);
}
