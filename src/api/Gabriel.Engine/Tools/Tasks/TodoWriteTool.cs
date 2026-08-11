using System.Text.Json;
using Gabriel.Core.Repositories;

namespace Gabriel.Engine.Tools.Tasks;

// Claude Code-style task list. The agent calls this at the start of any
// multi-step task, then again as steps start/finish. Full-replacement
// semantics: every call carries the complete list (no partial patching -
// simpler contract for the model, and stale items can't linger invisibly).
// The list persists on the conversation row, and the rendered observation
// lands in message history so subsequent iterations see the current plan
// without any extra context plumbing.
//
// Not parallel-safe: loads + saves the conversation through the scoped
// repository/DbContext.
public sealed class TodoWriteTool : ITool
{
    private const int MaxItems = 50;
    private static readonly string[] ValidStatuses = ["pending", "in_progress", "completed"];

    private readonly IConversationRepository _conversations;
    private readonly IUnitOfWork _uow;
    private readonly IToolExecutionContext _context;

    public TodoWriteTool(IConversationRepository conversations, IUnitOfWork uow, IToolExecutionContext context)
    {
        _conversations = conversations;
        _uow = uow;
        _context = context;
    }

    public string Name => "todo_write";

    public string Description =>
        "Create or update your working task list for this conversation. Use " +
        "for any task with 3+ distinct steps: write the full plan up front, " +
        "mark exactly one item 'in_progress' before working on it, and mark " +
        "it 'completed' immediately when done. Each call REPLACES the entire " +
        "list, so always send every item. Keeps multi-step work transparent " +
        "to the user and keeps you from dropping steps.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "todos": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "content": { "type": "string", "description": "imperative description of the step" },
                  "status":  { "type": "string", "enum": ["pending", "in_progress", "completed"] }
                },
                "required": ["content", "status"]
              }
            }
          },
          "required": ["todos"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        if (_context.ConversationId is not { } conversationId || _context.UserId is not { } userId)
        {
            return "Error: no conversation context available.";
        }

        TodoWriteArgs args;
        try
        {
            args = JsonSerializer.Deserialize<TodoWriteArgs>(argumentsJson, JsonOpts)
                ?? throw new InvalidOperationException("null args");
        }
        catch (Exception ex)
        {
            return $"Error: invalid arguments JSON — {ex.Message}";
        }

        var todos = args.Todos ?? [];
        if (todos.Count > MaxItems)
        {
            return $"Error: too many items ({todos.Count}); keep the list under {MaxItems}.";
        }
        if (todos.Any(t => string.IsNullOrWhiteSpace(t.Content)))
        {
            return "Error: every todo needs non-empty content.";
        }
        var badStatus = todos.FirstOrDefault(t => !ValidStatuses.Contains(t.Status, StringComparer.OrdinalIgnoreCase));
        if (badStatus is not null)
        {
            return $"Error: invalid status '{badStatus.Status}' — use pending, in_progress, or completed.";
        }
        if (todos.Count(t => t.Status.Equals("in_progress", StringComparison.OrdinalIgnoreCase)) > 1)
        {
            return "Error: at most one item may be in_progress at a time.";
        }

        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct);
        if (conversation is null)
        {
            return "Error: conversation not found.";
        }

        var normalized = todos
            .Select(t => new TodoItem(t.Content.Trim(), t.Status.ToLowerInvariant()))
            .ToList();
        conversation.SetTodoList(todos.Count == 0 ? null : JsonSerializer.Serialize(normalized, JsonOpts));
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);

        return todos.Count == 0 ? "Todo list cleared." : Render(normalized);
    }

    internal static string Render(IReadOnlyList<TodoItem> todos)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Todo list updated:");
        foreach (var t in todos)
        {
            var marker = t.Status switch
            {
                "completed" => "[x]",
                "in_progress" => "[~]",
                _ => "[ ]",
            };
            sb.Append("- ").Append(marker).Append(' ').AppendLine(t.Content);
        }
        var done = todos.Count(t => t.Status == "completed");
        sb.Append('(').Append(done).Append('/').Append(todos.Count).Append(" completed)");
        return sb.ToString();
    }

    internal static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    internal sealed record TodoItem(string Content, string Status);

    private sealed record TodoWriteArgs(List<TodoItem>? Todos);
}
