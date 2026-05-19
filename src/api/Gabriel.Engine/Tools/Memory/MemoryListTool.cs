using System.Text;
using Gabriel.Core.Services;

namespace Gabriel.Engine.Tools.Memory;

// Lets the agent enumerate the memories it can see for this conversation
// before deciding whether to read the body of any specific one. Already-
// loaded memories appear in the system prompt as a list (name + description
// pairs); this tool is the way for the agent to walk that list on demand
// without bloating the system prompt.
public sealed class MemoryListTool : ITool
{
    private readonly IMemoryService _memories;
    private readonly IToolExecutionContext _context;

    public MemoryListTool(IMemoryService memories, IToolExecutionContext context)
    {
        _memories = memories;
        _context = context;
    }

    public string Name => "memory_list";

    public string Description =>
        "List every memory the user has saved that's visible to this conversation: " +
        "all user-scope memories plus this project's memories (if any). Returns " +
        "type, scope, name, and one-line description per entry. Use this when " +
        "deciding whether a prior memory is relevant before responding.";

    public string ParametersJsonSchema => """{"type":"object","properties":{}}""";

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        IReadOnlyList<Core.Entities.MemoryEntry> entries;
        try
        {
            entries = await _memories.ListForConversationAsync(_context.ProjectId, ct);
        }
        catch (Exception ex)
        {
            return $"Error: could not list memories — {ex.Message}";
        }

        if (entries.Count == 0) return "No memories saved yet.";

        var sb = new StringBuilder();
        sb.Append("Memories visible to this conversation (").Append(entries.Count).AppendLine("):");
        foreach (var m in entries)
        {
            var scope = m.ProjectId is null ? "user" : "project";
            sb.Append("- [").Append(m.Type.ToString().ToLowerInvariant()).Append(", ").Append(scope).Append("] ")
              .Append(m.Name).Append(" — ").AppendLine(m.Description);
        }
        return sb.ToString().TrimEnd();
    }
}
