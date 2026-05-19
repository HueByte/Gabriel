using System.Text.Json;
using Gabriel.Core.Entities;
using Gabriel.Core.Services;

namespace Gabriel.Engine.Tools.Memory;

// Agent-callable "remember this" tool. Idempotent — if a memory with the
// given Name already exists in the requested scope, it's updated in place
// instead of duplicated. The agent picks the scope by setting `scope` to
// "user" (applies to every conversation) or "project" (only inside the
// current project).
public sealed class MemorySaveTool : ITool
{
    private readonly IMemoryService _memories;
    private readonly IToolExecutionContext _context;

    public MemorySaveTool(IMemoryService memories, IToolExecutionContext context)
    {
        _memories = memories;
        _context = context;
    }

    public string Name => "memory_save";

    public string Description =>
        "Save a memory entry that future conversations will see. Use this when " +
        "the user tells you something durable about themselves, gives you " +
        "feedback worth keeping ('don't do X', 'keep doing Y'), shares project " +
        "context not visible in the code, or points at an external reference. " +
        "Pick `scope`: 'user' for things that apply across every project, " +
        "'project' for things that only matter inside THIS project. Pick `type` " +
        "from {user, feedback, project, reference}. `name` is a short kebab-case " +
        "slug — saving twice with the same name updates the existing entry.";

    // Note the explicit enums on `scope` + `type` so the model can't invent values.
    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "scope":       { "type": "string", "enum": ["user", "project"] },
            "type":        { "type": "string", "enum": ["user", "feedback", "project", "reference"] },
            "name":        { "type": "string", "description": "kebab-case slug, unique within scope" },
            "description": { "type": "string", "description": "one-line summary used at retrieval time" },
            "body":        { "type": "string", "description": "the actual content. For feedback/project entries: rule/fact then **Why:** and **How to apply:** lines." }
          },
          "required": ["scope", "type", "name", "description", "body"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        MemorySaveArgs args;
        try
        {
            args = JsonSerializer.Deserialize<MemorySaveArgs>(argumentsJson, JsonOpts)
                ?? throw new InvalidOperationException("null args");
        }
        catch (Exception ex)
        {
            return $"Error: invalid arguments JSON — {ex.Message}";
        }

        if (string.IsNullOrWhiteSpace(args.Name) ||
            string.IsNullOrWhiteSpace(args.Description) ||
            string.IsNullOrWhiteSpace(args.Body))
        {
            return "Error: name, description, and body must all be non-empty.";
        }

        Guid? projectId;
        if (string.Equals(args.Scope, "project", StringComparison.OrdinalIgnoreCase))
        {
            if (_context.ProjectId is not { } pid)
            {
                return "Error: scope='project' but this conversation isn't attached to a project. " +
                       "Use scope='user' or attach the conversation to a project first.";
            }
            projectId = pid;
        }
        else
        {
            projectId = null;  // user-scope
        }

        if (!TryParseType(args.Type, out var type))
        {
            return $"Error: type must be one of user, feedback, project, reference (got '{args.Type}').";
        }

        try
        {
            var saved = await _memories.SaveAsync(
                new MemoryEntrySpec(projectId, type, args.Name, args.Description, args.Body),
                ct);

            var scopeLabel = projectId is null ? "user-scope" : "project-scope";
            return $"Saved {scopeLabel} memory [{saved.Type.ToString().ToLowerInvariant()}] '{saved.Name}'.";
        }
        catch (Exception ex)
        {
            return $"Error: could not save memory — {ex.Message}";
        }
    }

    private static bool TryParseType(string raw, out MemoryEntryType type)
        => Enum.TryParse(raw, ignoreCase: true, out type);

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record MemorySaveArgs(
        string Scope,
        string Type,
        string Name,
        string Description,
        string Body);
}
